using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Widgets;
using BlisterUI.Input;
using RTSEngine.Graphics;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Data.Parsers;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework.Input;
using RTSEngine.Controllers;

namespace RTS {
    public class ColorSchemeScreen : GameScreen<App> {
        private const string FX_FILE_PATH = @"Content\FX\RTS.fx";
        private static readonly Color BACK_COLOR = new Color(10, 10, 10, 255);

        private const float YAW_SPEED = 1f;
        private const float PITCH_SPEED = 1f;
        private const float FOV_ZOOM_SPEED = 0.5f;
        private const float DIST_ZOOM_SPEED = 7f;

        public override int Next {
            get { return -1; }
            protected set { }
        }
        public override int Previous {
            get { return game.MenuScreen.Index; }
            protected set { }
        }

        // Camera Controller
        private OrbitingCamera camera;
        private InputManager input;

        // Viewing Information
        private RTSUnitData unitData;
        private RTSUnitModel unitModel;
        private RTSTeam team;
        private RTSUnit unit;
        private RTSColorScheme colorScheme;
        private object drawLock = new object();

        // File Searched Data
        private List<FileInfo> unitDataFiles;
        private List<FileInfo> schemeFiles;
        private Thread tSearch;
        private bool searchDone;
        private int curUnit;

        // Renderer
        private GameEngine engine;
        private RTSEffect fx;
        private WidgetRenderer wr;
        private IDisposable fontDisp;

        // Widgets
        RectWidget wBackPanel;
        ColorSwatch sP, sS, sT;

        public override void Build() {
            input = new InputManager();
            wr = new WidgetRenderer(G, XNASpriteFont.Compile(G, "Arial", 16, out fontDisp));

            wBackPanel = new RectWidget(wr);
            wBackPanel.Anchor = new Point(0, 0);
            wBackPanel.Width = 200;
            wBackPanel.Height = G.Viewport.Height;
            wBackPanel.LayerDepth = 1f;
            wBackPanel.Color = new Color(14, 14, 14, 230);

            sP = new ColorSwatch(10, 30, 170, 15, 35, 5, Color.Black, 0.9f, wr);
            sS = new ColorSwatch(10, 120, 170, 15, 35, 5, Color.Black, 0.9f, wr);
            sT = new ColorSwatch(10, 210, 170, 15, 35, 5, Color.Black, 0.9f, wr);
            sP.OnColorChange += (c) => { colorScheme.Primary = c; };
            sS.OnColorChange += (c) => { colorScheme.Secondary = c; };
            sT.OnColorChange += (c) => { colorScheme.Tertiary = c; };
        }
        public override void Destroy(GameTime gameTime) {
            wBackPanel.Dispose();
            sP.Dispose();
            sS.Dispose();
            sT.Dispose();
            wr.Dispose();
            fontDisp.Dispose();
        }

        public override void OnEntry(GameTime gameTime) {
            input.Refresh();
            MouseEventDispatcher.OnMousePress += sP.OnMousePress;
            MouseEventDispatcher.OnMouseMotion += sP.OnMouseMovement;
            MouseEventDispatcher.OnMouseRelease += sP.OnMouseRelease;
            MouseEventDispatcher.OnMousePress += sS.OnMousePress;
            MouseEventDispatcher.OnMouseMotion += sS.OnMouseMovement;
            MouseEventDispatcher.OnMouseRelease += sS.OnMouseRelease;
            MouseEventDispatcher.OnMousePress += sT.OnMousePress;
            MouseEventDispatcher.OnMouseMotion += sT.OnMouseMovement;
            MouseEventDispatcher.OnMouseRelease += sT.OnMouseRelease;

            engine = new GameEngine(game.Graphics, game.Window);

            // Rendering Effect
            fx = new RTSEffect(engine.LoadEffect(FX_FILE_PATH));

            // Default Team
            team = new RTSTeam();
            sP.Color = RTSColorScheme.Default.Primary;
            sS.Color = RTSColorScheme.Default.Secondary;
            sT.Color = RTSColorScheme.Default.Tertiary;
            colorScheme.Name = RTSColorScheme.Default.Name;

            // Create Camera
            camera = new OrbitingCamera(Vector3.Zero, 4f, G.Viewport.AspectRatio);
            camera.Pitch = MathHelper.PiOver4;
            camera.Yaw = 0;
            camera.FOV = MathHelper.PiOver4;

            unitDataFiles = new List<FileInfo>();
            schemeFiles = new List<FileInfo>();

            FindAllData();
            curUnit = 0;
        }
        public override void OnExit(GameTime gameTime) {
            if(unitModel != null) DisposeUnit();
            engine.Dispose();
            camera = null;
            MouseEventDispatcher.OnMousePress -= sP.OnMousePress;
            MouseEventDispatcher.OnMouseMotion -= sP.OnMouseMovement;
            MouseEventDispatcher.OnMouseRelease -= sP.OnMouseRelease;
            MouseEventDispatcher.OnMousePress -= sS.OnMousePress;
            MouseEventDispatcher.OnMouseMotion -= sS.OnMouseMovement;
            MouseEventDispatcher.OnMouseRelease -= sS.OnMouseRelease;
            MouseEventDispatcher.OnMousePress -= sT.OnMousePress;
            MouseEventDispatcher.OnMouseMotion -= sT.OnMouseMovement;
            MouseEventDispatcher.OnMouseRelease -= sT.OnMouseRelease;
        }

