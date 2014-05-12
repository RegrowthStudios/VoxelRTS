using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI.Input;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Data.Parsers;
using RTSEngine.Interfaces;
using Grey.Vox;
using Grey.Graphics;

namespace RTSEngine.Graphics {
    public class RendererInitArgs {
        public string CamPointerTexture;
        public Vector3 CamPointerRadii;
        public Vector3 CamPointerHeights;

        public string FXEntity;
        public string FXMap;
        public string FXParticle;
    }
    public class RTSRenderer : IDisposable {
        private const float SELECTION_RADIUS_MODIFIER = 1.1f;
        private const float SELECTION_HEIGHT_PLACEMENT = 0.05f;
        private static readonly Color HEALTH_FULL_COLOR = new Color(0.07f, 0.3f, 0.8f, 1f);
        private static readonly Color HEALTH_EMPTY_COLOR = new Color(0.97f, 0.1f, 0f, 1f);

        // Really Should Not Be Holding This Though
        private GameWindow window;
        public GameWindow Window {
            get { return window; }
        }
        private GraphicsDeviceManager gManager;
        public GraphicsDevice G {
            get { return gManager.GraphicsDevice; }
        }

        // Selection Box
        private Texture2D tPixel;
        private bool drawBox;
        private Vector2 start, end;

        // The Camera
        public Camera Camera {
            get;
            set;
        }
        public CameraPointer CamPointer {
            get;
            private set;
        }

        // Map To Render
        public VoxMap Map {
            get;
            set;
        }
        public Minimap Minimap {
            get;
            private set;
        }

        // All The Models To Render
        public List<RTSUnitModel> NonFriendlyUnitModels {
            get;
            private set;
        }
        public List<RTSUnitModel> FriendlyUnitModels {
            get;
            private set;
        }
        public List<RTSBuildingModel> NonFriendlyBuildingModels {
            get;
            private set;
        }
        public List<RTSBuildingModel> FriendlyBuildingModels {
            get;
            private set;
        }

        // Effects
        private BasicEffect fxSimple;
        private RTSFXEntity fxAnim;
        private RTSFXMap fxMap;
        private Effect fxParticle;

        // The Friendly Team To Be Visualizing
        private int teamIndex;
        private ACInputController teamInput;
        public Texture2D SelectionCircleTexture {
            get;
            set;
        }

        // Whether To Draw FOW
        public bool UseFOW {
            get;
            set;
        }
        public Texture2D FOWTexture {
            get { return UseFOW ? Map.FogOfWarTexture : tPixel; }
        }

        // Particle Effects
        private ParticleRenderer pRenderer;

        // Icons
        public Dictionary<string, Texture2D> IconLibrary {
            get;
            private set;
        }

        // Graphics Data To Dispose
        private readonly ConcurrentBag<IDisposable> toDispose;

        public RTSRenderer(GraphicsDeviceManager gdm, RendererInitArgs ria, GameWindow w) {
            window = w;
            gManager = gdm;
            toDispose = new ConcurrentBag<IDisposable>();

            NonFriendlyUnitModels = new List<RTSUnitModel>();
            FriendlyUnitModels = new List<RTSUnitModel>();
            NonFriendlyBuildingModels = new List<RTSBuildingModel>();
            FriendlyBuildingModels = new List<RTSBuildingModel>();
            IconLibrary = new Dictionary<string, Texture2D>();

            tPixel = CreateTexture2D(1, 1);
            tPixel.SetData(new Color[] { Color.White });
            IconLibrary.Add("None", tPixel);

            fxMap = new RTSFXMap(LoadEffect(ria.FXMap));

            fxSimple = CreateEffect();
            fxSimple.LightingEnabled = false;
            fxSimple.FogEnabled = false;
            fxSimple.TextureEnabled = false;
            fxSimple.VertexColorEnabled = true;
            fxSimple.World = Matrix.Identity;
            fxSimple.Texture = tPixel;

            fxAnim = new RTSFXEntity(LoadEffect(ria.FXEntity));
            fxAnim.World = Matrix.Identity;
            fxAnim.CPrimary = Vector3.UnitX;
            fxAnim.CSecondary = Vector3.UnitY;
            fxAnim.CTertiary = Vector3.UnitZ;

            fxParticle = LoadEffect(ria.FXParticle);
            UseFOW = true;

            pRenderer = new ParticleRenderer();
            Minimap = new Minimap();

            CamPointer = new CameraPointer();
            CamPointer.Build(this, ria.CamPointerTexture, ria.CamPointerRadii, ria.CamPointerHeights);

            drawBox = false;
            MouseEventDispatcher.OnMousePress += OnMousePress;
            MouseEventDispatcher.OnMouseRelease += OnMouseRelease;
            MouseEventDispatcher.OnMouseMotion += OnMouseMove;
        }
        public void Dispose() {
            MouseEventDispatcher.OnMousePress -= OnMousePress;
            MouseEventDispatcher.OnMouseRelease -= OnMouseRelease;
            MouseEventDispatcher.OnMouseMotion -= OnMouseMove;

            // Dispose All In The List
            IDisposable[] td = new IDisposable[toDispose.Count];
            toDispose.CopyTo(td, 0);
            for(int i = 0; i < td.Length; i++) {
                td[i].Dispose();
                td[i] = null;
            }
        }

