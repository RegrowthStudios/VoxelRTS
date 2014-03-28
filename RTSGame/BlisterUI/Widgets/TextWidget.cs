using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI.Widgets {
    public class TextWidget : BaseWidget {

        protected DrawableText drawText;
        private Point textLocation;
        public override int X {
            get { return textLocation.X; }
            protected set {
                textLocation.X = value;
                drawText.location.X = value;
            }
        }
        public override int Y {
            get { return textLocation.Y; }
            protected set {
                textLocation.Y = value;
                drawText.location.Y = value;
            }
        }
        public override int Width {
            get { return (int)drawText.TextWidth; }
            set { throw new NotImplementedException(); }
        }
        public override int Height {
            get { return (int)drawText.TextHeight; }
            set { drawText.TextHeight = value; }
        }
        public override float LayerDepth {
            get { return drawText.layerDepth; }
            set { drawText.layerDepth = value; }
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
        public string Text {
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
        }
        protected override void DisposeOther() {
        }

        public override void AddAllDrawables(WidgetRenderer r) {
            r.Add(drawText);
        }
        public override void RemoveAllDrawables(WidgetRenderer r) {
            r.Remove(drawText);
        }
    }
}