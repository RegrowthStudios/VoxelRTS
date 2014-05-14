using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI.Widgets {
    public class TextWidget : BaseWidget {
        protected DrawableText drawText;

        public override int Width {
            get { return (int)drawText.TextWidth; }
            set { throw new NotImplementedException(); }
        }

        public SpriteFont Font {
            get { return drawText.Font; }
            set {
                drawText.Font = value;
                Recompute();
            }
        }
        public Color Color {
            get { return drawText.color; }
            set { drawText.color = value; }
        }
        public virtual string Text {
            get { return drawText.Text; }
            set {
                drawText.Text = value;
                Recompute();
            }
        }

        public TextWidget(WidgetRenderer r, SpriteFont f = null)
            : base(r) {
            Font = f == null ? r.DefaultFont : f;
        }

        public override void PreInit() {
            drawText = new DrawableText();
            drawText.Text = "";
            OnRecompute += OnSelfCompute;
        }
        protected override void DisposeOther() {
        }

        public override void AddAllDrawables(WidgetRenderer r) {
            r.Add(drawText);
        }
        public override void RemoveAllDrawables(WidgetRenderer r) {
            r.Remove(drawText);
        }

        protected virtual void OnSelfCompute(BaseWidget w) {
            drawText.TextHeight = Height;
            drawText.location = new Vector2(X, Y);
            drawText.layerDepth = layer;
        }
    }
}