        #region Graphics Data Creation That Will Be Ready For Disposal At The End Of The Game
        public VertexBuffer CreateVertexBuffer(VertexDeclaration vd, int s, BufferUsage usage = BufferUsage.WriteOnly) {
            VertexBuffer vb = new VertexBuffer(G, vd, s, usage);
            toDispose.Add(vb);
            return vb;
        }
        public DynamicVertexBuffer CreateDynamicVertexBuffer(VertexDeclaration vd, int s, BufferUsage usage = BufferUsage.WriteOnly) {
            DynamicVertexBuffer vb = new DynamicVertexBuffer(G, vd, s, usage);
            toDispose.Add(vb);
            return vb;
        }
        public IndexBuffer CreateIndexBuffer(IndexElementSize id, int s, BufferUsage usage = BufferUsage.WriteOnly) {
            IndexBuffer ib = new IndexBuffer(G, id, s, usage);
            toDispose.Add(ib);
            return ib;
        }
        public Texture2D CreateTexture2D(int w, int h, SurfaceFormat format = SurfaceFormat.Color, bool mipmap = false) {
            Texture2D t = new Texture2D(G, w, h, mipmap, format);
            toDispose.Add(t);
            return t;
        }
        public Texture2D LoadTexture2D(Stream s) {
            Texture2D t = Texture2D.FromStream(G, s);
            toDispose.Add(t);
            return t;
        }
        public Texture2D LoadTexture2D(string file) {
            Texture2D t;
            using(FileStream fs = File.OpenRead(file)) {
                t = LoadTexture2D(fs);
            }
            return t;
        }
        public RenderTarget2D CreateRenderTarget2D(int w, int h, SurfaceFormat sf = SurfaceFormat.Color, DepthFormat df = DepthFormat.Depth24Stencil8, RenderTargetUsage usage = RenderTargetUsage.DiscardContents, int msc = 1, bool mipmap = false) {
            RenderTarget2D t = new RenderTarget2D(G, w, h, mipmap, sf, df, msc, usage);
            toDispose.Add(t);
            return t;
        }
        public Effect LoadEffect(string file) {
            Effect fx = XNAEffect.Compile(G, file);
            toDispose.Add(fx);
            return fx;
        }
        public BasicEffect CreateEffect() {
            BasicEffect fx = new BasicEffect(G);
            toDispose.Add(fx);
            return fx;
        }
        public SpriteFont LoadFont(string file) {
            IDisposable disp;
            SpriteFont f = XNASpriteFont.Compile(G, file, out disp);
            toDispose.Add(disp);
            return f;
        }
        public SpriteFont LoadFont(string file, out IDisposable disp) {
            SpriteFont f = XNASpriteFont.Compile(G, file, out disp);
            toDispose.Add(disp);
            return f;
        }
        public SpriteFont CreateFont(string fontName, int size, int spacing = 0, bool useKerning = true, string style = "Regular", char defaultChar = '*', int cStart = 32, int cEnd = 126) {
            IDisposable disp;
            SpriteFont f = XNASpriteFont.Compile(G, fontName, size, out disp, spacing, useKerning, style, defaultChar, cStart, cEnd);
            toDispose.Add(disp);
            return f;
        }
        public SpriteFont CreateFont(string fontName, int size, out IDisposable disp, int spacing = 0, bool useKerning = true, string style = "Regular", char defaultChar = '*', int cStart = 32, int cEnd = 126) {
            SpriteFont f = XNASpriteFont.Compile(G, fontName, size, out disp, spacing, useKerning, style, defaultChar, cStart, cEnd);
            toDispose.Add(disp);
            return f;
        }
        #endregion

