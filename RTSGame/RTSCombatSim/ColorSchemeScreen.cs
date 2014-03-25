using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;
using RTSEngine.Graphics;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Data.Parsers;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework.Input;

namespace RTSCS {
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
        private RTSEffect fx;

        public override void Build() {
            input = new InputManager();
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            input.Refresh();

            // Rendering Effect
            fx = new RTSEffect(XNAEffect.Compile(G, FX_FILE_PATH));

            // Default Team
            team = new RTSTeam();
            colorScheme = RTSColorScheme.Default;

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
            fx.Dispose();
            camera = null;
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
            lock(drawLock) {
                if(unitModel != null) {
                    unitModel.UpdateInstances(G);
                    unitModel.SetInstances(G);
                    fx.ApplyPassAnimation();
                    unitModel.DrawInstances(G);
                }
            }

            // Unset Buffers
            G.SetVertexBuffers(null);
            G.Indices = null;
        }

        private void LoadUnit(FileInfo fi) {
            Thread tLoad = new Thread(UnitLoader);
            tLoad.IsBackground = true;
            tLoad.Priority = ThreadPriority.BelowNormal;
            tLoad.Start(fi);
        }
        private void UnitLoader(object _fi) {
            FileInfo fi = _fi as FileInfo;

            RTSUnitResult res = RTSUnitDataParser.Parse(G, fi);
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

                // Set Up The Textures
                fx.TexMain = unitModel.ModelTexture;
                fx.TexKey = unitModel.ColorCodeTexture;
                fx.TexAnimation = unitModel.AnimationTexture;
            }
        }
        private void DisposeUnit() {
            unit = null;
            unitData = null;

            unitModel.Dispose();
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
}