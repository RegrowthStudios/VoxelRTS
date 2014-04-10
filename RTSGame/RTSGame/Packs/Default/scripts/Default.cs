using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Team;
using RTSEngine.Algorithms;

namespace RTS.Default {
    namespace Unit {
        public class Action : ACUnitActionController {
            int teamIndex;
            Action<GameState, float> fDecide, fApply;

            public override void DecideAction(GameState g, float dt) {
                fDecide(g, dt);
            }
            public override void ApplyAction(GameState g, float dt) {
                fApply(g, dt);
                if(unit.Target != null) {
                    unit.TurnToFace(unit.Target.GridPosition);
                }
            }

            public override void Init(GameState s, GameplayController c) {
                unit.State = BehaviorFSM.Rest;
                unit.TargetingOrders = BehaviorFSM.TargetAggressively;
                unit.CombatOrders = BehaviorFSM.CombatRanged;
                unit.MovementOrders = 0;

                fDecide = DSRest;
                fApply = ASRest;

                teamIndex = unit.Team.Input.TeamIndex;
            }

            void DSRest(GameState g, float dt) {
                unit.State = BehaviorFSM.Rest;
                if(unit.Target != null) {
                    FogOfWar f = g.CGrid.GetFogOfWar(unit.Target.GridPosition, teamIndex);
                    if(f != FogOfWar.Active) {
                        return;
                    }
                    switch(unit.TargetingOrders) {
                        case BehaviorFSM.TargetOmnisciently:
                        case BehaviorFSM.TargetAggressively:
                            fDecide = DSFollowTarget;
                            fApply = ASFollowTarget;
                            fDecide(g, dt);
                            break;
                        case BehaviorFSM.TargetPassively:
                            float mr = unit.UnitData.BaseCombatData.MaxRange;
                            float d2 = (unit.Target.GridPosition - unit.GridPosition).LengthSquared();
                            if(d2 <= mr * mr) {
                                origin = unit.Target.GridPosition;
                                fDecide = DSPassiveTarget;
                                fApply = ASPassiveTarget;
                                fDecide(g, dt);
                            }
                            break;
                    }
                }
            }
            void ASRest(GameState g, float dt) {
                // Do Nothing
            }

            Vector2 targetLast = Vector2.Zero;
            void DSFollowTarget(GameState g, float dt) {
                if(unit.Target == null) {
                    unit.State = BehaviorFSM.Rest;
                    fDecide = DSRest;
                    fApply = ASRest;
                    return;
                }

                FogOfWar f = g.CGrid.GetFogOfWar(unit.Target.GridPosition, teamIndex);
                switch(f) {
                    case FogOfWar.Active:
                        targetLast = unit.Target.GridPosition;
                        float mr = unit.UnitData.BaseCombatData.MaxRange;
                        float d = (unit.Target.GridPosition - unit.GridPosition).Length();
                        float dBetween = d - unit.CollisionGeometry.BoundingRadius - unit.Target.CollisionGeometry.BoundingRadius;
                        switch(unit.CombatOrders) {
                            case BehaviorFSM.CombatRanged:
                                if(d <= mr * 0.75) {
                                    unit.State = BehaviorFSM.CombatRanged;
                                    etCombat = 0;
                                    fDecide = DSCombatRanged;
                                    fApply = ASCombatRanged;
                                }
                                else unit.State = BehaviorFSM.Walking;
                                break;
                            case BehaviorFSM.CombatMelee:
                                if(dBetween <= unit.CollisionGeometry.InnerRadius * 0.2f) {
                                    unit.State = BehaviorFSM.CombatMelee;
                                    etCombat = 0;
                                    fDecide = DSCombatMelee;
                                    fApply = ASCombatMelee;
                                }
                                else unit.State = BehaviorFSM.Walking;
                                break;
                        }
                        break;
                }
            }
            void ASFollowTarget(GameState g, float dt) {
                switch(unit.State) {
                    case BehaviorFSM.Walking:
                        Vector2 dir = targetLast - unit.GridPosition;
                        float dl = dir.Length();
                        if(dl > 0.001) {
                            dir /= dl;
                            float m = unit.MovementSpeed * dt;
                            if(m > dl) {
                                unit.Move(dir * dl);
                            }
                            else {
                                unit.Move(dir * m);
                            }
                        }
                        break;
                }
            }