        public void HookToGame(GameState state, int ti, Camera camera) {
            // Get The Team To Be Visualized
            teamIndex = ti;
            teamInput = state.teams[teamIndex].Input;
            SelectionCircleTexture = LoadTexture2D(@"Content\Textures\SelectionCircle.png");

            // Get The Camera
            Camera = camera;

            // Create The Map
            CreateVoxGeos(state.VoxState.World.Atlas);
            //Heightmap map = state.Map;
            Map = new VoxMap(this, state.CGrid.numCells.X, state.CGrid.numCells.Y);
            // TODO: Parse This In
            VoxMapConfig vmc = new VoxMapConfig();
            vmc.VoxState = state.VoxState;
            vmc.TexVoxMap = @"voxmap.png";
            vmc.RootPath = state.LevelGrid.Directory.FullName;
            vmc.FXFile = @"Content\FX\Voxel.fx";
            Map.Build(gManager, vmc);

            //Map = MapParser.ParseModel(this, state.LevelGrid, new FileInfo(state.LevelGrid.InfoFile));
            Camera.MoveTo(state.CGrid.size.X * 0.5f, state.CGrid.size.Y * 0.5f);
            fxMap.MapSize = state.CGrid.size;
            fxParticle.Parameters["MapSize"].SetValue(state.CGrid.size);

            // Hook FOW
            state.CGrid.OnFOWChange += OnFOWChange;
            Minimap.Hook(this, state, ti);


            // Load Particles
            // TODO: Config
            ParticleOptions pOpt = ZXParser.ParseFile(@"Content\FX\Particles\Particle.conf", typeof(ParticleOptions)) as ParticleOptions;
            pRenderer.Load(this, pOpt);

            // Load Team Visuals
            for(int i = 0; i < state.teams.Length; i++) {
                if(state.teams[i] == null) continue;
                LoadTeamVisuals(state, i);
            }

            // Set FOW
            for(int y = 0; y < Map.FogOfWarTexture.Height; y++) {
                for(int x = 0; x < Map.FogOfWarTexture.Width; x++) {
                    switch(state.CGrid.GetFogOfWar(x, y, teamIndex)) {
                        case FogOfWar.Active:
                            Map.SetFOW(x, y, 1f);
                            break;
                        case FogOfWar.Passive:
                            Map.SetFOW(x, y, 0.5f);
                            break;
                        case FogOfWar.Nothing:
                            Map.SetFOW(x, y, 0f);
                            break;
                    }
                }
            }
        }
        public void CreateVoxGeos(VoxAtlas atlas) {
            float DUV = 0.125f;
            for(int i = 1; i < 6; i++) {
                var vgpTop = new VGPCube6();
                var vgpTrans = new VGPCube6();
                var vgpCliff = new VGPCube6();
                for(int fi = 0; fi < 6; fi++) {
                    vgpTop.Colors[fi] = Color.White;
                    switch(fi) {
                        case Voxel.FACE_NY:
                            vgpTop.UVRects[fi] = new Vector4(DUV * i, DUV * 4, DUV, DUV);
                            vgpTrans.UVRects[fi] = new Vector4(DUV * i, DUV * 4, DUV, DUV);
                            vgpCliff.UVRects[fi] = new Vector4(DUV * i, DUV * 4, DUV, DUV);
                            break;
                        case Voxel.FACE_PY:
                            vgpTop.UVRects[fi] = new Vector4(DUV * i, DUV * 0, DUV, DUV);
                            vgpTrans.UVRects[fi] = new Vector4(DUV * i, DUV * 4, DUV, DUV);
                            vgpCliff.UVRects[fi] = new Vector4(DUV * i, DUV * 4, DUV, DUV);
                            break;
                        default:
                            vgpTop.UVRects[fi] = new Vector4(DUV * i, DUV * 1, DUV, DUV);
                            vgpTrans.UVRects[fi] = new Vector4(DUV * i, DUV * 2, DUV, DUV);
                            vgpCliff.UVRects[fi] = new Vector4(DUV * i, DUV * 3, DUV, DUV);
                            break;
                    }
                }
                atlas[(ushort)(i)].GeoProvider = vgpTop;
                atlas[(ushort)(i + 5)].GeoProvider = vgpTrans;
                atlas[(ushort)(i + 10)].GeoProvider = vgpCliff;
            }
            for(int vi = 0; vi < 4; vi++) {
                var vd = atlas[(ushort)(vi + 16)];
                var vgp = new VGPCustom();
                Vector4 uvr = new Vector4(DUV * 3, DUV * 0, DUV, DUV);
                switch(vi) {
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
                if((vi % 2) == 1) {
                    if(vi == 1) for(int fi = 0; fi < 6; fi++)
                            vgp.CustomVerts[Voxel.FACE_PY][fi].Position.X = 1 - vgp.CustomVerts[Voxel.FACE_PY][fi].Position.X;
                    else for(int fi = 0; fi < 6; fi++)
                            vgp.CustomVerts[Voxel.FACE_PY][fi].Position.Z = 1 - vgp.CustomVerts[Voxel.FACE_PY][fi].Position.Z;
                    for(int ti = 0; ti < vgp.CustomInds[Voxel.FACE_PY].Length; ) {
                        int buf = vgp.CustomInds[Voxel.FACE_PY][ti + 2];
                        vgp.CustomInds[Voxel.FACE_PY][ti + 2] = vgp.CustomInds[Voxel.FACE_PY][ti];
                        vgp.CustomInds[Voxel.FACE_PY][ti] = buf;
                        ti += 3;
                    }
                }
                vd.GeoProvider = vgp;
            }
        }
        private void LoadTeamVisuals(GameState state, int ti) {
            RTSTeam team = state.teams[ti];

            // Create Unit Graphics
            var ums = ti == teamIndex ? FriendlyUnitModels : NonFriendlyUnitModels;
            for(int i = 0; i < team.Race.ActiveUnits.Length; i++) {
                int ui = team.Race.ActiveUnits[i].Index;
                RTSUnitData uData = team.Race.Units[ui];
                RTSUnitModel uModel = RTSUnitDataParser.ParseModel(this, new FileInfo(uData.InfoFile), team.Race);
                uModel.Hook(this, state, ti, team.Race.ActiveUnits[i].Index);
                uModel.ColorScheme = team.ColorScheme;
                ums.Add(uModel);
            }

            // Create Building Graphics
            var bms = ti == teamIndex ? FriendlyBuildingModels : NonFriendlyBuildingModels;
            for(int i = 0; i < team.Race.ActiveBuildings.Length; i++) {
                RTSBuildingModel bModel = RTSBuildingDataParser.ParseModel(this, new FileInfo(team.Race.ActiveBuildings[i].InfoFile), team.Race);
                bModel.Hook(this, state, ti, teamIndex, team.Race.ActiveBuildings[i].Index);
                bModel.ColorScheme = team.ColorScheme;
                bms.Add(bModel);
            }
        }
        private void OnFOWChange(int x, int y, int p, FogOfWar f) {
            if(p != teamIndex) return;
            switch(f) {
                case FogOfWar.Active:
                    Map.SetFOW(x, y, 1f);
                    break;
                case FogOfWar.Passive:
                    Map.SetFOW(x, y, 0.5f);
                    break;
                case FogOfWar.Nothing:
                    Map.SetFOW(x, y, 0f);
                    break;
            }
        }

        public void UpdateAnimations(GameState s, float dt) {
            var np = s.GetParticles();
            if(np == null) np = new List<Particle>();
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                RTSTeam team = s.activeTeams[ti].Team;
                for(int i = 0; i < team.Units.Count; i++) {
                    if(team.Units[i].AnimationController != null) {
                        team.Units[i].AnimationController.Update(s, dt);
                        if(team.Units[i].AnimationController.HasParticles) {
                            team.Units[i].AnimationController.GetParticles(np);
                        }
                    }
                }
            }
            pRenderer.Update(np, dt);
        }

