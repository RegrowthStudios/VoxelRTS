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
using RTSCS.Gameplay;
using RTSCS.Graphics;

namespace RTSCS {
    public class App : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Renderer renderer;
        CombatMap map;

        public App() {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {


            base.Initialize();
        }
        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            renderer = new Renderer(GraphicsDevice);
            renderer.View = Matrix.CreateLookAt(new Vector3(0, 0, -1f), Vector3.Zero, Vector3.Up);
            renderer.Projection = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 2f);

            map = new CombatMap(GraphicsDevice, @"Content\Textures\Smoke.png");
            map.Tiling = Vector2.One * 10f;
            map.Scaling = Vector2.One * 200f;
            map.Translation = Vector3.Zero;
        }
        protected override void UnloadContent() {
            renderer.Dispose();
            map.Dispose();
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
            renderer.RenderMap(GraphicsDevice, map);
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