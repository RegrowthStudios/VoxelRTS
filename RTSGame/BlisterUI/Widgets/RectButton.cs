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

        private ButtonHighlightOptions optDefault, optHighlight;

        private bool isHovered;
        public bool IsHovered {
            get { return isHovered; }
        }

        private bool isHooked;

        public RectButton(WidgetRenderer r, ButtonHighlightOptions inactive, ButtonHighlightOptions active, Texture2D t = null)
            : base(r, t) {
            optDefault = inactive;
            optHighlight = active;

            // Set To Default
            isHooked = false;
            isHovered = false;
            Width = optDefault.Width;
            Height = optDefault.Height;
            Color = optDefault.Color;
        }
        protected override void DisposeOther() {
            base.DisposeOther();
            Unhook();
        }

        public void Hook() {
            if(isHooked) return;
            MouseEventDispatcher.OnMouseMotion += OnMouseMotion;
            MouseEventDispatcher.OnMousePress += OnMousePress;
        }
        public void Unhook() {
            if(!isHooked) return;
            MouseEventDispatcher.OnMouseMotion -= OnMouseMotion;
            MouseEventDispatcher.OnMousePress -= OnMousePress;
        }

        public void SetHover(bool b, Vector2 m) {
            if(isHovered != b) {
                isHovered = b;
                if(isHovered) {
                    Width = optHighlight.Width;
                    Height = optHighlight.Height;
                    Color = optHighlight.Color;
                    if(OnMouseEntry != null)
                        OnMouseEntry(this, m);
                }
                else {
                    Width = optDefault.Width;
                    Height = optDefault.Height;
                    Color = optDefault.Color;
                    if(OnMouseExit != null)
                        OnMouseExit(this, m);
                }
            }
        }

        public void OnMouseMotion(Vector2 p, Vector2 d) {
            int x = (int)p.X;
            int y = (int)p.Y;
            Vector2 r;
            SetHover(Inside(x, y, out r), p);
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