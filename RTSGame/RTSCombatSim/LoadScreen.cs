using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;
using RTSEngine.Controllers;
using RTSEngine.Data.Parsers;
using RTSEngine.Data.Team;

namespace RTSCS {
    public class LoadScreen : GameScreenIndexed {
        const string IMAGE_DIR = @"Content\LoadImages";
        const int BOUNDS_OFFSET = 20;
        const int BAR_HEIGHT = 30;
        const int BACK_SIZE = 3;
        static readonly Color COLOR_LOW = Color.Maroon;
        static readonly Color COLOR_HIGH = Color.Teal;
        static readonly Color COLOR_BACK = Color.DarkGray;

        public LoadScreen(int i) : base(i) { }
        public LoadScreen(int p, int n) : base(p, n) { }

        // View Info
        private Texture2D tLoad, tPixel;
        private List<FileInfo> imageList;

        // Engine Data
        private EngineLoadData loadData;
        public EngineLoadData LoadData {
            get { return loadData; }
            set { loadData = value; }
        }
        public GameEngine LoadedEngine {
            get;
            private set;
        }

        // Loading Information
        private float percent;
        private bool isLoaded;

        public override void Build() {
            DirectoryInfo id = new DirectoryInfo(IMAGE_DIR);
            imageList = new List<FileInfo>();
            foreach(var file in id.GetFiles()) {
                if(file.Extension.EndsWith("png"))
                    imageList.Add(file);
            }
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

            Thread tWork = new Thread(WorkThread);
            tWork.Priority = ThreadPriority.AboveNormal;
            tWork.IsBackground = true;
            tWork.Start();
        }
        public override void OnExit(GameTime gameTime) {
            tLoad.Dispose();
            tPixel.Dispose();
        }

        public override void Update(GameTime gameTime) {
            if(isLoaded) State = ScreenState.ChangeNext;
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Transparent);

            // Calculate Progress Bar
            Rectangle rBar = G.Viewport.Bounds;
            rBar.Width -= BOUNDS_OFFSET * 2;
            rBar.X = BOUNDS_OFFSET;
            rBar.Y = rBar.Height - BOUNDS_OFFSET - BAR_HEIGHT;
            rBar.Height = BAR_HEIGHT;
            Rectangle rBack = rBar;
            rBack.X -= BACK_SIZE;
            rBack.Y -= BACK_SIZE;
            rBack.Width += BACK_SIZE * 2;
            rBack.Height += BACK_SIZE * 2;
            rBar.Width = (int)(rBar.Width * percent);

            SB.Begin(SpriteSortMode.Immediate, BlendState.Opaque, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            // Draw A Background Image
            SB.Draw(tLoad, G.Viewport.Bounds, Color.White);
            // Draw The Progress Bar
            SB.Draw(tPixel, rBack, COLOR_BACK);
            SB.Draw(tPixel, rBar, Color.Lerp(COLOR_LOW, COLOR_HIGH, percent));
            SB.End();
        }

        private void WorkThread() {
            loadData = new EngineLoadData();
            loadData.MapDirectory = new DirectoryInfo(@"Packs\Default\maps\0");
            RTSTeamResult teamRes = RTSTeamParser.ParseAll(new DirectoryInfo("Packs"))[0];
            loadData.Teams = new RTSTeamResult[2];
            loadData.Teams[0] = teamRes;
            loadData.Teams[1] = teamRes;
            EngineLoadData.InputType[] types = { EngineLoadData.InputType.Player, EngineLoadData.InputType.AI, EngineLoadData.InputType.Environment};
            LoadedEngine = new GameEngine(game.Graphics, game.Window, loadData, LoadCallback, types);
            isLoaded = true;
        }
        private void LoadCallback(string m, float p) {
            percent = p;
        }
    }
}