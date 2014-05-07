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

        [ZXParse]
        public LevelGrid LGrid;
        [ZXParse]
        public List<Region> Regions;
        public string VoxWorldFile;

        public TerrainData() {
            Regions = new List<Region>();
            LGrid = new LevelGrid();
        }

        public void ReadHeightData(string rootPath, string filePath) {
            string path = Path.Combine(rootPath, filePath);
            LGrid.L0 = new Heightmap(path);
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

    public static class MapParser {
        // Data Detection
        const string EXTENSION = "map";

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