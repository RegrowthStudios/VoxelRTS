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
using BlisterUI.Widgets;

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

        // Voxel Modifying Data
        Dictionary<string, VoxData> dVox;
        ushort curVoxID;

        // Widgets
        WidgetRenderer wr;
        IDisposable font;
        ScrollMenu voxMenu;

        InputManager input;
        FreeCamera camera;

        public override void Build() {
            input = new InputManager();
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            camera = new FreeCamera(Vector3.UnitY * Region.HEIGHT * 0.6f, 0, 0, G.Viewport.AspectRatio);

            CreateVoxWorld();
            CreateWidgets();
            curVoxID = 0;
            MouseEventDispatcher.OnMousePress += OnMP;
        }
        private void CreateVoxWorld() {
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
        }
        private void CreateWidgets() {
            wr = new WidgetRenderer(G, XNASpriteFont.Compile(G, "Impact", 24, out font));

            voxMenu = new ScrollMenu(wr, 180, 30, 5, 12, 30);
            voxMenu.Build(dVox.Keys.ToArray());
            voxMenu.BaseColor = Color.LightCoral;
            voxMenu.HighlightColor = Color.Red;
            voxMenu.TextColor = Color.Black;
            voxMenu.ScrollBarBaseColor = Color.Green;
            voxMenu.Widget.AlignY = Alignment.BOTTOM;
            voxMenu.Widget.Anchor = new Point(0, game.Window.ClientBounds.Height);
            voxMenu.Hook();
        }
        public override void OnExit(GameTime gameTime) {
            MouseEventDispatcher.OnMousePress -= OnMP;
            DestroyWidgets();
            DestroyVoxWorld();
        }
        private void DestroyVoxWorld() {
            state.VWorkPool.Dispose();
            renderer.Dispose();

        }
        private void DestroyWidgets() {
            voxMenu.Unhook();
            voxMenu.Dispose();
            wr.Dispose();
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

            if(!input.Mouse.IsBound) {
                G.DepthStencilState = DepthStencilState.None;
                G.BlendState = BlendState.NonPremultiplied;

                wr.Draw(SB);
                game.DrawMouse();
            }
        }

        public void OnMP(Vector2 sPos, MouseButton button) {
            Point mPos = new Point((int)sPos.X, (int)sPos.Y);

            // Check For New Voxel Selection
            if(voxMenu.Inside(mPos.X, mPos.Y)) {
                string vt = voxMenu.GetSelection(mPos.X, mPos.Y);
                if(string.IsNullOrWhiteSpace(vt)) return;
                VoxData vd;
                if(dVox.TryGetValue(vt, out vd))
                    curVoxID = vd.ID;
                return;
            }


            Ray r = camera.GetViewRay(sPos, G.Viewport.Width, G.Viewport.Height);
            r.Position -= new Vector3(state.World.worldMin.X * Region.WIDTH, 0, state.World.worldMin.Y * Region.DEPTH);
            VRay vr = new VRay(r.Position, r.Direction);
            Vector3I loc = vr.GetNextLocation();
            BoundingBox bb = new BoundingBox(Vector3.Zero, new Vector3(VoxWorld.WIDTH * Region.WIDTH, Region.HEIGHT, VoxWorld.DEPTH * Region.DEPTH));
            if(!r.Intersects(bb).HasValue) {
                return;
            }
            while(!IsInBounds(loc))
                loc = vr.GetNextLocation();

            Point cr, pr;
            Vector3I cv, pv;

            if(button == MouseButton.Left) {
                while(IsInBounds(loc)) {
                    ToRV(loc, out cr, out cv);
                    Region region = state.World.regions[VoxWorld.ToIndex(cr)];
                    ushort id = region.voxels[Region.ToIndex(cv)].ID;
                    if(id != 0) {
                        region.RemoveVoxel(cv.X, cv.Y, cv.Z);
                        break;
                    }
                    loc = vr.GetNextLocation();
                }
            }
            else if(button == MouseButton.Right) {
                Vector3I pLoc = loc;
                while(IsInBounds(loc)) {
                    ToRV(loc, out cr, out cv);
                    Region region = state.World.regions[VoxWorld.ToIndex(cr)];
                    ushort id = region.voxels[Region.ToIndex(cv)].ID;
                    if(id != 0) {
                        ToRV(pLoc, out pr, out pv);
                        state.World.regions[VoxWorld.ToIndex(pr)].AddVoxel(pv.X, pv.Y, pv.Z, curVoxID);
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