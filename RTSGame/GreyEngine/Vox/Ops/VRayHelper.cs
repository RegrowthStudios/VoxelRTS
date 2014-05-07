using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grey.Engine;
using Microsoft.Xna.Framework;

namespace Grey.Vox.Ops {
    public static class VRayHelper {
        public static bool IsInBounds(Vector3I v) {
            return
                v.X >= 0 && v.X < VoxWorld.WIDTH * Region.WIDTH &&
                v.Y >= 0 && v.Y < Region.HEIGHT &&
                v.Z >= 0 && v.Z < VoxWorld.DEPTH * Region.DEPTH
                ;
        }
        public static VoxLocation? GetInner(Ray camRay, VoxState state) {
            camRay.Position -= new Vector3(state.World.worldMin.X * Region.WIDTH, 0, state.World.worldMin.Y * Region.DEPTH);
            VRay vr = new VRay(camRay.Position, camRay.Direction);
            Vector3I loc = vr.GetNextLocation();

            // Check For World Intersect
            BoundingBox bb = new BoundingBox(Vector3.Zero, new Vector3(VoxWorld.WIDTH * Region.WIDTH, Region.HEIGHT, VoxWorld.DEPTH * Region.DEPTH));
            if(!camRay.Intersects(bb).HasValue) return null;

            // Move In World
            while(!IsInBounds(loc))
                loc = vr.GetNextLocation();

            // Move Through World
            while(IsInBounds(loc)) {
                VoxLocation vl = new VoxLocation(loc);
                Region region = state.World.regions[vl.RegionIndex];
                ushort id = region.voxels[vl.VoxelIndex].ID;
                if(id != 0) return vl;
                loc = vr.GetNextLocation();
            }
            return null;
        }
        public static VoxLocation? GetOuter(Ray camRay, VoxState state) {
            camRay.Position -= new Vector3(state.World.worldMin.X * Region.WIDTH, 0, state.World.worldMin.Y * Region.DEPTH);
            VRay vr = new VRay(camRay.Position, camRay.Direction);
            Vector3I loc = vr.GetNextLocation();

            // Check For World Intersect
            BoundingBox bb = new BoundingBox(Vector3.Zero, new Vector3(VoxWorld.WIDTH * Region.WIDTH, Region.HEIGHT, VoxWorld.DEPTH * Region.DEPTH));
            if(!camRay.Intersects(bb).HasValue) return null;

            // Move In World
            while(!IsInBounds(loc))
                loc = vr.GetNextLocation();

            // Move Through World
            VoxLocation pvl = new VoxLocation(loc);
            while(IsInBounds(loc)) {
                VoxLocation vl = new VoxLocation(loc);
                Region region = state.World.regions[vl.RegionIndex];
                if(region != null) {
                    ushort id = region.voxels[vl.VoxelIndex].ID;
                    if(id != 0) return pvl;
                }
                loc = vr.GetNextLocation();
                pvl = vl;
            }
            return null;
        }
        public static VoxLocation? GetLevel(Ray camRay, VoxState state, int h) {
            camRay.Position -= new Vector3(state.World.worldMin.X * Region.WIDTH, 0, state.World.worldMin.Y * Region.DEPTH);
            VRay vr = new VRay(camRay.Position, camRay.Direction);
            Vector3I loc = vr.GetNextLocation();

            // Check For World Intersect
            BoundingBox bb = new BoundingBox(new Vector3(0, h - 1, 0), new Vector3(VoxWorld.WIDTH * Region.WIDTH, h, VoxWorld.DEPTH * Region.DEPTH));
            if(!camRay.Intersects(bb).HasValue) return null;

            // Move In World
            while(!IsInBounds(loc))
                loc = vr.GetNextLocation();

            // Move Through World
            while(IsInBounds(loc)) {
                VoxLocation vl = new VoxLocation(loc);
                Region region = state.World.regions[vl.RegionIndex];
                ushort id = region.voxels[vl.VoxelIndex].ID;
                if(loc.Y == h) return vl;
                loc = vr.GetNextLocation();
            }
            return null;
        }
    }
}