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
using System.Text.RegularExpressions;

namespace RTS {
    public class ColorSchemeScreen : GameScreen<App> {
        private static readonly Regex rgxSetFile = RegexHelper.GenerateFile("file");
        private static readonly Regex rgxSetName = RegexHelper.GenerateString("name");
        private static readonly Regex rgxSave = new Regex(@"save");
        private static readonly Regex rgxLoad = new Regex(@"load");

        private const string FX_FILE_PATH = @"FX\RTS";
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
        private RTSRenderer renderer;
        private RTSFXEntity fx;
        private WidgetRenderer wr;

        // Widgets
        RectWidget wBackPanel;
        ColorSwatch sP, sS, sT;

        public FileInfo file;
        public string name;

        public override void Build() {
            input = new InputManager();
            wr = new WidgetRenderer(G, game.Content.Load<SpriteFont>(@"Fonts\Default"));

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
        }

        public override void OnEntry(GameTime gameTime) {
            DevConsole.OnNewCommand += DevConsole_OnNewCommand;
            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;
            input.Refresh();
            sP.Hook();
            sS.Hook();
            sT.Hook();
            RendererInitArgs ria = ZXParser.ParseFile(@"Content\FX\RIA.txt", typeof(RendererInitArgs)) as RendererInitArgs;
            renderer = new RTSRenderer(game.Graphics, game.Content, ria, game.Window);

            // Rendering Effect
            fx = new RTSFXEntity(renderer.LoadEffect(FX_FILE_PATH));

            // Default Team
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
            DevConsole.OnNewCommand -= DevConsole_OnNewCommand;
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;
            DevConsole.Deactivate();
            if(unitModel != null) DisposeUnit();
            renderer.Dispose();
            camera = null;
            sP.Unhook();
            sS.Unhook();
            sT.Unhook();
            //MouseEventDispatcher.OnMousePress -= sP.OnMousePress;
            //MouseEventDispatcher.OnMouseMotion -= sP.OnMouseMovement;
            //MouseEventDispatcher.OnMouseRelease -= sP.OnMouseRelease;
            //MouseEventDispatcher.OnMousePress -= sS.OnMousePress;
            //MouseEventDispatcher.OnMouseMotion -= sS.OnMouseMovement;
            //MouseEventDispatcher.OnMouseRelease -= sS.OnMouseRelease;
            //MouseEventDispatcher.OnMousePress -= sT.OnMousePress;
            //MouseEventDispatcher.OnMouseMotion -= sT.OnMouseMovement;
            //MouseEventDispatcher.OnMouseRelease -= sT.OnMouseRelease;
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
            G.RasterizerState = RasterizerState.CullNone;
            G.BlendState = BlendState.Opaque;
            lock(drawLock) {
                if(unitModel != null) {
                    // Set Up The Textures
                    G.VertexSamplerStates[0] = SamplerState.PointClamp;
                    G.SamplerStates[1] = SamplerState.LinearClamp;
                    G.SamplerStates[2] = SamplerState.LinearClamp;
                    fx.SetTextures(G, unitModel.AnimationTexture, unitModel.ModelTexture, unitModel.ColorCodeTexture);

                    unitModel.UpdateInstances(G, GameplayController.IsUnitDead, (u) => { return true; });
                    unitModel.SetInstances(G);
                    fx.ApplyPassUnit();
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
            if(DevConsole.IsActivated) {
                game.DrawDevConsole();
            }

            game.mRenderer.BeginPass(G);
            game.mRenderer.Draw(G);
        }

        private void LoadUnit(FileInfo fi) {
            Thread tLoad = new Thread(UnitLoader);
            tLoad.IsBackground = true;
            tLoad.Priority = ThreadPriority.BelowNormal;
            tLoad.Start(fi);
        }
        private void UnitLoader(object _fi) {
            FileInfo fi = _fi as FileInfo;

            GameState state = new GameState();
            state.teams[0] = new RTSTeam(0, RTSInputType.None);
            state.UpdateActiveTeams();
            RTSUnitData _unitData = RTSUnitDataParser.ParseData(GameEngine.Scripts, fi, 0);
            state.teams[0].Race.Units[0] = _unitData;
            state.teams[0].Race.UpdateActiveUnits();
            RTSUnitModel _unitModel = RTSUnitDataParser.ParseModel(renderer, fi, state.teams[0].Race);
            _unitModel.Hook(renderer, state, 0, 0);
            RTSUnit _unit = new RTSUnit(state.teams[0], state.teams[0].Race.Units[0], Vector2.Zero);
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
                    else if(fi.Extension.EndsWith(".scheme"))
                        schemeFiles.Add(fi);
                }
            }
            searchDone = true;
        }

