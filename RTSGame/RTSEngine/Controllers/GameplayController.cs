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

    public class GameplayController : IDisposable {
        public const int SQUAD_BUDGET_BINS = 10;
        public const int ENTITY_BUDGET_BINS = 30;
        public const int FOW_BUDGET_BINS = 8;

        // Queue Of Events
        private LinkedList<GameInputEvent> events;

        // Queue Of Commands
        private Queue<DevCommand> commands;

        // Budgeted Tasks That Do Not Require Immediate Computation
        private TimeBudget tbSquadDecisions;
        private TimeBudget tbEntityDecisions;
        private TimeBudget tbFOWCalculations;

        // Pathfinding
        private Pathfinder pathfinder;
        private List<SquadQuery> squadQueries;

        struct SquadQuery {
            public RTSSquad squad;
            public PathQuery query;
            public SquadQuery(RTSSquad s, PathQuery q) {
                squad = s;
                query = q;
            }
        }

        public GameplayController() {
            commands = new Queue<DevCommand>();

            tbSquadDecisions = new TimeBudget(SQUAD_BUDGET_BINS);
            tbEntityDecisions = new TimeBudget(ENTITY_BUDGET_BINS);
            tbFOWCalculations = new TimeBudget(FOW_BUDGET_BINS);

            squadQueries = new List<SquadQuery>();
        }
        public void Dispose() {
            DevConsole.OnNewCommand -= OnDevCommand;
            pathfinder.Dispose();
        }

        public void Init(GameState s) {
            DevConsole.OnNewCommand += OnDevCommand;
            tbFOWCalculations.ClearTasks();
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                tbFOWCalculations.AddTask(new FOWTask(s, s.activeTeams[ti].Index));
            }
            pathfinder = new Pathfinder(s.CGrid);

            // Start The AI
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                AIInputController aic = s.activeTeams[ti].Team.Input as AIInputController;
                if(aic != null) aic.Start();
            }
        }

        // The Update Function
        public void Update(GameState s, float dt) {
            s.IncrementFrame(dt);

            // Input Pass
            ResolveInput(s, dt);
            ApplyInput(s, dt);

            // Pathfinding Pass
            ApplyFinishedPathQueries();

            // Logic Pass
            ApplyLogic(s, dt);

            // Physics Pass
            ResolvePhysics(s, dt);

            // Cleanup The State
            Cleanup(s, dt);
        }

        // Input Stage
        private void ResolveInput(GameState s, float dt) {
            events = new LinkedList<GameInputEvent>();
            foreach(var active in s.activeTeams) {
                if(active.Team.Input != null) {
                    active.Team.Input.AppendEvents(events);
                }
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
                    default:
                        throw new Exception("Event does not exist.");
                }
                events.RemoveFirst();
            }
        }
        private void ApplyInput(GameState s, float dt, SelectEvent e) {
            RTSTeam team = s.teams[e.Team];
            team.Input.selected.Clear();
            if(e.Selected != null && e.Selected.Count > 0)
                team.Input.selected.AddRange(e.Selected);
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
                        squad.Add(u);
                    }
                }
            }
            if(squad != null) {
                AddTask(s, squad);
                SendPathQuery(squad, e);
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
                squad.TargetingController.Target = e.Target as RTSUnit;
                AddTask(s, squad);
                SendPathQuery(squad, e);
            }
        }
        private void ApplyInput(GameState s, float dt, SpawnUnitEvent e) {
            RTSTeam team = s.teams[e.Team];
            RTSUnit unit = team.AddUnit(e.Type, e.Position);
            AddTask(s, unit);

            // Add A Single Unit Squad
            RTSSquad squad = team.AddSquad();
            squad.Add(unit);
            squad.RecalculateGridPosition();
            AddTask(s, squad);
        }
        private void ApplyInput(GameState s, float dt, SpawnBuildingEvent e) {
            RTSTeam team = s.teams[e.Team];
            Vector2 wp = new Vector2(e.GridPosition.X + 0.5f, e.GridPosition.Y + 0.5f) * s.CGrid.cellSize;
            RTSBuilding building = team.AddBuilding(e.Type, wp);
            AddTask(s, building);

            // Set Default Height
            building.Height = s.Map.HeightAt(building.GridPosition.X, building.GridPosition.Y);
            building.CollisionGeometry.Height = building.Height;
            s.IGrid.AddImpactGenerator(building);
        }
        private void AddTask(GameState s, RTSUnit unit) {
            var btu = new BTaskUnitDecision(s, unit);
            unit.OnDestruction += (o) => {
                tbEntityDecisions.RemoveTask(btu);
            };
            tbEntityDecisions.AddTask(btu);
        }
        private void AddTask(GameState s, RTSSquad squad) {
            var bts = new BTaskSquadDecision(s, squad);
            squad.OnDeath += (o) => {
                tbSquadDecisions.RemoveTask(bts);
            };
            tbSquadDecisions.AddTask(bts);
        }
        private void AddTask(GameState s, RTSBuilding building) {
            var btu = new BTaskBuildingDecision(s, building);
            building.OnDestruction += (o) => {
                tbEntityDecisions.RemoveTask(btu);
            };
            tbEntityDecisions.AddTask(btu);
        }

        // Setup And Send Pathfinding Query
        private void SendPathQuery(RTSSquad squad, GameInputEvent e) {
            foreach(var squadQuery in squadQueries) {
                if(squadQuery.squad.Equals(squad)) {
                    squadQuery.query.IsOld = true;
                }
            }
            squad.RecalculateGridPosition();
            PathQuery query = null;
            var swe = e as SetWayPointEvent;
            var ste = e as SetTargetEvent;
            if(swe != null)
                query = new PathQuery(squad.GridPosition, swe.Waypoint);
            else if(ste != null)
                query = new PathQuery(squad.GridPosition, ste.Target.GridPosition);
            squadQueries.Add(new SquadQuery(squad, query));
            pathfinder.Add(query);
            // TODO: Get The Formation Order From The Input Event
            if(squad.MovementController != null)
                squad.MovementController.ApplyMovementFormation(BehaviorFSM.BoxFormation);
        }

        // Apply Results Of Any Finished Pathfinding
        private void ApplyFinishedPathQueries() {
            List<SquadQuery> newQueries = new List<SquadQuery>();
            foreach(var squadQuery in squadQueries) {
                RTSSquad squad = squadQuery.squad;
                PathQuery query = squadQuery.query;
                if(!query.IsOld && query.IsComplete) {
                    squad.MovementController.Waypoints = query.waypoints;
                    // Tell All The Units In The Squad To Head To The First Waypoint
                    foreach(var unit in squad.Units) {
                        squad.MovementController.CurrentWaypointIndices[unit.UUID] = query.waypoints.Count - 1;
                    }
                }
                else if(!query.IsOld) {
                    newQueries.Add(squadQuery);
                }
            }
            squadQueries = newQueries;
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
                    case DevCommandType.Kill:
                        ApplyLogic(s, dt, comm as DevCommandKill);
                        break;
                }
            }

            // Find Decisions For Currently Budgeted Tasks
            tbSquadDecisions.DoTasks(dt);
            tbEntityDecisions.DoTasks(dt);

            // Apply Decisions For Squads
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                team = s.activeTeams[ti].Team;
                for(int i = 0; i < team.squads.Count; i++)
                    if(team.squads[i].ActionController != null)
                        team.squads[i].ActionController.ApplyAction(s, dt);
            }
            // Apply Decisions For Units And Buildings
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                team = s.activeTeams[ti].Team;
                for(int i = 0; i < team.units.Count; i++)
                    if(team.units[i].ActionController != null)
                        team.units[i].ActionController.ApplyAction(s, dt);
                for(int i = 0; i < team.buildings.Count; i++)
                    if(team.buildings[i].ActionController != null)
                        team.buildings[i].ActionController.ApplyAction(s, dt);
            }

            // Calculate FOW
            tbFOWCalculations.DoTasks(dt);
        }
        private void ApplyLogic(GameState s, float dt, DevCommandSpawn c) {
            // Multiple Spawn Events
            SpawnUnitEvent e = new SpawnUnitEvent(c.TeamIndex, c.UnitIndex, new Vector2(c.X, c.Z));
            for(int i = 0; i < c.Count; i++) ApplyInput(s, dt, e);
        }
        private void ApplyLogic(GameState s, float dt, DevCommandStopMotion c) {
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                foreach(var squad in s.activeTeams[ti].Team.squads) {
                    if(squad.MovementController != null) {
                        foreach(var unit in squad.Units) {
                            squad.MovementController.CurrentWaypointIndices[unit.UUID] = -1;
                        }
                    }
                }
            }
        }
        private void ApplyLogic(GameState s, float dt, DevCommandKill c) {
            RTSTeam team;
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                team = s.activeTeams[ti].Team;
                foreach(var unit in team.units) {
                    unit.Damage(9001); // OVER 9000
                }
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
                foreach(RTSUnit unit in team.units) {
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
                foreach(RTSUnit unit in team.units) {
                    CollisionController.CollideHeightmap(unit.CollisionGeometry, s.Map);
                    unit.GridPosition = unit.CollisionGeometry.Center;
                    unit.Height = unit.CollisionGeometry.Height;
                }
            }
        }

        // Cleanup Stage
        private void Cleanup(GameState s, float dt) {
            RTSTeam team;

            // Remove All Dead Entities
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                team = s.activeTeams[ti].Team;
                team.RemoveAll(IsUnitDead);
                team.RemoveAll(IsBuildingDead);
                team.RemoveAll(IsSquadEmpty);
            }

            // Add Newly Created Instances
            AddInstantiatedData(s);
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
        public void AddInstantiatedData(GameState s) {

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
            else if(DevCommandKill.TryParse(s, out c)) {
                commands.Enqueue(c);
                return;
            }
        }
    }
}