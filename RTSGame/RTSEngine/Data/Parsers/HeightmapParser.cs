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
using RTSEngine.Controllers;

namespace RTSEngine.Data.Parsers {
    public static class MapParser {
        // Data Detection
        const string EXTENSION = "map";
        const ParsingFlags READ_FLAGS = ParsingFlags.ConversionOpenGL;
        static readonly Regex rgxDataFile = RegexHelper.GenerateFile("DAT");
        static readonly Regex rgxHModelPFile = RegexHelper.GenerateFile("HMP");
        static readonly Regex rgxHTexPFile = RegexHelper.GenerateFile("HTP");
        static readonly Regex rgxHModelSFile = RegexHelper.GenerateFile("HMS");
        static readonly Regex rgxHTexSFile = RegexHelper.GenerateFile("HTS");
        static readonly Regex rgxHeight = RegexHelper.GenerateNumber("HEIGHT");
        static readonly Regex rgxRegions = RegexHelper.GenerateFile("REGIONS");
        static readonly Regex rgxSpawn = RegexHelper.GenerateFile("SPAWNS");

        private static void ConvertPixel(byte[] cols, int ci, float[] h, byte[] d, int i) {
            h[i] = 1f - (cols[ci + 2] / 255f);
            d[i] = cols[ci + 1] > 128 ? (byte)0x01u : (byte)0x00u;
        }

