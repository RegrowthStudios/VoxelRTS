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
    public class TerrainData {
        public float MapHeight;
        public Microsoft.Xna.Framework.Point[] PlayerSpawns;

        public LevelGrid LGrid;
        public List<Region> Regions;

        public TerrainData() {
            Regions = new List<Region>();
            LGrid = new LevelGrid();
        }

        public void ReadHeightData(string rootPath, string filePath) {
            string path = Path.Combine(rootPath, filePath);
            //byte[] col;
            //int w, h;
            //using(var bmp = Bitmap.FromFile(path) as Bitmap) {
            //    w = bmp.Width;
            //    h = bmp.Height;
            //    col = new byte[w * h * 4];

            //    // Convert Bitmap
            //    System.Drawing.Imaging.BitmapData bd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
            //    System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, col, 0, bd.Stride * bd.Height);
            //    bmp.UnlockBits(bd);
            //}

            //// TODO: Make Pixels As Floating Point Values
            //int i = 0, ci = 0;
            //float[] hd = new float[w * h];
            //for(int y = 0; y < h; y++) {
            //    for(int x = 0; x < w; x++) {
            //        hd[i++] = 1f - (col[ci + 2] / 255f);
            //        ci += 4;
            //    }
            //}
            LGrid.L0 = new Heightmap(path);
            LGrid.L0.ScaleHeights(MapHeight);
        }
        public void ReadGridData(string rootPath, string filePath) {
            string path = Path.Combine(rootPath, filePath);
            int[] ids;
            int w, h;
            using(var bmp = Bitmap.FromFile(path) as Bitmap) {
                w = bmp.Width;
                h = bmp.Height;
                ids = new int[w * h];

                // Convert Bitmap
                System.Drawing.Imaging.BitmapData bd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, ids, 0, ids.Length);
                bmp.UnlockBits(bd);
            }

            var regionCells = new Dictionary<int, List<Microsoft.Xna.Framework.Point>>();
            LGrid.L1 = new CollisionGrid(w * 2, h * 2, RTSConstants.CGRID_SIZE);
            LGrid.L2 = new ImpactGrid(LGrid.L1);

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

            // Create The Regions
            foreach(var kv in regionCells) {
                Region r = new Region(kv.Value);
                Regions.Add(r);
                foreach(var p in r.Cells) {
                    LGrid.L2.Region[p.X, p.Y] = r;
                }
            }

            // Apply Heightmap Size
            LGrid.L0.Width = LGrid.L1.size.X;
            LGrid.L0.Depth = LGrid.L1.size.Y;
        }
    }
    public class TerrainViewData {
        [ZXParse]
        public HeightmapModel View;
        [ZXParse]
        public BoundingBox AABB;
        [ZXParse]
        public Vector3 Scaling;

        public void BuildPrimary(RTSRenderer ge, string rootPath, LevelGrid grid, string fModel, string fTexture) {
            Scaling = new Vector3(grid.L0.Width, grid.L0.ScaleY, grid.L0.Depth);
            int fWidth = grid.L1.numCells.X, fHeight = grid.L1.numCells.Y;
            Vector2 uvScale = new Vector2(1f / Scaling.X, 1f / Scaling.Z);

            // Try To Find The First Model
            FileInfo mpfi = new FileInfo(Path.Combine(rootPath, fModel));
            FileInfo tpfi = new FileInfo(Path.Combine(rootPath, fTexture));
            if(!mpfi.Exists || !tpfi.Exists) return;

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
            if(t == null) return;

            // Try To Parse The Primary Model
            VertexPositionNormalTexture[] vertsPNT;
            VertexPositionTexture[] verts;
            int[] inds;
            bool error;
            using(var ms = File.OpenRead(mpfi.FullName)) {
                error = !ObjParser.TryParse(ms, out vertsPNT, out inds, ParsingFlags.ConversionOpenGL);
            }
            if(error) return;

            // Reposition Model
            AABB = new BoundingBox(new Vector3(float.MaxValue), new Vector3(-float.MaxValue));
            verts = new VertexPositionTexture[vertsPNT.Length];
            for(int i = 0; i < vertsPNT.Length; i++) {
                // Copy Over Information
                verts[i].Position = vertsPNT[i].Position;
                verts[i].TextureCoordinate = vertsPNT[i].TextureCoordinate;

                // Calculate Bounding Box
                if(verts[i].Position.X > AABB.Max.X) AABB.Max.X = verts[i].Position.X;
                if(verts[i].Position.X < AABB.Min.X) AABB.Min.X = verts[i].Position.X;
                if(verts[i].Position.Y > AABB.Max.Y) AABB.Max.Y = verts[i].Position.Y;
                if(verts[i].Position.Y < AABB.Min.Y) AABB.Min.Y = verts[i].Position.Y;
                if(verts[i].Position.Z > AABB.Max.Z) AABB.Max.Z = verts[i].Position.Z;
                if(verts[i].Position.Z < AABB.Min.Z) AABB.Min.Z = verts[i].Position.Z;
            }
            // Find Scaling
            Scaling /= AABB.Max - AABB.Min;
            for(int i = 0; i < verts.Length; i++) {
                // Move Model Minimum To Origin
                verts[i].Position -= AABB.Min;
                // Scale Heights To [0,size.Y]
                verts[i].Position *= Scaling;
                verts[i].Position.Y = grid.L0.HeightAt(verts[i].Position.X, verts[i].Position.Z);
                verts[i].TextureCoordinate = new Vector2(
                    verts[i].Position.X * uvScale.X,
                    verts[i].Position.Z * uvScale.Y
                    );
            }

            // Create The Primary Model
            View = new HeightmapModel(ge, fWidth, fHeight);
            View.BVH.Build(verts, inds);
            RTSModelHelper.CreateBuffers(ge, verts, VertexPositionTexture.VertexDeclaration, inds, out View.VBPrimary, out View.IBPrimary, BufferUsage.WriteOnly);
            View.PrimaryTexture = t;
        }
        public void BuildSecondary(RTSRenderer ge, string rootPath, LevelGrid grid, string fModel, string fTexture) {
            if(View == null) return;

            // Try To Find The First Model
            FileInfo mpfi = new FileInfo(Path.Combine(rootPath, fModel));
            FileInfo tpfi = new FileInfo(Path.Combine(rootPath, fTexture));
            if(!mpfi.Exists || !tpfi.Exists) return;

            // Try To Get The Second Texture
            Texture2D t;
            FileStream ts = null;
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
            if(t == null) return;

            // Try To Parse The Secondary Model
            VertexPositionNormalTexture[] vertsPNT;
            int[] inds;
            bool error;
            using(var ms = File.OpenRead(mpfi.FullName)) {
                error = !ObjParser.TryParse(ms, out vertsPNT, out inds, ParsingFlags.ConversionOpenGL);
            }
            if(error) return;
            // Reposition Secondary To Coincide With The First
            VertexPositionTexture[] verts = new VertexPositionTexture[vertsPNT.Length];
            for(int i = 0; i < verts.Length; i++) {
                verts[i].Position -= AABB.Min;
                verts[i].Position *= Scaling;
            }

            // Create The Secondary Model
            RTSModelHelper.CreateBuffers(ge, verts, VertexPositionTexture.VertexDeclaration, inds, out View.VBSecondary, out View.IBSecondary, BufferUsage.WriteOnly);
            View.SecondaryTexture = t;
        }
    }

    public static class MapParser {
        // Data Detection
        const string EXTENSION = "map";

        public static HeightmapModel ParseModel(RTSRenderer ge, LevelGrid grid, FileInfo infoFile) {
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return null;

            ZXParser.SetEnvironment("FILEROOTDIR", infoFile.Directory.FullName);
            ZXParser.SetEnvironment("RENDERER", ge);
            ZXParser.SetEnvironment("LGRID", grid);
            TerrainViewData mio = ZXParser.ParseFile(infoFile.FullName, typeof(TerrainViewData)) as TerrainViewData;
            return mio.View;
        }
        public static TerrainData ParseData(FileInfo infoFile, List<Region> regions) {
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return null;

            ZXParser.SetEnvironment("FILEROOTDIR", infoFile.Directory.FullName);
            TerrainData mio = ZXParser.ParseFile(infoFile.FullName, typeof(TerrainData)) as TerrainData;
            mio.LGrid.InfoFile = PathHelper.GetRelativePath(infoFile.FullName);
            regions.AddRange(mio.Regions); // TODO: Remove
            return mio;
        }
    }
}