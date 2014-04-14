using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI.Widgets;
using RTSEngine.Interfaces;
using RTSEngine.Controllers;
using RTSEngine.Data.Team;

namespace RTSEngine.Graphics {
    public class RTSUIUnitGroup : IDisposable {
        public static Color COLOR_INACTIVE = new Color(0.7f, 0.7f, 0.7f);
        public static Color COLOR_ACTIVE = new Color(1f, 1f, 1f);

        public RectButton Widget {
            get;
            private set;
        }
        public List<IEntity> Selection {
            get;
            set;
        }

        public RTSUIUnitGroup(WidgetRenderer wr, int s) {
            Widget = new RectButton(wr, s, s, COLOR_INACTIVE, COLOR_ACTIVE);
        }
        public void Dispose() {
            Widget.Dispose();
            Widget = null;
        }

        public void Hook() {
            Widget.Hook();
        }
        public void Unhook() {
            Widget.Unhook();
        }
    }

    public class RTSUISelectionPanel : IDisposable {
        public const float GROUPS_LAYER_OFF = 0.01f;

        public RectWidget BackPanel {
            get;
            private set;
        }
        public float LayerDepth {
            get { return BackPanel.LayerDepth; }
            set {
                BackPanel.LayerDepth = value;
                foreach(var ug in groups) ug.Widget.LayerDepth = BackPanel.LayerDepth - GROUPS_LAYER_OFF;
            }
        }

        public Dictionary<string, Texture2D> IconLibrary {
            get;
            set;
        }

        public readonly RTSUIUnitGroup[] groups;
        public readonly int Rows, Columns;

        public RTSUISelectionPanel(WidgetRenderer wr, int r, int c, int s, int buf) {
            Rows = r;
            Columns = c;
            groups = new RTSUIUnitGroup[Columns * Rows];
            int i = 0;
            for(int y = 0; y < Rows; y++) {
                for(int x = 0; x < Columns; x++) {
                    groups[i] = new RTSUIUnitGroup(wr, s);
                    if(x > 0) {
                        groups[i].Widget.Parent = groups[i - 1].Widget;
                        groups[i].Widget.Offset = new Point(buf, 0);
                        groups[i].Widget.OffsetAlignX = Alignment.RIGHT;
                    }
                    i++;
                }
                if(y > 0) {
                    int yi = y * Columns;
                    groups[yi].Widget.Parent = groups[yi - Columns].Widget;
                    groups[yi].Widget.Offset = new Point(0, buf);
                    groups[yi].Widget.OffsetAlignY = Alignment.BOTTOM;
                }
            }

            BackPanel = new RectWidget(wr);
            BackPanel.Width = Columns * s + (Columns + 1) * buf;
            BackPanel.Height = Rows * s + (Rows + 1) * buf;
            groups[0].Widget.Parent = BackPanel;
            groups[0].Widget.Offset = new Point(buf, buf);

            LayerDepth = 1f;
            Clear();
        }

        public void Dispose() {
            foreach(var ug in groups) ug.Dispose();
            BackPanel.Dispose();
            BackPanel = null;
        }

        public void Hook() {
            foreach(var ug in groups) ug.Hook();
        }
        public void Unhook() {
            foreach(var ug in groups) ug.Unhook();
        }

        private void Clear() {
            foreach(var ug in groups) {
                ug.Widget.ActiveColor = Color.Transparent;
                ug.Widget.InactiveColor = Color.Transparent;
                ug.Widget.Texture = null;
                ug.Selection = null;
            }
        }
        private void Show(RTSUIUnitGroup ug) {
            ug.Widget.ActiveColor = RTSUIUnitGroup.COLOR_ACTIVE;
            ug.Widget.InactiveColor = RTSUIUnitGroup.COLOR_INACTIVE;
        }

        public RTSUIUnitGroup GetSelection(int x, int y) {
            if(BackPanel.Inside(x, y)) {
                foreach(var ug in groups) {
                    if(ug.Widget.Inside(x, y)) {
                        return ug;
                    }
                }
            }
            return null;
        }

        public void OnNewSelection(ACInputController ic, List<IEntity> entities) {
            Clear();
            if(IconLibrary == null)
                return;

            var units = new Dictionary<RTSUnitData, List<IEntity>>();
            var buildings = new Dictionary<RTSBuildingData, List<IEntity>>();
            for(int i = 0; i < entities.Count; i++) {
                RTSUnit u = entities[i] as RTSUnit;
                if(u != null) {
                    if(units.ContainsKey(u.UnitData))
                        units[u.UnitData].Add(u);
                    else {
                        var lu = new List<IEntity>();
                        lu.Add(u);
                        units.Add(u.UnitData, lu);
                    }
                }
                else {
                    RTSBuilding b = entities[i] as RTSBuilding;
                    if(buildings.ContainsKey(b.BuildingData))
                        buildings[b.BuildingData].Add(b);
                    else {
                        var lb = new List<IEntity>();
                        lb.Add(b);
                        buildings.Add(b.BuildingData, lb);
                    }
                }
            }

            int wi = 0;
            foreach(var kv in buildings) {
                if(wi >= groups.Length) break;
                string key = string.Join(".", ic.Team.Race.FriendlyName, kv.Key.FriendlyName);
                Texture2D t = IconLibrary[key];
                if(t == null) continue;
                groups[wi].Widget.Texture = t;
                groups[wi].Selection = kv.Value;
                Show(groups[wi]);
                wi++;
            }
            foreach(var kv in units) {
                if(wi >= groups.Length) break;
                string key = string.Join(".", ic.Team.Race.FriendlyName, kv.Key.FriendlyName);
                Texture2D t = IconLibrary[key];
                if(t == null) continue;
                groups[wi].Widget.Texture = t;
                groups[wi].Selection = kv.Value;
                Show(groups[wi]);
                wi++;
            }
        }
    }
}