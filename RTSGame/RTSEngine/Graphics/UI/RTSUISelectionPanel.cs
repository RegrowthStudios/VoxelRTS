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

namespace RTSEngine.Graphics.UI {
    public class RTSUIUnitGroup : IDisposable {
        private static Color COLOR_INACTIVE = new Color(0.7f, 0.7f, 0.7f);
        private static Color COLOR_ACTIVE = new Color(1f, 1f, 1f);

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
        public RectWidget BackPanel {
            get;
            private set;
        }
        public Dictionary<string, Texture2D> IconLibrary {
            get;
            private set;
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
                    groups[i++] = new RTSUIUnitGroup(wr, s);
                }
            }
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

        public void OnNewSelection(InputController ic, List<IEntity> entities) {
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
                string key = string.Join(".", ic.Team.race.FriendlyName, kv.Key.FriendlyName);
                Texture2D t = IconLibrary[key];
                if(t == null) continue;
                groups[wi].Widget.Texture = t;
                groups[wi].Selection = kv.Value;
                wi++;
            }
            foreach(var kv in units) {
                if(wi >= groups.Length) break;
                string key = string.Join(".", ic.Team.race.FriendlyName, kv.Key.FriendlyName);
                Texture2D t = IconLibrary[key];
                if(t == null) continue;
                groups[wi].Widget.Texture = t;
                groups[wi].Selection = kv.Value;
                wi++;
            }
        }
    }
}