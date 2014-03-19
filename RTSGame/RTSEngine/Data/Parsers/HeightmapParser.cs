using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using BColor = System.Drawing.Color;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Interfaces;
using RTSEngine.Graphics;
using System.Collections.Concurrent;

namespace RTSEngine.Data.Parsers {
    public class HeightmapResult {
        public Heightmap Data;
        public HeightmapModel View;
    }

    public static class HeightmapParser {
        // Data Detection
        const string EXTENSION = "map";
        static readonly Regex rgxDataFile = RegexHelper.GenerateFile("DAT");
        static readonly Regex rgxHModelPFile = RegexHelper.GenerateFile("HMP");
        static readonly Regex rgxHTexPFile = RegexHelper.GenerateFile("HTP");
        static readonly Regex rgxHModelSFile = RegexHelper.GenerateFile("HMS");
        static readonly Regex rgxHTexSFile = RegexHelper.GenerateFile("HTS");
        static readonly Regex rgxSize = RegexHelper.GenerateVec3("SIZE");

        private static void ConvertPixel(byte[] cols, int ci, float[] h, byte[] d, int i) {
            h[i] = 1f - (cols[ci + 2] / 255f);
            d[i] = cols[ci + 1] > 128 ? (byte)0x01u : (byte)0x00u;
        }

        private static HeightmapResult ParseFromInfo(GraphicsDevice g, Stream s, string rootDir) {
            HeightmapResult res = new HeightmapResult();
            StreamReader sr = new StreamReader(s);
            string ms = sr.ReadToEnd();

            // Read All Data First
            Vector3 sz = RegexHelper.ExtractVec3(rgxSize.Match(ms));
            FileInfo hfi = RegexHelper.ExtractFile(rgxDataFile.Match(ms), rootDir);
            FileInfo mpfi = RegexHelper.ExtractFile(rgxHModelPFile.Match(ms), rootDir);
            FileInfo tpfi = RegexHelper.ExtractFile(rgxHTexPFile.Match(ms), rootDir);

            FileInfo msfi = null, tsfi = null;
            Match s1 = rgxHModelSFile.Match(ms);
            Match s2 = rgxHTexSFile.Match(ms);
            if(s1.Success && s2.Success) {
                msfi = RegexHelper.ExtractFile(s1, rootDir);
                tsfi = RegexHelper.ExtractFile(s2, rootDir);
            }

            // Read Height Data
            using(var bmp = Bitmap.FromFile(hfi.FullName) as Bitmap) {
                int w = bmp.Width;
                int h = bmp.Height;
                float[] hd = new float[w * h];
                byte[] cd = new byte[w * h];
                byte[] col = new byte[w * h * 4];
                int i = 0, ci = 0;

                // Convert Bitmap
                System.Drawing.Imaging.BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, col, 0, data.Stride * data.Height);
                for(int y = 0; y < h; y++) {
                    for(int x = 0; x < w; x++) {
                        ConvertPixel(col, ci, hd, cd, i++);
                        ci += 4;
                    }
                }
                bmp.UnlockBits(data);
                res.Data = new Heightmap(hd, cd, w, h);

                // Apply Heightmap Size
                res.Data.Width = sz.X;
                res.Data.Depth = sz.Z;
                res.Data.ScaleHeights(sz.Y);
            }

            // Must Read Primary Model
            using(var fs = File.OpenRead(mpfi.FullName)) {
                // Try To Read Secondary Data
                Stream fss = null;
                if(msfi != null) {
                    fss = File.OpenRead(msfi.FullName);
                }
                res.View = new HeightmapModel(g, sz, res.Data.BuildBVH, fs, fss);
                if(fss != null)
                    fss.Dispose();
            }

            // Read Primary Texture
            using(var fs = File.OpenRead(tpfi.FullName)) {
                res.View.PrimaryTexture = Texture2D.FromStream(g, fs);
            }

            // Try To Read Secondary Texture
            if(tsfi != null) {
                using(var fs = File.OpenRead(tsfi.FullName)) {
                    res.View.SecondaryTexture = Texture2D.FromStream(g, fs);
                }
            }
            return res;
        }
        public static HeightmapResult Parse(GraphicsDevice g, DirectoryInfo dir) {
            // Find The Information File
            var files = dir.GetFiles();
            FileInfo infoFile = files.FirstOrDefault((f) => {
                return f.Extension.ToLower().EndsWith(EXTENSION);
            });
            if(infoFile == null)
                throw new ArgumentException("Map Information File Could Not Be Found In The Directory");

            // Parse Data
            HeightmapResult res;
            using(Stream s = File.OpenRead(infoFile.FullName)) {
                res = ParseFromInfo(g, s, dir.FullName);
            }
            return res;
        }
    }
}