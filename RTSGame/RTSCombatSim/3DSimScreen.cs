using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Controllers;

namespace RTSCS {
    public class _3DSimScreen : GameScreenIndexed {
        private GameEngine engine;

        public _3DSimScreen(int i) : base(i) { }
        public _3DSimScreen(int p, int n) : base(p, n) { }

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            game.IsMouseVisible = true;

            EngineLoadData data = new EngineLoadData();
            data.MapDirectory = new DirectoryInfo(@"Packs\Default\maps\0");
            data.Teams = new RTSTeam[2];
            data.Teams[0] = new RTSTeam();
            data.Teams[1] = new RTSTeam();
            engine = new GameEngine(game.Graphics, game.Window, data);
        }
        public override void OnExit(GameTime gameTime) {
            game.IsMouseVisible = false;

            engine.Dispose();
        }

        public override void Update(GameTime gameTime) {
            engine.Update(1f / 60f);
        }
        public override void Draw(GameTime gameTime) {
            engine.Draw(G, 1f / 60f);
        }
    }
}