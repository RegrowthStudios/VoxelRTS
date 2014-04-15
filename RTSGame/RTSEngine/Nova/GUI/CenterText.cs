using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace NovaLibrary.GUI {
    public class CenterText {
        float fadeRate = 1f;

        Color fadeColor1;
        Color fadeColor2;

        float fadeSize1;
        float fadeSize2;

        Vector2 position;
        Vector2 textHalfSize;
        SpriteFont font;
        float fontSize;
        Color color;
        float scale;

        String text;
        float fade;

        public void build(SpriteFont font) {
            this.font = font;
            text = "Texty";
            fontSize = font.MeasureString(text).Y;

            position = new Vector2(GameArea.GameBounds.Center.X, GameArea.GameBounds.Center.Y);
            fadeColor1 = Color.Transparent;
            fadeColor2 = Color.Red;
            fadeSize1 = 42f;
            fadeSize2 = 62f;
        }

        public void setText(String text) {
            fade = 1f;
            this.text = text;
            textHalfSize = font.MeasureString(text) / 2f;
        }
        public void setText(CenterTextOptions options) {
            fadeColor1 = options.f1;
            fadeColor2 = options.f2;
            fadeRate = options.fadeRate;
            setText(options.text);
        }

        public void update(float elapsedTime) {
            if(fade > 0f) {
                fade -= elapsedTime * fadeRate;
                if(fade < 0f) {
                    fade = 0f;
                }
                color = Color.Lerp(fadeColor1, fadeColor2, fade);
                scale = MathHelper.Lerp(fadeSize1, fadeSize2, fade) / fontSize;
            }
        }

        public bool isVisible() {
            return fade > 0f;
        }

        public void onWindowResize() {
            position = new Vector2(GameArea.bounds.Center.X, GameArea.bounds.Center.Y);
        }

        public void draw(SpriteBatch batch) {
            if(fade > 0) {
                batch.DrawString(font, text, position, color, 0f, textHalfSize, scale, SpriteEffects.None, 0f);
            }
        }
    }

    public struct CenterTextOptions {
        public String text;
        public Color f1;
        public Color f2;
        public float fadeRate;

        public static CenterTextOptions MAIN = new CenterTextOptions("Main", Color.Transparent, Color.LawnGreen, 1.3f);
        public static CenterTextOptions END_GAME = new CenterTextOptions("End", Color.Transparent, Color.Red, 1f);
        public static CenterTextOptions GOOD_POWER_UP = new CenterTextOptions("Good", Color.Transparent, Color.PowderBlue, 1f);
        public static CenterTextOptions BAD_POWER_UP = new CenterTextOptions("Bad", Color.Transparent, Color.PeachPuff, 1f);

        public CenterTextOptions(String text, Color end, Color start, float fadeTime = 1f) {
            this.text = text;
            this.f1 = end;
            this.f2 = start;
            fadeRate = 1f / fadeTime;
        }
        public CenterTextOptions setText(String text) {
            return new CenterTextOptions(text, f1, f2, 1f / fadeRate);
        }
        public CenterTextOptions setTime(float fadeTime) {
            return new CenterTextOptions(text, f1, f2, fadeTime);
        }
    }
}