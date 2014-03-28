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
using BlisterUI.Widgets;

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

        WidgetRenderer wr;
        IDisposable fontDisp;
        RectButton[] buttons;
        TextWidget[] buttonsText;
        SoundEffect seHover, seClick;

        public override void Build() {
            using(var s = File.OpenRead(@"Content\Audio\btnClick.wav")) {
                seClick = SoundEffect.FromStream(s);
            }
        }
        public override void Destroy(GameTime gameTime) {
            seClick.Dispose();
        }

        public override void OnEntry(GameTime gameTime) {
            SpriteFont f = XNASpriteFont.Compile(G, "Arial", 32, out fontDisp);
            wr = new WidgetRenderer(G, f);

            using(var s = File.OpenRead(@"Content\Audio\btnHover.wav")) {
                seHover = SoundEffect.FromStream(s);
            }

            ButtonHighlightOptions o1 = new ButtonHighlightOptions(240, 40, Color.DarkGray);
            ButtonHighlightOptions o2 = new ButtonHighlightOptions(280, 50, Color.RoyalBlue);
            buttons = new RectButton[2];
            buttonsText = new TextWidget[buttons.Length];
            for(int i = 0; i < buttons.Length; i++) {
                buttons[i] = new RectButton(wr, o1, o2);
                buttons[i].Hook();
                buttons[i].OnButtonPress += MenuScreen_OnButtonPress;
                buttons[i].OnMouseEntry += MenuScreen_OnMouseEntry;
                buttons[i].LayerDepth = 1f;

                buttonsText[i] = new TextWidget(wr);
                buttonsText[i].Font = f;
                buttonsText[i].OffsetAlignX = Alignment.MID;
                buttonsText[i].OffsetAlignY = Alignment.MID;
                buttonsText[i].Offset = new Point(0, 0);
                buttonsText[i].AlignX = Alignment.MID;
                buttonsText[i].AlignY = Alignment.MID;
                buttonsText[i].Parent = buttons[i];
                buttonsText[i].LayerDepth = 0.9f;
                buttonsText[i].Color = Color.Lime;
                buttonsText[i].Height = (o1.Height * 6) / 7;
            }
            buttons[1].Parent = buttons[0];
            buttons[1].OffsetAlignY = Alignment.BOTTOM;
            buttons[1].Offset = new Point(0, 0);

            buttonsText[0].Text = "Play Game";
            buttonsText[1].Text = "Army Painter";

            buttons[0].Anchor = new Point(0, 0);

            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;

            if(fontDisp != null) {
                fontDisp.Dispose();
                fontDisp = null;
            }

            foreach(var button in buttons) {
                button.OnButtonPress -= MenuScreen_OnButtonPress;
                button.OnMouseEntry -= MenuScreen_OnMouseEntry;
                button.Dispose();
            }
            foreach(var text in buttonsText) text.Dispose();
            wr.Dispose();

            seHover.Dispose();
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);

            wr.Draw(SB);
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
        private void MenuScreen_OnButtonPress(RectButton obj) {
            seClick.Play();
            if(obj == buttons[0]) {
                Next = game.LoadScreen.Index;
                State = ScreenState.ChangeNext;
            }
            else if(obj == buttons[1]) {
                Next = game.ColorSchemeScreen.Index;
                State = ScreenState.ChangeNext;
            }
        }
        private void MenuScreen_OnMouseEntry(RectButton obj) {
            seHover.Play();
        }
    }
}