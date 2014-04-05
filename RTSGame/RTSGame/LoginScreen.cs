using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;
using BlisterUI.Widgets;
using RTSEngine.Data;

namespace RTS {
    public class LoginScreen : GameScreen<App> {
        public override int Next {
            get { return game.MenuScreen.Index; }
            protected set { }
        }
        public override int Previous {
            get { return -1; }
            protected set { }
        }

        WidgetRenderer wr;
        SpriteFont font;
        IDisposable tFont;

        TextInput tInput;
        TextWidget wUsername;
        RectButton bFinish;

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            font = XNASpriteFont.Compile(G, "Courier New", 32, out tFont);
            wr = new WidgetRenderer(G, font);

            wUsername = new TextWidget(wr);
            wUsername.Anchor = new Point(10, 10);
            wUsername.Color = Color.White;
            wUsername.Height = 32;

            ButtonHighlightOptions h1 = new ButtonHighlightOptions(100, 30, Color.Red);
            ButtonHighlightOptions h2 = new ButtonHighlightOptions(100, 30, Color.Green);
            bFinish = new RectButton(wr, h1, h2);
            bFinish.OffsetAlignY = Alignment.BOTTOM;
            bFinish.Offset = new Point(0, 0);
            bFinish.Parent = wUsername;
            bFinish.OnButtonPress += bFinish_OnButtonPress;
            bFinish.Hook();

            tInput = new TextInput();
            tInput.OnTextChanged += tInput_OnTextChanged;
            tInput.Activate();
            tInput.Text = UserConfig.UserName;

            if(UserConfig.UserName.Equals(UserConfig.DEFAULT_USER_NAME)) {
                // Must Create A Default User Name
                wUsername.Text = "Please Enter A New Username";
            }
        }
        public override void OnExit(GameTime gameTime) {
            bFinish.OnButtonPress -= bFinish_OnButtonPress;
            tInput.OnTextChanged -= tInput_OnTextChanged;

            tInput.Dispose();
            wr.Dispose();
            wUsername.Dispose();
            bFinish.Dispose();
            tFont.Dispose();
        }

        void bFinish_OnButtonPress(RectButton obj, Vector2 m) {
            string n = tInput.Text;
            if(!n.Equals(UserConfig.DEFAULT_USER_NAME) && !string.IsNullOrWhiteSpace(n)) {
                UserConfig.UserName = n;
                State = ScreenState.ChangeNext;
            }
        }
        void tInput_OnTextChanged(TextInput arg1, string arg2) {
            wUsername.Text = arg2;
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);

            wr.Draw(SB);

            game.mRenderer.BeginPass(G);
            game.mRenderer.Draw(G);
        }
    }
}
