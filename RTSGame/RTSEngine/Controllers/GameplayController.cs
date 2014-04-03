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
    #endregion

    public class GameplayController : IDisposable {
        public const int SQUAD_BUDGET_BINS = 10;
        public const int UNIT_BUDGET_BINS = 30;
        public const int FOW_BUDGET_BINS = 8;

        // Queue Of Events
        private LinkedList<GameInputEvent> events;

        // Queue Of Commands
        private Queue<DevCommand> commands;

        private TimeBudget tbSquadDecisions;
        private TimeBudget tbUnitDecisions;
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
            tbUnitDecisions = new TimeBudget(UNIT_BUDGET_BINS);
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
            // TODO: Use InputControllers From The Teams
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
            e.Team.Input.selected.Clear();
            e.Team.Input.selected.AddRange(e.Selected);
        }
        private void ApplyInput(GameState s, float dt, SetWayPointEvent e) {
            List<Vector2> wp = new List<Vector2>();
            wp.Add(e.Waypoint);
            List<IEntity> selected = e.Team.Input.selected;
            RTSSquad squad = null;
            if(selected != null && selected.Count > 0) {
                foreach(var unit in selected) {
                    RTSUnit u = unit as RTSUnit;
                    if(u != null) {
                        if(squad == null) squad = u.Team.AddSquad();
                        u.MovementController.Waypoints = wp;
                        u.Target = null;
                        squad.Add(u);
                    }
                }
            }
            if(squad != null) {
                AddSquadTask(s, squad);
                // Setup Pathfinding Query
                foreach(var squadQuery in squadQueries) {
                    if(squadQuery.squad == squad) {
                        squadQuery.query.IsOld = true;
                    }
                }
                squad.MovementController = e.Team.race.scMovement.CreateInstance<ACSquadMovementController>();
                squad.RecalculateGridPosition();
                PathQuery query = new PathQuery(squad.GridPosition, e.Waypoint);
                squadQueries.Add(new SquadQuery(squad, query));
                pathfinder.Add(query);
            }
        }
        private void ApplyInput(GameState s, float dt, SetTargetEvent e) {
            List<IEntity> selected = e.Team.Input.selected;

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
                AddSquadTask(s, squad);
                squad.ActionController = e.Team.race.scAction.CreateInstance<ACSquadActionController>();
                squad.TargettingController = e.Team.race.scTargetting.CreateInstance<ACSquadTargettingController>();
                squad.TargettingController.Target = e.Target as RTSUnit;
            }
        }

        private void ApplyInput(GameState s, float dt, SpawnUnitEvent e) {
            RTSSquad squad = e.Team.AddSquad();
            RTSUnit unit = e.Team.AddUnit(e.Type.Index, e.Position);
            unit.ActionController = e.Type.Data.DefaultActionController.CreateInstance<ACUnitActionController>();
            unit.AnimationController = e.Type.Data.DefaultAnimationController.CreateInstance<ACUnitAnimationController>();
            unit.MovementController = e.Type.Data.DefaultMoveController.CreateInstance<ACUnitMovementController>();
            unit.CombatController = e.Type.Data.DefaultCombatController.CreateInstance<ACUnitCombatController>();
            squad.Add(unit);
            AddUnitTask(s, unit);
            squad.RecalculateGridPosition();
            AddSquadTask(s, squad);
        }

        private void ApplyInput(GameState s, float dt, SpawnBuildingEvent e) {

        }

        // Apply Results Of Any Finished Pathfinding
        private void ApplyFinishedPathQueries() {
            List<SquadQuery> newQueries = new List<SquadQuery>();
            foreach(var squadQuery in squadQueries) {
                RTSSquad squad = squadQuery.squad;
                PathQuery query = squadQuery.query;
                if(!query.IsOld && query.IsComplete) {
                    squad.MovementController.Waypoints = query.waypoints;
                }
                else if (!query.IsOld) {
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
            tbUnitDecisions.DoTasks(dt);

            // Apply Decisions
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                team = s.activeTeams[ti].Team;
                for(int i = 0; i < team.squads.Count; i++)
                    if(team.squads[i].ActionController != null)
                        team.squads[i].ActionController.ApplyAction(s, dt);
            }
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                team = s.activeTeams[ti].Team;
                for(int i = 0; i < team.units.Count; i++)
                    if(team.units[i].ActionController != null)
                        team.units[i].ActionController.ApplyAction(s, dt);
            }

            // Calculate FOW
            tbFOWCalculations.DoTasks(dt);
        }
        private void ApplyLogic(GameState s, float dt, DevCommandSpawn c) {
            RTSTeam team = s.teams[c.TeamIndex];
            RTSSquad squad = team.AddSquad();
            for(int ci = 0; ci < c.Count; ci++) {
                RTSUnit unit = team.AddUnit(c.UnitIndex, new Vector2(c.X, c.Z));
                unit.ActionController = team.race.units[c.UnitIndex].DefaultActionController.CreateInstance<ACUnitActionController>();
                unit.AnimationController = team.race.units[c.UnitIndex].DefaultAnimationController.CreateInstance<ACUnitAnimationController>();
                unit.MovementController = team.race.units[c.UnitIndex].DefaultMoveController.CreateInstance<ACUnitMovementController>();
                unit.CombatController = team.race.units[c.UnitIndex].DefaultCombatController.CreateInstance<ACUnitCombatController>();
                squad.Add(unit);
                AddUnitTask(s, unit);
            }
            squad.RecalculateGridPosition();
            AddSquadTask(s, squad);
        }
        private void ApplyLogic(GameState s, float dt, DevCommandStopMotion c) {
            for(int ti = 0; ti < s.activeTeams.Length; ti++) {
                foreach(var unit in s.activeTeams[ti].Team.units) {
                    if(unit.MovementController != null)
                        unit.MovementController.Waypoints = null;
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
        private void AddUnitTask(GameState s, RTSUnit unit) {
            var btu = new BTaskUnitDecision(s, unit);
            unit.OnDestruction += (o) => {
                tbUnitDecisions.RemoveTask(btu);
            };
            tbUnitDecisions.AddTask(btu);
        }
        private void AddSquadTask(GameState s, RTSSquad squad) {
            var bts = new BTaskSquadDecision(s, squad);
            squad.OnDeath += (o) => {
                tbSquadDecisions.RemoveTask(bts);
            };
            tbSquadDecisions.AddTask(bts);
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
                team.RemoveAll(IsEntityDead);
                team.RemoveAll(IsSquadEmpty);
            }

            // Add Newly Created Instances
            AddInstantiatedData(s);
        }
        public static bool IsEntityDead(IEntity e) {
            return !e.IsAlive;
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