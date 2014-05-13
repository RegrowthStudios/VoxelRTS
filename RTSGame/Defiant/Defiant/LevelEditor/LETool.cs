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
using Grey.Vox.Ops;

namespace RTS {

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
        public const int HEIGHT = Region.HEIGHT - 4;

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
                    VoxLocation? vl = VRayHelper.GetLevel(r, s, HEIGHT);
                    if(vl.HasValue) locs.Add(vl.Value);
                }
                foreach(var loc in locs.Distinct())
                    s.World.regions[loc.RegionIndex].RemoveVoxel(loc.VoxelLoc.X, loc.VoxelLoc.Y, loc.VoxelLoc.Z);
            }
            else if(button == MouseButton.Right) {
                foreach(var r in GetRays(mPos, Vector2.One * 2, new Point(25, 25), camera, vp)) {
                    VoxLocation? vl = VRayHelper.GetLevel(r, s, HEIGHT);
                    if(vl.HasValue) locs.Add(vl.Value);
                }
                foreach(var loc in locs.Distinct())
                    s.World.regions[loc.RegionIndex].AddVoxel(loc.VoxelLoc.X, loc.VoxelLoc.Y, loc.VoxelLoc.Z, CurVoxID);
            }
        }
    }
}