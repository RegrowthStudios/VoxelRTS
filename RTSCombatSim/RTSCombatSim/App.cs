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
using RTSCS.Controllers;

namespace RTSCS {
    public class GameRestartArgs {
        public RTSTeam[] Teams;
        public RTSUnit[] Units;
    };

    public class App : Microsoft.Xna.Framework.Game {
        const int MAX_TEAMS = 3;
        const int MAX_INSTANCES_PER_UNIT = 100;

        static readonly Color[] teamColors = new Color[]{
            Color.Red,
            Color.Blue,
            Color.Green
        };

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Renderer renderer;
        CombatMap map;
        GameRestartArgs rArgs;
        GameState gameState;
        UnitGeometry[] unitGeometry;

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

            RTSUnit[] units = new RTSUnit[1];
            units[0] = new RTSUnit();
            units[0].BaseCombatData.Armor = 0;
            units[0].BaseCombatData.AttackDamage = 10;
            units[0].BaseCombatData.AttackTimer = 1f;
            units[0].BaseCombatData.CriticalChance = 0.5;
            units[0].BaseCombatData.CriticalDamage = 20;
            units[0].BaseCombatData.MaxRange = 20;
            units[0].BaseCombatData.MinRange = 0;
            units[0].Health = 100;
            units[0].ICollidableShape = new CollisionCircle(3, Vector2.Zero);
            units[0].MovementSpeed = 2f;

            RTSTeam[] teams = new RTSTeam[1];
            teams[0] = new RTSTeam();
            var u = teams[0].AddUnit(units[0], Vector3.Zero);
            u.ActionController = new ActionController(u);
            u.CombatController = new CombatController(u);
            u.MovementController = new MovementContoller(new Vector2[] { new Vector2(10, 10) });
            u.TargettingController = new TargettingController(u);

            rArgs = new GameRestartArgs();
            rArgs.Teams = teams;
            rArgs.Units = units;
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

            foreach(UnitGeometry ug in unitGeometry) {
                ug.InstanceUnits();
                ug.Draw(GraphicsDevice);
            }

            base.Draw(gameTime);
        }

        public void CheckRestartGame() {
            // Check For Restart
            if(rArgs == null) return;

            // TODO: Restart
            gameState = new GameState(rArgs.Teams);
            unitGeometry = new UnitGeometry[rArgs.Units.Length];
            for(int i = 0; i < rArgs.Units.Length; i++) {
                gameState.AddRTSUnit(rArgs.Units[i]);
                unitGeometry[i] = new UnitGeometry(GraphicsDevice, new VertexPositionColor[] {
                    new VertexPositionColor(new Vector3(-1, 1, 0), Color.White),
                    new VertexPositionColor(new Vector3(1, 1, 0), Color.White),
                    new VertexPositionColor(new Vector3(-1, -1, 0), Color.White),
                    new VertexPositionColor(new Vector3(1, -1, 0), Color.White)
                }, new int[]{
                    0, 1, 2,
                    2, 1, 3
                }, MAX_INSTANCES_PER_UNIT, rArgs.Units[i]);
                int ti = 0;
                foreach(var team in gameState.teams) {
                    foreach(var unit in team.Units) {
                        if(unit.UnitData == rArgs.Units[i])
                            unitGeometry[i].AddUnit(unit, teamColors[ti]);
                    }
                    ti++;
                }
            }


            rArgs = null;
        }
        static bool IsDead(RTSUnitInstance u) {
            return u.Health < 0;
        }
        public void StepGame(float dt) {
            CheckRestartGame();
            if(gameState == null) return;

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

            // Kill Dead Units
            foreach(RTSTeam team in gameState.teams) {
                team.RemoveAll(IsDead);
            }
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