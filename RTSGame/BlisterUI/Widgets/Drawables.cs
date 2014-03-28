using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI.Widgets {
    public class DrawableRect {
        // Visual Information
        public Texture2D texture;
        public Color color;

        // Spatial Information
        public Rectangle location;
        public float layerDepth;
    }

    public class DrawableText {
        // Visual Information
        private SpriteFont font;
        public SpriteFont Font {
            get { return font; }
            set {
                font = value;
                ComputeScale();
            }
        }
        public Color color;
        private string text;
        public string Text {
            get { return text; }
            set {
                text = value;
                ComputeScale();
            }
        }
        public float layerDepth;

        // Spatial Information
        public Vector2 location;
        private float height;
        public float TextHeight {
            get { return height; }
            set {
                height = value;
                ComputeScale();
            }
        }
        public float TextWidth {
            get;
            private set;
        }
        public float TextScale {
            get;
            private set;
        }

        private void ComputeScale() {
            if(font == null || string.IsNullOrWhiteSpace(text)) {
                TextScale = 1f;
                return;
            }

            // Find Sizes
            Vector2 s = font.MeasureString(text);
            TextScale = TextHeight / s.Y;
            TextWidth = s.X * TextScale;
        }
    }
}