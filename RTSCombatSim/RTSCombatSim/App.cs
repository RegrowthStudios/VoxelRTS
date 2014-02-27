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

namespace RTSCS {
    public class App : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;

        public App() {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {


            base.Initialize();
        }
        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);

        }
        protected override void UnloadContent() {
        }

        protected override void Update(GameTime gameTime) {
            if(DataForm.Instance == null) {
                Exit();
                return;
            }
            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            base.Draw(gameTime);
        }

        static void Main(string[] args) {
            // Create Form Thread
            Thread t = new Thread(() => {
                using(var f = new DataForm()) {
                    f.ShowDialog();
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.Priority = ThreadPriority.Lowest;
            t.IsBackground = true;
            t.Start();

            // Wait For Max 1 Second To Initialize The Form Else Exit
            int trials = 10;
            while(trials > 0) {
                if(DataForm.Instance != null) {
                    trials = 10;
                    break;
                }
                Thread.Sleep(100);
                trials--;
            }

            // Run The Simulator
            using(App game = new App()) {
                game.Run();
            }

            // Stop The Form Thread
            if(t.ThreadState != ThreadState.Stopped || t.ThreadState != ThreadState.Aborted) {
                t.Abort();
                while(t.ThreadState == ThreadState.Running || t.ThreadState != ThreadState.Aborted) {
                    Thread.Sleep(500);
                    Console.WriteLine("Waiting For Form Thread To Stop");
                }
            }
        }
    }
}