        public override void Update(GameTime gameTime) {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(unit != null) unit.AnimationController.Update(null, dt);

            ControlCamera(dt);
            camera.UpdateProjection(G.Viewport.AspectRatio);
            camera.UpdateView();

            if(searchDone) {
                tSearch = null;
            }

            if(tSearch == null) {
                if(input.Keyboard.IsKeyJustPressed(Keys.N)) {
                    LoadUnit(unitDataFiles[curUnit]);
                    curUnit = (curUnit + 1) % unitDataFiles.Count;
                }
            }

            if(input.Keyboard.IsKeyJustPressed(Keys.P))
                State = ScreenState.ChangePrevious;
            if(input.Keyboard.IsKeyJustPressed(Keys.Escape))
                State = ScreenState.ExitApplication;

            input.Refresh();
        }
        private void ControlCamera(float dt) {
            Point p = input.Mouse.Displacement;
            if(input.Keyboard.Current.IsKeyDown(Keys.LeftControl)) {
                if(input.Mouse.Current.MiddleButton == ButtonState.Pressed) {
                    // Zoom By FOV
                    if(p.Y != 0) camera.FOV *= (float)Math.Pow(FOV_ZOOM_SPEED, dt * p.Y);
                }
            }
            else if(input.Keyboard.Current.IsKeyDown(Keys.LeftShift)) {
                if(input.Mouse.Current.MiddleButton == ButtonState.Pressed) {
                    // Pan
                    Vector3 eye = camera.Eye;
                    Ray r1 = new Ray(eye, G.Viewport.Unproject(
                        new Vector3(input.Mouse.Previous.X, input.Mouse.Previous.Y, 1),
                        camera.Projection,
                        camera.View,
                        Matrix.Identity
                        ));
                    Ray r2 = new Ray(eye, G.Viewport.Unproject(
                        new Vector3(input.Mouse.Current.X, input.Mouse.Current.Y, 1),
                        camera.Projection,
                        camera.View,
                        Matrix.Identity
                        ));

                    Plane hit = new Plane(Vector3.Normalize(eye - camera.Center), 0f);

                    r1.Direction -= r1.Position; r1.Direction.Normalize();
                    float d1 = r1.Intersects(hit).Value;
                    Vector3 p1 = r1.Position + d1 * r1.Direction;

                    r2.Direction -= r2.Position; r2.Direction.Normalize();
                    float d2 = r2.Intersects(hit).Value;
                    Vector3 p2 = r2.Position + d2 * r2.Direction;

                    camera.Center += p1 - p2;
                }
            }
            else if(input.Mouse.Current.MiddleButton == ButtonState.Pressed) {
                // Yaw
                if(p.X != 0) camera.Yaw -= YAW_SPEED * (p.X * dt);
                // Pitch
                if(p.Y != 0) camera.Pitch += PITCH_SPEED * (p.Y * dt);
            }

            // Zoom By Distance
            if(input.Mouse.ScrollDisplacement != 0)
                camera.Distance = MathHelper.Clamp(camera.Distance + DIST_ZOOM_SPEED * (input.Mouse.ScrollDisplacement / 60) * dt, 0.1f, 10f);

            if(input.Keyboard.IsKeyJustPressed(Keys.R)) {
                // Reset The Camera
                camera.Center = Vector3.Zero;
                camera.Distance = 4f;
                camera.Pitch = MathHelper.PiOver4;
                camera.Yaw = 0;
                camera.FOV = MathHelper.PiOver4;
            }
        }

