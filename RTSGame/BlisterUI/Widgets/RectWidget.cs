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
            OnRecompute += OnSelfCompute;
        }
        protected override void DisposeOther() {
        }

        public override void AddAllDrawables(WidgetRenderer r) {
            r.Add(drawRect);
        }
        public override void RemoveAllDrawables(WidgetRenderer r) {
            r.Remove(drawRect);
        }

        private void OnSelfCompute(BaseWidget w) {
            drawRect.location = widgetRect;
            drawRect.layerDepth = layer;
        }
    }
}