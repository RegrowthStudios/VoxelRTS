using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RTSEngine.Data;
using RTSEngine.Interfaces;
using RTSEngine.Data.Team;
using Microsoft.Xna.Framework;
using RTSEngine.Algorithms;
using RTSEngine.Data.Parsers;
using System.IO;
using Grey.Vox.Managers;
using RTSEngine.Graphics;

namespace RTSEngine.Controllers {
    #region Time Budgeting
    class BTaskSquadDecision : ACBudgetedTask {
        private GameState state;
        private RTSSquad squad;

        public BTaskSquadDecision(GameState g, RTSSquad s) :
            base(s.Units.Count) {
            squad = s;
            state = g;
        }

        public override void DoWork(float dt) {
            squad.RecalculateGridPosition();
            if(squad.ActionController != null)
                squad.ActionController.DecideAction(state, dt);
        }
    }
    class BTaskUnitDecision : ACBudgetedTask {
        private GameState state;
        private RTSUnit unit;

        public BTaskUnitDecision(GameState g, RTSUnit u) :
            base(1) {
            unit = u;
            state = g;
        }

        public override void DoWork(float dt) {
            if(unit.ActionController != null)
                unit.ActionController.DecideAction(state, dt);
        }
    }
    class BTaskBuildingDecision : ACBudgetedTask {
        private GameState state;
        private RTSBuilding building;

        public BTaskBuildingDecision(GameState g, RTSBuilding b) :
            base(1) {
            building = b;
            state = g;
        }

        public override void DoWork(float dt) {
            if(building.ActionController != null)
                building.ActionController.DecideAction(state, dt);
        }
    }
    #endregion

    // TODO: Use This From Config
    public struct GCDecisionBudgeting {
        public int SquadBins;
        public int EntityBins;
        public int FOWBins;
    }
    public struct GCInitArgs {
        public string GameTypeScript;
    }

    public class GameplayController : IDisposable {
        public const int SQUAD_BUDGET_BINS = 10;
        public const int ENTITY_BUDGET_BINS = 30;
        public const int FOW_BUDGET_BINS = GameState.MAX_PLAYERS;

        // Queue Of Events
        private LinkedList<GameInputEvent> events;

        // Queue Of Commands
        private Queue<DevCommand> commands;

        // Budgeted Tasks That Do Not Require Immediate Computation
        private TimeBudget tbSquadDecisions;
        private TimeBudget tbEntityDecisions;
        private TimeBudget tbFOWCalculations;

        // Pathfinding
        public Pathfinder pathfinder;

        private List<SquadQuery> squadQueries = new List<SquadQuery> ();

        public struct SquadQuery {
            public RTSSquad Squad;
            public PathQuery Query;

            public SquadQuery(RTSSquad s, PathQuery q) {
                Squad = s;
                Query = q;
            }
        }

        // Vox World Manager
        WorldManager vManager;

        public GameplayController() {
            commands = new Queue<DevCommand>();

            tbSquadDecisions = new TimeBudget(SQUAD_BUDGET_BINS);
            tbEntityDecisions = new TimeBudget(ENTITY_BUDGET_BINS);
            tbFOWCalculations = new TimeBudget(FOW_BUDGET_BINS);
        }
        public void Dispose() {
            DevConsole.OnNewCommand -= OnDevCommand;
            pathfinder.Dispose();
        }

        public void Init(GameState s, GCInitArgs args) {
            DevConsole.OnNewCommand += OnDevCommand;
            tbFOWCalculations.ClearTasks();
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                tbFOWCalculations.AddTask(new FOWTask(s, s.activeTeams[ti].Index));
            }
            pathfinder = new Pathfinder(s);
            vManager = new WorldManager(s.VoxState);

            // Add All Tasks
            foreach(var at in s.activeTeams) {
                foreach(var unit in at.Team.Units) {
                    AddTask(s, unit);
                }
                foreach(var building in at.Team.Buildings) {
                    AddTask(s, building);
                }
                foreach(var squad in at.Team.Squads) {
                    AddTask(s, squad);
                }
            }

