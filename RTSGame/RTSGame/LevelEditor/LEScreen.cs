using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;
using Grey.Engine;
using Grey.Vox.Managers;
using Grey.Graphics;
using Grey.Vox;

namespace RTS {
    public struct LEVT {
        public string Name;
        public uint FaceType;
        public uint FaceMask;
        public IVGeoProvider Geo;
    }

    public class LEScreen : GameScreen<App> {
        public override int Next {
            get { return -1; }
            protected set { }
        }
        public override int Previous {
            get { return game.MenuScreen.Index; }
            protected set { }
        }

        WorldManager vManager;
        VoxState state;
        VoxelRenderer renderer;
        Dictionary<string, VoxData> dVox;

        InputManager input;
        OrbitingCamera camera;

        public override void Build() {
            camera = new OrbitingCamera(Vector3.UnitY * Region.HEIGHT * 0.6f, 100, G.Viewport.AspectRatio);
            input = new InputManager();
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            dVox = new Dictionary<string, VoxData>();
            state = new VoxState();

            var vts = ZXParser.ParseFile(@"LevelEditor\Geo.zxp", typeof(LEVT[])) as LEVT[];
            foreach(var vt in vts) {
                var v = state.World.Atlas.Create();
                v.FaceType.SetAllTypes(vt.FaceType);
                v.FaceType.SetAllMasks(vt.FaceMask);
                v.GeoProvider = vt.Geo;
                dVox.Add(vt.Name, v);
            }

            renderer = new VoxelRenderer(game.Graphics);
            renderer.Hook(state);
            renderer.LoadEffect(@"LevelEditor\Voxel.fx");
            renderer.LoadVMap(@"LevelEditor\VoxMap.png");

            vManager = new WorldManager(state);
            for(int z = 0; z < VoxWorld.DEPTH; z++) {
                for(int x = 0; x < VoxWorld.WIDTH; x++) {
                    state.AddEvent(new VEWorldMod(state.World.worldMin.X + x, state.World.worldMin.Y + z, VEWMType.RegionAdd));
                }
            }
            state.VWorkPool.Start(2);

            MouseEventDispatcher.OnMousePress += OnMP;
        }
        public override void OnExit(GameTime gameTime) {
            MouseEventDispatcher.OnMousePress -= OnMP;

            state.VWorkPool.Dispose();
            renderer.Dispose();
        }

        public override void Update(GameTime gameTime) {
            input.Refresh();
            camera.ControlCamera((float)gameTime.ElapsedGameTime.TotalSeconds, input, G.Viewport);
            camera.UpdateView();

            vManager.Update();
            renderer.RetaskVisualChanges();
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);
            renderer.DrawAll(camera.View, camera.Projection);

            G.BlendState = BlendState.NonPremultiplied;
            game.DrawMouse();
        }

        public void OnMP(Vector2 sPos, MouseButton button) {
            Ray r = camera.GetViewRay(sPos, G.Viewport.Width, G.Viewport.Height);
            r.Position -= new Vector3(state.World.worldMin.X * Region.WIDTH, 0, state.World.worldMin.Y * Region.DEPTH);
            VRay vr = new VRay(r.Position, r.Direction);
            Vector3I loc = vr.GetNextLocation();
            BoundingBox bb = new BoundingBox(Vector3.Zero, new Vector3(VoxWorld.WIDTH * Region.WIDTH, Region.HEIGHT, VoxWorld.DEPTH * Region.DEPTH));
            if(!r.Intersects(bb).HasValue) {
                return;
            }
            while(!IsInBounds(loc)) loc = vr.GetNextLocation();
            if(button == MouseButton.Left) {
                while(IsInBounds(loc)) {
                    int rx = loc.X >> Region.XZ_SHIFT;
                    int vx = loc.X & ((0x01 << Region.XZ_SHIFT) - 1);
                    int rz = loc.Z >> Region.XZ_SHIFT;
                    int vz = loc.Z & ((0x01 << Region.XZ_SHIFT) - 1);
                    Region region = state.World.regions[VoxWorld.ToIndex(rx, rz)];
                    ushort id = region.voxels[Region.ToIndex(vx, loc.Y, vz)].ID;
                    if(id != 0) {
                        region.RemoveVoxel(vx, loc.Y, vz);
                        break;
                    }
                    loc = vr.GetNextLocation();
                }
            }
            else if(button == MouseButton.Right) {
                Vector3I pLoc = loc;
                Vector3I pv;
                Point pr;
                while(IsInBounds(loc)) {
                    int rx = loc.X >> Region.XZ_SHIFT;
                    int vx = loc.X & ((0x01 << Region.XZ_SHIFT) - 1);
                    int rz = loc.Z >> Region.XZ_SHIFT;
                    int vz = loc.Z & ((0x01 << Region.XZ_SHIFT) - 1);
                    Region region = state.World.regions[VoxWorld.ToIndex(rx, rz)];
                    ushort id = region.voxels[Region.ToIndex(vx, loc.Y, vz)].ID;
                    if(id != 0) {
                        ToRV(pLoc, out pr, out pv);
                        state.World.regions[VoxWorld.ToIndex(pr.X, pr.Y)].AddVoxel(pv.X, pv.Y, pv.Z, (ushort)1u);
                        break;
                    }
                    pLoc = loc;
                    loc = vr.GetNextLocation();
                }
            }
        }
        public bool IsInBounds(Vector3I v) {
            return
                v.X >= 0 && v.X < VoxWorld.WIDTH * Region.WIDTH &&
                v.Y >= 0 && v.Y < Region.HEIGHT &&
                v.Z >= 0 && v.Z < VoxWorld.DEPTH * Region.DEPTH
                ;
        }

        public void ToRV(Vector3I loc, out Point r, out Vector3I v) {
            r = new Point(loc.X >> Region.XZ_SHIFT, loc.Z >> Region.XZ_SHIFT);
            v = new Vector3I(
                loc.X & ((0x01 << Region.XZ_SHIFT) - 1),
                loc.Y,
                loc.Z & ((0x01 << Region.XZ_SHIFT) - 1)
                );
        }
    }
}