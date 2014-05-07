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
using System.IO;
using System.IO.Compression;
using Microsoft.Xna.Framework.Input;

namespace RTS {
    public struct LEVT {
        public string Name;
        public uint FaceType;
        public uint FaceMask;
        public IVGeoProvider Geo;
    }
    public struct Tool {

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

        // Widgets
        WidgetRenderer wr;
        IDisposable font;
        ScrollMenu voxMenu;

        InputManager input;
        FreeCamera camera;

        // Tools
        LETool curTool;
        LETool[] tools;
        ScrollMenu toolMenu;

        public override void Build() {
            input = new InputManager();
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            camera = new FreeCamera(Vector3.UnitY * Region.HEIGHT * 0.6f, 0, 0, G.Viewport.AspectRatio);

            CreateVoxWorld();
            CreateWidgets();
            CreateTools();
            MouseEventDispatcher.OnMousePress += OnMP;
            KeyboardEventDispatcher.OnKeyPressed += OnKP;
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
            Random r = new Random();
            for(int i = 0; i < 20; i++) {
                var v = state.World.Atlas.Create();
                v.FaceType.SetAllTypes(1);
                v.FaceType.SetAllMasks(254);
                var vgp = new VGPCube();
                vgp.Color = new Color(r.Next(256), r.Next(256), r.Next(256));
                vgp.UVRect = new Vector4(0, 0.25f, 0.25f, 0.25f);
                v.GeoProvider = vgp;
                dVox.Add("Region " + i, v);
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
        private void CreateTools() {
            tools = new LETool[] {
                new LETSingle(),
                new LETCluster(),
                new LETPaint()
            };
            curTool = tools[0];

            // Create Menu
            toolMenu = new ScrollMenu(wr, 180, 30, 5, 12, 30);
            toolMenu.Build((from tool in tools select tool.Name).ToArray());
            toolMenu.BaseColor = Color.LightCoral;
            toolMenu.HighlightColor = Color.Red;
            toolMenu.TextColor = Color.Black;
            toolMenu.ScrollBarBaseColor = Color.Green;
            toolMenu.Widget.AlignY = Alignment.BOTTOM;
            toolMenu.Parent = voxMenu.Widget;
            toolMenu.Hook();

            // Set The Textures
            foreach(var tool in tools)
                tool.LoadMouseTexture(G, @"LevelEditor\Mouse\" + tool.Name + ".png");
            game.mRenderer.Texture = curTool.MouseTexture;
        }
        public override void OnExit(GameTime gameTime) {
            MouseEventDispatcher.OnMousePress -= OnMP;
            KeyboardEventDispatcher.OnKeyPressed -= OnKP;
            DestroyTools();
            DestroyWidgets();
            DestroyVoxWorld();
            game.mRenderer.Texture = game.tMouseMain;
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
        private void DestroyTools() {
            foreach(var tool in tools) tool.Dispose();
            tools = null;
            curTool = null;
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
                if(dVox.TryGetValue(vt, out vd)) {
                    foreach(var tool in tools)
                        tool.CurVoxID = vd.ID;
                }
                return;
            }

            if(toolMenu.Inside(mPos.X, mPos.Y)) {
                string vt = toolMenu.GetSelection(mPos.X, mPos.Y);
                curTool = (from tool in tools where tool.Name.Equals(vt) select tool).FirstOrDefault();
                if(curTool != null)
                    game.mRenderer.Texture = curTool.MouseTexture;
                return;
            }

            if(curTool != null)
                curTool.OnMouseClick(state, camera, sPos, button, G.Viewport);
        }
        public void OnKP(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.F2:
                    Write(@"LevelEditor\Saved", 100, 100);
                    break;
            }
        }

        private void Write(string dir, int w, int h) {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            if(!dirInfo.Exists) dirInfo.Create();

            WriteHeights(dirInfo.FullName + @"\height.hmd", w, h);
        }
        private void WriteHeights(string file, int w, int h) {
            byte[] hd = new byte[w * h * 16 + 8];
            int i = 0;
            BitConverter.GetBytes(w).CopyTo(hd, i); i += 4;
            BitConverter.GetBytes(h).CopyTo(hd, i); i += 4;
            Vector3I loc = Vector3I.Zero;
            RTSEngine.Data.HeightTile ht = new RTSEngine.Data.HeightTile();
            for(loc.Z = 0; loc.Z < h; loc.Z++) {
                for(loc.X = 0; loc.X < w; loc.X++) {
                    loc.Y = Region.HEIGHT - 1;
                    VoxLocation vl = new VoxLocation(loc);

                    ht = new RTSEngine.Data.HeightTile();
                    Region r = state.World.regions[vl.RegionIndex];
                    for(; vl.VoxelLoc.Y > 0; vl.VoxelLoc.Y--) {
                        ushort id = r.voxels[vl.VoxelIndex].ID;
                        if(id == 1) {
                            ht.XNZN = vl.VoxelLoc.Y;
                            ht.XPZN = vl.VoxelLoc.Y;
                            ht.XNZP = vl.VoxelLoc.Y;
                            ht.XPZP = vl.VoxelLoc.Y;
                            break;
                        }
                    }

                    BitConverter.GetBytes(ht.XNZN).CopyTo(hd, i); i += 4;
                    BitConverter.GetBytes(ht.XPZN).CopyTo(hd, i); i += 4;
                    BitConverter.GetBytes(ht.XNZP).CopyTo(hd, i); i += 4;
                    BitConverter.GetBytes(ht.XPZP).CopyTo(hd, i); i += 4;
                }
            }

            using(MemoryStream ms = new MemoryStream()) {
                var gs = new GZipStream(ms, CompressionMode.Compress, true);
                gs.Write(hd, 0, hd.Length);
                gs.Close();
                ms.Position = 0;
                using(var s = File.Create(file)) {
                    var bw = new BinaryWriter(s);
                    bw.Write(hd.Length);
                    bw.Flush();
                    ms.CopyTo(s);
                    s.Flush();
                }
            }
        }
    }
}