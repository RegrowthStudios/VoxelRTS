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
using RTSCS.Gameplay;
using RTSCS.Graphics;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using RTSEngine.Interfaces;
using RTSEngine.Data;
using RTSEngine.Data.Parsers;
using RTSEngine.Data.Team;

namespace RTSCS {
    public struct RTSUISpawnArgs {
        public RTSUnit UnitData;
        public RTSTeam Team;
        public IActionController AC;
        public ICombatController CC;
        public IMovementController MC;
        public ITargettingController TC;
        public Vector3 SpawnPos;
        public Vector2[] Waypoints;
        public Color Color;
    }

    public class App : Microsoft.Xna.Framework.Game {
        // Instancing And Data Counts
        public const int MAX_TEAMS = 3;
        public const int MAX_UNITS = 3;
        public const int MAX_INSTANCES_PER_UNIT = 300;
        public const string DEFAULT_ACTION_CONTROLLER = "RTSCS.Controllers.ActionController";
        public const string DEFAULT_MOVEMENT_CONTROLLER = "RTSCS.Controllers.MovementController";
        public const string DEFAULT_NO_MOVEMENT_CONTROLLER = "RTSCS.Controllers.NoMovementController";
        public const string DEFAULT_TARGETTING_CONTROLLER = "RTSCS.Controllers.TargettingController";
        public const string DEFAULT_COMBAT_CONTROLLER = "RTSCS.Controllers.CombatController";

        // The Static Instances
        private static App app;
        private static DataForm form;
        private static Exception formException;

        // Graphics Data
        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        Renderer renderer;
        CombatAnimator combAnimator;
        CombatMap map;

        // For Pausing The Simulation
        public bool IsPaused {
            get;
            set;
        }

        // Input States
        private KeyboardState ks, pks;
        private MouseState ms, pms;

        // The Game State
        public GameState GameState {
            get;
            private set;
        }
        public RTSUnit[] Units {
            get;
            private set;
        }
        public RTSTeam[] Teams {
            get { return GameState.teams; }
        }
        public Dictionary<string, ReflectedEntityController> Controllers {
            get;
            private set;
        }
        private System.Collections.Concurrent.ConcurrentBag<RTSUISpawnArgs> toSpawn;

        // Used To Render Units
        UnitGeometry[] unitGeometry;

        public App() {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
            IsPaused = false;
            toSpawn = new System.Collections.Concurrent.ConcurrentBag<RTSUISpawnArgs>();

            // Setup Teams
            RTSTeam[] teams = new RTSTeam[MAX_TEAMS];
            for(int i = 0; i < teams.Length; i++)
                teams[i] = new RTSTeam();

            // Setup Units
            Units = new RTSUnit[MAX_UNITS];
            for(int i = 0; i < Units.Length; i++) {
                Units[i] = new RTSUnit();
            }

            // Create Game State
            GameState = new GameState(teams);
            foreach(var uD in Units)
                GameState.AddRTSUnit(uD);

            // Add Controllers
            Controllers = new Dictionary<string, ReflectedEntityController>();
            CompiledEntityControllers cec;
            string error;
            string[] references = {
                "System.dll",
                "System.Core.dll",
                "System.Data.dll",
                "System.Xml.dll",
                "System.Xml.Linq.dll",
                @"lib\Microsoft.Xna.Framework.dll",
                "RTSEngine.dll"
            };
            cec = EntityControllerParser.Compile(@"Controllers\ActionController.cs", references, out error);
            foreach(KeyValuePair<string, ReflectedEntityController> kv in cec.Controllers)
                Controllers.Add(kv.Key, kv.Value);
            cec = EntityControllerParser.Compile(@"Controllers\MovementController.cs", references, out error);
            foreach(KeyValuePair<string, ReflectedEntityController> kv in cec.Controllers)
                Controllers.Add(kv.Key, kv.Value);
            cec = EntityControllerParser.Compile(@"Controllers\NoMovementController.cs", references, out error);
            foreach(KeyValuePair<string, ReflectedEntityController> kv in cec.Controllers)
                Controllers.Add(kv.Key, kv.Value);
            cec = EntityControllerParser.Compile(@"Controllers\CombatController.cs", references, out error);
            foreach(KeyValuePair<string, ReflectedEntityController> kv in cec.Controllers)
                Controllers.Add(kv.Key, kv.Value);
            cec = EntityControllerParser.Compile(@"Controllers\TargettingController.cs", references, out error);
            foreach(KeyValuePair<string, ReflectedEntityController> kv in cec.Controllers)
                Controllers.Add(kv.Key, kv.Value);
        }