        public void Update(GameState state) {
            if(Map.Reset) Map.ApplyFOW();
            Minimap.Refresh(this);
            Map.Update();
        }

        // Rendering Passes
        public void Draw(GameState s, float dt) {
            UpdateVisible(s.CGrid);

            G.Clear(Color.Black);

            DrawMap(Camera.View, Camera.Projection);
            DrawBuildings();
            DrawUnits();
            DrawParticles();
            DrawSelectionCircles(s.teams[teamIndex].ColorScheme.Secondary);
            if(drawBox) DrawSelectionBox();

            G.Textures[0] = null;
            G.Textures[1] = null;
            G.Textures[2] = null;
        }
        public void UpdateVisible(CollisionGrid cg) {
            // All Team Friendly Units Are Visible
            BoundingFrustum frustum = new BoundingFrustum(Camera.View * Camera.Projection);

            // Update Units
            Predicate<RTSUnit> fFVU = (u) => {
                return frustum.Intersects(u.BBox);
            };
            foreach(var um in FriendlyUnitModels)
                um.UpdateInstances(G, GameplayController.IsUnitDead, fFVU);

            Predicate<RTSUnit> fNFVU = (u) => {
                Point up = HashHelper.Hash(u.GridPosition, cg.numCells, cg.size);
                if(cg.GetFogOfWar(up.X, up.Y, teamIndex) != FogOfWar.Active)
                    return false;
                return frustum.Intersects(u.BBox);
            };
            foreach(var um in NonFriendlyUnitModels)
                um.UpdateInstances(G, GameplayController.IsUnitDead, UseFOW ? fNFVU : fFVU);

            // Update Buildings
            Predicate<BoundingBox> fFVB = (b) => {
                return frustum.Intersects(b);
            };
            foreach(var bm in FriendlyBuildingModels)
                bm.UpdateInstances(G, fFVB);
            foreach(var bm in NonFriendlyBuildingModels)
                bm.UpdateInstances(G, fFVB);
        }

