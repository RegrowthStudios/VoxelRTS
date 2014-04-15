using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NovaLibrary.Object;

namespace NovaLibrary.GUI {
    public class TimeBar {
        public double time;
        public const double maxTime = 1000.0;
        public const double witherRate = 0.6f;

        double wither = 0f;

        Rectangle backRect;
        Texture2D bar;
        Rectangle barRect;
        Color barColor;
        Color bf1;
        Color bf2;

        public void build(int screenWidth) {
            bar = NovaObjectContent.Texture(0);

            backRect = new Rectangle(0, 0, screenWidth, 20);
            barRect = new Rectangle(0, 0, 0, 20);

            bf1 = Color.Red;
            bf1.A = 10;
            bf2 = Color.Gray;
            bf2.A = 10;
            setTime(0);
        }

        public void onWindowResize(int screenWidth) {
            backRect = new Rectangle(0, 0, screenWidth, 20);
            setTime(time);
        }

        public void update(double elapsed) {
            wither += witherRate * elapsed;
            setTime(time - wither * elapsed);
        }

        public void setTime(double t) {
            if(t < 0) { t = 0; }
            else if(t > maxTime) { t = maxTime; }
            time = t;

            float p = (float)(time / maxTime);

            barRect.Width = ZMath.fastFloor(p * backRect.Width);
            barColor = Color.Lerp(bf1, bf2, p);
            barColor.A = 10;
        }

        public void draw(SpriteBatch batch) {
            batch.Draw(bar, barRect, barColor);
        }
    }
}