        protected override void Initialize() {
            IsMouseVisible = true;

            base.Initialize();
        }
        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            combAnimator = new CombatAnimator(GraphicsDevice, "Content\\Textures\\Laser.png");

            renderer = new Renderer(GraphicsDevice, XNAEffect.Compile(GraphicsDevice, "Content\\FX\\Unit.fx"));
            renderer.View = Matrix.CreateLookAt(new Vector3(0, 0, 1f), Vector3.Zero, Vector3.Up);
            renderer.Projection = Matrix.CreateOrthographic(GraphicsDevice.Viewport.Width, GraphicsDevice.Viewport.Height, 0, 2f);

            map = new CombatMap(GraphicsDevice, @"Content\Textures\Smoke.png");
            map.Tiling = Vector2.One * 6f;
            map.Scaling = Vector2.One * 600f;
            map.Translation = Vector3.Zero;

            unitGeometry = new UnitGeometry[Units.Length];
            unitGeometry[0] = new UnitGeometry(GraphicsDevice, "Content\\Textures\\Unit1.png", MAX_INSTANCES_PER_UNIT, Units[0]);
            unitGeometry[1] = new UnitGeometry(GraphicsDevice, "Content\\Textures\\Unit2.png", MAX_INSTANCES_PER_UNIT, Units[1]);
            unitGeometry[2] = new UnitGeometry(GraphicsDevice, "Content\\Textures\\Unit3.png", MAX_INSTANCES_PER_UNIT, Units[2]);
        }
        protected override void UnloadContent() {
            renderer.Dispose();
            map.Dispose();
        }

        protected override void Update(GameTime gameTime) {
            // Check For Form Error
            if(formException != null)
                throw formException;

            // Kill When Form Is Closed
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
            renderer.RenderCombat(GraphicsDevice, combAnimator);
            renderer.BeginUnitPass(GraphicsDevice);
            foreach(UnitGeometry ug in unitGeometry) {
                ug.InstanceUnits();
                ug.SetBuffers(GraphicsDevice);
                ug.DrawUnits(GraphicsDevice);
            }

            base.Draw(gameTime);
        }

        private static bool IsDeadApplier(RTSUnitInstance u) {
            if(!u.IsAlive) {
                u.Destroy();
                return true;
            }
            return false;
        }
        private static bool IsDead(RTSUnitInstance u) {
            return !u.IsAlive;
        }

