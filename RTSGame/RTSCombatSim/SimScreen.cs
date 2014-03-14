using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using RTSCS.Gameplay;
using RTSCS.Graphics;
using Microsoft.CSharp;
using System.CodeDom.Compiler;
using RTSEngine.Interfaces;
using RTSEngine.Data;
using RTSEngine.Data.Parsers;
using RTSEngine.Data.Team;

namespace RTSCS {
    public class RTSUISpawnArgs {
        public int UnitData;
        public RTSTeam Team;
        public List<ReflectedEntityController> Controllers;
        public List<Vector3> SpawnPos;
        public Vector2[] Waypoints;

        public RTSUISpawnArgs() {
            SpawnPos = new List<Vector3>();
            Controllers = new List<ReflectedEntityController>();
        }
    }

    public class SimScreen : GameScreenIndexed {
        // Instancing And Data Counts 
        public const int MAX_TEAMS = 3;
        public const int MAX_UNITS = 3;
        public const int MAX_INSTANCES_PER_UNIT = 300;
        public const string DEFAULT_ACTION_CONTROLLER = "RTSCS.Controllers.ActionController";
        public const string DEFAULT_MOVEMENT_CONTROLLER = "RTSCS.Controllers.MovementController";
        public const string DEFAULT_NO_MOVEMENT_CONTROLLER = "RTSCS.Controllers.NoMovementController";
        public const string DEFAULT_TARGETTING_CONTROLLER = "RTSCS.Controllers.TargettingController";
        public const string DEFAULT_COMBAT_CONTROLLER = "RTSCS.Controllers.CombatController";

        // Graphics Data
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
            get { return GameState.Teams; }
        }
        public Dictionary<string, ReflectedEntityController> Controllers {
            get;
            private set;
        }
        private System.Collections.Concurrent.ConcurrentBag<RTSUISpawnArgs> toSpawn;

        // Used To Render Units
        UnitGeometry[] unitGeometry;

        public SimScreen(int i)
            : base(i) {
            InitParams();
        }
        public SimScreen(int p, int n)
            : base(p, n) {
            InitParams();
        }

