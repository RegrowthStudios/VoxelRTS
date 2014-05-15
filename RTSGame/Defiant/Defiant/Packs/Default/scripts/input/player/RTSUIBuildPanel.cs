using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;

namespace RTS.Input {
    public class RTSUIBuildPanel : IDisposable {
        const float TEXT_H_RATIO = 0.95f;
        const int TEXT_X_OFF = 5;
        const int MOVE_SPEED = 15;

        public ScrollMenu Menu {
            get;
            private set;
        }
        Dictionary<string, RTSBuildingData> buildings;

        public RTSUIBuildPanel(WidgetRenderer wr, int w, int h, int bCount, int sbw, int sbh) {
            Menu = new ScrollMenu(wr, w, h, bCount, sbw, sbh);
            Menu.BaseColor = UserConfig.MainScheme.WidgetBase;
            Menu.HighlightColor = UserConfig.MainScheme.WidgetActive;
            Menu.TextColor = UserConfig.MainScheme.Text;
            Menu.ScrollBarBaseColor = UserConfig.MainScheme.WidgetInactive;
        }
        public void Dispose() {
            Menu.Dispose();
        }

        public void Hook() {
            Menu.Hook();
        }
        public void Unhook() {
            Menu.Unhook();
        }

        public void Build(RTSTeam team) {
            string[] vText = new string[team.Race.ActiveBuildings.Length];
            buildings = new Dictionary<string, RTSBuildingData>();
            for(int i = 0; i < vText.Length; i++) {
                var bd = team.Race.ActiveBuildings[i];
                buildings.Add(bd.FriendlyName, bd);
                vText[i] = bd.FriendlyName;
            }
            Array.Sort(vText);
            Menu.Build(vText);
        }

        public bool Inside(int x, int y) {
            return Menu.Inside(x, y);
        }
        public RTSBuildingData GetSelection(int x, int y) {
            string key = Menu.GetSelection(x, y);
            if(string.IsNullOrWhiteSpace(key)) return null;
            RTSBuildingData d;
            if(buildings.TryGetValue(key, out d)) return d;
            else return null;
        }
    }
}