        // Game Controller
        public void StepGame(float dt) {
            // Get Keyboard Input
            pks = ks; pms = ms;
            ks = Keyboard.GetState();
            ms = Mouse.GetState();

            if(ms.ScrollWheelValue != pms.ScrollWheelValue) {
                if(ms.ScrollWheelValue > pms.ScrollWheelValue)
                    renderer.Projection *= Matrix.CreateScale(1.1f, 1.1f, 1);
                else
                    renderer.Projection *= Matrix.CreateScale(1 / 1.1f, 1 / 1.1f, 1);
            }
            if(ks.IsKeyDown(Keys.LeftControl) && (pms.X != ms.X || pms.Y != ms.Y))
                renderer.Projection *= Matrix.CreateTranslation(
                    2f * (ms.X - pms.X) / (float)GraphicsDevice.Viewport.Width,
                    2f * (pms.Y - ms.Y) / (float)GraphicsDevice.Viewport.Height,
                    0);

            // Toggle Pausing
            if(ks.IsKeyDown(Keys.P) && !pks.IsKeyDown(Keys.P))
                IsPaused = !IsPaused;
            if(ks.IsKeyDown(Keys.W))
                dt /= 14f;
            else if(ks.IsKeyDown(Keys.Q))
                dt /= 4f;


            // Don't Run When Paused
            if(IsPaused) return;

            // Check For Selections
            bool[] selections = {
                ks.IsKeyDown(Keys.D1),
                ks.IsKeyDown(Keys.D2),
                ks.IsKeyDown(Keys.D3)
            };

            // Check For New Location
            if(ms.RightButton == ButtonState.Pressed && pms.RightButton != ButtonState.Pressed) {
                Vector3 target = GraphicsDevice.Viewport.Unproject(
                    new Vector3(ms.X, ms.Y, 1),
                    renderer.Projection,
                    renderer.View,
                    Matrix.Identity
                    );
                Vector3 source = GraphicsDevice.Viewport.Unproject(
                    new Vector3(ms.X, ms.Y, 0),
                    renderer.Projection,
                    renderer.View,
                    Matrix.Identity
                    );
                Vector3 dir = Vector3.Normalize(target - source);
                Ray mouseRay = new Ray(source, dir);
                Plane pPlane = new Plane(Vector3.Backward, 0);
                float? distN = mouseRay.Intersects(pPlane);
                float dist = distN.HasValue ? distN.Value : 0;
                Vector3 loc = mouseRay.Position + mouseRay.Direction * dist;
                int ti = 0;
                foreach(var team in GameState.teams) {
                    if(selections[ti]) {
                        foreach(RTSUnitInstance unit in team.Units) {
                            unit.MovementController = Controllers[DEFAULT_MOVEMENT_CONTROLLER].CreateInstance() as IMovementController;
                            unit.MovementController.SetWaypoints(new Vector2[] { new Vector2(loc.X, loc.Y) });
                        }
                    }
                    ti++;
                }
            }

            // Find Decisions
            foreach(RTSTeam team in GameState.teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.ActionController.DecideAction(GameState, dt);
                }
            }

