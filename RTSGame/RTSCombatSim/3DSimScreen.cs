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
using RTSEngine.Data.Parsers;

namespace RTSCS {
    public class _3DSimScreen : GameScreen<App> {
        private GameEngine engine;

        public override int Next {
            get {
                return -1;
            }
            protected set {
                throw new NotImplementedException();
            }
        }
        public override int Previous {
            get {
                return game.LoadScreen.Index;
            }
            protected set {
                throw new NotImplementedException();
            }
        }

        public _3DSimScreen()
            : base() {
        }

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            game.IsMouseVisible = true;

            engine = game.LoadScreen.LoadedEngine;
            RTSTeam team0 = engine.state.Teams[0];
            /*for (int i = 0; i < 2; i++) {
                team0.AddUnit(0, new Vector3(200, 10, 200));
            }*/
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