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

        const string TIPS_FILE = @"Content\UI\tips.txt";
        const string TIPS_FONT = @"Courier New";
        const int TIPS_FONT_SIZE = 12;
        const int TIPS_HEIGHT = 140;
        const int TIPS_OFFSET = 5;

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
            FindAllImages();
            ReadAllTips();
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

            // Font For Tips
            font = XNASpriteFont.Compile(G, TIPS_FONT, TIPS_FONT_SIZE, out tFont);
            tip = tips[r.Next(tips.Count)];

            // Create The Loading Thread
            isLoaded = false;
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

            SB.Draw(tPixel, new Rectangle(TIPS_OFFSET, TIPS_OFFSET, G.Viewport.Width - TIPS_OFFSET * 2, TIPS_HEIGHT), COLOR_BACK);
            SB.DrawString(font, tip, Vector2.One * (TIPS_OFFSET * 2), COLOR_HIGH);
            SB.End();
        }

        private void FindAllImages() {
            DirectoryInfo id = new DirectoryInfo(IMAGE_DIR);
            imageList = new List<FileInfo>();
            foreach(var file in id.GetFiles()) {
                if(file.Extension.EndsWith("png"))
                    imageList.Add(file);
            }
        }
        private void ReadAllTips() {
            tips = new List<string>();
            Regex rgxTip = new Regex(@"\x5b([^\x5d]+)\x5d");
            string mStr = null;
            using(var s = File.OpenRead(TIPS_FILE)) {
                mStr = new StreamReader(s).ReadToEnd();
            }
            Match m = rgxTip.Match(mStr);
            while(m.Success) {
                tips.Add(m.Groups[1].Value);
                m = m.NextMatch();
            }
        }
        private void WorkThread() {
            // Grab The Initialization Info
            loadData = game.LobbyScreen.InitInfo;
            var races = game.LobbyScreen.Races;

            // Build The Local Game State
            LoadedState = new GameState();
            GameEngine.BuildLocal(LoadedState, LoadData, races);

            // Create Camera
            LoadedCamera = new Camera(G.Viewport);
            LoadedCamera.Controller.Hook(game.Window);

            // Load The Renderer
            LoadedRenderer = new RTSRenderer(game.Graphics, @"Content\FX\RTS.fx", @"Content\FX\Map.fx", game.Window);
            LoadedRenderer.HookToGame(LoadedState, 0, LoadedCamera, game.LoadScreen.LoadData.MapFile);
            VisualTeam vt;
            for(int i = 0; i < loadData.Teams.Length; i++) {
                var td = loadData.Teams[i];
                if(td.InputType == InputType.None)
                    continue;

                // Create The Visual Data
                vt.TeamIndex = i;
                vt.RaceFileInfo = races[td.Race];
                vt.ColorScheme = td.Colors;
                LoadedRenderer.LoadTeamVisuals(LoadedState, vt);
            }
            isLoaded = true;
        }
    }
}