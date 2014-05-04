using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Grey.Graphics;

namespace Grey.Vox {
    public struct RegionMeshResult {
        public List<Vector3I> Vertices;
        public List<int[]> Faces;
        public List<MeshedFace> MFaces;
    }

    public static class RegionGreedyMesher {
        public static List<MeshedFace> Mesh(Region region) {
            var res = new List<MeshedFace>();
            MeshY(region, res);
            MeshX(region, res);
            MeshZ(region, res);
            return res;
        }

        public static void MeshY(Region region, List<MeshedFace> res) {
            // Loop Through All Planes
            Vector3I pos = Vector3I.Zero;
            for(pos.Y = 0; pos.Y <= Region.HEIGHT; pos.Y++) {
                int[] mask = new int[Region.WIDTH * Region.DEPTH];
                int mi = 0;

                // Create Mask
                int nID, pID;
                for(pos.Z = 0; pos.Z < Region.DEPTH; pos.Z++) {
                    for(pos.X = 0; pos.X < Region.WIDTH; pos.X++) {
                        // Get Negative Voxel
                        if(pos.Y > 0)
                            nID = VoxelID(region, pos.X, pos.Y - 1, pos.Z);
                        else
                            nID = VoxelID(region, pos.X, pos.Y, pos.Z);

                        // Get Positive Voxel
                        if(pos.Y < Region.HEIGHT)
                            pID = VoxelID(region, pos.X, pos.Y, pos.Z);
                        else
                            pID = 0;

                        // Create Mask
                        if(nID == pID)
                            mask[mi] = 0;
                        else if(nID != 0)
                            mask[mi] = pos.Y > 0 ? nID : 0;
                        else
                            mask[mi] = pos.Y < Region.HEIGHT ? -pID : 0;
                        mi++;
                    }
                }

                // Generate Rectangles
                mi = 0;
                for(int v = 0; v < Region.DEPTH; v++) {
                    for(int u = 0; u < Region.WIDTH; u++, mi++) {
                        int id = mask[mi];
                        if(id != 0) {
                            int su = u;
                            int w = 1;
                            u++;
                            mi++;
                            while(u < Region.WIDTH && mask[mi] == id) {
                                w++;
                                u++;
                                mi++;
                            }
                            u--;
                            mi--;

                            MeshedFace mf;
                            if(id < 0) {
                                mf = new MeshedFace(
                                    new Vector3I(su, pos.Y, v + 1),
                                    new Vector3I(1, 0, 0),
                                    new Vector3I(0, 0, -1),
                                    new Point(w, 1),
                                    -id,
                                    Voxel.FACE_NY
                                    );
                            }
                            else {
                                mf = new MeshedFace(
                                    new Vector3I(su, pos.Y, v),
                                    new Vector3I(1, 0, 0),
                                    new Vector3I(0, 0, 1),
                                    new Point(w, 1),
                                    id,
                                    Voxel.FACE_PY
                                    );
                            }
                            res.Add(mf);
                        }
                    }
                }
            }
        }
        public static void MeshX(Region region, List<MeshedFace> res) {
            // Find Neighbor Regions
            Region rNeg = region.rNX;
            Region rPos = region.rPX;

            // Loop Through All Planes
            Vector3I pos = Vector3I.Zero;
            for(pos.X = 0; pos.X <= Region.WIDTH; pos.X++) {
                int[] mask = new int[Region.HEIGHT * Region.DEPTH];
                int mi = 0;

                // Create Mask
                int nID, pID;
                for(pos.Y = 0; pos.Y < Region.HEIGHT; pos.Y++) {
                    for(pos.Z = 0; pos.Z < Region.DEPTH; pos.Z++) {
                        // Get Negative Voxel
                        if(pos.X > 0)
                            nID = VoxelID(region, pos.X - 1, pos.Y, pos.Z);
                        else if(rNeg != null)
                            nID = VoxelID(rNeg, Region.WIDTH - 1, pos.Y, pos.Z);
                        else
                            nID = 0;

                        // Get Positive Voxel
                        if(pos.X < Region.WIDTH)
                            pID = VoxelID(region, pos.X, pos.Y, pos.Z);
                        else if(rPos != null)
                            pID = VoxelID(rPos, 0, pos.Y, pos.Z);
                        else
                            pID = 0;

                        // Create Mask
                        if(nID == pID)
                            mask[mi] = 0;
                        else if(nID != 0)
                            mask[mi] = pos.X > 0 ? nID : 0;
                        else
                            mask[mi] = pos.X < Region.WIDTH ? -pID : 0;
                        mi++;
                    }
                }

                // Generate Rectangles
                mi = 0;
                for(pos.Y = 0; pos.Y < Region.HEIGHT; pos.Y++) {
                    for(pos.Z = 0; pos.Z < Region.DEPTH; pos.Z++, mi++) {
                        int id = mask[mi];
                        if(id != 0) {
                            int su = pos.Z;
                            int w = 1;
                            pos.Z++;
                            mi++;
                            while(pos.Z < Region.DEPTH && mask[mi] == id) {
                                w++;
                                pos.Z++;
                                mi++;
                            }
                            pos.Z--;
                            mi--;

                            MeshedFace mf;
                            if(id < 0) {
                                mf = new MeshedFace(
                                    new Vector3I(pos.X, pos.Y + 1, su),
                                    new Vector3I(0, 0, 1),
                                    new Vector3I(0, -1, 0),
                                    new Point(w, 1),
                                    -id,
                                    Voxel.FACE_NX
                                    );
                            }
                            else {
                                mf = new MeshedFace(
                                    new Vector3I(pos.X, pos.Y + 1, pos.Z + 1),
                                    new Vector3I(0, 0, -1),
                                    new Vector3I(0, -1, 0),
                                    new Point(w, 1),
                                    id,
                                    Voxel.FACE_PX
                                    );
                            }
                            res.Add(mf);
                        }
                    }
                }
            }
        }
        public static void MeshZ(Region region, List<MeshedFace> res) {
            // Find Neighbor Regions
            Region rNeg = region.rNZ;
            Region rPos = region.rPZ;

            // Loop Through All Planes
            Vector3I pos = Vector3I.Zero;
            for(pos.Z = 0; pos.Z <= Region.DEPTH; pos.Z++) {
                int[] mask = new int[Region.HEIGHT * Region.WIDTH];
                int mi = 0;

                // Create Mask
                int nID, pID;
                for(pos.Y = 0; pos.Y < Region.HEIGHT; pos.Y++) {
                    for(pos.X = 0; pos.X < Region.WIDTH; pos.X++) {
                        // Get Negative Voxel
                        if(pos.Z > 0)
                            nID = VoxelID(region, pos.X, pos.Y, pos.Z - 1);
                        else if(rNeg != null)
                            nID = VoxelID(rNeg, pos.X, pos.Y, Region.DEPTH - 1);
                        else
                            nID = 0;

                        // Get Positive Voxel
                        if(pos.Z < Region.DEPTH)
                            pID = VoxelID(region, pos.X, pos.Y, pos.Z);
                        else if(rPos != null)
                            pID = VoxelID(rPos, pos.X, pos.Y, 0);
                        else
                            pID = 0;

                        // Create Mask
                        if(nID == pID)
                            mask[mi] = 0;
                        else if(nID != 0)
                            mask[mi] = pos.Z > 0 ? nID : 0;
                        else
                            mask[mi] = pos.Z < Region.DEPTH ? -pID : 0;
                        mi++;
                    }
                }

                // Generate Rectangles
                mi = 0;
                for(pos.Y = 0; pos.Y < Region.HEIGHT; pos.Y++) {
                    for(pos.X = 0; pos.X < Region.WIDTH; pos.X++, mi++) {
                        int id = mask[mi];
                        if(id != 0) {
                            int su = pos.X;
                            int w = 1;
                            pos.X++;
                            mi++;
                            while(pos.X < Region.WIDTH && mask[mi] == id) {
                                w++;
                                pos.X++;
                                mi++;
                            }
                            pos.X--;
                            mi--;

                            MeshedFace mf;
                            if(id < 0) {
                                mf = new MeshedFace(
                                    new Vector3I(pos.X + 1, pos.Y + 1, pos.Z),
                                    new Vector3I(-1, 0, 0),
                                    new Vector3I(0, -1, 0),
                                    new Point(w, 1),
                                    -id,
                                    Voxel.FACE_NZ
                                    );
                            }
                            else {
                                mf = new MeshedFace(
                                    new Vector3I(su, pos.Y + 1, pos.Z),
                                    new Vector3I(1, 0, 0),
                                    new Vector3I(0, -1, 0),
                                    new Point(w, 1),
                                    id,
                                    Voxel.FACE_PZ
                                    );
                            }
                            res.Add(mf);
                        }
                    }
                }
            }
        }

        private static ushort VoxelID(Region region, int x, int y, int z) {
            return region.voxels[Region.ToIndex(x, y, z)].ID;
        }
    }
}