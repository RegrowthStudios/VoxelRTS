using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading.Tasks;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;
using RTSEngine.Graphics;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Data.Parsers;

namespace AnimViewer {
    public class App : MainGame {
        protected override void BuildScreenList() {
            screenList = new ScreenList(this, 0,
                new FalseFirstScreen(1),
                new MainScreen(1)
                );
        }
        protected override void FullInitialize() {
            WMHookInput.Initialize(Window);
        }
        protected override void FullLoad() {
        }
    }

    public class MainScreen : GameScreenIndexed {
        public MainScreen(int i) : base(i) { }
        public MainScreen(int p, int n) : base(p, n) { }

        private Effect fx;

        private VertexBuffer vb;
        private IndexBuffer ib;
        private Texture2D tMain, tKey, tAnim;

        Matrix mView, mProj;

        RTSUnitData d;
        RTSUnitModel m;

        public override void Build() {
            fx = XNAEffect.Compile(G, @"RTS.fx");
            fx.CurrentTechnique = fx.Techniques[0];

            //using(var fs = File.OpenRead(@"3\MechBase.obj")) {
            //    ObjParser.TryParse(fs, G, out vb, out ib, ParsingFlags.ConversionOpenGL);
            //}
            //using(var fs = File.OpenRead(@"3\Main.png")) {
            //    tMain = Texture2D.FromStream(G, fs);
            //}
            //using(var fs = File.OpenRead(@"3\Key.png")) {
            //    tKey = Texture2D.FromStream(G, fs);
            //}
            //using(var fs = File.OpenRead(@"3\MechBase.png")) {
            //    tAnim = Texture2D.FromStream(G, fs);
            //}
            //var res = RTSUnitDataParser.Parse(G, new DirectoryInfo("3"));
            //m = res.View;
            //d = res.Data;
            //RTSTeam t = new RTSTeam();
            //RTSUnit unit = new RTSUnit(t, d, Vector2.Zero);
            //unit.Move(-Vector2.UnitX);
            //unit.Move(Vector2.UnitX);
            //m.OnUnitSpawn(unit);

            mView = Matrix.CreateLookAt(Vector3.UnitX * 3 + Vector3.Up, Vector3.Up, Vector3.Up);
            mProj = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, G.Viewport.AspectRatio, 0.01f, 100f);
            m.UpdateInstances(G);
        }
        public override void Destroy(GameTime gameTime) {
            fx.Dispose();
            tMain.Dispose();
            tKey.Dispose();
        }

        public override void OnEntry(GameTime gameTime) {
        }
        public override void OnExit(GameTime gameTime) {
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.White);
            //fx.Parameters["VP"].SetValue(mView * mProj);
            //fx.Parameters["World"].SetValue(Matrix.Identity);
            //fx.Parameters["TexColor"].SetValue(tMain);
            //fx.Parameters["TexOverlay"].SetValue(tKey);
            //fx.Parameters["TexModelMap"].SetValue(m.AnimationTexture);
            //fx.Parameters["TexelSize"].SetValue(new Vector2(1f / m.AnimationTexture.Width, 1f / m.AnimationTexture.Height));
            //fx.Parameters["CPrimary"].SetValue(Vector3.UnitZ);
            //fx.Parameters["CSecondary"].SetValue(Vector3.UnitZ * 0.8f);
            //fx.Parameters["CTertiary"].SetValue(Vector3.UnitZ * 0.3f);
            //fx.CurrentTechnique.Passes["Animation"].Apply();
            //m.SetInstances(G);
            //m.DrawInstances(G);
        }
    }
}