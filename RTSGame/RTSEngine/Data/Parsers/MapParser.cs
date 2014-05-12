using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
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
            using(var bmp = System.Drawing.Bitmap.FromFile(path) as System.Drawing.Bitmap) {
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

    public interface IVoxGridResolver {
        bool TryGetFlat(ushort id);
        bool TryGetRamp(ushort id, out int direction);
        Point HeightIndex(VoxWorld world, int x, int z, int direction);
    }
    public struct ColumnResult {
        public HeightTile Height;
        public byte Walls;
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
            Region rN;
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
                            if(h > 0) r.SetVoxel(vl.VoxelLoc.X, h, vl.VoxelLoc.Z, (ushort)(ramp + 16));
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
            for(int vi = 0; vi < 4; vi++) {
                var vd = vw.Atlas.Create();
                vd.FaceType = new VoxFaceType();
                vd.FaceType.SetAllTypes(0xffffffffu);
                vd.FaceType.SetAllMasks(0xffffffffu);
            }
        }
        public static ColumnResult GetColumn(VoxWorld vw, int x, int z, int w, int h, IVoxGridResolver vgr) {
            VoxLocation vl = new VoxLocation(new Vector3I(x, Region.HEIGHT - 1, z));
            ColumnResult cr = new ColumnResult();
            ushort id;

            // Get Height Information
            cr.Height = new RTSEngine.Data.HeightTile();
            Region r = vw.regions[vl.RegionIndex];
            int ramp;
            for(; vl.VoxelLoc.Y > 0; vl.VoxelLoc.Y--) {
                id = r.voxels[vl.VoxelIndex].ID;
                if(vgr.TryGetFlat(id)) {
                    cr.Height.XNZN = vl.VoxelLoc.Y + 1;
                    cr.Height.XPZN = vl.VoxelLoc.Y + 1;
                    cr.Height.XNZP = vl.VoxelLoc.Y + 1;
                    cr.Height.XPZP = vl.VoxelLoc.Y + 1;
                    break;
                }
                else if(vgr.TryGetRamp(id, out ramp)) {
                    switch(ramp) {
                        case 0:
                            cr.Height.XNZN = vl.VoxelLoc.Y + 1;
                            cr.Height.XPZN = vl.VoxelLoc.Y + 0;
                            cr.Height.XNZP = vl.VoxelLoc.Y + 1;
                            cr.Height.XPZP = vl.VoxelLoc.Y + 0;
                            break;
                        case 1:
                            cr.Height.XNZN = vl.VoxelLoc.Y + 0;
                            cr.Height.XPZN = vl.VoxelLoc.Y + 1;
                            cr.Height.XNZP = vl.VoxelLoc.Y + 0;
                            cr.Height.XPZP = vl.VoxelLoc.Y + 1;
                            break;
                        case 2:
                            cr.Height.XNZN = vl.VoxelLoc.Y + 1;
                            cr.Height.XPZN = vl.VoxelLoc.Y + 1;
                            cr.Height.XNZP = vl.VoxelLoc.Y + 0;
                            cr.Height.XPZP = vl.VoxelLoc.Y + 0;
                            break;
                        case 3:
                            cr.Height.XNZN = vl.VoxelLoc.Y + 0;
                            cr.Height.XPZN = vl.VoxelLoc.Y + 0;
                            cr.Height.XNZP = vl.VoxelLoc.Y + 1;
                            cr.Height.XPZP = vl.VoxelLoc.Y + 1;
                            break;
                    }
                    if(vl.VoxelLoc.Y > 0)
                        vl.VoxelLoc.Y--;
                    break;
                }
            }


            // Get Wall Information
            cr.Walls = 0x00;
            if(x == 0) {
                cr.Walls |= CollisionGrid.Direction.XN;
            }
            else {
                Point hi = vgr.HeightIndex(vw, x, z, 0);
                Point hin = vgr.HeightIndex(vw, x - 1, z, 1);
                if(hin != hi)
                    cr.Walls |= CollisionGrid.Direction.XN;
            }
            if(x == w - 1) {
                cr.Walls |= CollisionGrid.Direction.XP;
            }
            else {
                Point hi = vgr.HeightIndex(vw, x, z, 1);
                Point hin = vgr.HeightIndex(vw, x + 1, z, 0);
                if(hin != hi)
                    cr.Walls |= CollisionGrid.Direction.XP;
            }
            if(z == 0) {
                cr.Walls |= CollisionGrid.Direction.ZN;
            }
            else {
                Point hi = vgr.HeightIndex(vw, x, z, 2);
                Point hin = vgr.HeightIndex(vw, x, z - 1, 3);
                if(hin != hi)
                    cr.Walls |= CollisionGrid.Direction.ZN;
            }
            if(z == h - 1) {
                cr.Walls |= CollisionGrid.Direction.ZP;
            }
            else {
                Point hi = vgr.HeightIndex(vw, x, z, 3);
                Point hin = vgr.HeightIndex(vw, x, z + 1, 2);
                if(hin != hi)
                    cr.Walls |= CollisionGrid.Direction.ZP;
            }

            if((cr.Walls & CollisionGrid.Direction.XN) != 0 && (cr.Walls & CollisionGrid.Direction.ZN) != 0)
                cr.Walls |= CollisionGrid.Direction.XNZN;
            if((cr.Walls & CollisionGrid.Direction.XP) != 0 && (cr.Walls & CollisionGrid.Direction.ZN) != 0)
                cr.Walls |= CollisionGrid.Direction.XPZN;
            if((cr.Walls & CollisionGrid.Direction.XN) != 0 && (cr.Walls & CollisionGrid.Direction.ZP) != 0)
                cr.Walls |= CollisionGrid.Direction.XNZP;
            if((cr.Walls & CollisionGrid.Direction.XP) != 0 && (cr.Walls & CollisionGrid.Direction.ZP) != 0)
                cr.Walls |= CollisionGrid.Direction.XPZP;
            return cr;
        }
    }
}