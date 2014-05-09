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
using CGrid = RTSEngine.Data.CollisionGrid;
using System.Text.RegularExpressions;
using RTSEngine.Data.Parsers;
using RTSEngine.Controllers;
using RTSEngine.Interfaces;

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
        const float DUV = 0.125f;
        private static readonly Regex rgxSave = RegexHelper.GenerateVec2Int("SAVE");
        private static readonly Color[] REGION_COLORS = new Color[] {
            Color.Blue, Color.Red, Color.Green, Color.Yellow, Color.Orange,
            Color.Purple, Color.Pink, Color.Gray, Color.GhostWhite, new Color(10, 10, 10),
            Color.DarkTurquoise, Color.Honeydew, Color.Brown, Color.Salmon, Color.Sienna,
            Color.Wheat, Color.Plum, Color.MidnightBlue, Color.MintCream, Color.LightCyan
        };
        private const int MINID_TERRAIN = 1;
        private const int COUNT_TERRAIN = 5;
        private const int MINID_SCENERY = MINID_TERRAIN + COUNT_TERRAIN;
        private const int COUNT_SCENERY = 10;
        private const int MINID_RAMP = MINID_SCENERY + COUNT_SCENERY;
        private const int COUNT_RAMP = 4;
        private const int MINID_REGION = MINID_RAMP + COUNT_RAMP;


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
        List<ACGameTypeController> gtcList;
        List<ACInputController> icList;

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
            gtcList = new List<ACGameTypeController>();
            icList = new List<ACInputController>();
            foreach(var kvp in GameEngine.Scripts) {
                ReflectedScript rs = kvp.Value;
                if(rs.ScriptType == ScriptType.GameType)
                    gtcList.Add(rs.CreateInstance<ACGameTypeController>());
                else if(rs.ScriptType == ScriptType.Input)
                    icList.Add(rs.CreateInstance<ACInputController>());
            }

            CreateVoxWorld();
            CreateWidgets();
            CreateTools();
            MouseEventDispatcher.OnMousePress += OnMP;
            KeyboardEventDispatcher.OnKeyPressed += OnKP;
        }
        private void CreateVoxWorld() {
            dVox = new Dictionary<string, VoxData>();
            state = new VoxState();
            state.World.worldMin.X = 0;
            state.World.worldMin.Y = 0;
            CreateVoxTypes();
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
            state.World.OnRegionAddition += (w, r) => {
                if(r.loc.X == 0 && r.loc.Y == 0)
                    r.AddVoxel(1, Region.HEIGHT - 2, 1, (ushort)0x06u);
            };
        }
        private void CreateVoxTypes() {
            Random r = new Random(343);
            for(int i = 0; i < COUNT_TERRAIN; i++) {
                var vd = state.World.Atlas.Create();
                vd.FaceType = new VoxFaceType();
                vd.FaceType.SetAllTypes(0x00000001u);
                vd.FaceType.SetAllMasks(0xfffffffeu);
                var vgp = new VGPCube();
                switch(i) {
                    case 0: vgp.Color = Color.White; break;
                    case 1: vgp.Color = Color.Goldenrod; break;
                    case 2: vgp.Color = Color.ForestGreen; break;
                    case 3: vgp.Color = Color.Brown; break;
                    case 4: vgp.Color = Color.Red; break;
                }
                vgp.UVRect = new Vector4(DUV * 0, DUV * 0, DUV, DUV);
                vd.GeoProvider = vgp;
                dVox.Add("Terrain " + i, vd);
            }
            for(int i = 0; i < COUNT_SCENERY; i++) {
                var vd = state.World.Atlas.Create();
                vd.FaceType = new VoxFaceType();
                vd.FaceType.SetAllTypes(0x00000001u);
                vd.FaceType.SetAllMasks(0xfffffffeu);
                var vgp = new VGPCube();
                switch(i) {
                    case 0: vgp.Color = Color.White; break;
                    case 1: vgp.Color = Color.Goldenrod; break;
                    case 2: vgp.Color = Color.ForestGreen; break;
                    case 3: vgp.Color = Color.Brown; break;
                    case 4: vgp.Color = Color.Red; break;
                    case 5: vgp.Color = Color.Green; break;
                    case 6: vgp.Color = Color.Red; break;
                    case 7: vgp.Color = Color.Orange; break;
                    case 8: vgp.Color = Color.Purple; break;
                    case 9: vgp.Color = Color.DarkGray; break;
                }
                vgp.UVRect = new Vector4(DUV * 1, DUV * 0, DUV, DUV);
                vd.GeoProvider = vgp;
                dVox.Add("Scenery " + i, vd);
            }
            for(int i = 0; i < COUNT_RAMP; i++) {
                var vd = state.World.Atlas.Create();
                vd.FaceType = new VoxFaceType();
                vd.FaceType.SetAllTypes(0xffffffffu);
                vd.FaceType.SetAllMasks(0xffffffffu);
                var vgp = new VGPCustom();
                Vector4 uvr = new Vector4(DUV * 3, DUV * 0, DUV, DUV);
                switch(i) {
                    case 0:
                    case 1:
                        vgp.CustomVerts[Voxel.FACE_PY] = new VertexVoxel[] {
                            new VertexVoxel(new Vector3(0, 0, 0), Vector2.UnitX, uvr, Color.White),
                            new VertexVoxel(new Vector3(1, -1, 0), Vector2.One, uvr, Color.White),
                            new VertexVoxel(new Vector3(1, -1, 1), Vector2.UnitY, uvr, Color.White),
                            new VertexVoxel(new Vector3(0, 0, 1), Vector2.Zero, uvr, Color.White),
                            new VertexVoxel(new Vector3(0, -1, 0), Vector2.UnitX, uvr, Color.White),
                            new VertexVoxel(new Vector3(0, -1, 1), Vector2.Zero, uvr, Color.White)
                        };
                        vgp.CustomInds[Voxel.FACE_PY] = new int[] {
                            0, 1, 3, 3, 1, 2,
                            3, 2, 5, 1, 0, 4,
                            0, 3, 4, 4, 3, 5
                        };
                        break;
                    default:
                        vgp.CustomVerts[Voxel.FACE_PY] = new VertexVoxel[] {
                            new VertexVoxel(new Vector3(1, 0, 0), Vector2.UnitX, uvr, Color.White),
                            new VertexVoxel(new Vector3(1, -1, 1), Vector2.One, uvr, Color.White),
                            new VertexVoxel(new Vector3(0, -1, 1), Vector2.UnitY, uvr, Color.White),
                            new VertexVoxel(new Vector3(0, 0, 0), Vector2.Zero, uvr, Color.White),
                            new VertexVoxel(new Vector3(1, -1, 0), Vector2.UnitX, uvr, Color.White),
                            new VertexVoxel(new Vector3(0, -1, 0), Vector2.Zero, uvr, Color.White)
                        };
                        vgp.CustomInds[Voxel.FACE_PY] = new int[] {
                            0, 1, 3, 3, 1, 2,
                            3, 2, 5, 1, 0, 4,
                            0, 3, 4, 4, 3, 5
                        };
                        break;
                }
                if((i % 2) == 1) {
                    if(i == 1) for(int vi = 0; vi < 6; vi++)
                            vgp.CustomVerts[Voxel.FACE_PY][vi].Position.X = 1 - vgp.CustomVerts[Voxel.FACE_PY][vi].Position.X;
                    else for(int vi = 0; vi < 6; vi++)
                            vgp.CustomVerts[Voxel.FACE_PY][vi].Position.Z = 1 - vgp.CustomVerts[Voxel.FACE_PY][vi].Position.Z;
                    for(int ti = 0; ti < vgp.CustomInds[Voxel.FACE_PY].Length; ) {
                        int buf = vgp.CustomInds[Voxel.FACE_PY][ti + 2];
                        vgp.CustomInds[Voxel.FACE_PY][ti + 2] = vgp.CustomInds[Voxel.FACE_PY][ti];
                        vgp.CustomInds[Voxel.FACE_PY][ti] = buf;
                        ti += 3;
                    }
                }
                vd.GeoProvider = vgp;
                dVox.Add("Ramp " + i, vd);
            }
            for(int i = 0; i < REGION_COLORS.Length; i++) {
                var vd = state.World.Atlas.Create();
                vd.FaceType = new VoxFaceType();
                vd.FaceType.SetAllTypes(0x00000001u);
                vd.FaceType.SetAllMasks(0xfffffffeu);
                var vgp = new VGPCube();
                vgp.Color = REGION_COLORS[i];
                vgp.UVRect = new Vector4(DUV * 2, DUV * 0, DUV, DUV);
                vd.GeoProvider = vgp;
                dVox.Add("Region " + i, vd);
            }
            // Load Custom Voxels
            foreach(var gtc in gtcList) {
                var l = gtc.CreateVoxels(state.World.Atlas);
                if(l == null) continue;
                foreach(var lv in l) {
                    dVox.Add(lv.Name, lv.VData);
                }
            }
            foreach(var ic in icList) {
                var l = ic.CreateVoxels(state.World.Atlas);
                if(l == null) continue;
                foreach(var lv in l) {
                    dVox.Add(lv.Name, lv.VData);
                }
            }
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
            if(DevConsole.IsActivated) DevConsole.Toggle(OnDC);
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
                if(DevConsole.IsActivated) {
                    game.DrawDevConsole();
                }
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
                case DevConsole.ACTIVATION_KEY:
                    DevConsole.Toggle(OnDC);
                    break;
                case Keys.P:
                    if(DevConsole.IsActivated) return;
                    State = ScreenState.ChangePrevious;
                    break;
            }
        }

        private void Write(string dir, int w, int h) {
            DirectoryInfo dirInfo = new DirectoryInfo(dir);
            if(!dirInfo.Exists) dirInfo.Create();

            WriteHeights(dirInfo.FullName + @"\height.hmd", w, h);
            WriteWorld(dirInfo.FullName + @"\vox.world", w, h);
            WriteRegions(dirInfo.FullName + @"\regions.png", w, h);
            foreach(var gtc in gtcList) {
                gtc.LESave(state.World, w, h, dirInfo);
            }
            foreach(var ic in icList) {
                ic.LESave(state.World, w, h, dirInfo);
            }
        }
        private void WriteHeights(string file, int w, int h) {
            byte[] hd = new byte[w * h * 17 + 8];
            int i = 0;
            BitConverter.GetBytes(w).CopyTo(hd, i); i += 4;
            BitConverter.GetBytes(h).CopyTo(hd, i); i += 4;
            Vector3I loc = Vector3I.Zero;
            RTSEngine.Data.HeightTile ht = new RTSEngine.Data.HeightTile();
            for(loc.Z = 0; loc.Z < h; loc.Z++) {
                for(loc.X = 0; loc.X < w; loc.X++) {
                    loc.Y = Region.HEIGHT - 1;
                    VoxLocation vl = new VoxLocation(loc);

                    // Get Height Information
                    ht = new RTSEngine.Data.HeightTile();
                    Region r = state.World.regions[vl.RegionIndex];
                    for(; vl.VoxelLoc.Y > 0; vl.VoxelLoc.Y--) {
                        ushort id = r.voxels[vl.VoxelIndex].ID;
                        int terr = id - MINID_TERRAIN;
                        int ramp = id - MINID_RAMP;
                        if(terr >= 0 && terr < COUNT_TERRAIN) {
                            ht.XNZN = vl.VoxelLoc.Y + 1;
                            ht.XPZN = vl.VoxelLoc.Y + 1;
                            ht.XNZP = vl.VoxelLoc.Y + 1;
                            ht.XPZP = vl.VoxelLoc.Y + 1;
                            break;
                        }
                        else if(ramp >= 0 && ramp < COUNT_RAMP) {
                            switch(ramp) {
                                case 0:
                                    ht.XNZN = vl.VoxelLoc.Y + 1;
                                    ht.XPZN = vl.VoxelLoc.Y + 0;
                                    ht.XNZP = vl.VoxelLoc.Y + 1;
                                    ht.XPZP = vl.VoxelLoc.Y + 0;
                                    break;
                                case 1:
                                    ht.XNZN = vl.VoxelLoc.Y + 0;
                                    ht.XPZN = vl.VoxelLoc.Y + 1;
                                    ht.XNZP = vl.VoxelLoc.Y + 0;
                                    ht.XPZP = vl.VoxelLoc.Y + 1;
                                    break;
                                case 2:
                                    ht.XNZN = vl.VoxelLoc.Y + 1;
                                    ht.XPZN = vl.VoxelLoc.Y + 1;
                                    ht.XNZP = vl.VoxelLoc.Y + 0;
                                    ht.XPZP = vl.VoxelLoc.Y + 0;
                                    break;
                                case 3:
                                    ht.XNZN = vl.VoxelLoc.Y + 0;
                                    ht.XPZN = vl.VoxelLoc.Y + 0;
                                    ht.XNZP = vl.VoxelLoc.Y + 1;
                                    ht.XPZP = vl.VoxelLoc.Y + 1;
                                    break;
                            }
                            break;
                        }
                    }
                    BitConverter.GetBytes(ht.XNZN).CopyTo(hd, i); i += 4;
                    BitConverter.GetBytes(ht.XPZN).CopyTo(hd, i); i += 4;
                    BitConverter.GetBytes(ht.XNZP).CopyTo(hd, i); i += 4;
                    BitConverter.GetBytes(ht.XPZP).CopyTo(hd, i); i += 4;

                    // Get Wall Information
                    byte walls = 0x00;
                    if(loc.X == 0) walls |= CGrid.Direction.XN;
                    if(loc.X == w - 1) walls |= CGrid.Direction.XP;
                    if(loc.Z == 0) walls |= CGrid.Direction.ZN;
                    if(loc.Z == h - 1) walls |= CGrid.Direction.ZP;
                    if(loc.X == 0 && loc.Z == 0) walls |= CGrid.Direction.XNZN;
                    if(loc.X == 0 && loc.Z == h - 1) walls |= CGrid.Direction.XNZP;
                    if(loc.X == w - 1 && loc.Z == 0) walls |= CGrid.Direction.XPZN;
                    if(loc.X == w - 1 && loc.Z == h - 1) walls |= CGrid.Direction.XPZP;

                    hd[i++] = walls;
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
        private void WriteWorld(string file, int w, int h) {
            List<byte> data = new List<byte>(w * h * 4 + 8);
            int i = 0;
            data.AddRange(BitConverter.GetBytes(w));// .CopyTo(data, i); i += 4;
            data.AddRange(BitConverter.GetBytes(h));// .CopyTo(data, i); i += 4;
            Vector3I loc = Vector3I.Zero;
            for(loc.Z = 0; loc.Z < h; loc.Z++) {
                for(loc.X = 0; loc.X < w; loc.X++) {
                    loc.Y = Region.HEIGHT - 1;
                    VoxLocation vl = new VoxLocation(loc);
                    Region r = state.World.regions[vl.RegionIndex];
                    ushort id = 0;
                    int terr = -1, scen = -1, ramp = -1;
                    // Write Scenery
                    for(; vl.VoxelLoc.Y > 0; vl.VoxelLoc.Y--) {
                        id = r.voxels[vl.VoxelIndex].ID;
                        terr = id - MINID_TERRAIN;
                        scen = id - MINID_SCENERY;
                        ramp = id - MINID_RAMP;
                        if(terr >= 0 && terr < COUNT_TERRAIN) break;
                        else if(ramp >= 0 && ramp < COUNT_RAMP) break;
                        else if(scen >= 0 && ramp < COUNT_SCENERY) {
                            data.AddRange(BitConverter.GetBytes(scen));// .CopyTo(data, i); i += 4;
                        }
                    }
                    data.AddRange(BitConverter.GetBytes(-1));// .CopyTo(data, i); i += 4;

                    // Write Surface
                    data.AddRange(BitConverter.GetBytes(vl.VoxelLoc.Y));// .CopyTo(data, i); i += 4;
                    if(terr >= 0 && terr < COUNT_TERRAIN) {
                        data.AddRange(BitConverter.GetBytes(0));// .CopyTo(data, i); i += 4;
                        data.AddRange(BitConverter.GetBytes(terr));// .CopyTo(data, i); i += 4;
                    }
                    else if(ramp >= 0 && ramp < COUNT_RAMP) {
                        data.AddRange(BitConverter.GetBytes(1));// .CopyTo(data, i); i += 4;
                        data.AddRange(BitConverter.GetBytes(ramp));// .CopyTo(data, i); i += 4;
                    }
                    else {
                        data.AddRange(BitConverter.GetBytes(-1));// .CopyTo(data, i); i += 4;
                    }
                }
            }

            using(MemoryStream ms = new MemoryStream()) {
                var gs = new GZipStream(ms, CompressionMode.Compress, true);
                gs.Write(data.ToArray(), 0, data.Count);
                gs.Close();
                ms.Position = 0;
                using(var s = File.Create(file)) {
                    var bw = new BinaryWriter(s);
                    bw.Write(data.Count);
                    bw.Flush();
                    ms.CopyTo(s);
                    s.Flush();
                }
            }
        }
        private void WriteRegions(string file, int w, int h) {
            w /= 2; h /= 2;
            byte[] data = new byte[w * h * 4];
            Vector3I loc = Vector3I.Zero;
            int i = 0, t;
            for(loc.Z = 0; loc.Z < h * 2; loc.Z += 2) {
                for(loc.X = 0; loc.X < w * 2; loc.X += 2) {
                    t = 0;
                    loc.Y = LETPaint.HEIGHT;
                    VoxLocation vl = new VoxLocation(loc);
                    Region r = state.World.regions[vl.RegionIndex];
                    t = r.voxels[vl.VoxelIndex].ID - MINID_REGION;
                    if(t < 0 || t >= REGION_COLORS.Length) t = 0;
                    data[i++] = REGION_COLORS[t].B;
                    data[i++] = REGION_COLORS[t].G;
                    data[i++] = REGION_COLORS[t].R;
                    data[i++] = REGION_COLORS[t].A;
                }
            }

            // Save The Image
            using(var bmp = new System.Drawing.Bitmap(w, h, System.Drawing.Imaging.PixelFormat.Format32bppArgb)) {
                System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.WriteOnly, bmp.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(data, 0, bmpData.Scan0, data.Length);
                bmp.UnlockBits(bmpData);
                bmp.Save(file, System.Drawing.Imaging.ImageFormat.Png);
            }
        }
        private void OnDC(string comm) {
            Match m = rgxSave.Match(comm);
            if(m.Success) {
                int[] buf = RegexHelper.ExtractVec2I(m);
                Write(@"LevelEditor\Saved", buf[0], buf[1]);
            }
        }
    }
}