        // Draw The Map
        public void DrawMap(Matrix mV, Matrix mP) {
            // Set States
            G.DepthStencilState = DepthStencilState.Default;
            G.RasterizerState = RasterizerState.CullCounterClockwise;
            G.BlendState = BlendState.Opaque;
            G.SamplerStates[0] = SamplerState.LinearClamp;

            // Primary Map Model
            G.Textures[1] = Map.FogOfWarTexture;
            G.SamplerStates[1] = SamplerState.PointClamp;
            Map.Draw(G, mV, mP);
        }

        // Draw Buildings
        private void DrawBuildings() {
            // Set Camera
            fxAnim.VP = Camera.View * Camera.Projection;

            // Loop Through Models
            G.SamplerStates[1] = SamplerState.LinearClamp;
            G.SamplerStates[2] = SamplerState.LinearClamp;
            foreach(RTSBuildingModel buildingModel in NonFriendlyBuildingModels) {
                if(buildingModel.VisibleInstanceCount < 1) continue;
                fxAnim.SetTextures(G, buildingModel.ModelTexture, buildingModel.ColorCodeTexture);
                fxAnim.CPrimary = buildingModel.ColorScheme.Primary;
                fxAnim.CSecondary = buildingModel.ColorScheme.Secondary;
                fxAnim.CTertiary = buildingModel.ColorScheme.Tertiary;
                fxAnim.ApplyPassBuilding();
                buildingModel.SetInstances(G);
                buildingModel.DrawInstances(G);
            }
            foreach(RTSBuildingModel buildingModel in FriendlyBuildingModels) {
                if(buildingModel.VisibleInstanceCount < 1) continue;
                fxAnim.SetTextures(G, buildingModel.ModelTexture, buildingModel.ColorCodeTexture);
                fxAnim.CPrimary = buildingModel.ColorScheme.Primary;
                fxAnim.CSecondary = buildingModel.ColorScheme.Secondary;
                fxAnim.CTertiary = buildingModel.ColorScheme.Tertiary;
                fxAnim.ApplyPassBuilding();
                buildingModel.SetInstances(G);
                buildingModel.DrawInstances(G);
            }
        }

