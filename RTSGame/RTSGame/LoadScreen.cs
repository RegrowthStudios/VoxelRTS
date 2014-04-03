using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Graphics;
using RTSEngine.Data.Parsers;
using RTSEngine.Data.Team;

namespace RTS {
    public class LoadScreen : GameScreen<App> {
        // Constants For Loading Bar
        const string IMAGE_DIR = @"Content\LoadImages";
        const int BOUNDS_OFFSET = 20;
        const int BAR_HEIGHT = 10;
        const int BAR_WIDTH = 180;
        const int BACK_SIZE = 3;
        static readonly Color COLOR_LOW = Color.Maroon;
        static readonly Color COLOR_HIGH = Color.Teal;
        static readonly Color COLOR_BACK = new Color(8, 8, 8);

        public override int Next {
            get { return game.RTSScreen.Index; }
            protected set { }
        }
        public override int Previous {
            get { return game.MenuScreen.Index; }
            protected set { }
        }

        // View Info
        private Texture2D tLoad, tPixel;
        private List<FileInfo> imageList;
        private float percent;
        public List<string> tips;
        string tip;
        SpriteFont font;
        IDisposable tFont;

        // Engine Data
        private EngineLoadData loadData;
        public EngineLoadData LoadData {
            get { return loadData; }
            set { loadData = value; }
        }
        public GameState LoadedState {
            get;
            private set;
        }
        public Camera LoadedCamera {
            get;
            private set;
        }
        public RTSRenderer LoadedRenderer {
            get;
            private set;
        }

        // Loading Information
        private bool isLoaded;

        public override void Build() {
            DirectoryInfo id = new DirectoryInfo(IMAGE_DIR);
            imageList = new List<FileInfo>();
            foreach(var file in id.GetFiles()) {
                if(file.Extension.EndsWith("png"))
                    imageList.Add(file);
            }

            // Read All Tips
            tips = new List<string>();
            Regex rgxTip = new Regex(@"\x5b([^\x5d]+)\x5d");
            string mStr = null;
            using(var s = File.OpenRead(@"Content\UI\tips.txt")) {
                mStr = new StreamReader(s).ReadToEnd();
            }
            Match m = rgxTip.Match(mStr);
            while(m.Success) {
                tips.Add(m.Groups[1].Value);
                m = m.NextMatch();
            }
            return;
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            Random r = new Random();
            FileInfo fi = imageList[r.Next(imageList.Count)];
            using(var s = File.OpenRead(fi.FullName)) {
                tLoad = Texture2D.FromStream(G, s);
            }
            tPixel = new Texture2D(G, 1, 1);
            tPixel.SetData(new Color[] { Color.White });

            percent = 0f;
            isLoaded = false;

            font = XNASpriteFont.Compile(G, "Courier New", 14, out tFont);
            tip = tips[r.Next(tips.Count)];

            Thread tWork = new Thread(WorkThread);
            tWork.Priority = ThreadPriority.AboveNormal;
            tWork.IsBackground = true;
            tWork.Start();
        }
        public override void OnExit(GameTime gameTime) {
            tFont.Dispose();
            font = null;
            tLoad.Dispose();
            tPixel.Dispose();
        }

        public override void Update(GameTime gameTime) {
            percent += 0.01f;
            while(percent > 1) percent -= 1;

            if(isLoaded) State = ScreenState.ChangeNext;
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Transparent);

            int minX = BOUNDS_OFFSET - BAR_WIDTH;
            int maxX = G.Viewport.Bounds.Width - BOUNDS_OFFSET;

            // Calculate Progress Bar
            Rectangle rBar = G.Viewport.Bounds;
            rBar.X = (int)(percent * (maxX - minX)) + minX;
            rBar.Y = G.Viewport.Height - BOUNDS_OFFSET - BAR_HEIGHT;
            rBar.Height = BAR_HEIGHT;
            rBar.Width = BAR_WIDTH;
            if(rBar.Width + rBar.X > maxX)
                rBar.Width = maxX - rBar.X;
            else if(rBar.X < BOUNDS_OFFSET) {
                rBar.Width = rBar.X + rBar.Width - BOUNDS_OFFSET;
                rBar.X = BOUNDS_OFFSET;
            }

            Rectangle rBack = G.Viewport.Bounds;
            rBack.X = BOUNDS_OFFSET - BACK_SIZE;
            rBack.Y = G.Viewport.Bounds.Height - BOUNDS_OFFSET - BAR_HEIGHT - BACK_SIZE;
            rBack.Width -= (BOUNDS_OFFSET - BACK_SIZE) * 2;
            rBack.Height = BAR_HEIGHT + BACK_SIZE * 2;

            SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            // Draw A Background Image
            SB.Draw(tLoad, G.Viewport.Bounds, Color.White);
            // Draw The Progress Bar
            SB.Draw(tPixel, rBack, COLOR_BACK);
            SB.Draw(tPixel, rBar, Color.Lerp(COLOR_LOW, COLOR_HIGH, percent));

            SB.Draw(tPixel, new Rectangle(5, 5, 480, 160), new Color(0, 0, 0, 230));
            SB.DrawString(font, tip, new Vector2(10, 10), new Color(10, 80, 180, 255));
            SB.End();
        }

        private void WorkThread() {
            loadData = new EngineLoadData();
            loadData.MapFile = new FileInfo(@"Packs\Default\maps\0\test.map");
            var teamRes = RTSRaceParser.ParseAll(new DirectoryInfo("Packs"));

            loadData = game.LobbyScreen.InitInfo;

            LoadedState = new GameState();
            GameEngine.BuildLocal(LoadedState, LoadData);

            // Create Camera And Graphics
            LoadedCamera = new Camera(G.Viewport);
            LoadedCamera.Controller.Hook(game.Window);
            LoadedRenderer = new RTSRenderer(game.Graphics, @"Content\FX\RTS.fx", @"Content\FX\Map.fx", game.Window);

            LoadedRenderer.HookToGame(LoadedState, 0, LoadedCamera, game.LoadScreen.LoadData.MapFile);
            LoadedRenderer.LoadTeamVisuals(LoadedState, new VisualTeam() {
                TeamIndex = 0,
                ColorScheme = loadData.Teams[0].Colors,
                RaceFileInfo = loadData.Races[loadData.Teams[0].Race]
            });
            LoadedRenderer.LoadTeamVisuals(LoadedState, new VisualTeam() {
                TeamIndex = 1,
                ColorScheme = loadData.Teams[1].Colors,
                RaceFileInfo = loadData.Races[loadData.Teams[1].Race]
            });

            isLoaded = true;
        }
        private void LoadCallback(string m, float p) {
            // percent = p;
        }
    }
}