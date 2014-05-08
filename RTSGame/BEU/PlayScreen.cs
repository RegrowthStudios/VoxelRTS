using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;
using BlisterUI.Widgets;
using BEU.Data;
using BEU.Controllers;

namespace BEU {
    public class PlayScreen : GameScreen<App> {
        public override int Next {
            get { return -1; }
            protected set { }
        }
        public override int Previous {
            get { return -1; }
            protected set { }
        }

        GameplayController gpc;
        GameState state;

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            Race rc = new Race();
            rc.Name = "Player";
            rc.AddType(0, new TankData());

            state = new GameState();
            state.CreatePlayer(rc, 0, Vector2.Zero);

            gpc = new GameplayController();
        }
        public override void OnExit(GameTime gameTime) {
            state = null;
            gpc = null;
        }

        public override void Update(GameTime gameTime) {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            gpc.Update(state, dt);
        }
        public override void Draw(GameTime gameTime) {

            G.SetRenderTarget(null);
            G.Clear(Color.Black);

        }
    }
}