            Vector2 origin = Vector2.Zero;
            bool moveToOrigin = false;
            void DSPassiveTarget(GameState g, float dt) {
                unit.State = BehaviorFSM.Rest;
                Vector2 d = unit.GridPosition - origin;
                float mr = unit.UnitData.BaseCombatData.MaxRange;
                if(d.LengthSquared() > mr * mr) {
                    moveToOrigin = true;
                }
                else {
                    DSFollowTarget(g, dt);
                }
            }
            void ASPassiveTarget(GameState g, float dt) {
                if(moveToOrigin) {
                    Vector2 dir = origin - unit.GridPosition;
                    float dl = dir.Length();
                    if(dl > 0.001) {
                        dir /= dl;
                        float m = unit.MovementSpeed * dt;
                        if(m > dl) {
                            unit.Move(dir * dl);
                        }
                        else {
                            unit.Move(dir * m);
                        }
                    }
                    moveToOrigin = dl < unit.CollisionGeometry.InnerRadius;
                    unit.State = moveToOrigin ? BehaviorFSM.Walking : BehaviorFSM.Rest;
                }
                else {
                    ASFollowTarget(g, dt);
                }
            }

            float etCombat = 0;
            Random r = new Random();
            void DSCombatRanged(GameState g, float dt) {
                if(unit.Target == null) {
                    unit.State = BehaviorFSM.Rest;
                    fDecide = DSRest;
                    fApply = ASRest;
                    return;
                }

                float mr = unit.UnitData.BaseCombatData.MaxRange;
                float d = (unit.Target.GridPosition - unit.GridPosition).Length();
                if(d > mr) {
                    unit.State = BehaviorFSM.Rest;
                    fDecide = DSRest;
                    fApply = ASRest;
                    fDecide(g, dt);
                }
            }
            void ASCombatRanged(GameState g, float dt) {
                if(unit.Target == null || unit.CombatOrders != BehaviorFSM.CombatRanged) return;

                float mr = unit.UnitData.BaseCombatData.MaxRange;
                float d = (unit.Target.GridPosition - unit.GridPosition).Length();
                etCombat += dt;
                if(d < mr) {
                    if(etCombat > unit.UnitData.BaseCombatData.AttackTimer) {
                        unit.DamageTarget(r.NextDouble());
                        etCombat = 0;
                    }
                }
            }

            void DSCombatMelee(GameState g, float dt) {
                DSCombatRanged(g, dt);
            }
            void ASCombatMelee(GameState g, float dt) {
                ASCombatRanged(g, dt);
            }

            void DS(GameState g, float dt) {

            }
            void AS(GameState g, float dt) {

            }

            public override void Serialize(BinaryWriter s) {
            }
            public override void Deserialize(BinaryReader s) {
            }
        }
    }

    namespace Squad {
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

        public class Targetting : ACSquadTargetingController {
            int teamIndex;
            public override void Init(GameState s, GameplayController c) {
                teamIndex = squad.Team.Input.TeamIndex;
            }

            public override void DecideTarget(GameState g, float dt) {
                //if(targetSquad == null) {
                //    FindTargetSquad(g);
                //    return;
                //}
                //else {
                //    if(targetSquad.IsDead) {
                //        targetSquad = null;
                //        return;
                //    }
                //    else 
                //}

                if(targetUnit == null) {
                    FindTargetUnit(g);
                }
                else if(!targetUnit.IsAlive) {
                    targetUnit = null;
                }
                else if(g.CGrid.GetFogOfWar(targetUnit.GridPosition, teamIndex) != FogOfWar.Active) {
                    targetUnit = null;
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
                targetUnit = null;
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
                            targetUnit = team.Units[i];
                            minDist = d;
                        }
                    }
                }
            }
            public override void ApplyTarget(GameState g, float dt) {
                foreach(var unit in squad.Units) {
                    unit.Target = targetUnit;
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
}