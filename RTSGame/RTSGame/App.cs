using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using RTSEngine.Interfaces;
using RTSEngine.Data;
using RTSEngine.Data.Parsers;
using RTSEngine.Data.Team;

namespace RTS {
    public static class AppSettings {
        public const string PACKS_DIR = @"Packs";
        public const string CONTENT_DIR = @"Content";
    }

    public class App : BlisterUI.MainGame {
        // The Static Instances
        private static App app;

        // Screens
        public MenuScreen MenuScreen {
            get;
            private set;
        }
        public LoadScreen LoadScreen {
            get;
            private set;
        }
        public RTSScreen RTSScreen {
            get;
            private set;
        }
        public ColorSchemeScreen ColorSchemeScreen {
            get;
            private set;
        }

        public App()
            : base() {
            graphics.IsFullScreen = UserConfig.UseFullscreen;
            IsMouseVisible = true;
            MenuScreen = new RTS.MenuScreen();
            LoadScreen = new RTS.LoadScreen();
            RTSScreen = new RTS.RTSScreen();
            ColorSchemeScreen = new RTS.ColorSchemeScreen();
        }
        
        protected override void FullInitialize() {
            BlisterUI.Input.WMHookInput.Initialize(Window);
            Graphics.IsFullScreen = true;
        }
        protected override void FullLoad() {
        }

        protected override void BuildScreenList() {
            screenList = new BlisterUI.ScreenList(this, 0,
                new BlisterUI.FalseFirstScreen(2),
                new RTSEngine.Screens.InduZtryScreen(1),
                MenuScreen,
                LoadScreen,
                RTSScreen,
                ColorSchemeScreen
                );
        }

        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
        }

        #region Entry Point
        private static void Main(string[] args) {
            UserConfig.Load("user.config");
            using(app = new App()) {
                app.Run();
            }
            UserConfig.Save("user.config");
        }
        #endregion
    }
}