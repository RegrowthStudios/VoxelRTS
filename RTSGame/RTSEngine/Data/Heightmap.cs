using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data {
    public class Heightmap {
        // Height Values
        private float[] heights;
        private byte[] data;

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

        // Scaling Of Grid From Grid To Object Space
        public float ScaleX {
            get { return Width / GridWidth; }
            set { Width = GridWidth * value; }
        }
        public float ScaleZ {
            get { return Height / GridHeight; }
            set { Height = GridHeight * value; }
        }
        public Vector2 Scale {
            get { return new Vector2(ScaleX, ScaleZ); }
            set { ScaleX = value.X; ScaleZ = value.Y; }
        }

        // Constructor With Heightmap Data Passed In
        public Heightmap(float[] v, byte[] d, int w, int h) {
            HValueWidth = w;
            HValueHeight = h;
            heights = new float[HValueWidth * HValueHeight];
            v.CopyTo(heights, 0);
            data = new byte[HValueWidth * HValueHeight];
            d.CopyTo(data, 0);
            GridWidth = HValueWidth - 1;
            GridHeight = HValueHeight - 1;

        }

        // Scale The Heights By A Certain Value
        public void ScaleHeights(float s) {
            for(int i = 0; i < heights.Length; i++)
                heights[i] *= s;
        }

        // Find The Floored Location On The Heightmap
        public Point Map(float x, float z) {
            // Convert To Grid Space
            x /= ScaleX; z /= ScaleZ;

            // Find The Floored Values
            return new Point(
                x <= 0 ? 0 : (x >= GridWidth - 1 ? GridWidth - 1 : (int)x),
                z <= 0 ? 0 : (z >= GridHeight - 1 ? GridHeight - 1 : (int)z)
                );
        }

        // Retrieve Interpolated Height From The Heightmap
        private float Bilerp(float v1, float v2, float v3, float v4, float rx, float rz) {
            return MathHelper.Lerp(
                MathHelper.Lerp(v1, v2, rx),
                MathHelper.Lerp(v3, v4, rx),
                rz
                );
        }
        public float HeightAt(float x, float z) {
            // Convert To Grid Space
            x /= ScaleX; z /= ScaleZ;

            // Find The Floored Values And The Remainder
            int fx = x <= 0 ? 0 : (x >= GridWidth - 1 ? GridWidth - 1 : (int)x);
            int fz = z <= 0 ? 0 : (z >= GridHeight - 1 ? GridHeight - 1 : (int)z);
            float rx = x - fx;
            float rz = z - fz;

            // Bilerp For Value
            return Bilerp(
                heights[fz * HValueWidth + fx],
                heights[fz * HValueWidth + fx + 1],
                heights[(fz + 1) * HValueWidth + fx],
                heights[(fz + 1) * HValueWidth + fx + 1],
                rx, rz
                );
        }
    }
}