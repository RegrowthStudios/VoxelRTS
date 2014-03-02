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
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using RTSEngine.Data;
using RTSEngine.Data.Team;

namespace RTSCS {
    public class GameRestartArgs {
        public RTSTeam[] Teams;
        public RTSUnit[] Units;
    };

    public class App : Microsoft.Xna.Framework.Game {
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Renderer renderer;
        CombatMap map;
        GameRestartArgs rArgs;
        GameState gameState;


        public App() {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }

        protected override void Initialize() {
            rArgs = null;

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

            StepGame((float)gameTime.ElapsedGameTime.TotalSeconds);

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);
            renderer.RenderMap(GraphicsDevice, map);
            base.Draw(gameTime);
        }

        public void CheckRestartGame() {
            // Check For Restart
            if(rArgs == null) return;

            // TODO: Restart
            gameState = new GameState(rArgs.Teams);
            foreach(var u in rArgs.Units) {
                gameState.AddRTSUnit(u);
            }

            rArgs = null;
        }
        public void StepGame(float dt) {
            CheckRestartGame();
            if (gameState == null) return;

            // Find Decisions
            foreach(RTSTeam team in gameState.teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.ActionController.DecideAction(gameState, dt);
                }
            }

            // Apply Controllers
            foreach(RTSTeam team in gameState.teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.ActionController.ApplyAction(gameState, dt);
                }
            }

            // 
        }

        static void Main(string[] args) {
            using(App game = new App()) {
                DataForm form = null;

                // Create Form Thread
                Thread t = new Thread(() => {
                    using(form = new DataForm()) {
                        form.OnGameRestart += (rA) => {
                            game.rArgs = rA;
                        };
                        form.ShowDialog();
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
                game.Run();

                // Stop The Form Thread
                if(DataForm.Instance != null && form != null) {
                    form.Invoke(form.Closer);
                    while(DataForm.Instance != null) {
                        Thread.Sleep(500);
                        Console.WriteLine("Waiting For Form Thread To Stop");
                    }
                }
            }
        }
    }
}