        public static HeightmapModel ParseModel(RTSRenderer ge, Vector3 size, int fWidth, int fHeight, FileInfo infoFile) {
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return null;
            Vector2 uvScale = new Vector2(1f / size.X, 1f / size.Z);

            // Read The Entire File
            string mStr;
            using(FileStream fs = File.OpenRead(infoFile.FullName)) {
                StreamReader s = new StreamReader(fs);
                mStr = s.ReadToEnd();
            }

            // Match Tokens
            Match[] mp = {
                rgxHModelPFile.Match(mStr),
                rgxHTexPFile.Match(mStr),
                rgxHModelSFile.Match(mStr),
                rgxHTexSFile.Match(mStr)
            };

            #region Create Primary
            // Try To Find The First Model
            if(!mp[0].Success || !mp[1].Success) return null;
            FileInfo mpfi = RegexHelper.ExtractFile(mp[0], infoFile.Directory.FullName);
            FileInfo tpfi = RegexHelper.ExtractFile(mp[1], infoFile.Directory.FullName);
            if(!mpfi.Exists || !tpfi.Exists) return null;

            // Try To Get The First Texture
            Texture2D t;
            FileStream ts = null;
            try {
                ts = File.OpenRead(tpfi.FullName);
                t = ge.LoadTexture2D(ts);
                ts.Dispose();
            }
            catch(Exception) {
                if(ts != null) ts.Dispose();
                t = null;
            }
            if(t == null) return null;

            // Try To Parse The Primary Model
            VertexPositionNormalTexture[] vertsPNT;
            VertexPositionTexture[] verts;
            int[] inds;
            bool error;
            using(var ms = File.OpenRead(mpfi.FullName)) {
                error = !ObjParser.TryParse(ms, out vertsPNT, out inds, READ_FLAGS);
            }
            if(error) return null;

            // Reposition Model
            BoundingBox aabb = new BoundingBox(new Vector3(float.MaxValue), new Vector3(-float.MaxValue));
            verts = new VertexPositionTexture[vertsPNT.Length];
            for(int i = 0; i < vertsPNT.Length; i++) {
                // Copy Over Information
                verts[i].Position = vertsPNT[i].Position;
                verts[i].TextureCoordinate = vertsPNT[i].TextureCoordinate;

                // Calculate Bounding Box
                if(verts[i].Position.X > aabb.Max.X) aabb.Max.X = verts[i].Position.X;
                if(verts[i].Position.X < aabb.Min.X) aabb.Min.X = verts[i].Position.X;
                if(verts[i].Position.Y > aabb.Max.Y) aabb.Max.Y = verts[i].Position.Y;
                if(verts[i].Position.Y < aabb.Min.Y) aabb.Min.Y = verts[i].Position.Y;
                if(verts[i].Position.Z > aabb.Max.Z) aabb.Max.Z = verts[i].Position.Z;
                if(verts[i].Position.Z < aabb.Min.Z) aabb.Min.Z = verts[i].Position.Z;
            }
            // Find Scaling
            size /= aabb.Max - aabb.Min;
            for(int i = 0; i < verts.Length; i++) {
                // Move Model Minimum To Origin
                verts[i].Position -= aabb.Min;
                // Scale Heights To [0,size.Y]
                verts[i].Position *= size;
                verts[i].TextureCoordinate = new Vector2(
                    verts[i].Position.X * uvScale.X,
                    verts[i].Position.Z * uvScale.Y
                    );
            }

            // Create The Primary Model
            HeightmapModel view = new HeightmapModel(ge, fWidth, fHeight);
            ModelHelper.CreateBuffers(ge, verts, VertexPositionTexture.VertexDeclaration, inds, out view.VBPrimary, out view.IBPrimary, BufferUsage.WriteOnly);
            view.PrimaryTexture = t;
            #endregion

            #region Create Secondary
            // Try To Find The Second Model
            if(!mp[2].Success || !mp[3].Success) return view;
            mpfi = RegexHelper.ExtractFile(mp[2], infoFile.Directory.FullName);
            tpfi = RegexHelper.ExtractFile(mp[3], infoFile.Directory.FullName);
            if(!mpfi.Exists || !tpfi.Exists) return view;

            // Try To Get The Second Texture
            try {
                ts = null;
                ts = File.OpenRead(tpfi.FullName);
                t = ge.LoadTexture2D(ts);
                ts.Dispose();
            }
            catch(Exception) {
                if(ts != null) ts.Dispose();
                t = null;
            }
            if(t == null) return view;

            // Try To Parse The Secondary Model
            using(var ms = File.OpenRead(mpfi.FullName)) {
                error = !ObjParser.TryParse(ms, out vertsPNT, out inds, READ_FLAGS);
            }
            if(error) return view;
            // Reposition Secondary To Coincide With The First
            for(int i = 0; i < verts.Length; i++) {
                verts[i].Position -= aabb.Min;
                verts[i].Position *= size;
            }

            // Create The Secondary Model
            ModelHelper.CreateBuffers(ge, verts, VertexPositionTexture.VertexDeclaration, inds, out view.VBSecondary, out view.IBSecondary, BufferUsage.WriteOnly);
            view.SecondaryTexture = t;
            #endregion

            return view;
        }
        public static LevelGrid? ParseData(FileInfo data, out FileInfo fiEnvSpawn) {
            // Check File Existence
            fiEnvSpawn = null;
            if(data == null || !data.Exists) return null;

            // Read The Entire File
            string mStr;
            using(FileStream fs = File.OpenRead(data.FullName)) {
                StreamReader s = new StreamReader(fs);
                mStr = s.ReadToEnd();
            }

            // Match Tokens
            Match[] mp = {
                rgxDataFile.Match(mStr),
                rgxHeight.Match(mStr),
                rgxHModelPFile.Match(mStr),
                rgxRegions.Match(mStr),
                rgxSpawn.Match(mStr)
            };

            if(!mp[0].Success || !mp[1].Success || !mp[2].Success) return null;
            FileInfo hfi = RegexHelper.ExtractFile(mp[0], data.Directory.FullName);
            float height = RegexHelper.ExtractFloat(mp[1]);
            FileInfo mfi = RegexHelper.ExtractFile(mp[2], data.Directory.FullName);
            FileInfo rfi = RegexHelper.ExtractFile(mp[3], data.Directory.FullName);
            if(!hfi.Exists || !mfi.Exists || !rfi.Exists) return null;

            if(mp[4].Success)
                fiEnvSpawn = RegexHelper.ExtractFile(mp[4], data.Directory.FullName);

            // Read Height Data
            LevelGrid grid = new LevelGrid();
            int w, h;
            using(var bmp = Bitmap.FromFile(hfi.FullName) as Bitmap) {
                w = bmp.Width;
                h = bmp.Height;
                float[] hd = new float[w * h];
                byte[] cd = new byte[w * h];
                byte[] col = new byte[w * h * 4];
                int i = 0, ci = 0;

                // Convert Bitmap
                System.Drawing.Imaging.BitmapData bd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, col, 0, bd.Stride * bd.Height);
                bmp.UnlockBits(bd);

                for(int y = 0; y < h; y++) {
                    for(int x = 0; x < w; x++) {
                        ConvertPixel(col, ci, hd, cd, i++);
                        ci += 4;
                    }
                }
                grid.L0 = new Heightmap(hd, cd, w, h);
            }

