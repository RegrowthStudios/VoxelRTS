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

namespace RTS.Input {
    public class RTSUIGroup<T> : IDisposable where T : class {
        public static Color COLOR_INACTIVE = new Color(0.7f, 0.7f, 0.7f);
        public static Color COLOR_ACTIVE = new Color(1f, 1f, 1f);

        public RectButton Widget {
            get;
            private set;
        }
        public T Data {
            get;
            set;
        }

        public RTSUIGroup(WidgetRenderer wr, int s) {
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

    public class RTSUIPanel2D<T> : IDisposable where T : class {
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

        public readonly RTSUIGroup<T>[] groups;
        public readonly int Rows, Columns;

        public RTSUIPanel2D(WidgetRenderer wr, int r, int c, int s, int buf) {
            Rows = r;
            Columns = c;
            groups = new RTSUIGroup<T>[Columns * Rows];
            int i = 0;
            for(int y = 0; y < Rows; y++) {
                for(int x = 0; x < Columns; x++) {
                    groups[i] = new RTSUIGroup<T>(wr, s);
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

        protected void Clear() {
            foreach(var ug in groups) {
                ug.Widget.ActiveColor = Color.Transparent;
                ug.Widget.InactiveColor = Color.Transparent;
                ug.Widget.Texture = null;
                ug.Data = null;
            }
        }
        protected void Show(RTSUIGroup<T> ug) {
            ug.Widget.ActiveColor = RTSUIGroup<T>.COLOR_ACTIVE;
            ug.Widget.InactiveColor = RTSUIGroup<T>.COLOR_INACTIVE;
        }

        public T GetSelection(int x, int y) {
            if(BackPanel.Inside(x, y)) {
                foreach(var ug in groups) {
                    if(ug.Widget.Inside(x, y)) {
                        return ug.Data;
                    }
                }
            }
            return null;
        }
    }

    public class RTSUISelectionPanel : RTSUIPanel2D<List<IEntity>> {
        public RTSUISelectionPanel(WidgetRenderer wr, int r, int c, int s, int buf)
            : base(wr, r, c, s, buf) { }
        public void OnNewSelection(ACInputController ic, List<IEntity> entities) {
            Clear();
            if(IconLibrary == null)
                return;

            var units = new Dictionary<RTSUnitData, List<IEntity>>();
            var buildings = new Dictionary<RTSBuildingData, List<IEntity>>();
            for(int i = 0; i < entities.Count; i++) {
                RTSUnit u = entities[i] as RTSUnit;
                if(u != null) {
                    if(units.ContainsKey(u.Data))
                        units[u.Data].Add(u);
                    else {
                        var lu = new List<IEntity>();
                        lu.Add(u);
                        units.Add(u.Data, lu);
                    }
                }
                else {
                    RTSBuilding b = entities[i] as RTSBuilding;
                    if(buildings.ContainsKey(b.Data))
                        buildings[b.Data].Add(b);
                    else {
                        var lb = new List<IEntity>();
                        lb.Add(b);
                        buildings.Add(b.Data, lb);
                    }
                }
            }

            int wi = 0;
            foreach(var kv in buildings) {
                if(wi >= groups.Length) break;
                Texture2D t = IconLibrary[kv.Key.IconKey];
                if(t == null) continue;
                groups[wi].Widget.Texture = t;
                groups[wi].Data = kv.Value;
                Show(groups[wi]);
                wi++;
            }
            foreach(var kv in units) {
                if(wi >= groups.Length) break;
                Texture2D t = IconLibrary[kv.Key.IconKey];
                if(t == null) continue;
                groups[wi].Widget.Texture = t;
                groups[wi].Data = kv.Value;
                Show(groups[wi]);
                wi++;
            }
        }
    }

    public class RTSUIBuildingButtonPanel : RTSUIPanel2D<List<ACBuildingButtonController>> {
        public RTSUIBuildingButtonPanel(WidgetRenderer wr, int r, int c, int s, int buf)
            : base(wr, r, c, s, buf) { }
        public void OnNewSelection(ACInputController ic, List<IEntity> entities) {
            Clear();
            if(IconLibrary == null)
                return;

            int bType = -1;
            List<RTSBuilding> sBuildings = new List<RTSBuilding>();
            for(int i = 0; i < entities.Count; i++) {
                if(entities[i] as RTSUnit != null) return;
                RTSBuilding b = entities[i] as RTSBuilding;
                if(b == null || b.Team.Index != ic.TeamIndex || !b.IsBuilt || !b.IsAlive) continue;

                if(bType < 0) bType = b.Data.Index;
                else if(bType != b.Data.Index) return;

                sBuildings.Add(b);
            }

            if(bType < 0) return;

            int wi = 0;
            var bd = ic.Team.Race.Buildings[bType].DefaultButtonControllers;
            foreach(var bc in sBuildings[0].ButtonControllers) {
                if(wi >= groups.Length) break;
                if(!IconLibrary.ContainsKey(bc.IconKey)) continue;
                groups[wi].Widget.Texture = IconLibrary[bc.IconKey];
                var bcs = new List<ACBuildingButtonController>();
                foreach(var nb in sBuildings) {
                    bcs.Add(nb.ButtonControllers[wi]);
                }
                groups[wi].Data = bcs;
                Show(groups[wi]);
                wi++;
            }
        }
    }
}