        void DevConsole_OnNewCommand(string obj) {
            Match m;
            if((m = rgxSetFile.Match(obj)).Success) {
                file = RegexHelper.ExtractFile(m);
            }
            else if((m = rgxSetName.Match(obj)).Success) {
                name = RegexHelper.Extract(m);
            }
            else if((m = rgxSave.Match(obj)).Success) {
                if(file == null) {
                    DevConsole.AddCommand("No File Specified");
                }
                else if(string.IsNullOrWhiteSpace(name)) {
                    DevConsole.AddCommand("No Name Specified");
                }
                else {
                    using(var s = file.Create()) {
                        StreamWriter w = new StreamWriter(s);
                        w.WriteLine("NAME [{0}]", name);
                        w.WriteLine("PRIMARY [{0}, {1}, {2}]", colorScheme.Primary.X, colorScheme.Primary.Y, colorScheme.Primary.Z);
                        w.WriteLine("SECONDARY [{0}, {1}, {2}]", colorScheme.Secondary.X, colorScheme.Secondary.Y, colorScheme.Secondary.Z);
                        w.WriteLine("TERTIARY [{0}, {1}, {2}]", colorScheme.Tertiary.X, colorScheme.Tertiary.Y, colorScheme.Tertiary.Z);
                        w.Flush();
                    }
                }
            }
            else if((m = rgxLoad.Match(obj)).Success) {
                if(file == null) {
                    DevConsole.AddCommand("No File Specified");
                }
                else if(!file.Exists) {
                    DevConsole.AddCommand("File Does Not Exist");
                }
                var cs = ZXParser.ParseFile(file.FullName, typeof(RTSColorScheme));
                if(cs == null) {
                    DevConsole.AddCommand("Incorrect File Format");
                }
                else {
                    colorScheme = (RTSColorScheme)cs;
                    name = colorScheme.Name;
                    sP.Color = colorScheme.Primary;
                    sS.Color = colorScheme.Secondary;
                    sT.Color = colorScheme.Tertiary;
                }
            }
        }
        void KeyboardEventDispatcher_OnKeyPressed(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case DevConsole.ACTIVATION_KEY:
                    if(DevConsole.IsActivated)
                        DevConsole.Deactivate();
                    else
                        DevConsole.Activate();
                    break;
                case Keys.N:
                    if(!DevConsole.IsActivated) {
                        LoadUnit(unitDataFiles[curUnit]);
                        curUnit = (curUnit + 1) % unitDataFiles.Count;
                    }
                    break;
                case Keys.P:
                    if(!DevConsole.IsActivated) {
                        State = ScreenState.ChangePrevious;
                    }
                    break;
                case Keys.Escape:
                    State = ScreenState.ExitApplication;
                    break;
            }
        }
    }

    #region Color Swatch Widget
    class ColorSwatch : IDisposable {
        private Vector3 col;
        public Vector3 Color {
            get { return col; }
            set {
                col = value;
                sbR.ScrollRatio = col.X;
                sbG.ScrollRatio = col.Y;
                sbB.ScrollRatio = col.Z;
                wCol.Color = new Color(col.X, col.Y, col.Z);
                if(OnColorChange != null)
                    OnColorChange(col);
            }
        }
        public event Action<Vector3> OnColorChange;

        RectWidget wBorder, wR, wG, wB, wCol;
        ScrollBar sbR, sbG, sbB;

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

            int sw = w - border;
            int sh = hPart - border;
            var csbb = new Microsoft.Xna.Framework.Color(28, 28, 28);

            wR = new RectWidget(r);
            wR.Anchor = new Point(x, y);
            wR.Width = w;
            wR.Height = hPart;
            wR.LayerDepth = lD;
            wR.Color = Microsoft.Xna.Framework.Color.Red;
            y += hPart;
            sbR = new ScrollBar(r);
            sbR.Color = csbb;
            sbR.Width = sw;
            sbR.Height = sh;
            sbR.LayerDepth = lD - 0.01f;
            sbR.ScrollButton.ActiveWidth = sw / 5;
            sbR.ScrollButton.ActiveHeight = sh;
            sbR.ScrollButton.InactiveWidth = sw / 5;
            sbR.ScrollButton.InactiveHeight = sh;
            sbR.ScrollButton.InactiveColor = Microsoft.Xna.Framework.Color.Black;
            sbR.ScrollButton.ActiveColor = Microsoft.Xna.Framework.Color.White;
            sbR.ScrollButton.LayerDepth = lD - 0.02f;
            sbR.Parent = wR;
            sbR.OffsetAlignX = Alignment.MID;
            sbR.OffsetAlignY = Alignment.MID;
            sbR.AlignX = Alignment.MID;
            sbR.AlignY = Alignment.MID;
            sbR.IsVertical = false;
            sbR.OnScrollValueChanged += (sb, v) => {
                col.X = v;
                sbR.ScrollButton.InactiveColor = new Color(col.X, 0f, 0f);
                wCol.Color = new Color(col.X, col.Y, col.Z);
                if(OnColorChange != null)
                    OnColorChange(col);
            };
            sbR.ScrollRatio = 0;

            wG = new RectWidget(r);
            wG.Anchor = new Point(x, y);
            wG.Width = w;
            wG.Height = hPart;
            wG.LayerDepth = lD;
            wG.Color = Microsoft.Xna.Framework.Color.Green;
            y += hPart;
            sbG = new ScrollBar(r);
            sbG.Color = csbb;
            sbG.Width = sw;
            sbG.Height = sh;
            sbG.LayerDepth = lD - 0.01f;
            sbG.ScrollButton.ActiveWidth = sw / 5;
            sbG.ScrollButton.ActiveHeight = sh;
            sbG.ScrollButton.InactiveWidth = sw / 5;
            sbG.ScrollButton.InactiveHeight = sh;
            sbG.ScrollButton.InactiveColor = Microsoft.Xna.Framework.Color.Black;
            sbG.ScrollButton.ActiveColor = Microsoft.Xna.Framework.Color.White;
            sbG.ScrollButton.LayerDepth = lD - 0.02f;
            sbG.Parent = wG;
            sbG.OffsetAlignX = Alignment.MID;
            sbG.OffsetAlignY = Alignment.MID;
            sbG.AlignX = Alignment.MID;
            sbG.AlignY = Alignment.MID;
            sbG.IsVertical = false;
            sbG.OnScrollValueChanged += (sb, v) => {
                col.Y = v;
                sbG.ScrollButton.InactiveColor = new Color(0f, col.Y, 0f);
                wCol.Color = new Color(col.X, col.Y, col.Z);
                if(OnColorChange != null)
                    OnColorChange(col);
            };
            sbG.ScrollRatio = 0;

            wB = new RectWidget(r);
            wB.Anchor = new Point(x, y);
            wB.Width = w;
            wB.Height = hPart;
            wB.LayerDepth = lD;
            wB.Color = Microsoft.Xna.Framework.Color.Blue;
            y += hPart;
            sbB = new ScrollBar(r);
            sbB.Color = csbb;
            sbB.Width = sw;
            sbB.Height = sh;
            sbB.LayerDepth = lD - 0.01f;
            sbB.ScrollButton.ActiveWidth = sw / 5;
            sbB.ScrollButton.ActiveHeight = sh;
            sbB.ScrollButton.InactiveWidth = sw / 5;
            sbB.ScrollButton.InactiveHeight = sh;
            sbB.ScrollButton.InactiveColor = Microsoft.Xna.Framework.Color.Black;
            sbB.ScrollButton.ActiveColor = Microsoft.Xna.Framework.Color.White;
            sbB.ScrollButton.LayerDepth = lD - 0.02f;
            sbB.Parent = wB;
            sbB.OffsetAlignX = Alignment.MID;
            sbB.OffsetAlignY = Alignment.MID;
            sbB.AlignX = Alignment.MID;
            sbB.AlignY = Alignment.MID;
            sbB.IsVertical = false;
            sbB.OnScrollValueChanged += (sb, v) => {
                col.Z = v;
                sbB.ScrollButton.InactiveColor = new Color(0f, 0f, col.Z);
                wCol.Color = new Color(col.X, col.Y, col.Z);
                if(OnColorChange != null)
                    OnColorChange(col);
            };
            sbB.ScrollRatio = 0;

            wCol = new RectWidget(r);
            wCol.Anchor = new Point(x, y);
            wCol.Width = w;
            wCol.Height = hFull;
            wCol.LayerDepth = lD;

            Color = Vector3.One;
        }

        private void OnScrollValueChanged(ScrollBar arg1, float arg2) {

        }
        public void Dispose() {
            wBorder.Dispose();
            wR.Dispose();
            sbR.Dispose();
            wG.Dispose();
            sbG.Dispose();
            wB.Dispose();
            sbB.Dispose();
            wCol.Dispose();
        }

        public void Hook() {
            sbR.Hook();
            sbG.Hook();
            sbB.Hook();
        }
        public void Unhook() {
            sbR.Unhook();
            sbG.Unhook();
            sbB.Unhook();
        }

        //public void OnMousePress(Vector2 pos, MouseButton b) {
        //    int x = (int)pos.X;
        //    int y = (int)pos.Y;
        //    Vector2 ratio;
        //    if(wBorder.Inside(x, y, out ratio)) {
        //        if(wR.Inside(x, y, out ratio)) {
        //            Color = new Vector3(ratio.X, col.Y, col.Z);
        //            active = 0;
        //        }
        //        else if(wG.Inside(x, y, out ratio)) {
        //            Color = new Vector3(col.X, ratio.X, col.Z);
        //            active = 1;
        //        }
        //        else if(wB.Inside(x, y, out ratio)) {
        //            Color = new Vector3(col.X, col.Y, ratio.X);
        //            active = 2;
        //        }
        //    }
        //}
        //public void OnMouseMovement(Vector2 pos, Vector2 d) {
        //    if(active < 0) return;
        //    Vector2 r;
        //    int x = (int)pos.X;
        //    int y = (int)pos.Y;
        //    float v;
        //    switch(active) {
        //        case 0:
        //            wR.Inside(x, y, out r);
        //            v = MathHelper.Clamp(r.X, 0, 1);
        //            Color = new Vector3(v, col.Y, col.Z);
        //            break;
        //        case 1:
        //            wG.Inside(x, y, out r);
        //            v = MathHelper.Clamp(r.X, 0, 1);
        //            Color = new Vector3(col.X, v, col.Z);
        //            break;
        //        case 2:
        //            wB.Inside(x, y, out r);
        //            v = MathHelper.Clamp(r.X, 0, 1);
        //            Color = new Vector3(col.X, col.Y, v);
        //            break;
        //    }
        //}
        //public void OnMouseRelease(Vector2 pos, MouseButton b) {
        //    if(b == MouseButton.Left && active >= 0) active = -1;
        //}
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

        public override void Init(GameState s, GameplayController c) {
        }

        public override void Update(GameState s, float dt) {
            animLoop.Step(dt);
            AnimationFrame = animLoop.CurrentFrame;
        }


        public override void Serialize(BinaryWriter s) {
        }
        public override void Deserialize(BinaryReader s) {
        }
    }
    #endregion
}