            // Apply Controllers
            foreach(RTSTeam team in GameState.teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.ActionController.ApplyAction(GameState, dt);
                }
            }

            // Collide
            foreach(RTSTeam team in GameState.teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.CollisionGeometry.Center = unit.GridPosition;
                }
            }
            foreach(RTSTeam team1 in GameState.teams) {
                foreach(RTSUnitInstance unit1 in team1.Units) {
                    foreach(RTSTeam team2 in GameState.teams) {
                        foreach(RTSUnitInstance unit2 in team2.Units) {
                            if(unit1 == unit2) continue;
                            CollisionController.ProcessCollision(unit1.CollisionGeometry, unit2.CollisionGeometry);
                        }
                    }
                }
            }
            foreach(RTSTeam team in GameState.teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.GridPosition = unit.CollisionGeometry.Center;
                }
            }

            // Kill Dead Units
            foreach(RTSTeam team in GameState.teams) {
                team.RemoveAll(IsDeadApplier);
            }
            foreach(var ug in unitGeometry) {
                ug.RemoveAll(IsDead);
            }

            // Spawn New Units
            int c = toSpawn.Count;
            RTSUISpawnArgs sa;
            for(int i = 0; i < c; i++) {
                if(toSpawn.TryTake(out sa)) {
                    var u = sa.Team.AddUnit(sa.UnitData, sa.SpawnPos);
                    u.ActionController = sa.AC;
                    u.CombatController = sa.CC;
                    u.MovementController = sa.MC;
                    if(u.MovementController != null)
                        u.MovementController.SetWaypoints(sa.Waypoints);
                    u.TargettingController = sa.TC;

                    // Add Events
                    u.OnDestruction += OnUnitDeath;
                    u.OnDamage += OnUnitDamage;
                    u.OnNewTarget += OnUnitNewTarget;
                    u.OnAttackMade += OnUnitCombat;
                    foreach(var ug in unitGeometry) {
                        if(ug.UnitData == u.UnitData)
                            ug.AddUnit(u, sa.Color);
                    }
                }
                else i--;
            }

            combAnimator.Update(dt);
        }

        public void AddNewUnit(RTSUISpawnArgs u) {
            toSpawn.Add(u);
        }

        private static void OnUnitDeath(IEntity e) {
            Console.WriteLine("Entity [{0,12}] Was Killed", e.GetHashCode());
        }
        private static void OnUnitNewTarget(IEntity s, IEntity t) {
            Console.WriteLine("Entity [{0,12}] Is Now Targeting [{1,12}]", s.GetHashCode(), t == null ? "NONE" : t.GetHashCode().ToString());
        }
        private static void OnUnitDamage(IDestructibleEntity e, int d) {
            Console.WriteLine("Entity [{0,12}] Was Damaged By {1,-5} - Health = {2}", e.GetHashCode(), d, e.Health);
        }
        private void OnUnitCombat(ICombatEntity s, IDestructibleEntity t) {
            RTSUnitInstance rs = s as RTSUnitInstance;
            RTSUnitInstance rt = t as RTSUnitInstance;
            combAnimator.Add(new CombatAnimation(
                rs.WorldPosition, rt.WorldPosition, rs.CollisionGeometry.InnerRadius * 0.9f,
                new Color(1f, 0.4f, 0.1f), new Color(0.1f, 0.8f, 0.2f), rs.UnitData.BaseCombatData.AttackTimer
                ));
        }
        //private void AddDeath(IEntity e) {
        //    RTSUnitInstance u = e as RTSUnitInstance;
        //    Vector3 x = Vector3.UnitX * u.CollisionGeometry.BoundingRadius * 0.7f;
        //    Vector3 y = Vector3.UnitY * u.CollisionGeometry.BoundingRadius * 0.7f;
        //    deathVerts.Add(new VertexPositionTexture(u.WorldPosition - x + y, Vector2.Zero));
        //    deathVerts.Add(new VertexPositionTexture(u.WorldPosition + x + y, Vector2.UnitX));
        //    deathVerts.Add(new VertexPositionTexture(u.WorldPosition - x - y, Vector2.UnitY));
        //    deathVerts.Add(new VertexPositionTexture(u.WorldPosition - x - y, Vector2.UnitY));
        //    deathVerts.Add(new VertexPositionTexture(u.WorldPosition + x + y, Vector2.UnitX));
        //    deathVerts.Add(new VertexPositionTexture(u.WorldPosition + x - y, Vector2.One));
        //}

        #region Entry Point
        private static void RunMainInstance(string[] args) {
            using(app = new App()) {
                form = null;

                // Create Form Thread
                Thread t = new Thread(() => {
                    try {
                        formException = null;
                        using(form = new DataForm(app.Units, app.Teams, app.Controllers)) {
                            form.OnUnitSpawn += app.AddNewUnit;
                            form.ShowDialog();
                        }
                    }
                    catch(Exception e) {
                        formException = e;
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
                app.Run();

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
        private static bool InputRunNew() {
            Console.WriteLine("Want to try and run a new instance?");
            string input = Console.ReadLine().ToLower().Trim();
            switch(input) {
                case "y":
                case "ye":
                case "yes":
                case "yeah":
                case "sure":
                case "why not":
                case "whatever":
                case "ok":
                case "okay":
                case "cool":
                    return true;
            }
            return false;
        }
        private static void Main(string[] args) {
            bool running = true;
            while(running) {
                Console.WriteLine("A New Instance Will Attempt To Be Run\n\n");
                running = false;
                try {
                    // Close Out Of Any Previous Instances
                    try { app.Exit(); app.Dispose(); }
                    catch(Exception) { }
                    try { form.Invoke(form.Closer); form.Dispose(); }
                    catch(Exception) { }

                    RunMainInstance(args);

                    Console.WriteLine("\n\nHooray, no errors appeared.");
                    running = InputRunNew();
                }
                catch(Exception e) {
                    Console.WriteLine(new string('\n', 10));
                    Console.WriteLine("Oh man bro... looks like an exception was thrown:");
                    Console.WriteLine("Exception Type: {0}", e.GetType().Name);
                    Console.WriteLine("Exception Message:\n{0}\n\n", e.Message);
                    Console.WriteLine("Stack Trace:\n{0}", e.StackTrace);
                    running = InputRunNew();
                }
            }
        }
        #endregion
    }
}