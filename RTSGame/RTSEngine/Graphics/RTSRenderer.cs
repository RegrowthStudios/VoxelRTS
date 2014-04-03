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

namespace RTSEngine.Graphics {
    public struct VisualTeam {
        public int TeamIndex;
        public RTSColorScheme ColorScheme;
        public RTSRaceData RaceFileInfo;
    }

    public class RTSRenderer : IDisposable {
        // Really Should Not Be Holding This Though
        private GameWindow window;
        private GraphicsDeviceManager gManager;
        private GraphicsDevice G {
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

        // Map To Render
        public HeightmapModel Map {
            get;
            set;
        }

        // All The Unit Models To Render
        public List<RTSUnitModel> NonFriendlyUnitModels {
            get;
            private set;
        }
        public List<RTSUnitModel> FriendlyUnitModels {
            get;
            private set;
        }

        // Effects
        private BasicEffect fxSelection;
        private RTSFXEntity fxAnim;
        private RTSFXMap fxMap;

        // The Friendly Team To Be Visualizing
        private int teamIndex;

        // Particle Effects
        private ParticleRenderer pRenderer;

        // Graphics Data To Dispose
        private readonly ConcurrentBag<IDisposable> toDispose;

        public RTSRenderer(GraphicsDeviceManager gdm, string fxAnimFile, string fxMapFile, GameWindow w) {
            window = w;
            gManager = gdm;
            toDispose = new ConcurrentBag<IDisposable>();

            NonFriendlyUnitModels = new List<RTSUnitModel>();
            FriendlyUnitModels = new List<RTSUnitModel>();

            tPixel = CreateTexture2D(1, 1);
            tPixel.SetData(new Color[] { Color.White });

            fxMap = new RTSFXMap(LoadEffect(fxMapFile));

            fxSelection = CreateEffect();
            fxSelection.LightingEnabled = false;
            fxSelection.FogEnabled = false;
            fxSelection.TextureEnabled = false;
            fxSelection.VertexColorEnabled = true;
            fxSelection.World = Matrix.Identity;
            fxSelection.Texture = tPixel;

            fxAnim = new RTSFXEntity(LoadEffect(fxAnimFile));
            fxAnim.World = Matrix.Identity;
            fxAnim.CPrimary = Vector3.UnitX;
            fxAnim.CSecondary = Vector3.UnitY;
            fxAnim.CTertiary = Vector3.UnitZ;

            drawBox = false;
            MouseEventDispatcher.OnMousePress += OnMousePress;
            MouseEventDispatcher.OnMouseRelease += OnMouseRelease;
            MouseEventDispatcher.OnMouseMotion += OnMouseMove;

            pRenderer = new ParticleRenderer();
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

        public void HookToGame(GameState state, int ti, Camera camera, FileInfo mapFile) {
            // Get The Camera
            Camera = camera;

            // Create The Map
            teamIndex = ti;
            Heightmap map = state.Map;
            Map = HeightmapParser.ParseModel(this, new Vector3(map.Width, map.ScaleY, map.Depth), state.CGrid.numCells.X, state.CGrid.numCells.Y, mapFile);
            Camera.MoveTo(map.Width * 0.5f, map.Depth * 0.5f);
            fxMap.MapSize = new Vector2(map.Width, map.Depth);

            // Hook FOW
            state.CGrid.OnFOWChange += OnFOWChange;
        }
        public void LoadTeamVisuals(GameState state, VisualTeam vt) {
            RTSTeam team = state.teams[vt.TeamIndex];
            team.ColorScheme = vt.ColorScheme;
            RTSRaceData res = vt.RaceFileInfo;
            for(int ui = 0; ui < team.race.activeUnits.Length; ui++) {
                RTSUnitModel uModel = RTSUnitDataParser.ParseModel(this, team.race.activeUnits[ui].Data, res.UnitTypes[ui]);
                uModel.ColorPrimary = team.ColorScheme.Primary;
                uModel.ColorSecondary = team.ColorScheme.Secondary;
                uModel.ColorTertiary = team.ColorScheme.Tertiary;
                team.OnUnitSpawn += uModel.OnUnitSpawn;
                NonFriendlyUnitModels.Add(uModel);
            }
        }
        private void OnFOWChange(int x, int y, int p, FogOfWar f) {
            if(p != teamIndex) return;
            switch(f) {
                case FogOfWar.All:
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

        public void UpdateVisible(CollisionGrid cg) {
            // All Team Friendly Units Are Visible
            BoundingFrustum frustum = new BoundingFrustum(Camera.View * Camera.Projection);

            Predicate<RTSUnit> fFV = (u) => {
                return frustum.Intersects(u.BBox);
            };
            foreach(var um in FriendlyUnitModels)
                um.UpdateInstances(G, fFV);

            Predicate<RTSUnit> fNFV = (u) => {
                Point up = HashHelper.Hash(u.GridPosition, cg.numCells, cg.size);
                if(cg.GetFogOfWar(up.X, up.Y, teamIndex) != FogOfWar.Active)
                    return false;
                return frustum.Intersects(u.BBox);
            };
            foreach(var um in NonFriendlyUnitModels)
                um.UpdateInstances(G, fNFV);
        }
        public void UpdateAnimations(GameState s, float dt) {
            var np = new List<Particle>();
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                RTSTeam team = s.activeTeams[ti].Team;
                for(int i = 0; i < team.units.Count; i++) {
                    if(team.units[i].AnimationController != null) {
                        team.units[i].AnimationController.Update(s, dt);
                        if(team.units[i].AnimationController.HasParticles) {
                            team.units[i].AnimationController.GetParticles(np);
                        }
                    }
                }
            }
            pRenderer.Update(np, dt);
        }

        // Rendering Passes
        public void Draw(GameState s, float dt) {
            G.Clear(Color.Black);
            
            DrawMap();
            // TODO: Draw Static
            UpdateVisible(s.CGrid);
            DrawAnimated();
            if(drawBox) DrawSelectionBox();
        }
        private void DrawMap() {
            if(Map.Reset) Map.ApplyFOW();

            // Set States
            G.DepthStencilState = DepthStencilState.Default;
            G.RasterizerState = RasterizerState.CullCounterClockwise;
            G.BlendState = BlendState.Opaque;
            G.SamplerStates[0] = SamplerState.LinearClamp;

            // Set Camera
            fxMap.VP = Camera.View * Camera.Projection;

            // Primary Map Model
            if(Map.TrianglesPrimary > 0) {
                fxMap.SetTextures(G, Map.PrimaryTexture, Map.FogOfWarTexture);
                G.SetVertexBuffer(Map.VBPrimary);
                G.Indices = Map.IBPrimary;
                fxMap.ApplyPassPrimary();
                G.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Map.VBPrimary.VertexCount, 0, Map.TrianglesPrimary);
            }
            // Secondary Map Model
            if(Map.TrianglesSecondary > 0) {
                fxMap.SetTextures(G, Map.SecondaryTexture, Map.FogOfWarTexture);
                G.SetVertexBuffer(Map.VBSecondary);
                G.Indices = Map.IBSecondary;
                fxMap.ApplyPassSecondary();
                G.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Map.VBSecondary.VertexCount, 0, Map.TrianglesSecondary);
            }
        }
        private void DrawAnimated() {
            // Set Camera
            fxAnim.VP = Camera.View * Camera.Projection;

            // Loop Through Models
            G.VertexSamplerStates[0] = SamplerState.PointClamp;
            G.SamplerStates[1] = SamplerState.LinearClamp;
            G.SamplerStates[2] = SamplerState.LinearClamp;
            foreach(RTSUnitModel unitModel in NonFriendlyUnitModels) {
                fxAnim.SetTextures(G, unitModel.AnimationTexture, unitModel.ModelTexture, unitModel.ColorCodeTexture);
                fxAnim.CPrimary = unitModel.ColorPrimary;
                fxAnim.CSecondary = unitModel.ColorSecondary;
                fxAnim.CTertiary = unitModel.ColorTertiary;
                fxAnim.ApplyPassAnimation();
                unitModel.SetInstances(G);
                unitModel.DrawInstances(G);
            }

            // Cause XNA Is Retarded Like That
            G.VertexTextures[0] = null;
            G.VertexSamplerStates[0] = SamplerState.LinearClamp;
        }
        private void DrawSelectionBox() {
            Vector2 ss = new Vector2(G.Viewport.TitleSafeArea.Width, G.Viewport.TitleSafeArea.Height);
            fxSelection.View = Matrix.CreateLookAt(new Vector3(ss / 2, -1), new Vector3(ss / 2, 0), Vector3.Down);
            fxSelection.Projection = Matrix.CreateOrthographic(ss.X, ss.Y, 0, 2);

            G.DepthStencilState = DepthStencilState.None;
            G.BlendState = BlendState.NonPremultiplied;
            G.RasterizerState = RasterizerState.CullNone;

            Vector3 min = new Vector3(Vector2.Min(start, end), 0);
            Vector3 max = new Vector3(Vector2.Max(start, end), 0);
            fxSelection.CurrentTechnique.Passes[0].Apply();
            G.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionColor[] {
                    new VertexPositionColor(min, new Color(0f, 0, 1f, 0.3f)),
                    new VertexPositionColor(new Vector3(max.X, min.Y, 0), new Color(1f, 0, 1f, 0.3f)),
                    new VertexPositionColor(new Vector3(min.X, max.Y, 0), new Color(1f, 0, 1f, 0.3f)),
                    new VertexPositionColor(max, new Color(1f, 0, 0f, 0.3f)),
                }, 0, 2, VertexPositionColor.VertexDeclaration);
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