        private void InitParams() {
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
        public override void Build() {
            IsPaused = false;
            toSpawn = new System.Collections.Concurrent.ConcurrentBag<RTSUISpawnArgs>();

            combAnimator = new CombatAnimator(G, "Content\\Textures\\Laser.png");

            renderer = new Renderer(G, XNAEffect.Compile(G, "Content\\FX\\Unit.fx"));
            renderer.View = Matrix.CreateLookAt(new Vector3(0, 0, 1f), Vector3.Zero, Vector3.Up);
            renderer.Projection = Matrix.CreateOrthographic(G.Viewport.Width, G.Viewport.Height, 0, 2f);

            map = new CombatMap(G, @"Content\Textures\Smoke.png");
            map.Tiling = Vector2.One * 6f;
            map.Scaling = Vector2.One * 600f;
            map.Translation = Vector3.Zero;

            unitGeometry = new UnitGeometry[Units.Length];
            unitGeometry[0] = new UnitGeometry(G, "Content\\Textures\\Unit1.png", MAX_INSTANCES_PER_UNIT, Units[0]);
            unitGeometry[1] = new UnitGeometry(G, "Content\\Textures\\Unit2.png", MAX_INSTANCES_PER_UNIT, Units[1]);
            unitGeometry[2] = new UnitGeometry(G, "Content\\Textures\\Unit3.png", MAX_INSTANCES_PER_UNIT, Units[2]);
        }
        public override void Destroy(GameTime gameTime) {
            renderer.Dispose();
            map.Dispose();
        }

        public override void OnEntry(GameTime gameTime) {
            game.IsMouseVisible = true;

            HeightmapResult res = HeightmapParser.Parse(G, new System.IO.DirectoryInfo(@"Packs\Default\maps\0"));
            return;
        }
        public override void OnExit(GameTime gameTime) {
            game.IsMouseVisible = false;
        }

        public override void Update(GameTime gameTime) {
            // Check For Form Error
            if(App.FormException != null)
                throw App.FormException;

            // Kill When Form Is Closed
            if(DataForm.Instance == null) {
                State = ScreenState.ExitApplication;
                return;
            }
            StepGame((float)gameTime.ElapsedGameTime.TotalSeconds);
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);

            renderer.RenderMap(G, map);
            renderer.RenderCombat(G, combAnimator);
            renderer.BeginUnitPass(G);
            foreach(UnitGeometry ug in unitGeometry) {
                ug.InstanceUnits();
                ug.SetBuffers(G);
                ug.DrawUnits(G);
            }
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

            // Camera Functionality
            if(ms.ScrollWheelValue != pms.ScrollWheelValue) {
                if(ms.ScrollWheelValue > pms.ScrollWheelValue)
                    renderer.Projection *= Matrix.CreateScale(1.1f, 1.1f, 1);
                else
                    renderer.Projection *= Matrix.CreateScale(1 / 1.1f, 1 / 1.1f, 1);
            }
            if(ks.IsKeyDown(Keys.LeftControl) && (pms.X != ms.X || pms.Y != ms.Y))
                renderer.Projection *= Matrix.CreateTranslation(
                    2f * (ms.X - pms.X) / (float)G.Viewport.Width,
                    2f * (pms.Y - ms.Y) / (float)G.Viewport.Height,
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
                Vector3 target = G.Viewport.Unproject(
                    new Vector3(ms.X, ms.Y, 1),
                    renderer.Projection,
                    renderer.View,
                    Matrix.Identity
                    );
                Vector3 source = G.Viewport.Unproject(
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
                foreach(var team in GameState.Teams) {
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
            foreach(RTSTeam team in GameState.Teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.ActionController.DecideAction(GameState, dt);
                }
            }

            // Apply Controllers
            foreach(RTSTeam team in GameState.Teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.ActionController.ApplyAction(GameState, dt);
                }
            }

            // Collision
            foreach(RTSTeam team in GameState.Teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.CollisionGeometry.Center = unit.GridPosition;
                }
            }
            foreach(RTSTeam team1 in GameState.Teams) {
                foreach(RTSUnitInstance unit1 in team1.Units) {
                    foreach(RTSTeam team2 in GameState.Teams) {
                        foreach(RTSUnitInstance unit2 in team2.Units) {
                            if(unit1 == unit2) continue;
                            CollisionController.ProcessCollision(unit1.CollisionGeometry, unit2.CollisionGeometry);
                        }
                    }
                }
            }
            foreach(RTSTeam team in GameState.Teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.GridPosition = unit.CollisionGeometry.Center;
                }
            }

            // Kill Dead Units
            foreach(RTSTeam team in GameState.Teams) {
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
                    var squad = sa.Team.AddSquad();
                    var u = sa.Team.AddUnit(sa.UnitData, sa.SpawnPos[0]);
                    int ci = 0;
                    foreach(var rec in sa.Controllers) {
                        var reci = rec.CreateInstance();
                        reci.SetEntity(u);
                        if(rec.ControllerType.HasFlag(EntityControllerType.Action) && ci < 1) {
                            u.ActionController = reci as IActionController;
                        }
                        if(rec.ControllerType.HasFlag(EntityControllerType.Combat) && ci < 2) {
                            u.CombatController = reci as ICombatController;
                        }
                        if(rec.ControllerType.HasFlag(EntityControllerType.Movement) && ci < 3) {
                            u.MovementController = reci as IMovementController;
                        }
                        ci++;
                    }
                    if(u.MovementController != null)
                        u.MovementController.SetWaypoints(sa.Waypoints);

                    // Add Events
                    u.OnDestruction += OnUnitDeath;
                    u.OnDamage += OnUnitDamage;
                    u.OnNewTarget += OnUnitNewTarget;
                    u.OnAttackMade += OnUnitCombat;
                    foreach(var ug in unitGeometry) {
                        if(ug.UnitData == u.UnitData)
                            ug.AddUnit(u, u.Team.ColorSheme);
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

    }
}