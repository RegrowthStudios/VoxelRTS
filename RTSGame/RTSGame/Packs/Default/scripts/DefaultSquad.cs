using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Algorithms;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;

namespace RTS.Default.Squad {
    public class Action : ACSquadActionController {
        public override void Init(GameState s, GameplayController c) {

        }

        public override void DecideAction(GameState g, float dt) {
            if(squad.TargetingController != null)
                squad.TargetingController.DecideTarget(g, dt);
            if(squad.MovementController != null)
                squad.MovementController.DecideMoves(g, dt);
        }
        public override void ApplyAction(GameState g, float dt) {
            if(squad.TargetingController != null)
                squad.TargetingController.ApplyTarget(g, dt);
            if(squad.MovementController != null)
                squad.MovementController.ApplyMoves(g, dt);
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }

    public class Movement : ACSquadMovementController {
        // Whether Units In This Squad Have Decided To Move (Key: UUID)
        Dictionary<int, bool> doMove = new Dictionary<int, bool>();
        Dictionary<int, int> origTargetOrders = new Dictionary<int, int>();

        float a = 0f;

        public override void Init(GameState s, GameplayController c) {
            for(int i = 0; i < squad.Units.Count; i++) {
                RTSUnit unit = squad.Units[i];
                origTargetOrders.Add(unit.UUID, unit.TargetingOrders);
            }
        }

        public override void DecideMoves(GameState g, float dt) {
            // Pathfinding Has Not Finished: Make The Formation At The Average Squad Position
            a = 0f;
            foreach(var unit in squad.Units) {
                a += unit.CollisionGeometry.BoundingRadius * unit.CollisionGeometry.BoundingRadius * MathHelper.Pi;
            }
            if(Waypoints == null || Waypoints.Count == 0) {
                foreach(var unit in squad.Units) {
                    SetNetForceAndMove(g, unit, squad.GridPosition, null);
                }
            }
            // Having A Target Trumps Regular Movement
            else if(squad.TargetingController != null && squad.TargetingController.Target != null) {
                foreach(var unit in squad.Units) {
                    RTSUnit target = unit.Target as RTSUnit;
                    if(target != null) {
                        switch(unit.CombatOrders) {
                            case BehaviorFSM.UseMeleeAttack:
                                float r = unit.CollisionGeometry.BoundingRadius + target.CollisionGeometry.BoundingRadius;
                                r *= 1.3f;
                                DoTargeting(g, dt, unit, target, r);
                                if(!doMove[unit.UUID] && unit.State == BehaviorFSM.Walking)
                                    unit.State = BehaviorFSM.CombatMelee;
                                break;
                            default: // case BehaviorFSM.UseRangedAttack:
                                r = unit.UnitData.BaseCombatData.MaxRange;
                                DoTargeting(g, dt, unit, target, r);
                                if(!doMove[unit.UUID] && unit.State == BehaviorFSM.Walking)
                                    unit.State = BehaviorFSM.CombatRanged;
                                break;
                        }
                    }
                }
            }
            // Regular Movement 
            else {
                foreach(var unit in squad.Units) {
                    if(CurrentWaypointIndices.ContainsKey(unit.UUID) && IsValid(CurrentWaypointIndices[unit.UUID])) {
                        Vector2 waypoint = squad.MovementController.Waypoints[CurrentWaypointIndices[unit.UUID]];
                        SetNetForceAndMove(g, unit, waypoint, null);
                    }
                }
            }
        }
        public override void ApplyMoves(GameState g, float dt) {
            // The Whole Squad Will Move At The Min Default Movespeed
            float minDefaultMoveSpeed = float.MaxValue;
            foreach(var unit in squad.Units) {
                float moveSpeed = unit.MovementSpeed / unit.MovementMultiplier;
                if(moveSpeed < minDefaultMoveSpeed)
                    minDefaultMoveSpeed = moveSpeed;
            }
            foreach(var unit in squad.Units) {
                AddToHistory(unit, unit.GridPosition);
                if(!doMove.ContainsKey(unit.UUID)) continue;
                if(doMove[unit.UUID]) {
                    Vector2 change = NetForces.ContainsKey(unit.UUID) ? NetForces[unit.UUID] : Vector2.Zero;
                    if(change != Vector2.Zero) {
                        float magnitude = change.Length();
                        Vector2 scaledChange = (change / magnitude) * minDefaultMoveSpeed * dt;
                        if(scaledChange.LengthSquared() > magnitude * magnitude)
                            unit.Move(change);
                        else
                            unit.Move(scaledChange);
                    }
                    unit.State = BehaviorFSM.Walking;
                    unit.TargetingOrders = BehaviorFSM.TargetNone;
                }
                else if(unit.State != BehaviorFSM.CombatMelee && unit.State != BehaviorFSM.CombatRanged) {
                    unit.State = BehaviorFSM.Rest;
                    unit.TargetingOrders = origTargetOrders[unit.UUID];
                }
            }
        }