            // Read Regions
            var regionCells = new Dictionary<int, List<Microsoft.Xna.Framework.Point>>();
            using(var bmp = Bitmap.FromFile(rfi.FullName) as Bitmap) {
                w = bmp.Width;
                h = bmp.Height;

                // Create The Impact And Collision Grid Based Off The Image Size
                grid.L1 = new CollisionGrid(w * 2, h * 2, RTSConstants.CGRID_SIZE);
                grid.L2 = new ImpactGrid(grid.L1);

                // Convert Bitmap
                int[] ids = new int[w * h];
                System.Drawing.Imaging.BitmapData bd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, ids, 0, ids.Length);
                bmp.UnlockBits(bd);

                // Find All The Regions
                int ii = 0;
                for(int y = 0; y < h; y++) {
                    for(int x = 0; x < w; x++) {
                        if(regionCells.ContainsKey(ids[ii])) {
                            regionCells[ids[ii]].Add(new Microsoft.Xna.Framework.Point(x, y));
                        }
                        else {
                            var l = new List<Microsoft.Xna.Framework.Point>();
                            l.Add(new Microsoft.Xna.Framework.Point(x, y));
                            regionCells.Add(ids[ii], l);
                        }
                        ii++;
                    }
                }
            }

            // Create The Regions
            foreach(var kv in regionCells) {
                Region r = new Region(kv.Value);
                foreach(var p in r.Cells) {
                    grid.L2.Region[p.X, p.Y] = r;
                }
            }

            // Apply Heightmap Size
            grid.L0.Width = grid.L1.size.X;
            grid.L0.Depth = grid.L1.size.Y;
            grid.L0.ScaleHeights(height);

            // Try To Parse The Primary Model For BVH
            VertexPositionNormalTexture[] vertsPNT;
            VertexPositionTexture[] verts;
            int[] inds;
            bool error;
            using(var ms = File.OpenRead(mfi.FullName)) {
                error = !ObjParser.TryParse(ms, out vertsPNT, out inds, READ_FLAGS);
            }
            if(error) return null;
            BoundingBox aabb = new BoundingBox(new Vector3(float.MaxValue), new Vector3(-float.MaxValue));
            verts = new VertexPositionTexture[vertsPNT.Length];
            for(int i = 0; i < vertsPNT.Length; i++) {
                // Copy Over Information
                verts[i].Position = vertsPNT[i].Position;
                verts[i].TextureCoordinate = vertsPNT[i].TextureCoordinate;

                // Calculate Bounding Box
                if(verts[i].Position.X > aabb.Max.X) aabb.Max.X = verts[i].Position.X;
                if(verts[i].Position.X < aabb.Min.X) aabb.Min.X = verts[i].Position.X;
                if(verts[i].Position.Y > aabb.Max.Y) aabb.Max.Y = verts[i].Position.Y;
                if(verts[i].Position.Y < aabb.Min.Y) aabb.Min.Y = verts[i].Position.Y;
                if(verts[i].Position.Z > aabb.Max.Z) aabb.Max.Z = verts[i].Position.Z;
                if(verts[i].Position.Z < aabb.Min.Z) aabb.Min.Z = verts[i].Position.Z;
            }
            Vector3 scale = new Vector3(grid.L1.size.X, height, grid.L1.size.Y) / (aabb.Max - aabb.Min);
            for(int i = 0; i < verts.Length; i++) {
                verts[i].Position -= aabb.Min;
                verts[i].Position *= scale;
            }
            grid.L0.BVH.Build(verts, inds);

            return grid;
        }
    }
}