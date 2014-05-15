using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
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
using RTSEngine.Graphics;
using RTSEngine.Controllers;

namespace RTS {
    public static class AppSettings {
        public const string PACKS_DIR = @"Packs";
        public const string CONTENT_DIR = @"Content";
    }

    public class App : BlisterUI.MainGame {
        public const string USER_CONFIG_FILE_PATH = "user.config";

        // The Static Instances
        private static App app;

        // Screens
        public CompanyScreen CompanyScreen {
            get;
            private set;
        }
        public WispScreen WispScreen {
            get;
            private set;
        }
        public InduZtryScreen InduZtryScreen {
            get;
            private set;
        }
        public PressEnterScreen PressEnterScreen {
            get;
            private set;
        }
        public LoginScreen LoginScreen {
            get;
            private set;
        }
        public MenuScreen MenuScreen {
            get;
            private set;
        }
        public LobbyScreen LobbyScreen {
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
        public LEScreen LEScreen {
            get;
            private set;
        }
        public ColorSchemeScreen ColorSchemeScreen {
            get;
            private set;
        }

        public MouseRenderer mRenderer;
        public Texture2D tMouseMain;
        public DevConsoleView dcv;
        public BasicEffect fx;

        public App()
            : base() {
            // Make Sure We Are Using The Most Recent Graphics Version
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredBackBufferWidth = UserConfig.ResolutionWidth;
            graphics.PreferredBackBufferHeight = UserConfig.ResolutionHeight;

            // Get User Config Parameter For Fullscreen
            graphics.IsFullScreen = UserConfig.UseFullscreen;

            CompanyScreen = new RTS.CompanyScreen();
            WispScreen = new RTS.WispScreen();
            InduZtryScreen = new RTS.InduZtryScreen();
            PressEnterScreen = new RTS.PressEnterScreen();
            LoginScreen = new RTS.LoginScreen();
            MenuScreen = new RTS.MenuScreen();
            LobbyScreen = new RTS.LobbyScreen();
            LoadScreen = new RTS.LoadScreen();
            RTSScreen = new RTS.RTSScreen();
            LEScreen = new RTS.LEScreen();
            ColorSchemeScreen = new RTS.ColorSchemeScreen();
        }

        protected override void FullInitialize() {
            ZXPCExt.AddXNATypes();

            BlisterUI.Input.WMHookInput.Initialize(Window);
            fx = new BasicEffect(GraphicsDevice);
        }
        protected override void FullLoad() {
            using(var s = System.IO.File.OpenRead(@"Content\UI\Mouse\Main.png")) {
                tMouseMain = Texture2D.FromStream(GraphicsDevice, s);
            }
            mRenderer = new MouseRenderer(GraphicsDevice, Window);
            mRenderer.Texture = tMouseMain;
            mRenderer.InnerRadius = 28f;
            dcv = new DevConsoleView(Content, @"Fonts\CourierNew14", GraphicsDevice);

            ZXParser.SetEnvironment("GD", GraphicsDevice);
            ZXParser.SetEnvironment("GDM", graphics);
            ZXParser.SetEnvironment("Window", Window);
        }

        protected override void BuildScreenList() {
            screenList = new BlisterUI.ScreenList(this, 0,
                new BlisterUI.FalseFirstScreen(1),
                CompanyScreen,
                WispScreen,
                InduZtryScreen,
                PressEnterScreen,
                LoginScreen,
                MenuScreen,
                LobbyScreen,
                LoadScreen,
                RTSScreen,
                LEScreen,
                ColorSchemeScreen
                );
        }

        protected override void Update(GameTime gameTime) {
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            base.Draw(gameTime);
        }

        protected override void FullQuit(GameTime gameTime) {
            base.FullQuit(gameTime);
            tMouseMain.Dispose();
            mRenderer.Dispose();
            dcv.Dispose();
        }

        public void DrawMouse() {
            mRenderer.BeginPass(GraphicsDevice);
            mRenderer.Draw(GraphicsDevice);
        }
        public void DrawDevConsole() {
            if(DevConsole.IsActivated) {
                spriteBatch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
                dcv.Draw(spriteBatch, Vector2.Zero);
                spriteBatch.End();
            }
        }

        #region Entry Point
        [MTAThread]
        private static void Main(string[] args) {
            UserConfig.Load(USER_CONFIG_FILE_PATH);
            using(app = new App()) {
                app.Run();
            }
            UserConfig.Save(USER_CONFIG_FILE_PATH);
        }
        #endregion
    }
}