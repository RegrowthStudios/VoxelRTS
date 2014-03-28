using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
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
        public event Action<RectButton> OnMouseEntry;
        public event Action<RectButton> OnButtonPress;
        public event Action<RectButton> OnMouseExit;

        private ButtonHighlightOptions optDefault, optHighlight;

        private bool isHovered;
        public bool IsHovered {
            get { return isHovered; }
            set {
                if(isHovered != value) {
                    isHovered = value;
                    if(isHovered) {
                        Width = optHighlight.Width;
                        Height = optHighlight.Height;
                        Color = optHighlight.Color;
                        if(OnMouseEntry != null)
                            OnMouseEntry(this);
                    }
                    else {
                        Width = optDefault.Width;
                        Height = optDefault.Height;
                        Color = optDefault.Color;
                        if(OnMouseExit != null)
                            OnMouseExit(this);
                    }
                }
            }
        }

        private bool isHooked;

        public RectButton(WidgetRenderer r, ButtonHighlightOptions inactive, ButtonHighlightOptions active)
            : base(r) {
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

        public void OnMouseMotion(Vector2 p, Vector2 d) {
            int x = (int)p.X;
            int y = (int)p.Y;
            Vector2 r;
            IsHovered = Inside(x, y, out r);
        }
        public void OnMousePress(Vector2 p, MouseButton b) {
            OnMouseMotion(p, Vector2.Zero);
            if(b == MouseButton.Left) {
                if(IsHovered && OnButtonPress != null)
                    OnButtonPress(this);
            }
        }
    }
}