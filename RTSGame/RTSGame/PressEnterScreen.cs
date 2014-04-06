using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;

namespace RTS {
    public class PressEnterScreen : GameScreen<App> {
        private const string TEXTURE_FILE = @"Content\UI\PressEnter.png";
        private const string SOUND_FILE_THEME = @"Content\Audio\Theme.wav";

        public override int Next {
            get { return game.LoginScreen.Index; }
            protected set { }
        }
        public override int Previous {
            get { return -1; }
            protected set { }
        }

        Texture2D tImage;
        SoundEffect seTheme;
        SoundEffectInstance seiTheme;

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            using(var s = File.OpenRead(TEXTURE_FILE)) {
                tImage = Texture2D.FromStream(G, s);
            }
            using(var s = File.OpenRead(SOUND_FILE_THEME)) {
                seTheme = SoundEffect.FromStream(s);
            }
            seiTheme = seTheme.CreateInstance();
            seiTheme.IsLooped = true;
            seiTheme.Play();

            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;

            tImage.Dispose();
            seiTheme.Dispose();
            seTheme.Dispose();
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
            SB.Begin();
            SB.Draw(tImage, G.Viewport.Bounds, Color.White);
            SB.End();
        }

        void KeyboardEventDispatcher_OnKeyPressed(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.Space:
                case Keys.Enter:
                case Keys.Escape:
                    State = ScreenState.ChangeNext;
                    break;
            }
        }
    }
}
