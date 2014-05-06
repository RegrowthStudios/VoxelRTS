using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Grey.Engine;
using BlisterUI.Input;
using Grey.Vox;

namespace RTS {
    public struct VoxLocation {
        public Point RegionLoc;
        public int RegionIndex {
            get { return VoxWorld.ToIndex(RegionLoc); }
        }
        public Vector3I VoxelLoc;
        public int VoxelIndex {
            get { return Region.ToIndex(VoxelLoc); }
        }

        public VoxLocation(Vector3I loc) {
            RegionLoc = new Point(loc.X >> Region.XZ_SHIFT, loc.Z >> Region.XZ_SHIFT);
            VoxelLoc = new Vector3I(
                loc.X & ((0x01 << Region.XZ_SHIFT) - 1),
                loc.Y,
                loc.Z & ((0x01 << Region.XZ_SHIFT) - 1)
                );
        }
    }

    public abstract class LETool : IDisposable {
        public Texture2D MouseTexture {
            get;
            private set;
        }

        public string Name;

        public ushort CurVoxID {
            get;
            set;
        }

        public LETool() {
            MouseTexture = null;
            Name = "Unnamed";
        }
        public virtual void Dispose() {
            MouseTexture.Dispose();
        }

        public void LoadMouseTexture(GraphicsDevice g, string file) {
            // Load From The File
            if(!File.Exists(file)) return;
            using(var s = File.OpenRead(file))
                MouseTexture = Texture2D.FromStream(g, s);
        }

        public abstract void OnMouseClick(VoxState s, FreeCamera camera, Vector2 mPos, MouseButton button, Viewport vp);
    }

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
                ushort id = region.voxels[vl.VoxelIndex].ID;
                if(id != 0) return pvl;
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
    public class LETSingle : LETool {

        public LETSingle()
            : base() {
            CurVoxID = 0;
            Name = "Single";
        }

        public override void OnMouseClick(VoxState s, FreeCamera camera, Vector2 mPos, MouseButton button, Viewport vp) {
            Ray r = camera.GetViewRay(mPos, vp.Width, vp.Height);
            if(button == MouseButton.Left) {
                VoxLocation? vl = VRayHelper.GetInner(r, s);
                if(vl.HasValue) {
                    var loc = vl.Value;
                    s.World.regions[loc.RegionIndex].RemoveVoxel(loc.VoxelLoc.X, loc.VoxelLoc.Y, loc.VoxelLoc.Z);
                }
            }
            else if(button == MouseButton.Right) {
                VoxLocation? vl = VRayHelper.GetOuter(r, s);
                if(vl.HasValue) {
                    var loc = vl.Value;
                    s.World.regions[loc.RegionIndex].AddVoxel(loc.VoxelLoc.X, loc.VoxelLoc.Y, loc.VoxelLoc.Z, CurVoxID);
                }
            }
        }
    }
    public class LETCluster : LETool {
        public LETCluster()
            : base() {
            CurVoxID = 0;
            Name = "Cluster";
        }
        IEnumerable<Ray> GetRays(Vector2 center, Vector2 disp, Point count, FreeCamera camera, Viewport vp) {
            Vector2 start = center - new Vector2(count.X, count.Y) * 0.5f * disp;
            for(int y = 0; y < count.Y; y++) {
                for(int x = 0; x < count.X; x++) {
                    yield return camera.GetViewRay(start + disp * new Vector2(y, x), vp.Width, vp.Height);
                }
            }
        }
        public override void OnMouseClick(VoxState s, FreeCamera camera, Vector2 mPos, MouseButton button, Viewport vp) {
            List<VoxLocation> locs = new List<VoxLocation>();
            if(button == MouseButton.Left) {
                foreach(var r in GetRays(mPos, Vector2.One * 3, new Point(9, 9), camera, vp)) {
                    VoxLocation? vl = VRayHelper.GetInner(r, s);
                    if(vl.HasValue) locs.Add(vl.Value);
                }
                foreach(var loc in locs.Distinct())
                    s.World.regions[loc.RegionIndex].RemoveVoxel(loc.VoxelLoc.X, loc.VoxelLoc.Y, loc.VoxelLoc.Z);
            }
            else if(button == MouseButton.Right) {
                foreach(var r in GetRays(mPos, Vector2.One * 5, new Point(9, 9), camera, vp)) {
                    VoxLocation? vl = VRayHelper.GetOuter(r, s);
                    if(vl.HasValue) locs.Add(vl.Value);
                }
                foreach(var loc in locs.Distinct())
                    s.World.regions[loc.RegionIndex].AddVoxel(loc.VoxelLoc.X, loc.VoxelLoc.Y, loc.VoxelLoc.Z, CurVoxID);
            }
        }
    }
    public class LETPaint : LETool {
        public LETPaint()
            : base() {
            CurVoxID = 0;
            Name = "Paint";
        }
        IEnumerable<Ray> GetRays(Vector2 center, Vector2 disp, Point count, FreeCamera camera, Viewport vp) {
            Vector2 start = center - new Vector2(count.X, count.Y) * 0.5f * disp;
            for(int y = 0; y < count.Y; y++) {
                for(int x = 0; x < count.X; x++) {
                    yield return camera.GetViewRay(start + disp * new Vector2(y, x), vp.Width, vp.Height);
                }
            }
        }
        public override void OnMouseClick(VoxState s, FreeCamera camera, Vector2 mPos, MouseButton button, Viewport vp) {
            List<VoxLocation> locs = new List<VoxLocation>();
            if(button == MouseButton.Left) {
                foreach(var r in GetRays(mPos, Vector2.One * 2, new Point(25, 25), camera, vp)) {
                    VoxLocation? vl = VRayHelper.GetLevel(r, s, Region.HEIGHT - 4);
                    if(vl.HasValue) locs.Add(vl.Value);
                }
                foreach(var loc in locs.Distinct())
                    s.World.regions[loc.RegionIndex].RemoveVoxel(loc.VoxelLoc.X, loc.VoxelLoc.Y, loc.VoxelLoc.Z);
            }
            else if(button == MouseButton.Right) {
                foreach(var r in GetRays(mPos, Vector2.One * 2, new Point(25, 25), camera, vp)) {
                    VoxLocation? vl = VRayHelper.GetLevel(r, s, Region.HEIGHT - 4);
                    if(vl.HasValue) locs.Add(vl.Value);
                }
                foreach(var loc in locs.Distinct())
                    s.World.regions[loc.RegionIndex].AddVoxel(loc.VoxelLoc.X, loc.VoxelLoc.Y, loc.VoxelLoc.Z, CurVoxID);
            }
        }
    }
}