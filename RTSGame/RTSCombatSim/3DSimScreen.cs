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
            foreach(var team in engine.state.Teams) {
                int t = 0;
                foreach(var unitType in team.UnitData) {
                    for(int i = 0; i < unitType.MaxCount; i++) {
                        team.AddUnit(t, new Vector3(200, 10, 200));
                    }
                    t++;
                }
            }
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