        private void SetNetForceAndMove(GameState g, RTSUnit unit, Vector2 waypoint, List<Vector2> targetFormation) {
            // Set Net Force
            Vector2 netForce = squad.Units.Count * Force(unit, waypoint);
            CollisionGrid cg = g.CGrid;
            Point unitCell = HashHelper.Hash(unit.GridPosition, cg.numCells, cg.size);
            foreach(Point n in Pathfinder.NeighborhoodAlign(unitCell)) {
                RTSBuilding b = cg.EStatic[n.X, n.Y];
                if(b != null)
                    netForce += Force(unit, b);
            }
            foreach(Point n in Pathfinder.NeighborhoodDiag(unitCell)) {
                RTSBuilding b = cg.EStatic[n.X, n.Y];
                if(b != null)
                    netForce += 5 * Force(unit, b);
            }
            foreach(var otherUnit in cg.EDynamic[unitCell.X, unitCell.Y]) {
                netForce += Force(unit, otherUnit);
            }
            if(UnitHistory.ContainsKey(unit.UUID)) {
                foreach(var prevLocation in UnitHistory[unit.UUID]) {
                    netForce -= Force(unit, prevLocation);
                }
            }
            NetForces[unit.UUID] = netForce;

            // Set Move
            if(!CurrentWaypointIndices.ContainsKey(unit.UUID) || !IsValid(CurrentWaypointIndices[unit.UUID])) return;
            Point currWaypointCell = HashHelper.Hash(waypoint, cg.numCells, cg.size);
            bool inGoalCell = unitCell.X == currWaypointCell.X && unitCell.Y == currWaypointCell.Y;
            float stopDist = (float)Math.Sqrt(a / Math.PI) * 1.5f;// cg.cellSize / 3;
            if(inGoalCell || (waypoint - unit.GridPosition).LengthSquared() < stopDist * stopDist) {
                CurrentWaypointIndices[unit.UUID]--;
            }
            doMove[unit.UUID] = IsValid(CurrentWaypointIndices[unit.UUID]);
        }

        // TODO: Make This Predict (Or Omnisciently Read) Where The Target Will Be Because It Could Be Moving
        private void DoTargeting(GameState g, float dt, RTSUnit unit, RTSUnit target, float r) {
            if(CurrentWaypointIndices.ContainsKey(unit.UUID) && IsValid(CurrentWaypointIndices[unit.UUID])) {
                Vector2 waypoint = Waypoints[CurrentWaypointIndices[unit.UUID]];
                bool waypointIsTarget = waypoint.X == target.GridPosition.X && waypoint.Y == target.GridPosition.Y;
                if(waypointIsTarget) {
                    List<Vector2> targetFormation = new List<Vector2>();
                    float step = (float)Math.PI / 16;
                    float angle = 0;
                    while(angle < 2 * Math.PI) {
                        targetFormation.Add(new Vector2((float)(r * Math.Sin(angle)), (float)(r * Math.Sin(angle))));
                        angle += step;
                    }
                    SetNetForceAndMove(g, unit, waypoint, targetFormation);
                }
                else {
                    SetNetForceAndMove(g, unit, waypoint, null);
                }
            }
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }

    public class Targeting : ACSquadTargetingController {
        int teamIndex;
        public override void Init(GameState s, GameplayController c) {
            teamIndex = squad.Team.Input.TeamIndex;
        }

        public override void DecideTarget(GameState g, float dt) {
            if(target == null) {
                FindTargetUnit(g);
            }
            else if(!target.IsAlive) {
                target = null;
            }
            else if(g.CGrid.GetFogOfWar(target.GridPosition, teamIndex) != FogOfWar.Active) {
                target = null;
            }
        }
        private void FindTargetSquad(GameState g) {
            targetSquad = null;
            float minDist = float.MaxValue;
            for(int i = 0; i < g.activeTeams.Length; i++) {
                RTSTeam team = g.activeTeams[i].Team;
                if(team == squad.Team) continue;
                foreach(var sq in team.Squads) {
                    float d = (sq.GridPosition - squad.GridPosition).LengthSquared();
                    if(d < minDist) {
                        targetSquad = sq;
                        minDist = d;
                    }
                }
            }
        }
        private void FindTargetUnit(GameState g) {
            target = null;
            float minDist = float.MaxValue;
            for(int ti = 0; ti < g.activeTeams.Length; ti++) {
                // Don't Automatically Self-Target
                if(g.activeTeams[ti].Index == teamIndex)
                    continue;

                RTSTeam team = g.activeTeams[ti].Team;
                for(int i = 0; i < team.Units.Count; i++) {
                    if(g.CGrid.GetFogOfWar(team.Units[i].GridPosition, teamIndex) != FogOfWar.Active)
                        continue;
                    float d = (team.Units[i].GridPosition - squad.GridPosition).LengthSquared();
                    if(d < minDist) {
                        TargetUnit = team.Units[i];
                        minDist = d;
                    }
                }
            }
        }
        public override void ApplyTarget(GameState g, float dt) {
            foreach(var unit in squad.Units) {
                unit.Target = target;
            }
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }
}
