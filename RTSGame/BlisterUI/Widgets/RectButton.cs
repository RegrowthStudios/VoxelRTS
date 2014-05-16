using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI.Input;

namespace BlisterUI.Widgets {
    public struct ButtonHighlightOptions {
        public int Height;
        public int Width;
        public Color Color;

        public ButtonHighlightOptions(int w, int h, Color c) {
            Width = w;
            Height = h;
            Color = c;
        }
    }

    public class RectButton : RectWidget {
        public event Action<RectButton, Vector2> OnMouseEntry;
        public event Action<RectButton, Vector2> OnButtonPress;
        public event Action<RectButton, Vector2> OnMouseExit;

        // The Inactive State
        private ButtonHighlightOptions optInactive;
        public int InactiveWidth {
            get { return optInactive.Width; }
            set {
                optInactive.Width = value;
                if(!IsHovered) Width = optInactive.Width;
            }
        }
        public int InactiveHeight {
            get { return optInactive.Height; }
            set {
                optInactive.Height = value;
                if(!IsHovered) Height = optInactive.Height;
            }
        }
        public Color InactiveColor {
            get { return optInactive.Color; }
            set {
                optInactive.Color = value;
                if(!IsHovered) Color = optInactive.Color;
            }
        }

        private ButtonHighlightOptions optActive;
        public int ActiveWidth {
            get { return optActive.Width; }
            set {
                optActive.Width = value;
                if(IsHovered) Width = optActive.Width;
            }
        }
        public int ActiveHeight {
            get { return optActive.Height; }
            set {
                optActive.Height = value;
                if(IsHovered) Height = optActive.Height;
            }
        }
        public Color ActiveColor {
            get { return optActive.Color; }
            set {
                optActive.Color = value;
                if(IsHovered) Color = optActive.Color;
            }
        }

        private bool isHovered;
        public bool IsHovered {
            get { return isHovered; }
        }

        private bool isHooked;
        public bool IsHooked {
            get { return isHooked; }
        }

        public RectButton(WidgetRenderer r, ButtonHighlightOptions inactive, ButtonHighlightOptions active, Texture2D t = null)
            : base(r, t) {
            optInactive = inactive;
            optActive = active;

            // Set To Default
            isHooked = false;
            isHovered = false;
            Width = optInactive.Width;
            Height = optInactive.Height;
            Color = optInactive.Color;
        }
        public RectButton(WidgetRenderer r, int w, int h, Color cInactive, Color cActive, Texture2D t = null)
            : base(r, t) {
            optInactive = new ButtonHighlightOptions(w, h, cInactive);
            optActive = new ButtonHighlightOptions(w, h, cActive);

            // Set To Default
            isHooked = false;
            isHovered = false;
            Width = optInactive.Width;
            Height = optInactive.Height;
            Color = optInactive.Color;
        }
        protected override void DisposeOther() {
            base.DisposeOther();
            OnMouseEntry = null;
            OnButtonPress = null;
            OnMouseExit = null;
            Unhook();
        }

        public void Hook() {
            if(isHooked) return;
            isHooked = true;
            MouseEventDispatcher.OnMouseMotion += OnMouseMotion;
            MouseEventDispatcher.OnMousePress += OnMousePress;
        }
        public void Unhook() {
            if(!isHooked) return;
            isHooked = false;
            MouseEventDispatcher.OnMouseMotion -= OnMouseMotion;
            MouseEventDispatcher.OnMousePress -= OnMousePress;
        }

        public void SetActive(bool b, Vector2 m) {
            if(isHovered != b) {
                isHovered = b;
                if(isHovered) {
                    Width = optActive.Width;
                    Height = optActive.Height;
                    Color = optActive.Color;
                    if(OnMouseEntry != null)
                        OnMouseEntry(this, m);
                }
                else {
                    Width = optInactive.Width;
                    Height = optInactive.Height;
                    Color = optInactive.Color;
                    if(OnMouseExit != null)
                        OnMouseExit(this, m);
                }
            }
        }

        public void OnMouseMotion(Vector2 p, Vector2 d) {
            int x = (int)p.X;
            int y = (int)p.Y;
            Vector2 r;
            SetActive(Inside(x, y, out r), p);
        }
        public void OnMousePress(Vector2 p, MouseButton b) {
            OnMouseMotion(p, Vector2.Zero);
            if(b == MouseButton.Left) {
                if(IsHovered && OnButtonPress != null)
                    OnButtonPress(this, p);
            }
        }
    }
}