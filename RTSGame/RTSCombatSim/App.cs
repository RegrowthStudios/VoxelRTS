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

namespace RTSCS {
    public class App : BlisterUI.MainGame {
        // The Static Instances
        private static App app;

        public LoadScreen LoadScreen {
            get;
            private set;
        }
        public RTSScreen Sim3D {
            get;
            private set;
        }

        public App()
            : base() {
            graphics.IsFullScreen = UserConfig.UseFullscreen;
            LoadScreen = new RTSCS.LoadScreen(2);
            Sim3D = new RTSScreen();
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
                LoadScreen,
                Sim3D
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