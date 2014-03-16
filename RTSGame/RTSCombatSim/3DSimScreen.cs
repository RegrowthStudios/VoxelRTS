using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Controllers;
using RTSEngine.Data.Parsers;
using RTSEngine.Graphics;

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
                        team.AddUnit(t, new Vector2(200, 200));
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

            // TODO: Omit Move All Units To The Mouse
            var ms = Mouse.GetState();
            Ray r = engine.renderer.GetViewRay(new Vector2(ms.X, ms.Y));
            IntersectionRecord rec = new IntersectionRecord();
            if(engine.state.Map.BVH.Intersect(ref rec, r)) {
                Vector3 hit = r.Position + r.Direction * rec.T;
                Vector2 gp = new Vector2(hit.X, hit.Z);
                foreach(var team in engine.state.Teams) {
                    foreach(var unit in team.Units) {
                        var disp = Vector2.Normalize(gp - unit.GridPosition);
                        unit.GridPosition += 2 * disp / 60f;
                    }
                }
            }
        }
        public override void Draw(GameTime gameTime) {
            engine.Draw(G, 1f / 60f);
        }
    }
}