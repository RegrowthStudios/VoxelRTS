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
using System.Diagnostics;
using System.ComponentModel;
using RTSEngine.Data.Parsers;

namespace RTS {
    public class MenuScreen : GameScreen<App> {
        private const string UIC_FILE = @"Content\UI\Config\MainMenu.uic";

        public override int Next {
            get;
            protected set;
        }
        public override int Previous {
            get { return game.LoginScreen.Index; }
            protected set { }
        }

        // The UI Config Data
        private UICMainMenu uic;

        WidgetRenderer wr;
        IDisposable fontDisp;
        RectButton[] buttons;
        TextWidget[] buttonsText;
        TextWidget txtMainMenu;
        SoundEffect seHover, seClick;
        Texture2D[] tPanels;

        public override void Build() {
            using(var fs = File.OpenRead(UIC_FILE)) {
                uic = new UICMainMenu();
                ZXParser.ParseInto(new StreamReader(fs).ReadToEnd(), uic);
            }
            using(var s = File.OpenRead(uic.SoundClick)) {
                seClick = SoundEffect.FromStream(s);
            }
        }
        public override void Destroy(GameTime gameTime) {
            seClick.Dispose();
        }

        public override void OnEntry(GameTime gameTime) {
            SpriteFont f = XNASpriteFont.Compile(G, uic.Font, uic.FontResolution, out fontDisp);
            wr = new WidgetRenderer(G, f);

            using(var s = File.OpenRead(uic.SoundHover)) {
                seHover = SoundEffect.FromStream(s);
            }

            buttons = new RectButton[uic.Buttons.Length];
            buttonsText = new TextWidget[buttons.Length];
            tPanels = new Texture2D[buttons.Length];
            int bw = G.Viewport.Width;
            bw -= (buttons.Length + 1) * uic.ButtonSpacing.X;
            bw /= buttons.Length;
            int bh = G.Viewport.Height - uic.ButtonSpacing.Y * 3 - uic.TitlePanelTextSize;
            for(int i = 0; i < buttons.Length; i++) {
                var uicButton = uic.Buttons[i];
                using(var s = File.OpenRead(uicButton.ImageFile)) {
                    tPanels[i] = Texture2D.FromStream(G, s);
                }
                buttons[i] = new RectButton(wr, bw, bh, uicButton.ColorInactive, uicButton.ColorActive, tPanels[i]);
                buttons[i].Hook();
                buttons[i].OnButtonPress += MenuScreen_OnButtonPress;
                buttons[i].OnMouseEntry += MenuScreen_OnMouseEntry;
                buttons[i].LayerDepth = 1f;
                buttons[i].OffsetAlignX = Alignment.RIGHT;
                buttons[i].Offset = new Point(uic.ButtonSpacing.X, 0);
                if(i > 0)
                    buttons[i].Parent = buttons[i - 1];

                buttonsText[i] = new TextWidget(wr);
                buttonsText[i].Font = f;
                buttonsText[i].OffsetAlignX = Alignment.MID;
                buttonsText[i].OffsetAlignY = Alignment.TOP;
                buttonsText[i].Offset = new Point(0, 30);
                buttonsText[i].AlignX = Alignment.MID;
                buttonsText[i].AlignY = Alignment.MID;
                buttonsText[i].Parent = buttons[i];
                buttonsText[i].LayerDepth = 0.9f;
                buttonsText[i].Color = uicButton.ColorText;
                buttonsText[i].Height = uic.ButtonTextSize;
                buttonsText[i].Text = uicButton.Text;
            }
            buttons[0].Anchor = new Point(uic.ButtonSpacing.X, uic.TitlePanelTextSize + 2 * uic.ButtonSpacing.Y);

            txtMainMenu = new TextWidget(wr);
            txtMainMenu.Anchor = new Point(G.Viewport.Width / 2, uic.ButtonSpacing.Y);
            txtMainMenu.Height = uic.TitlePanelTextSize;
            txtMainMenu.AlignX = Alignment.MID;
            txtMainMenu.Color = uic.ColorTitleText;
            txtMainMenu.Text = uic.TitlePanelText;

            KeyboardEventDispatcher.OnKeyPressed += OnKeyPressed;
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= OnKeyPressed;

            if(fontDisp != null) {
                fontDisp.Dispose();
                fontDisp = null;
            }

            foreach(var button in buttons) {
                button.OnButtonPress -= MenuScreen_OnButtonPress;
                button.OnMouseEntry -= MenuScreen_OnMouseEntry;
                button.Dispose();
            }
            buttons = null;
            foreach(var bt in buttonsText) {
                bt.Dispose();
            }
            buttonsText = null;
            foreach(var t in tPanels) {
                t.Dispose();
            }
            tPanels = null;
            wr.Dispose();

            seHover.Dispose();
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(uic.ColorBackground);
            wr.Draw(SB);
            game.mRenderer.BeginPass(G);
            game.mRenderer.Draw(G);
        }

        private void OnKeyPressed(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.Escape:
                    State = ScreenState.ExitApplication;
                    break;
            }
        }
        private void MenuScreen_OnButtonPress(RectButton obj, Vector2 m) {
            seClick.Play();
            if(obj == buttons[0]) {
                Next = game.LobbyScreen.Index;
                State = ScreenState.ChangeNext;
            }
            else if(obj == buttons[1]) {
                Next = game.ColorSchemeScreen.Index;
                State = ScreenState.ChangeNext;
            }
            else if(obj == buttons[2]) {
                Next = game.LEScreen.Index;
                State = ScreenState.ChangeNext;

                //RTSEngine.Data.UserConfig.UseFullscreen = !RTSEngine.Data.UserConfig.UseFullscreen;
                //RTSEngine.Data.UserConfig.Save(App.USER_CONFIG_FILE_PATH);
                //ProcessStartInfo psi = new ProcessStartInfo("RTS.exe");
                //psi.WorkingDirectory = Process.GetCurrentProcess().StartInfo.WorkingDirectory;
                //Process.Start(psi);
                //State = ScreenState.ExitApplication;
            }
            else if(obj == buttons[3]) {
                State = ScreenState.ExitApplication;
            }
        }
        private void MenuScreen_OnMouseEntry(RectButton obj, Vector2 m) {
            seHover.Play();
        }
    }
}