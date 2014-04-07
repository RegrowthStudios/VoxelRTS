using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI.Widgets;
using RTSEngine.Interfaces;
using RTSEngine.Controllers;

namespace RTSEngine.Graphics.UI {
    public class RTSUIUnitGroup : IDisposable {
        private static Color COLOR_INACTIVE = new Color(0.7f, 0.7f, 0.7f);
        private static Color COLOR_ACTIVE = new Color(1f, 1f, 1f);

        public RectButton Widget {
            get;
            private set;
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

        public readonly RTSUIUnitGroup[,] groups;
        public readonly int Rows, Columns;

        public RTSUISelectionPanel(WidgetRenderer wr, int r, int c, int s, int buf) {
            Rows = r;
            Columns = c;
            groups = new RTSUIUnitGroup[Columns, Rows];
            for(int y = 0; y < Rows; y++) {
                for(int x = 0; x < Columns; x++) {
                    groups[x, y] = new RTSUIUnitGroup(wr, s);
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

        }
    }
}