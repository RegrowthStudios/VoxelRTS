using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using Microsoft.Xna.Framework;

namespace RTS.Input {
    public class RTSUIHoverPanel {
        public BaseWidget PanelWidget {
            get;
            private set;
        }
        public RectButton TargetButton {
            get;
            private set;
        }
        bool shouldShow;

        public RTSUIHoverPanel(RectButton b, BaseWidget p) {
            TargetButton = b;
            PanelWidget = p;

            TargetButton.OnMouseEntry += TargetButton_OnMouseEntry;
            TargetButton.OnMouseExit += TargetButton_OnMouseExit;
            shouldShow = false;
        }

        public void Update(int mx, int my) {
            if(shouldShow)
                PanelWidget.Anchor = new Point(mx, my);
        }

        void TargetButton_OnMouseEntry(RectButton arg1, Vector2 arg2) {
            shouldShow = true;
        }
        private void TargetButton_OnMouseExit(RectButton arg1, Vector2 arg2) {
            shouldShow = false;
            PanelWidget.Anchor = new Point(-1000000, -1000000);
        }
    }
}
