using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI.Widgets {
    public class WidgetRenderer : IDisposable {
        private Texture2D tPixel;
        private SpriteFont fDefault;

        private readonly List<DrawableRect> rects;
        private readonly List<DrawableText> texts;

        public WidgetRenderer(GraphicsDevice g, SpriteFont f) {
            tPixel = new Texture2D(g, 1, 1);
            tPixel.SetData(new Color[] { Color.White });
            fDefault = f;

            rects = new List<DrawableRect>();
            texts = new List<DrawableText>();
        }
        public void Dispose() {
            tPixel.Dispose();
        }

        public void Add(DrawableRect r) {
            rects.Add(r);
        }
        public void Add(DrawableText t) {
            texts.Add(t);
        }
        public void Remove(DrawableRect r) {
            rects.Remove(r);
        }
        public void Remove(DrawableText t) {
            texts.Remove(t);
        }

        public void Draw(SpriteBatch batch) {
            batch.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.Default, RasterizerState.CullNone);
            for(int i = 0; i < rects.Count; i++) {
                // Draw Rectangle
                batch.Draw(
                    rects[i].texture == null ? tPixel : rects[i].texture,
                    rects[i].location,
                    null,
                    rects[i].color,
                    0f,
                    Vector2.Zero,
                    SpriteEffects.None,
                    rects[i].layerDepth
                    );
            }
            for(int i = 0; i < texts.Count; i++) {
                // Draw Text
                if(texts[i].Font == null)
                    texts[i].Font = fDefault;
                batch.DrawString(
                    texts[i].Font,
                    texts[i].Text,
                    texts[i].location,
                    texts[i].color,
                    0f,
                    Vector2.Zero,
                    texts[i].TextScale,
                    SpriteEffects.None,
                    rects[i].layerDepth
                    );
            }
            batch.End();
        }
    }
}