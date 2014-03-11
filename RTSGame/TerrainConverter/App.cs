using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;

namespace TerrainConverter {
    public class App : MainGame {
        protected override void BuildScreenList() {
            screenList = new ScreenList(this, 0,
                new FalseFirstScreen(1),
                new TerrainScreen(1)
                );
        }

        protected override void FullInitialize() {
            WMHookInput.Initialize(Window);
        }
        protected override void FullLoad() {
        }

        #region Entry Point
        [STAThread]
        static void Main(string[] args) {
            using(App app = new App()) {
                app.Run();
            }
        }
        #endregion
    }
}