        // Draw Units
        private void DrawUnits() {
            // Set Camera
            fxAnim.VP = Camera.View * Camera.Projection;

            // Loop Through Models
            G.VertexSamplerStates[0] = SamplerState.PointClamp;
            G.SamplerStates[1] = SamplerState.LinearClamp;
            G.SamplerStates[2] = SamplerState.LinearClamp;
            foreach(RTSUnitModel unitModel in NonFriendlyUnitModels) {
                fxAnim.SetTextures(G, unitModel.AnimationTexture, unitModel.ModelTexture, unitModel.ColorCodeTexture);
                fxAnim.CPrimary = unitModel.ColorScheme.Primary;
                fxAnim.CSecondary = unitModel.ColorScheme.Secondary;
                fxAnim.CTertiary = unitModel.ColorScheme.Tertiary;
                fxAnim.ApplyPassUnit();
                unitModel.SetInstances(G);
                unitModel.DrawInstances(G);
            }
            foreach(RTSUnitModel unitModel in FriendlyUnitModels) {
                fxAnim.SetTextures(G, unitModel.AnimationTexture, unitModel.ModelTexture, unitModel.ColorCodeTexture);
                fxAnim.CPrimary = unitModel.ColorScheme.Primary;
                fxAnim.CSecondary = unitModel.ColorScheme.Secondary;
                fxAnim.CTertiary = unitModel.ColorScheme.Tertiary;
                fxAnim.ApplyPassUnit();
                unitModel.SetInstances(G);
                unitModel.DrawInstances(G);
            }

            // Cause XNA Is Retarded Like That
            G.VertexTextures[0] = null;
            G.VertexSamplerStates[0] = SamplerState.LinearClamp;
        }

        // Draw Selection Box
        private void DrawSelectionBox() {
            fxSimple.TextureEnabled = false;
            fxSimple.VertexColorEnabled = true;

            Vector2 ss = new Vector2(G.Viewport.TitleSafeArea.Width, G.Viewport.TitleSafeArea.Height);
            fxSimple.View = Matrix.CreateLookAt(new Vector3(ss / 2, -1), new Vector3(ss / 2, 0), Vector3.Down);
            fxSimple.Projection = Matrix.CreateOrthographic(ss.X, ss.Y, 0, 2);
            fxSimple.DiffuseColor = Vector3.One;

            G.DepthStencilState = DepthStencilState.None;
            G.BlendState = BlendState.NonPremultiplied;
            G.RasterizerState = RasterizerState.CullNone;

            Vector3 min = new Vector3(Vector2.Min(start, end), 0);
            Vector3 max = new Vector3(Vector2.Max(start, end), 0);
            fxSimple.CurrentTechnique.Passes[0].Apply();
            G.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionColor[] {
                    new VertexPositionColor(min, new Color(0f, 0, 1f, 0.3f)),
                    new VertexPositionColor(new Vector3(max.X, min.Y, 0), new Color(1f, 0, 1f, 0.3f)),
                    new VertexPositionColor(new Vector3(min.X, max.Y, 0), new Color(1f, 0, 1f, 0.3f)),
                    new VertexPositionColor(max, new Color(1f, 0, 0f, 0.3f)),
                }, 0, 2, VertexPositionColor.VertexDeclaration);
        }

        // Draw Selection Circles Under Selected Units
        private void DrawSelectionCircles(Vector3 c) {
            if(SelectionCircleTexture == null)
                return;
            VertexPositionColorTexture[] verts;
            int[] inds;
            UpdateSelections(out verts, out inds);
            if(verts == null || inds == null)
                return;

            fxSimple.TextureEnabled = true;
            fxSimple.VertexColorEnabled = false;

            G.SamplerStates[0] = SamplerState.LinearClamp;
            G.DepthStencilState = DepthStencilState.DepthRead;
            G.RasterizerState = RasterizerState.CullCounterClockwise;
            G.BlendState = BlendState.Additive;

            fxSimple.Texture = SelectionCircleTexture;
            fxSimple.World = Matrix.Identity;
            fxSimple.View = Camera.View;
            fxSimple.Projection = Camera.Projection;
            fxSimple.DiffuseColor = c;
            fxSimple.CurrentTechnique.Passes[0].Apply();

            G.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length, inds, 0, inds.Length / 3, VertexPositionColorTexture.VertexDeclaration);

