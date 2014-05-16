using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;
using BlisterUI.Widgets;

namespace RTS {
    public class OptionsScreen : GameScreen<App> {
        public override int Next {
            get { return -1; }
            protected set { }
        }
        public override int Previous {
            get { return game.MenuScreen.Index; }
            protected set { }
        }

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed += OnKP;
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= OnKP;
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);

            game.DrawMouse();
        }

        private void OnKP(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.P:
                case Keys.Escape:
                    State = ScreenState.ChangePrevious;
                    break;
            }
        }
    }
}