            // Start The Game Type Controller
            s.scrGTC = s.Scripts[args.GameTypeScript];
            s.gtC = s.scrGTC.CreateInstance<ACGameTypeController>();
            s.gtC.Load(s, new FileInfo(s.LevelGrid.InfoFile).Directory);
        }

        public void BeginPlaying(GameState s) {
            // Start The Various Threaded Elements
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                s.activeTeams[ti].Team.Input.Begin();
            }
            s.VoxState.VWorkPool.Start(1, System.Threading.ThreadPriority.BelowNormal);
            s.gtC.Start(s);
        }

        // The Update Function
        public void Update(GameState s, float dt) {
            s.IncrementFrame(dt);
            s.gtC.ApplyFrame(s, dt);

            // Input Pass
            ResolveInput(s, dt);
            ApplyInput(s, dt);

            // Apply Any Finished Squad-Level Pathfinding Queries
            ApplySquadQueries();

            // Logic Pass
            ApplyLogic(s, dt);

            // Physics Pass
            ResolvePhysics(s, dt);

            // Cleanup The State
            Cleanup(s, dt);

            vManager.Update();
        }

        // Input Stage
        private void ResolveInput(GameState s, float dt) {
            events = new LinkedList<GameInputEvent>();
            for(int i = 0; i < s.activeTeams.Length; i++) {
                var team = s.activeTeams[i].Team;
                if(team.Input != null)
                    team.Input.AppendEvents(events);
            }
        }
        private void ApplyInput(GameState s, float dt) {
            GameInputEvent e;
            GameEventType eType;

            while(events.Count > 0) {
                e = events.First();
                eType = e.Action;
                switch(eType) {
                    case GameEventType.Select:
                        ApplyInput(s, dt, e as SelectEvent);
                        break;
                    case GameEventType.SetWaypoint:
                        ApplyInput(s, dt, e as SetWayPointEvent);
                        break;
                    case GameEventType.SetTarget:
                        ApplyInput(s, dt, e as SetTargetEvent);
                        break;
                    case GameEventType.SpawnUnit:
                        ApplyInput(s, dt, e as SpawnUnitEvent);
                        break;
                    case GameEventType.SpawnBuilding:
                        ApplyInput(s, dt, e as SpawnBuildingEvent);
                        break;
                    case GameEventType.Impact:
                        ApplyInput(s, dt, e as ImpactEvent);
                        break;
                    case GameEventType.Capital:
                        ApplyInput(s, dt, e as CapitalEvent);
                        break;
                    default:
                        throw new Exception("Event does not exist.");
                }
                events.RemoveFirst();
            }
        }
        private void ApplyInput(GameState s, float dt, SelectEvent e) {
            s.teams[e.Team].Input.Select(e.Selected, e.Append);
        }
        private void ApplyInput(GameState s, float dt, SetWayPointEvent e) {
            RTSTeam team = s.teams[e.Team];
            List<IEntity> selected = team.Input.selected;
            RTSSquad squad = null;
            if(selected != null && selected.Count > 0) {
                foreach(var unit in selected) {
                    RTSUnit u = unit as RTSUnit;
                    if(u != null) {
                        if(squad == null) squad = u.Team.AddSquad();
                        u.Target = null;
                        u.MovementController.Goal = e.Waypoint;
                        squad.Add(u);
                    }
                }
            }
            if(squad != null) {
                AddTask(s, squad);
                SendSquadQuery(s, squad, e);
            }
        }
        private void ApplyInput(GameState s, float dt, SetTargetEvent e) {
            RTSTeam team = s.teams[e.Team];
            List<IEntity> selected = team.Input.selected;

            if(selected != null && selected.Count > 0) {
                RTSSquad squad = null;
                foreach(var unit in selected) {
                    RTSUnit u = unit as RTSUnit;
                    if(u != null) {
                        if(squad == null) squad = u.Team.AddSquad();
                        squad.Add(u);
                    }
                }
                if(squad == null) return;
                // Assign The Target To Every Unit In The Squad
                for(int u = 0; u < squad.Units.Count; u++) {
                    RTSUnit unit = squad.Units[u];
                    unit.Target = e.Target;
                }
                AddTask(s, squad);
                SendSquadQuery(s, squad, e);
            }
        }
        private void SendSquadQuery(GameState s, RTSSquad squad, GameInputEvent e) {
            squad.RecalculateGridPosition();
            Vector2 start = squad.GridPosition;
            var swe = e as SetWayPointEvent;
            var ste = e as SetTargetEvent;
            Vector2 goal = start;
            if(swe != null)
                goal = swe.Waypoint;
            else if(ste != null && ste.Target != null)
                goal = ste.Target.GridPosition;
            // Handle The Case Where The Squad Centroid Ends Up Inside A Building
            CollisionGrid cg = s.CGrid;
            if(cg.GetCollision(start)) {
                float minDistSq = float.MaxValue;
                for(int u = 0; u < squad.Units.Count; u++) {
                    RTSUnit unit = squad.Units[u];
                    float distSq = (goal - unit.GridPosition).LengthSquared();
                    if(distSq < minDistSq) {
                        minDistSq = distSq;
                        start = unit.GridPosition;
                    }
                }

            }
            var query = pathfinder.ReissuePathQuery(new PathQuery(start, goal, e.Team), start, goal, e.Team);
            squadQueries.Add(new SquadQuery(squad, query));
        }

        private void ApplySquadQueries() {
            List<SquadQuery> newSquadQueries = new List<SquadQuery>();
            foreach(var sq in squadQueries) {
                if(sq.Query.IsComplete && !sq.Query.IsOld) {
                    sq.Query.IsOld = true; // Pathfinder Needs To Know It Can Clear This
                    foreach(var unit in sq.Squad.Units) {
                        List<Vector2> waypoints = new List<Vector2>();
                        foreach(var wp in sq.Query.waypoints) {
                            waypoints.Add(wp);
                        }
                        unit.MovementController.Waypoints = waypoints;
                        unit.MovementController.CurrentWaypointIndex = waypoints.Count - 1;
                    }
                }
                if(!sq.Query.IsOld) {
                    newSquadQueries.Add(sq);
                }
            }
            squadQueries = newSquadQueries;
        }

        private void ApplyInput(GameState s, float dt, SpawnUnitEvent e) {
            RTSTeam team = s.teams[e.Team];
            RTSUnit unit = team.AddUnit(e.Type, e.Position);

            // Check If A Unit Was Possible
            if(unit == null) { return; }

            // Add Decision Tasks
            AddTask(s, unit);

            // Add A Single Unit Squad
            RTSSquad squad = team.AddSquad();
            squad.Add(unit);
            squad.RecalculateGridPosition();
            AddTask(s, squad);
        }
        private void ApplyInput(GameState s, float dt, SpawnBuildingEvent e) {
            RTSTeam team = s.teams[e.Team];

            // Check If We Can Add A Building There
            Vector2 wp = new Vector2(e.GridPosition.X + 0.5f, e.GridPosition.Y + 0.5f) * s.CGrid.cellSize;
            if(!s.CGrid.CanAddBuilding(wp, team.Race.Buildings[e.Type].GridSize)) return;

            RTSBuilding building = team.AddBuilding(e.Type, wp);
            if(building == null) return;

            // Check For Instant Building
            if(e.InstantBuild) {
                building.BuildAmountLeft = 0;
            }
            else {
                building.OnBuildingFinished += (b) => {
                    s.SendAlert(building.Data.FriendlyName + " Is Built", AlertLevel.Passive);
                };
            }

            // Check If A Building Was Possible
            if(building == null) return;

            // Set Default Height
            building.Height = s.CGrid.HeightAt(building.GridPosition);
            building.CollisionGeometry.Height = building.Height;
            s.CGrid.Add(building);

            // Add Building Decision Task
            AddTask(s, building, e.Team, e.Type);
        }
        private void ApplyInput(GameState s, float dt, ImpactEvent e) {
            Point p = HashHelper.Hash(e.Position, s.IGrid.numCells, s.IGrid.size);
            s.IGrid.Region[p.X, p.Y].AddToRegionImpact(e.ChangeAmount);
        }
        private void ApplyInput(GameState s, float dt, CapitalEvent e) {
            RTSTeam team = s.teams[e.Team];

            team.Capital += e.ChangeAmount;
        }
        private void AddTask(GameState s, RTSUnit unit) {
            // Init The Unit
            if(unit.CombatController != null) unit.CombatController.Init(s, this, unit.Data.CombatControllerInitArgs);
            if(unit.MovementController != null) unit.MovementController.Init(s, this, unit.Data.MovementControllerInitArgs);
            if(unit.AnimationController != null) unit.AnimationController.Init(s, this, unit.Data.AnimationControllerInitArgs);
            if(unit.ActionController != null) unit.ActionController.Init(s, this, unit.Data.ActionControllerInitArgs);

            var btu = new BTaskUnitDecision(s, unit);
            unit.OnDestruction += (o) => {
                tbEntityDecisions.RemoveTask(btu);
            };
            tbEntityDecisions.AddTask(btu);
        }
        private void AddTask(GameState s, RTSSquad squad) {
            // Init The Squad
            if(squad.TargetingController != null) squad.TargetingController.Init(s, this, squad.Team.Race.SCTargeting);
            if(squad.MovementController != null) squad.MovementController.Init(s, this, squad.Team.Race.SCMovementInitArgs);
            if(squad.ActionController != null) squad.ActionController.Init(s, this, squad.Team.Race.SCActionInitArgs);

            var bts = new BTaskSquadDecision(s, squad);
            squad.OnDeath += (o) => {
                tbSquadDecisions.RemoveTask(bts);
            };
            tbSquadDecisions.AddTask(bts);
        }
        private void AddTask(GameState s, RTSBuilding building) {
            // Init The Building
            if(building.ActionController != null) building.ActionController.Init(s, this, building.Data.ActionControllerInitArgs);
            for(int i = 0; i < building.ButtonControllers.Count; i++)
                building.ButtonControllers[i].Init(s, this, building.Data.DefaultButtonControllerInitArgs[i]);

            var btu = new BTaskBuildingDecision(s, building);
            building.OnDestruction += (o) => {
                tbEntityDecisions.RemoveTask(btu);
            };
            tbEntityDecisions.AddTask(btu);
        }
        private void AddTask(GameState s, RTSBuilding building, int fTeam, int type) {
            AddTask(s, building);
            ViewedBuilding vb = new ViewedBuilding();
            vb.Team = fTeam;
            vb.Type = type;
            vb.ViewDirection = building.ViewDirection;
            vb.WorldPosition = building.WorldPosition;
            vb.CellPoint = HashHelper.Hash(building.GridPosition, s.CGrid.numCells, s.CGrid.size);
            for(int i = 0; i < s.teams.Length; i++) {
                if(i == fTeam || s.teams[i] == null) continue;
                var ebu = new EnemyBuildingUpdater(s, i, vb, building);
                s.tbMemBuildings.AddTask(ebu);
            }
        }

        // Logic Stage
        private void ApplyLogic(GameState s, float dt) {
            RTSTeam team = null;

            // Apply Dev Commands
            int c = commands.Count;
            for(int i = 0; i < c; i++) {
                var comm = commands.Dequeue();
                switch(comm.Type) {
                    case DevCommandType.Spawn:
                        ApplyLogic(s, dt, comm as DevCommandSpawn);
                        break;
                    case DevCommandType.StopMotion:
                        ApplyLogic(s, dt, comm as DevCommandStopMotion);
                        break;
                    case DevCommandType.KillUnits:
                        ApplyLogic(s, dt, comm as DevCommandKillUnits);
                        break;
                    case DevCommandType.KillBuildings:
                        ApplyLogic(s, dt, comm as DevCommandKillBuildings);
                        break;
                    case DevCommandType.FOW:
                        ApplyLogic(s, dt, comm as DevCommandFOW);
                        break;
                    case DevCommandType.Save:
                        ApplyLogic(s, dt, comm as DevCommandSave);
                        break;
                    case DevCommandType.Capital:
                        ApplyLogic(s, dt, comm as DevCommandCapital);
                        break;
                }
            }

            // Find Decisions For Currently Budgeted Tasks
            tbSquadDecisions.DoTasks(dt);
            tbEntityDecisions.DoTasks(dt);

            // Apply Decisions For Squads
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                team = s.activeTeams[ti].Team;
                for(int i = 0; i < team.Squads.Count; i++)
                    if(team.Squads[i].ActionController != null)
                        team.Squads[i].ActionController.ApplyAction(s, dt);
            }
            // Apply Decisions For Units And Buildings
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                team = s.activeTeams[ti].Team;
                for(int i = 0; i < team.Units.Count; i++)
                    if(team.Units[i].ActionController != null)
                        team.Units[i].ActionController.ApplyAction(s, dt);
                for(int i = 0; i < team.Buildings.Count; i++)
                    if(team.Buildings[i].ActionController != null)
                        team.Buildings[i].ActionController.ApplyAction(s, dt);
            }

            // Calculate FOW
            tbFOWCalculations.DoTasks(dt);

            // Calculate Memorizations
            if(s.CurrentFrame % GameState.BUILDING_MEMORIZATION_LATENCY == 0)
                s.tbMemBuildings.ResortBins();
            s.tbMemBuildings.DoTasks(dt);
        }
        private void ApplyLogic(GameState s, float dt, DevCommandSpawn c) {
            // Multiple Spawn Events
            SpawnUnitEvent e = new SpawnUnitEvent(c.TeamIndex, c.UnitIndex, new Vector2(c.X, c.Z));
            for(int i = 0; i < c.Count; i++) ApplyInput(s, dt, e);
        }
        private void ApplyLogic(GameState s, float dt, DevCommandStopMotion c) {
            // TODO: Deprecate ?
            for(int z = 0; z < s.CGrid.numCells.Y; z++) {
                for(int x = 0; x < s.CGrid.numCells.X; x++) {
                    Point p = new Point(x, z);
                    Vector3 pos = new Vector3(x * 2 + 1, 0, z * 2 + 1);
                    pos.Y = s.CGrid.HeightAt(new Vector2(pos.X, pos.Z));
                    if(!s.CGrid.CanMoveTo(p, CollisionGrid.Direction.XP)) {
                        s.AddParticle(new LightningParticle(
                            pos + Vector3.UnitX, 1f, 12f, MathHelper.PiOver2,
                            5f, 1, Color.LightBlue
                            ));
                    }
                    if(!s.CGrid.CanMoveTo(p, CollisionGrid.Direction.XN)) {
                        s.AddParticle(new LightningParticle(
                            pos - Vector3.UnitX, 1f, 12f, MathHelper.PiOver2,
                            5f, 1, Color.LightBlue
                            ));
                    }
                    if(!s.CGrid.CanMoveTo(p, CollisionGrid.Direction.ZP)) {
                        s.AddParticle(new LightningParticle(
                            pos + Vector3.UnitZ, 1f, 12f, 0f,
                            5f, 1, Color.LightBlue
                            ));
                    }
                    if(!s.CGrid.CanMoveTo(p, CollisionGrid.Direction.ZN)) {
                        s.AddParticle(new LightningParticle(
                            pos - Vector3.UnitZ, 1f, 12f, 0f,
                            5f, 1, Color.LightBlue
                            ));
                    }
                }
            }
        }
        private void ApplyLogic(GameState s, float dt, DevCommandKillUnits c) {
            RTSTeam team;
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                team = s.activeTeams[ti].Team;
                foreach(var unit in team.Units) {
                    unit.Damage(9001); // OVER 9000
                }
            }
        }
        private void ApplyLogic(GameState s, float dt, DevCommandKillBuildings c) {
            RTSTeam team;
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                team = s.activeTeams[ti].Team;
                foreach(var building in team.Buildings) {
                    building.Damage(9001); // OVER 9000
                }
            }
        }
        private void ApplyLogic(GameState s, float dt, DevCommandFOW c) {
            foreach(var task in tbFOWCalculations.Tasks) {
                var t = task as FOWTask;
                t.SetAllFOW(c.fow, s.CGrid);
            }
        }
        private void ApplyLogic(GameState s, float dt, DevCommandSave c) {
            GameEngine.Save(s, c.file.FullName);
        }
        private void ApplyLogic(GameState s, float dt, DevCommandCapital c) {
            foreach(var t in s.activeTeams) {
                t.Team.Input.AddEvent(new CapitalEvent(t.Team.Index, c.change));
            }
        }

        // Physics Stage
        private void ResolvePhysics(GameState s, float dt) {
            RTSTeam team;

            // Initialize hash grid
            var hashGrid = s.CGrid;
            hashGrid.ClearDynamic();

            // Move Geometry To The Unit's Location and hash into the grid
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                team = s.activeTeams[ti].Team;
                foreach(RTSUnit unit in team.Units) {
                    unit.CollisionGeometry.Center = unit.GridPosition;
                    hashGrid.Add(unit);
                }
            }

            // Use hash grid to perform collision using 3 by 3 grid around the geometry
            for(int i = 0; i < hashGrid.ActiveGrids.Count; i++) {
                Point p = hashGrid.ActiveGrids[i];
                hashGrid.HandleGridCollision(p.X, p.Y);
                hashGrid.HandleGridCollision(p.X, p.Y, -1, -1);
                hashGrid.HandleGridCollision(p.X, p.Y, -1, 0);
                hashGrid.HandleGridCollision(p.X, p.Y, -1, 1);
                hashGrid.HandleGridCollision(p.X, p.Y, 0, -1);
                hashGrid.HandleGridCollision(p.X, p.Y, 0, 1);
                hashGrid.HandleGridCollision(p.X, p.Y, 1, -1);
                hashGrid.HandleGridCollision(p.X, p.Y, 1, 0);
                hashGrid.HandleGridCollision(p.X, p.Y, 1, 1);
            }

            // Move Unit's Location To The Geometry After Heightmap Collision
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                team = s.activeTeams[ti].Team;
                foreach(RTSUnit unit in team.Units) {
                    CollisionController.CollideHeightmap(unit.CollisionGeometry, s.CGrid);
                    unit.GridPosition = unit.CollisionGeometry.Center;
                    unit.Height = unit.CollisionGeometry.Height;
                }
            }
        }

        // Cleanup Stage
        private void Cleanup(GameState s, float dt) {
            // Remove All Dead Entities
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                RTSTeam team = s.activeTeams[ti].Team;
                team.RemoveAll(IsUnitDead);
                team.RemoveAll(IsBuildingDead);
                team.RemoveAll(IsSquadEmpty);
            }
        }
        public static bool IsEntityDead(IEntity e) {
            return !e.IsAlive;
        }
        public static bool IsUnitDead(RTSUnit u) {
            return IsEntityDead(u);
        }
        public static bool IsBuildingDead(RTSBuilding b) {
            return IsEntityDead(b);
        }
        private static bool IsSquadEmpty(RTSSquad s) {
            return s.IsDead;
        }

        // Dev Callback
        public void OnDevCommand(string s) {
            DevCommand c;
            if(DevCommandSpawn.TryParse(s, out c)) {
                commands.Enqueue(c);
                return;
            }
            else if(DevCommandStopMotion.TryParse(s, out c)) {
                commands.Enqueue(c);
                return;
            }
            else if(DevCommandKillUnits.TryParse(s, out c)) {
                commands.Enqueue(c);
                return;
            }
            else if(DevCommandKillBuildings.TryParse(s, out c)) {
                commands.Enqueue(c);
                return;
            }
            else if(DevCommandFOW.TryParse(s, out c)) {
                commands.Enqueue(c);
                return;
            }
            else if(DevCommandSave.TryParse(s, out c)) {
                commands.Enqueue(c);
                return;
            }
            else if(DevCommandCapital.TryParse(s, out c)) {
                commands.Enqueue(c);
                return;
            }
        }
    }
}