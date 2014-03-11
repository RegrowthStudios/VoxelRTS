using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data {
    public class Heightmap {
        // Height Values
        private float[] heights;

        // Corresponds To Texture Size
        public int HValueWidth {
            get;
            private set;
        }
        public int HValueHeight {
            get;
            private set;
        }

        // Texture Size - 1
        public int GridWidth {
            get;
            private set;
        }
        public int GridHeight {
            get;
            private set;
        }
        public int CellCount {
            get { return heights.Length; }
        }

        // Width And Height In Object/Model Space
        public float Width {
            get;
            private set;
        }
        public float Height {
            get;
            private set;
        }


        public float ScaleX {
            get { return Width / GridWidth; }
            set { Width = GridWidth * value; }
        }
        public float ScaleY {
            get { return Height / GridHeight; }
            set { Height = GridHeight * value; }
        }
        public Vector2 Scale {
            get { return new Vector2(ScaleX, ScaleY); }
            set { ScaleX = value.X; ScaleY = value.Y; }
        }

        public Heightmap(float[] v, int w, int h) {
            HValueWidth = w;
            HValueHeight = h;
            heights = new float[HValueWidth * HValueHeight];
            v.CopyTo(heights, 0);
            GridWidth = HValueWidth - 1;
            GridHeight = HValueHeight - 1;

        }

        public Point Map(float x, float y) {
            // Convert To Grid Space
            x /= ScaleX; y /= ScaleY;

            // Find The Floored Values
            return new Point(
                x < 0 ? 0 : (x >= GridWidth - 1 ? GridWidth - 1 : (int)x),
                y < 0 ? 0 : (y >= GridHeight - 1 ? GridHeight - 1 : (int)y)
                );
        }

        private float Bilerp(float v1, float v2, float v3, float v4, float rx, float ry) {
            return MathHelper.Lerp(
                MathHelper.Lerp(v1, v2, rx),
                MathHelper.Lerp(v3, v4, rx),
                ry
                );
        }
        public float HeightAt(float x, float y) {
            // Convert To Grid Space
            x /= ScaleX; y /= ScaleY;

            // Find The Floored Values And The Remainder
            int fx = x < 0 ? 0 : (x >= GridWidth - 1 ? GridWidth - 1 : (int)x);
            int fy = y < 0 ? 0 : (y >= GridHeight - 1 ? GridHeight - 1 : (int)y);
            float rx = x - fx;
            float ry = y - fy;

            // Bilerp For Value
            return Bilerp(
                heights[fy * HValueWidth + fx],
                heights[fy * HValueWidth + fx + 1],
                heights[(fy + 1) * HValueWidth + fx],
                heights[(fy + 1) * HValueWidth + fx + 1],
                rx, ry
                );
        }
    }
}