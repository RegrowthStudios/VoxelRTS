using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Grey.Vox {
    public class RegionFacer : IRegionWorker {
        const int INV_X_TOGGLE = 0x0f;
        const int INV_Z_TOGGLE = 0x0f << Region.INDZ_SHIFT;

        public static void CalcSeamX(Region rNX, Region rPX, int y, int z, VoxAtlas atlas) {
            int i1 = Region.ToIndex(Region.WIDTH - 1, y, z);
            int i2 = i1 ^ INV_X_TOGGLE;
            VoxData vd1 = atlas[rNX.voxels[i1].ID];
            VoxData vd2 = atlas[rPX.voxels[i2].ID];
            if(vd1.FaceType.CanShowFace(vd2.FaceType, Voxel.FACE_PX))
                Flags.SetFlagsOn(ref rNX.voxels[i1].Flags, Flags.BITS[Voxel.FACE_PX]);
            if(vd2.FaceType.CanShowFace(vd1.FaceType, Voxel.FACE_NX))
                Flags.SetFlagsOn(ref rPX.voxels[i2].Flags, Flags.BITS[Voxel.FACE_NX]);
        }
        public static void CalcSeamZ(Region rNZ, Region rPZ, int x, int y, VoxAtlas atlas) {
            int i1 = Region.ToIndex(x, y, Region.DEPTH - 1);
            int i2 = i1 ^ INV_Z_TOGGLE;
            VoxData vd1 = atlas[rNZ.voxels[i1].ID];
            VoxData vd2 = atlas[rPZ.voxels[i2].ID];
            if(vd1.FaceType.CanShowFace(vd2.FaceType, Voxel.FACE_PZ))
                Flags.SetFlagsOn(ref rNZ.voxels[i1].Flags, Flags.BITS[Voxel.FACE_PZ]);
            if(vd2.FaceType.CanShowFace(vd1.FaceType, Voxel.FACE_NZ))
                Flags.SetFlagsOn(ref rPZ.voxels[i2].Flags, Flags.BITS[Voxel.FACE_NZ]);
        }

        private Region region;

        public RegionFacer(Region r) {
            region = r;
        }

        public void DoWork() {
            //var world = region.World;
            //var atlas = world.Atlas;

            //Region[] neighbors = {
            //    world.pager.Obtain(region.loc.X - 1, region.loc.Y),
            //    world.pager.Obtain(region.loc.X + 1, region.loc.Y),
            //    world.pager.Obtain(region.loc.X, region.loc.Y - 1),
            //    world.pager.Obtain(region.loc.X, region.loc.Y + 1)
            //};

            //int i = 0;
            //Vector3I v = Vector3I.Zero;
            //for(v.Y = 0; v.Y < Region.HEIGHT; v.Y++) {
            //    for(v.Z = 0; v.Z < Region.DEPTH; v.Z++) {
            //        for(v.X = 0; v.X < Region.WIDTH; v.X++) {
            //            VoxData vd1 = atlas[region.voxels[i].ID], vd2;

            //            // Clear All Faces
            //            region.voxels[i].Flags &= ~Voxel.FACE_MASK;

            //            if(v.X != Region.WIDTH - 1) {
            //                vd2 = atlas[region.voxels[i + 1].ID];
            //                if(vd1.FaceType.CanShowFace(vd2.FaceType, Voxel.FACE_PX)) {
            //                    Flags.SetFlagsOn(ref region.voxels[i].Flags, Flags.BITS[Voxel.FACE_PX]);
            //                }
            //            }
            //            if(v.X != 0) {
            //                vd2 = atlas[region.voxels[i - 1].ID];
            //                if(vd1.FaceType.CanShowFace(vd2.FaceType, Voxel.FACE_NX)) {
            //                    Flags.SetFlagsOn(ref region.voxels[i].Flags, Flags.BITS[Voxel.FACE_NX]);
            //                }
            //            }

            //            if(v.Y != Region.HEIGHT - 1) {
            //                vd2 = atlas[region.voxels[i + Region.PLANE_SIZE].ID];
            //                if(vd1.FaceType.CanShowFace(vd2.FaceType, Voxel.FACE_PY)) {
            //                    Flags.SetFlagsOn(ref region.voxels[i].Flags, Flags.BITS[Voxel.FACE_PY]);
            //                }
            //            }
            //            if(v.Y != 0) {
            //                vd2 = atlas[region.voxels[i - Region.PLANE_SIZE].ID];
            //                if(vd1.FaceType.CanShowFace(vd2.FaceType, Voxel.FACE_NY)) {
            //                    Flags.SetFlagsOn(ref region.voxels[i].Flags, Flags.BITS[Voxel.FACE_NY]);
            //                }
            //            }

            //            if(v.Z != Region.DEPTH - 1) {
            //                vd2 = atlas[region.voxels[i + Region.WIDTH].ID];
            //                if(vd1.FaceType.CanShowFace(vd2.FaceType, Voxel.FACE_PZ)) {
            //                    Flags.SetFlagsOn(ref region.voxels[i].Flags, Flags.BITS[Voxel.FACE_PZ]);
            //                }
            //            }
            //            if(v.Z != 0) {
            //                vd2 = atlas[region.voxels[i - Region.WIDTH].ID];
            //                if(vd1.FaceType.CanShowFace(vd2.FaceType, Voxel.FACE_NZ)) {
            //                    Flags.SetFlagsOn(ref region.voxels[i].Flags, Flags.BITS[Voxel.FACE_NZ]);
            //                }
            //            }
            //            i++;
            //        }
            //    }
            //}

            //// TODO: Check Neighboring Regions
            //Region rNX = world.pager.Obtain(region.loc.X - 1, region.loc.Y);
            //if(rNX != null && rNX.IsLoaded) {
            //    for(v.Y = 0; v.Y < Region.HEIGHT; v.Y++) {
            //        for(v.Z = 0; v.Z < Region.DEPTH; v.Z++) {
            //            CalcSeamX(rNX, region, v.Y, v.Z, atlas);
            //        }
            //    }
            //    rNX.NotifyFacesChanged();
            //}

            //Region rPX = region.World.pager.Obtain(region.loc.X + 1, region.loc.Y);
            //if(rPX != null && rPX.IsLoaded) {
            //    for(v.Y = 0; v.Y < Region.HEIGHT; v.Y++) {
            //        for(v.Z = 0; v.Z < Region.DEPTH; v.Z++) {
            //            CalcSeamX(region, rPX, v.Y, v.Z, atlas);
            //        }
            //    }
            //    rPX.NotifyFacesChanged();
            //}

            //Region rNZ = region.World.pager.Obtain(region.loc.X, region.loc.Y - 1);
            //if(rNZ != null && rNZ.IsLoaded) {
            //    for(v.Y = 0; v.Y < Region.HEIGHT; v.Y++) {
            //        for(v.X = 0; v.X < Region.WIDTH; v.X++) {
            //            CalcSeamZ(rNZ, region, v.X, v.Y, atlas);
            //        }
            //    }
            //    rNZ.NotifyFacesChanged();
            //}

            //Region rPZ = region.World.pager.Obtain(region.loc.X, region.loc.Y + 1);
            //if(rPZ != null && rPZ.IsLoaded) {
            //    for(v.Y = 0; v.Y < Region.HEIGHT; v.Y++) {
            //        for(v.X = 0; v.X < Region.WIDTH; v.X++) {
            //            CalcSeamZ(region, rPZ, v.X, v.Y, atlas);
            //        }
            //    }
            //    rPZ.NotifyFacesChanged();
            //}

            // TODO: Send Notification Of Completeness
            region.NotifyFacesChanged();
        }

        public void CalcInner() {

        }
    }
}