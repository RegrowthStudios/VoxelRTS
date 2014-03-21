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
        const int BAR_HEIGHT = 10;
        const int BAR_WIDTH = 180;
        const int BACK_SIZE = 3;
        static readonly Color COLOR_LOW = Color.Maroon;
        static readonly Color COLOR_HIGH = Color.Teal;
        static readonly Color COLOR_BACK = new Color(8, 8, 8);

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
            var teamRes = RTSRaceParser.ParseAll(new DirectoryInfo("Packs"));

            loadData.Teams = new RTSTeamResult[2];
            loadData.Teams[0].TeamType = (from res in teamRes where res.Name.StartsWith("P") select res).First((o) => { return true; }); ;
            loadData.Teams[0].InputType = InputType.Player;
            loadData.Teams[0].Colors = RTSColorScheme.Default;
            loadData.Teams[0].Colors.Primary *= Vector3.UnitX;
            loadData.Teams[0].Colors.Secondary *= Vector3.UnitX;
            loadData.Teams[0].Colors.Tertiary *= Vector3.UnitX;
            loadData.Teams[1].TeamType = (from res in teamRes where res.Name.StartsWith("M") select res).First((o) => { return true; }); ;
            loadData.Teams[1].InputType = InputType.AI;
            loadData.Teams[1].Colors = RTSColorScheme.Default;
            loadData.Teams[1].Colors.Primary *= Vector3.UnitZ;
            loadData.Teams[1].Colors.Secondary *= Vector3.UnitZ;
            loadData.Teams[1].Colors.Tertiary *= Vector3.UnitZ;

            LoadedEngine = new GameEngine(game.Graphics, game.Window, loadData);
            isLoaded = true;
        }
        private void LoadCallback(string m, float p) {
            // percent = p;
        }
    }
}