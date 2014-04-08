using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using RTSEngine.Controllers;
using RTSEngine.Graphics;
using RTSEngine.Algorithms;

namespace RTS.Mech.Squad {
    public class Action : ACSquadActionController {
        public override void DecideAction(GameState g, float dt) {
            //if(squad.TargetingController != null)
            //    squad.TargetingController.DecideTarget(g, dt);
            if(squad.MovementController != null)
                squad.MovementController.DecideMoves(g, dt);
        }
        public override void ApplyAction(GameState g, float dt) {
            //if(squad.TargetingController != null)
            //    squad.TargetingController.ApplyTarget(g, dt);
            if(squad.MovementController != null)
                squad.MovementController.ApplyMoves(g, dt);
        }
    }

    public class Movement : ACSquadMovementController {
        // Whether Units In This Squad Have Decided To Move (Key: UUID)
        Dictionary<int, bool> doMove = new Dictionary<int, bool>();

        float a = 0f;

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
                }
                else if(unit.State != BehaviorFSM.CombatMelee && unit.State != BehaviorFSM.CombatRanged)
                    unit.State = BehaviorFSM.Rest;
            }
        }

        private void SetNetForceAndMove(GameState g, RTSUnit unit, Vector2 waypoint, List<Vector2> targetFormation) {
            // Set Net Force
            Vector2 netForce = squad.Units.Count*Force(unit, waypoint);
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
                    netForce += 5*Force(unit, b);
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
            if(inGoalCell || (waypoint - unit.GridPosition).LengthSquared() < stopDist*stopDist) {
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
    }

    public class Target : ACSquadTargetingController {
        public override void DecideTarget(GameState g, float dt) {
            if(targetSquad == null) {
                FindTargetSquad(g);
                return;
            }
            else {
                if(targetSquad.Units.Count < 1) {
                    targetSquad = null;
                    return;
                }
                else if(targetUnit == null) {
                    FindTargetUnit(g);
                }
                else if(!targetUnit.IsAlive) {
                    targetUnit = null;
                }
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
            for(int i = 0; i < targetSquad.Units.Count; i++) {
                float d = (targetSquad.Units[i].GridPosition - squad.GridPosition).LengthSquared();
                if(d < minDist) {
                    targetUnit = targetSquad.Units[i];
                    minDist = d;
                }
            }
        }
        public override void ApplyTarget(GameState g, float dt) {
            foreach(var unit in squad.Units) {
                unit.Target = targetUnit;
            }
        }
    }
}

namespace RTS.Mech.Unit {
    public class Action : ACUnitActionController {
        public override void DecideAction(GameState g, float dt) {
            // TODO: The Real FSM
            switch(unit.State) {
                case BehaviorFSM.CombatMelee:
                    unit.CollisionGeometry.IsStatic = true;
                    break;
                case BehaviorFSM.CombatRanged:
                    unit.CollisionGeometry.IsStatic = true;
                    break;
                case BehaviorFSM.Rest:
                    if(unit.Squad.MovementController == null)
                        unit.CollisionGeometry.IsStatic = true;
                    break;
                default:
                    unit.CollisionGeometry.IsStatic = false;
                    break;
            }
        }
        public override void ApplyAction(GameState g, float dt) {
            if(unit.CombatController != null)
                unit.CombatController.Attack(g, dt);
        }
    }

    public class Animation : ACUnitAnimationController {
        private static Random r = new Random();

        private AnimationLoop alRest, alWalk, alCombat;
        private AnimationLoop alCurrent;

        private float rt;
        private int lastState;

        public Animation() {
            alRest = new AnimationLoop(0, 59);
            alRest.FrameSpeed = 30;
            alWalk = new AnimationLoop(60, 119);
            alWalk.FrameSpeed = 80;
            alCombat = new AnimationLoop(120, 149);
            alCombat.FrameSpeed = 30;

            SetAnimation(BehaviorFSM.None);
        }

        public override void SetUnit(RTSUnit u) {
            base.SetUnit(u);
            if(unit != null) {
                unit.OnAttackMade += unit_OnAttackMade;
            }
        }

        void unit_OnAttackMade(ICombatEntity arg1, IEntity arg2) {
            Vector3 o = arg1.WorldPosition + Vector3.Up;
            Vector3 d = arg2.WorldPosition + Vector3.Up;
            d -= o;
            d.Normalize();
            BulletParticle bp = new BulletParticle(o, d, 0.05f, 1.4f, 0.1f);
            bp.instance.Tint = Color.Red;
            AddParticle(bp);
        }

        private void SetAnimation(int state) {
            switch(state) {
                case BehaviorFSM.Walking:
                    alCurrent = alWalk;
                    alCurrent.Restart(true);
                    break;
                case BehaviorFSM.CombatMelee:
                    alCurrent = alCombat;
                    alCurrent.Restart(true);
                    break;
                case BehaviorFSM.Rest:
                    alCurrent = alRest;
                    alCurrent.Restart(true);
                    break;
                default:
                    alCurrent = null;
                    return;
            }
        }
        public override void Update(GameState s, float dt) {
            if(lastState != unit.State) {
                // A New Animation State If Provided
                SetAnimation(unit.State);
                if(lastState == BehaviorFSM.None) {
                    rt = r.Next(120, 350) / 10f;
                }
            }

            // Save Last State
            lastState = unit.State;

            // Step The Current Animation
            if(alCurrent != null) {
                alCurrent.Step(dt);
                AnimationFrame = alCurrent.CurrentFrame;
            }

            if(lastState == BehaviorFSM.None) {
                // Check For A Random Animation
                if(alCurrent == null) {
                    rt -= dt;
                    if(rt <= 0) {
                        rt = r.Next(120, 350) / 10f;
                        alCurrent = alRest;
                        alCurrent.Restart(false);
                    }
                }
                else {
                    // Check If At The End Of The Loop
                    if(AnimationFrame == alCurrent.EndFrame) {
                        alCurrent = null;
                        rt = r.Next(120, 350) / 10f;
                    }
                }
            }
        }
    }

    public class Combat : ACUnitCombatController {
        // Random Object For Generating And Testing Critical Hit Rolls
        private static Random critRoller = new Random();

        // The Amount Of Time Remaining Before This Controller's Entity Can Attack Again
        private float attackCooldown;

        public override void Attack(GameState g, float dt) {
            if(attackCooldown > 0)
                attackCooldown -= dt;
            if(unit.State != BehaviorFSM.None)
                return;
            if(unit.Target != null) {
                if(!unit.Target.IsAlive) {
                    unit.Target = null;
                    return;
                }
                float minDistSquared = unit.UnitData.BaseCombatData.MinRange * unit.UnitData.BaseCombatData.MinRange;
                float distSquared = (unit.Target.WorldPosition - unit.WorldPosition).LengthSquared();
                float maxDistSquared = unit.UnitData.BaseCombatData.MaxRange * unit.UnitData.BaseCombatData.MaxRange;
                if(distSquared > maxDistSquared) return;

                if(attackCooldown <= 0) {
                    attackCooldown = unit.UnitData.BaseCombatData.AttackTimer;
                    unit.State = BehaviorFSM.CombatMelee;
                    if(minDistSquared <= distSquared) {
                        unit.DamageTarget(critRoller.NextDouble());
                        if(!unit.Target.IsAlive) unit.Target = null;
                    }
                }
            }
        }
    }

    public class Movement : ACUnitMovementController {
    
    }
}

// TODO: Verify
namespace RTS.Mech.Building {
    public class Action : ACBuildingActionController {
        private Queue<int> unitQueue = new Queue<int>();

        public float buildTime; // How Long It Takes To Finish Producing The Unit
        private int unit = -1; // Unit To Be Produced
        public override void DecideAction(GameState g, float dt) {
            if (unit < 0 && unitQueue.Count > 0)
            {
                unit = unitQueue.Dequeue();
                buildTime = building.Team.Race.Units[unit].BuildTime;
            }
        }

        public override void ApplyAction(GameState g, float dt) {
            // If The Unit Is Still Being Produced
            if (unit >= 0) {
                buildTime -= dt;
                // If Finished Building The Unit
                if(buildTime < 0) {
                    building.Team.AddUnit(unit, building.GridPosition);
                    buildTime = 0;
                    unit = -1;
                }
            }
        }
    }
}