        public override void Draw(GameTime gameTime) {
            G.Clear(BACK_COLOR);

            // Set Camera Matrices
            fx.VP = camera.View * camera.Projection;

            // Set The Scheme
            fx.CPrimary = colorScheme.Primary;
            fx.CSecondary = colorScheme.Secondary;
            fx.CTertiary = colorScheme.Tertiary;

            // Try To Draw The Model
            G.DepthStencilState = DepthStencilState.Default;
            G.RasterizerState = RasterizerState.CullCounterClockwise;
            G.BlendState = BlendState.Opaque;
            lock(drawLock) {
                if(unitModel != null) {
                    // Set Up The Textures
                    G.VertexSamplerStates[0] = SamplerState.PointClamp;
                    G.SamplerStates[1] = SamplerState.LinearClamp;
                    G.SamplerStates[2] = SamplerState.LinearClamp;
                    fx.SetTextures(G, unitModel.AnimationTexture, unitModel.ModelTexture, unitModel.ColorCodeTexture);

                    unitModel.UpdateInstances(G);
                    unitModel.SetInstances(G);
                    fx.ApplyPassAnimation();
                    unitModel.DrawInstances(G);

                    // Cause XNA Is Retarded Like That
                    G.VertexTextures[0] = null;
                    G.VertexSamplerStates[0] = SamplerState.LinearClamp;
                }
            }

            // Unset Buffers
            G.SetVertexBuffers(null);
            G.Indices = null;

            wr.Draw(SB);
        }

        private void LoadUnit(FileInfo fi) {
            Thread tLoad = new Thread(UnitLoader);
            tLoad.IsBackground = true;
            tLoad.Priority = ThreadPriority.BelowNormal;
            tLoad.Start(fi);
        }
        private void UnitLoader(object _fi) {
            FileInfo fi = _fi as FileInfo;

            RTSUnitResult res = RTSUnitDataParser.Parse(engine, fi);
            RTSUnitData _unitData = res.Data;
            RTSUnitModel _unitModel = res.View;
            RTSUnit _unit = new RTSUnit(team, _unitData, Vector2.Zero);
            _unit.Height = 0;
            _unitModel.OnUnitSpawn(_unit);

            // Create The Full Animation Loop
            _unit.AnimationController = new BlankAnimController(0, (_unitModel.AnimationTexture.Height / 3) - 1, 30f);

            // Make Sure To Only Modify At A Specific Point
            lock(drawLock) {
                // Check Previous Dispose
                if(unit != null) DisposeUnit();

                // Now Set Data
                unitData = _unitData;
                unit = _unit;
                unitModel = _unitModel;
            }
        }
        private void DisposeUnit() {
            unit = null;
            unitData = null;
            unitModel = null;
        }

        private void FindAllData() {
            searchDone = false;
            tSearch = new Thread(SearchWork);
            tSearch.IsBackground = true;
            tSearch.Priority = ThreadPriority.BelowNormal;
            tSearch.Start();
        }
        private void SearchWork() {
            // Begin Search Through All Packs
            var queue = new Queue<DirectoryInfo>();
            queue.Enqueue(new DirectoryInfo(AppSettings.PACKS_DIR));
            while(queue.Count > 0) {
                DirectoryInfo dir = queue.Dequeue();
                foreach(var cdir in dir.GetDirectories())
                    queue.Enqueue(cdir);
                foreach(var fi in dir.GetFiles()) {
                    if(fi.Extension.EndsWith(RTSUnitDataParser.EXTENSION))
                        unitDataFiles.Add(fi);
                    else if(fi.Extension.EndsWith(RTSColorSchemeParser.EXTENSION))
                        schemeFiles.Add(fi);
                }
            }
            searchDone = true;
        }
    }

