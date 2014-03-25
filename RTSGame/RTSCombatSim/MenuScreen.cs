using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;

namespace RTSCS {
    public class MenuScreen : GameScreen<App> {
        public override int Next {
            get;
            protected set;
        }
        public override int Previous {
            get { return -1; }
            protected set { }
        }

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);
        }

        private void KeyboardEventDispatcher_OnKeyPressed(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.D1:
                    Next = game.LoadScreen.Index;
                    State = ScreenState.ChangeNext;
                    break;
                case Keys.D2:
                    Next = game.ColorSchemeScreen.Index;
                    State = ScreenState.ChangeNext;
                    break;
                case Keys.Escape:
                    State = ScreenState.ExitApplication;
                    break;
            }
        }
    }
}