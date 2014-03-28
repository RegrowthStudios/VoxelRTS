using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI.Widgets {
    public class RectWidget : BaseWidget {
        // Where To Draw To Screen
        protected DrawableRect drawRect;
        public override int X {
            get { return drawRect.location.X; }
            protected set { drawRect.location.X = value; }
        }
        public override int Y {
            get { return drawRect.location.Y; }
            protected set { drawRect.location.Y = value; }
        }
        public override int Width {
            get { return drawRect.location.Width; }
            set {
                drawRect.location.Width = value;
                Recompute();
            }
        }
        public override int Height {
            get { return drawRect.location.Height; }
            set {
                drawRect.location.Height = value;
                Recompute();
            }
        }
        public override float LayerDepth {
            get { return drawRect.layerDepth; }
            set { drawRect.layerDepth = value; }
        }

        public Texture2D Texture {
            get { return drawRect.texture; }
            set { drawRect.texture = value; }
        }
        public Color Color {
            get { return drawRect.color; }
            set { drawRect.color = value; }
        }

        public RectWidget(WidgetRenderer r, Texture2D t = null)
            : base(r) {
            Texture = t == null ? r.DefaultTexture : t;
        }

        public override void PreInit() {
            drawRect = new DrawableRect();
        }
        protected override void DisposeOther() {
        }

        public override void AddAllDrawables(WidgetRenderer r) {
            r.Add(drawRect);
        }
        public override void RemoveAllDrawables(WidgetRenderer r) {
            r.Remove(drawRect);
        }

    }
}