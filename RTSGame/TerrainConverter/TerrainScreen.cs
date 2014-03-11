using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;

namespace TerrainConverter {
    public class TerrainScreen : GameScreenIndexed {
        public TerrainScreen(int i) : base(i) { }
        public TerrainScreen(int p, int n) : base(p, n) { }

        private bool rebuild;

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;
            rebuild = false;
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
        }

        void KeyboardEventDispatcher_OnKeyPressed(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.F2:
                    string file;
                    using(var f = new System.Windows.Forms.OpenFileDialog()) {
                        f.CheckFileExists = true;
                        f.CheckPathExists = true;
                        f.ShowDialog();
                        file = f.FileName;
                    }
                    
                    // Try To Open The File
                    Console.WriteLine("Trying To Open File: \n{0}", file);


                    rebuild = true;
                    break;
            }
        }
    }
}
