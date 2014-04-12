using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NovaLibrary.GUI {
    public struct TextOptions {
        private Vector2 position;
        public Vector2 alignment;
        public float height;
        private Vector2 scale;
        private Vector2 size;

        public static TextOptions DEFAULT = new TextOptions(
            Vector2.Zero,
            16f
            );

        public TextOptions(Vector2 alignment, float height) {
            this.alignment = alignment;
            this.height = height;
            position = Vector2.Zero;
            scale = Vector2.Zero;
            size = Vector2.Zero;
        }

        public void refresh(String text, SpriteFont font, Vector2 tl, Vector2 fullSize) {
            size = font.MeasureString(text);
            scale = new Vector2(height / size.Y);
            size *= scale;
            position = tl + (fullSize - size) * (alignment + Vector2.One) / 2f;
        }

        public Vector2 getPosition() {
            return position;
        }

        public Vector2 getScale() {
            return scale;
        }

        public Vector2 Size {
            get { return size; }
        }

        public void fitToSize(String text, SpriteFont font, Vector2 tl, Vector2 fullSize) {
            size = font.MeasureString(text);
            scale = fullSize / size;
            if(scale.X > scale.Y) {
                scale.X = scale.Y;
                height = fullSize.Y;
            }
            else {
                scale.Y = scale.X;
                height = fullSize.X * size.Y / size.X;
            }
            size *= scale;
            position = tl + (fullSize - size) * (alignment + Vector2.One) / 2f;
        }
    }

    public class PointCounter {
        Vector2 location;
        Vector2 size;
        SpriteFont font;
        String text;
        TextOptions options;
        public long points;

        public void build(SpriteFont font, int screenWidth) {
            size = new Vector2(140, 34);
            location = new Vector2(screenWidth - 148, 24);
            options = new TextOptions(Vector2.UnitX, 30f);
            this.font = font;
            setPoints(0);
        }
        public void onWindowResize(int screenWidth) {
            location = new Vector2(screenWidth - 148, 24);
            options.refresh(text, font, location, size);
        }

        public void addPoints(long p) {
            setPoints(points + p);
        }
        public void setPoints(long p) {
            points = p;
            text = Convert.ToString(points);
            options.refresh(text, font, location, size);
        }

        public void draw(SpriteBatch batch) {
            batch.DrawString(font, text, options.getPosition(), Color.White, 0f, Vector2.Zero, options.getScale(), SpriteEffects.None, 0f);
        }

        public Vector2 getLocation() {
            return location;
        }
        public Vector2 getSize() {
            return size;
        }
        public void setLocation(Vector2 location) {
            this.location = location;
        }
        public void setSize(Vector2 size) {
            this.size = size;
            options.refresh(text, font, location, size);
        }
        public Vector2 getTrueLocation() {
            return location;
        }
    }
}