            fxSimple.VertexColorEnabled = true;
            fxSimple.World = Matrix.CreateTranslation(0, (float)(DateTime.Now.TimeOfDay.TotalSeconds % 1.0), 0);
            fxSimple.DiffuseColor = Vector3.One;
            fxSimple.CurrentTechnique.Passes[0].Apply();

            G.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, verts.Length, inds, 0, inds.Length / 3, VertexPositionColorTexture.VertexDeclaration);

            // Draw The Camera On The Map
            CamPointer.Draw(G, Camera.View, Camera.Projection, Camera.CamTarget);
        }
        public void UpdateSelections(out VertexPositionColorTexture[] verts, out int[] inds) {
            // Check If We Need To Render Any Selected Entities
            if(teamInput.selected.Count < 1) {
                verts = null;
                inds = null;
                return;
            }

            // Build The Selections
            verts = new VertexPositionColorTexture[teamInput.selected.Count * 4];
            int i = 0;
            var sarr = teamInput.selected.ToArray();
            foreach(var e in sarr) {
                Vector2 c = e.GridPosition;
                float r = e.CollisionGeometry.BoundingRadius * SELECTION_RADIUS_MODIFIER;
                float h = e.Height;
                h += (e.BBox.Max.Y - e.BBox.Min.Y) * SELECTION_HEIGHT_PLACEMENT;
                float mh = 1f;
                if(e as RTSBuilding != null) mh = (e as RTSBuilding).Data.Health;
                else mh = (e as RTSUnit).Data.Health;
                Color cHealth = Color.Lerp(HEALTH_EMPTY_COLOR, HEALTH_FULL_COLOR, e.Health / mh);
                verts[i++] = new VertexPositionColorTexture(new Vector3(c.X - r, h, c.Y - r), cHealth, Vector2.Zero);
                verts[i++] = new VertexPositionColorTexture(new Vector3(c.X + r, h, c.Y - r), cHealth, Vector2.UnitX);
                verts[i++] = new VertexPositionColorTexture(new Vector3(c.X - r, h, c.Y + r), cHealth, Vector2.UnitY);
                verts[i++] = new VertexPositionColorTexture(new Vector3(c.X + r, h, c.Y + r), cHealth, Vector2.One);
            }

            inds = new int[(verts.Length * 3) / 2];
            for(int vi = 0, ii = 0; vi < verts.Length; ) {
                inds[ii++] = vi + 0;
                inds[ii++] = vi + 1;
                inds[ii++] = vi + 2;
                inds[ii++] = vi + 2;
                inds[ii++] = vi + 1;
                inds[ii++] = vi + 3;
                vi += 4;
            }
        }

        // Draw Particles
        private void DrawParticles() {
            G.DepthStencilState = DepthStencilState.DepthRead;
            G.RasterizerState = RasterizerState.CullNone;
            G.BlendState = BlendState.Additive;

            fxParticle.Parameters["VP"].SetValue(Camera.View * Camera.Projection);
            G.Textures[1] = FOWTexture;
            G.SamplerStates[1] = SamplerState.PointClamp;
            fxParticle.CurrentTechnique.Passes[0].Apply();

            pRenderer.SetBullets(G);
            pRenderer.DrawBullets(G);

            float t = (float)(DateTime.Now.TimeOfDay.TotalSeconds % 1000);

            pRenderer.SetFire(G, Camera.View * Camera.Projection, t);
            pRenderer.DrawFire(G);

            pRenderer.SetLightning(G, Camera.View * Camera.Projection, t);
            pRenderer.DrawLightning(G);
        }

        // Selection Box Handling
        private void OnMousePress(Vector2 p, MouseButton b) {
            if(b == MouseButton.Left) {
                drawBox = true;
                start = p;
            }
        }
        private void OnMouseRelease(Vector2 p, MouseButton b) {
            if(b == MouseButton.Left) {
                drawBox = false;
            }
        }
        private void OnMouseMove(Vector2 p, Vector2 d) {
            end = p;
        }
    }
}