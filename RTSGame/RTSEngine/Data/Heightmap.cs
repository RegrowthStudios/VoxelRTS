using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Graphics;

namespace RTSEngine.Data {
    public struct HeightTile {
        public float XNZN, XPZN, XNZP, XPZP;
    }

    public class Heightmap {
        // Height Values
        private HeightTile[] heights;

        // Texture Size - 1
        public int GridWidth {
            get;
            private set;
        }
        public int GridDepth {
            get;
            private set;
        }
        public int CellCount {
            get { return heights.Length; }
        }

        // Width And Depth In Object/Model Space
        public float Width {
            get;
            set;
        }
        public float Depth {
            get;
            set;
        }

        // Scaling Of Grid From Grid To Object Space
        public float ScaleX {
            get { return Width / GridWidth; }
        }
        public float ScaleZ {
            get { return Depth / GridDepth; }
        }
        public Vector2 Scale {
            get { return new Vector2(ScaleX, ScaleZ); }
        }
        public float ScaleY {
            get;
            private set;
        }

        // Constructor With Heightmap Data File Passed In
        public Heightmap(string file) {
            using(var s = File.OpenRead(file)) {
                // Read How Much Data To Allocate
                var br = new BinaryReader(s);
                int l = br.ReadInt32();

                // Decompress Data
                byte[] data = new byte[l];
                var gs = new GZipStream(s, CompressionMode.Decompress);
                gs.Read(data, 0, data.Length);
                
                // Convert Data
                ReadHeights(data);
            }
            ScaleY = 1f;
        }
        public void ReadHeights(byte[] data) {
            // Read Width And Height
            int ci = 0;
            GridWidth = BitConverter.ToInt32(data, ci); ci += 4;
            GridDepth = BitConverter.ToInt32(data, ci); ci += 4;
            heights = new HeightTile[GridWidth * GridDepth];

            // Read All Tiles
            for(int i = 0; i < heights.Length; i++) {
                heights[i].XNZN = BitConverter.ToSingle(data, ci); ci += 4;
                heights[i].XPZN = BitConverter.ToSingle(data, ci); ci += 4;
                heights[i].XNZP = BitConverter.ToSingle(data, ci); ci += 4;
                heights[i].XPZP = BitConverter.ToSingle(data, ci); ci += 4;
            }
        }

        // Scale The Heights By A Certain Value
        public void ScaleHeights(float s) {
            ScaleY *= s;
            for(int i = 0; i < heights.Length; i++) {
                heights[i].XNZN *= s;
                heights[i].XPZN *= s;
                heights[i].XNZP *= s;
                heights[i].XPZP *= s;
            }
        }

        // Find The Floored Location On The Heightmap
        public Point Map(float x, float z) {
            // Convert To Grid Space
            x /= ScaleX; z /= ScaleZ;

            // Find The Floored Values
            return new Point(
                x <= 0 ? 0 : (x >= GridWidth - 1 ? GridWidth - 1 : (int)x),
                z <= 0 ? 0 : (z >= GridDepth - 1 ? GridDepth - 1 : (int)z)
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
            int fz = z <= 0 ? 0 : (z >= GridDepth - 1 ? GridDepth - 1 : (int)z);
            int i = fz * GridWidth + fx;
            float rx = x - fx;
            float rz = z - fz;

            // Bilerp For Value
            return Bilerp(
                heights[i].XNZN,
                heights[i].XPZN,
                heights[i].XNZP,
                heights[i].XPZP,
                rx, rz
                );
        }
        public float SmoothHeightAt(float x, float z) {


            // Convert To Grid Space
            x /= ScaleX; z /= ScaleZ;

            // Find The Floored Values And The Remainder
            int fx = x <= 0 ? 0 : (x >= GridWidth - 1 ? GridWidth - 1 : (int)x);
            int fz = z <= 0 ? 0 : (z >= GridDepth - 1 ? GridDepth - 1 : (int)z);
            int i = fz * GridWidth + fx;
            float rx = x - fx;
            float rz = z - fz;

            // Bilerp For Value
            return Bilerp(
                HeightAt(x - 2 * 0.5f, z - 2 * 0.5f),
                HeightAt(x + 2 * 0.5f, z - 2 * 0.5f),
                HeightAt(x - 2 * 0.5f, z + 2 * 0.5f),
                HeightAt(x + 2 * 0.5f, z + 2 * 0.5f),
                rx, rz
                );
        }
    }
}