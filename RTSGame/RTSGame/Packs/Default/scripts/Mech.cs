using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using RTSEngine.Controllers;

namespace RTS.Mech.Squad {
    public class Action : ACSquadActionController {
        public override void DecideAction(GameState g, float dt) {
            if(squad.TargettingController != null)
                squad.TargettingController.DecideTarget(g, dt);
        }
        public override void ApplyAction(GameState g, float dt) {
            if(squad.TargettingController != null)
                squad.TargettingController.ApplyTarget(g, dt);
        }
    }

    public class Movement : ACSquadMovementController {

    }

    public class Target : ACSquadTargettingController {
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
                foreach(var sq in team.squads) {
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
            // Change FSM

            if(unit.MovementController != null)
                unit.MovementController.DecideMove(g, dt);
        }
        public override void ApplyAction(GameState g, float dt) {
            if(unit.MovementController != null)
                unit.MovementController.ApplyMove(g, dt);
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

            SetAnimation(FSMState.None);
        }

        private void SetAnimation(int state) {
            switch(state) {
                case FSMState.Walking:
                    alCurrent = alWalk;
                    alCurrent.Restart(true);
                    break;
                case FSMState.CombatMelee:
                    alCurrent = alCombat;
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
                if(lastState == FSMState.None) {
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

            if(lastState == FSMState.None) {
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
            if(unit.State != FSMState.None)
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
                    unit.State = FSMState.CombatMelee;
                    if(minDistSquared <= distSquared) {
                        unit.DamageTarget(critRoller.NextDouble());
                        if(!unit.Target.IsAlive) unit.Target = null;
                    }
                }
            }
        }
    }

    public class Movement : ACUnitMovementController {
        const float DECIDE_DIST = 1f;
        const float STOP_DIST = 0.5f;

        // The Waypoint To Which This Controller Has Decided To Send Its Entity
        bool doMove;
        protected Vector2 waypoint;

        public override void DecideMove(GameState g, float dt) {
            if(unit.Target != null) {
                waypoint = unit.Target.GridPosition;
                Vector2 udisp = waypoint - unit.GridPosition;
                float ur = unit.CollisionGeometry.BoundingRadius + unit.Target.CollisionGeometry.BoundingRadius;
                ur *= 1.3f;
                doMove = udisp.LengthSquared() > (ur * ur);
                if(!doMove && unit.State == FSMState.Walking)
                    unit.State = FSMState.CombatMelee;
                return;
            }
            else if(Waypoints.Count < 1) return;

            waypoint = Waypoints[Waypoints.Count - 1];
            Vector2 disp = waypoint - unit.GridPosition;
            doMove = disp.LengthSquared() > (DECIDE_DIST * DECIDE_DIST);

            if(!doMove && unit.State == FSMState.Walking)
                unit.State = FSMState.None;
        }
        public override void ApplyMove(GameState g, float dt) {
            if(!doMove) return;

            Vector2 change = waypoint - unit.GridPosition;
            if(change != Vector2.Zero) {
                float magnitude = change.Length();
                Vector2 scaledChange = (change / magnitude) * unit.MovementSpeed * dt;

                // This Logic Prevents The Unit From Hovering Around Its Goal
                if(magnitude < STOP_DIST) {
                    if(unit.State == FSMState.Walking)
                        unit.State = FSMState.None;
                    return;
                }
                unit.State = FSMState.Walking;

                if(scaledChange.LengthSquared() > magnitude * magnitude)
                    unit.Move(change);
                else
                    unit.Move(scaledChange);
            }
            else if(unit.State == FSMState.Walking)
                unit.State = FSMState.None;
        }
    }
}

// TODO: Verify
namespace RTS.Mech.Building {
    public class Action : ACBuildingActionController {
        public float buildTime; // How Long It Takes To Finish Producing The Unit
        private RTSUnit unit; // Unit To Be Produced

        public override void DecideAction(GameState g, float dt) {
            if (unit == null && Building.UnitQueue.Count > 0) {
                unit = Building.UnitQueue.Dequeue();
                buildTime = unit.UnitData.BuildTime;
            }
        }

        public override void ApplyAction(GameState g, float dt) {
            // If Finished Building The Unit
            if (buildTime < 0) {
                Building.Team.AddUnit(unit.Type, unit.GridPosition);
                buildTime = 0;
                unit == null;
            }
            // If The Unit Is Still Being Produced
            if (unit != null) {
                buildTime -= dt;
            }
        }
    }
}