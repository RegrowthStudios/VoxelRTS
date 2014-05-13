using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI;
using BlisterUI.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using System.IO;

namespace RTS {
    public class WispScreen : GameScreen<App> {
        const string FILE = @"Content\Textures\Wisp.png";
        const float DURATION = 8f;
        const float TRANS_DURATION = DURATION * 0.7f;

        bool early;
        public override int Next {
            get { return early ? game.LoginScreen.Index : game.InduZtryScreen.Index; }
            protected set { }
        }
        public override int Previous {
            get { return -1; }
            protected set { }
        }

        float et;
        Texture2D tWisp;

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            early = false;
            using(var s = File.OpenRead(FILE)) tWisp = Texture2D.FromStream(G, s);
            et = 0f;
            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;
            tWisp.Dispose();
            tWisp = null;
        }

        public override void Update(GameTime gameTime) {
            et += (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(et > DURATION) State = ScreenState.ChangeNext;
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);

            SB.Begin();
            SB.Draw(tWisp, G.Viewport.Bounds, Color.Lerp(Color.Transparent, Color.White, MathHelper.Clamp(et / TRANS_DURATION, 0, 1)));
            SB.End();
        }

        void KeyboardEventDispatcher_OnKeyPressed(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.Escape:
                case Keys.Space:
                case Keys.Enter:
                    State = ScreenState.ChangeNext;
                    break;
            }
        }
    }
}