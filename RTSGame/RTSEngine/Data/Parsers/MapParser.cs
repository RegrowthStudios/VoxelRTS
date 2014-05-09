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
using System.IO.Compression;
using Grey.Vox;

namespace RTSEngine.Data.Parsers {
    public class TerrainData {
        public Microsoft.Xna.Framework.Point[] PlayerSpawns;

        [ZXParse]
        public LevelGrid LGrid;
        [ZXParse]
        public List<ImpactRegion> Regions;
        public string VoxWorldFile;

        public TerrainData() {
            Regions = new List<ImpactRegion>();
            LGrid = new LevelGrid();
        }

        public void ReadHeightData(string rootPath, string filePath) {
            string path = Path.Combine(rootPath, filePath);
            byte[] data;
            using(var s = File.OpenRead(path)) {
                // Read How Much Data To Allocate
                var br = new BinaryReader(s);
                int l = br.ReadInt32();

                // Decompress Data
                data = new byte[l];
                var gs = new GZipStream(s, CompressionMode.Decompress);
                gs.Read(data, 0, data.Length);
            }

            // Read Width And Height
            int ci = 0;
            int w = BitConverter.ToInt32(data, ci); ci += 4;
            int h = BitConverter.ToInt32(data, ci); ci += 4;
            LGrid.L1 = new CollisionGrid(w, h);
            HeightTile ht;

            // Read All Tiles
            for(int z = 0; z < h; z++) {
                for(int x = 0; x < w; x++) {
                    ht.XNZN = BitConverter.ToSingle(data, ci); ci += 4;
                    ht.XPZN = BitConverter.ToSingle(data, ci); ci += 4;
                    ht.XNZP = BitConverter.ToSingle(data, ci); ci += 4;
                    ht.XPZP = BitConverter.ToSingle(data, ci); ci += 4;
                    LGrid.L1.SetHeight(x, z, ht);
                    LGrid.L1.AddWalls(x, z, data[ci]);
                    ci++;
                }
            }
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
            LGrid.L2 = new ImpactGrid(w, h);

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
                ImpactRegion r = new ImpactRegion(kv.Value);
                Regions.Add(r);
                foreach(var p in r.Cells) {
                    LGrid.L2.Region[p.X, p.Y] = r;
                }
            }
        }
    }

    public static class MapParser {
        // Data Detection
        const string EXTENSION = "map";

        public static TerrainData ParseData(FileInfo infoFile, List<ImpactRegion> regions) {
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return null;

            ZXParser.SetEnvironment("FILEROOTDIR", infoFile.Directory.FullName);
            TerrainData mio = ZXParser.ParseFile(infoFile.FullName, typeof(TerrainData)) as TerrainData;
            mio.LGrid.InfoFile = PathHelper.GetRelativePath(infoFile.FullName);
            regions.AddRange(mio.Regions); // TODO: Remove
            return mio;
        }
        public static void ParseVoxels(VoxWorld vw, string file) {
            // Set Voxel Data
            byte[] data;
            using(var s = File.OpenRead(file)) {
                // Read How Much Data To Allocate
                var br = new BinaryReader(s);
                int l = br.ReadInt32();

                // Decompress Data
                data = new byte[l];
                var gs = new GZipStream(s, CompressionMode.Decompress);
                gs.Read(data, 0, data.Length);
            }

            // Convert Data
            int i = 0;
            int x = BitConverter.ToInt32(data, i); i += 4;
            int z = BitConverter.ToInt32(data, i); i += 4;
            Vector3I loc = Vector3I.Zero;
            Grey.Vox.Region rN;
            for(loc.Z = 0; loc.Z < z; loc.Z++) {
                for(loc.X = 0; loc.X < x; loc.X++) {
                    loc.Y = 0;
                    VoxLocation vl = new VoxLocation(loc);
                    var r = vw.regions[vl.RegionIndex];
                    if(r == null) {
                        // Check If The Region Needs To Be Loaded
                        r = vw.TryCreateRegion(vl.RegionLoc.X, vl.RegionLoc.Y);
                        int rx = vl.RegionLoc.X;
                        int rz = vl.RegionLoc.Y;
                        if(r == null) continue;
                        // Look For Neighbors
                        rN = vw.pager.Obtain(rx - 1, rz);
                        if(rN != null) { r.rNX = rN; rN.rPX = r; }
                        rN = vw.pager.Obtain(rx + 1, rz);
                        if(rN != null) { r.rPX = rN; rN.rNX = r; }
                        rN = vw.pager.Obtain(rx, rz - 1);
                        if(rN != null) { r.rNZ = rN; rN.rPZ = r; }
                        rN = vw.pager.Obtain(rx, rz + 1);
                        if(rN != null) { r.rPZ = rN; rN.rNZ = r; }
                        vw.regions[vl.RegionIndex] = r;
                    }

                    // Read Scenery
                    while(true) {
                        int scen = BitConverter.ToInt32(data, i); i += 4;
                        if(scen == -1) break;

                    }


                    int h = BitConverter.ToInt32(data, i); i += 4;
                    int t = BitConverter.ToInt32(data, i); i += 4;
                    switch(t) {
                        case 0:
                            // Terrain
                            int terr = BitConverter.ToInt32(data, i) + 1; i += 4;
                            for(vl.VoxelLoc.Y = 0; vl.VoxelLoc.Y <= h; vl.VoxelLoc.Y++) {
                                r.SetVoxel(vl.VoxelLoc.X, vl.VoxelLoc.Y, vl.VoxelLoc.Z, (ushort)(terr + 10));
                            }
                            if(h > 0) r.SetVoxel(vl.VoxelLoc.X, h, vl.VoxelLoc.Z, (ushort)(terr));
                            if(h > 1) r.SetVoxel(vl.VoxelLoc.X, h - 1, vl.VoxelLoc.Z, (ushort)(terr + 5));
                            break;
                        case 1:
                            // Ramp
                            int ramp = BitConverter.ToInt32(data, i); i += 4;
                            for(vl.VoxelLoc.Y = 0; vl.VoxelLoc.Y <= h; vl.VoxelLoc.Y++) {
                                r.SetVoxel(vl.VoxelLoc.X, vl.VoxelLoc.Y, vl.VoxelLoc.Z, 11);
                            }
                            if(h > 0) r.SetVoxel(vl.VoxelLoc.X, h, vl.VoxelLoc.Z, 1);
                            if(h > 1) r.SetVoxel(vl.VoxelLoc.X, h - 1, vl.VoxelLoc.Z, 6);
                            break;
                    }
                    r.NotifyFacesChanged();
                }
            }

            for(int vi = 0; vi < 15; vi++) {
                var vd = vw.Atlas.Create();
                vd.FaceType = new VoxFaceType();
                vd.FaceType.SetAllTypes(0x01u);
                vd.FaceType.SetAllMasks(0xfeu);
            }
        }
    }
}