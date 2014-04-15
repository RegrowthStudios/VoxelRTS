using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace NovaLibrary.GUI {
    public class ComboText {
        const float fadeRate = 4.5f;
        const float comboFadeRate = 0.25f;

        Color fadeColor1;
        Color fadeColor2;

        float fadeSize1;
        float fadeSize2;

        Vector2 position;

        SpriteFont font;
        float fontSize;
        Color color;
        float scale;

        String text;
        public int combo;
        float comboF;
        float fade;

        public void build(SpriteFont font) {
            this.font = font;
            text = "Combo X 0";
            fontSize = font.MeasureString(text).Y;

            position = new Vector2(10, 24);

            fadeColor1 = Color.White;
            fadeColor2 = Color.RoyalBlue;

            fadeSize1 = 22f;
            fadeSize2 = 32f;

            setCombo(0);
        }

        public void setCombo(int c) {
            fade = 1f;
            combo = c;
            text = "Combo X " + Convert.ToString(combo);
            comboF = combo;
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
            if(combo > 0) {
                comboF -= elapsedTime * comboFadeRate;
                int dif = combo - ZMath.fastFloor(comboF + 1);
                if(dif > 0) {
                    combo -= dif;
                    text = "Combo X " + Convert.ToString(combo);
                }
            }
        }

        public void draw(SpriteBatch batch) {
            batch.DrawString(font, text, position, color, 0f, Vector2.Zero, scale, SpriteEffects.None, 0f);
        }
    }
}