    #region Color Swatch Widget
    class ColorSwatch : IDisposable {
        private int active;
        private Vector3 col;
        public Vector3 Color {
            get { return col; }
            set {
                col = value;
                wR.Color = new Color(col.X, 0f, 0f);
                wG.Color = new Color(0f, col.Y, 0f);
                wB.Color = new Color(0f, 0f, col.Z);
                wCol.Color = new Color(col.X, col.Y, col.Z);
                if(OnColorChange != null)
                    OnColorChange(col);
            }
        }
        public event Action<Vector3> OnColorChange;

        RectWidget wBorder, wR, wG, wB, wCol;

        public ColorSwatch(int x, int y, int w, int hPart, int hFull, int border, Color bColor, float lD, WidgetRenderer r) {
            wBorder = new RectWidget(r);
            wBorder.Anchor = new Point(x, y);
            wBorder.Width = w + border * 2;
            wBorder.Height = hPart * 3 + hFull + border * 2;
            wBorder.LayerDepth = lD;
            wBorder.Color = bColor;
            lD -= 0.001f;
            x += border;
            y += border;

            wR = new RectWidget(r);
            wR.Anchor = new Point(x, y);
            wR.Width = w;
            wR.Height = hPart;
            wR.LayerDepth = lD;
            y += hPart;

            wG = new RectWidget(r);
            wG.Anchor = new Point(x, y);
            wG.Width = w;
            wG.Height = hPart;
            wG.LayerDepth = lD;
            y += hPart;

            wB = new RectWidget(r);
            wB.Anchor = new Point(x, y);
            wB.Width = w;
            wB.Height = hPart;
            wB.LayerDepth = lD;
            y += hPart;

            wCol = new RectWidget(r);
            wCol.Anchor = new Point(x, y);
            wCol.Width = w;
            wCol.Height = hFull;
            wCol.LayerDepth = lD;

            Color = Vector3.One;
            active = -1;
        }
        public void Dispose() {
            wBorder.Dispose();
            wR.Dispose();
            wG.Dispose();
            wB.Dispose();
            wCol.Dispose();
        }

        public void OnMousePress(Vector2 pos, MouseButton b) {
            int x = (int)pos.X;
            int y = (int)pos.Y;
            Vector2 ratio;
            if(wBorder.Inside(x, y, out ratio)) {
                if(wR.Inside(x, y, out ratio)) {
                    Color = new Vector3(ratio.X, col.Y, col.Z);
                    active = 0;
                }
                else if(wG.Inside(x, y, out ratio)) {
                    Color = new Vector3(col.X, ratio.X, col.Z);
                    active = 1;
                }
                else if(wB.Inside(x, y, out ratio)) {
                    Color = new Vector3(col.X, col.Y, ratio.X);
                    active = 2;
                }
            }
        }
        public void OnMouseMovement(Vector2 pos, Vector2 d) {
            if(active < 0) return;
            Vector2 r;
            int x = (int)pos.X;
            int y = (int)pos.Y;
            float v;
            switch(active) {
                case 0:
                    wR.Inside(x, y, out r);
                    v = MathHelper.Clamp(r.X, 0, 1);
                    Color = new Vector3(v, col.Y, col.Z);
                    break;
                case 1:
                    wG.Inside(x, y, out r);
                    v = MathHelper.Clamp(r.X, 0, 1);
                    Color = new Vector3(col.X, v, col.Z);
                    break;
                case 2:
                    wB.Inside(x, y, out r);
                    v = MathHelper.Clamp(r.X, 0, 1);
                    Color = new Vector3(col.X, col.Y, v);
                    break;
            }
        }
        public void OnMouseRelease(Vector2 pos, MouseButton b) {
            if(b == MouseButton.Left && active >= 0) active = -1;
        }
    } 
    #endregion

    #region Full Animation Loop Controller
    class BlankAnimController : ACUnitAnimationController {
        private AnimationLoop animLoop;
        public float FrameSpeed {
            get { return animLoop.FrameSpeed; }
            set { animLoop.FrameSpeed = value; }
        }

        public BlankAnimController(int s, int e, float sp) {
            animLoop = new AnimationLoop(s, e);
            animLoop.FrameSpeed = sp;
        }

        public override void SetAnimation(AnimationType t) {
        }
        public override void Update(GameState s, float dt) {
            animLoop.Step(dt);
            AnimationFrame = animLoop.CurrentFrame;
        }
    } 
    #endregion
}