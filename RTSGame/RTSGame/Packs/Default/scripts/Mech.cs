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
            if(squad.MovementController != null)
                squad.MovementController.DecideMoves(g, dt);
        }
        public override void ApplyAction(GameState g, float dt) {
            if(squad.TargettingController != null)
                squad.TargettingController.ApplyTarget(g, dt);
            if(squad.MovementController != null)
                squad.MovementController.ApplyMoves(g, dt);
        }
    }

    public class Movement : ACSquadMovementController {
        ///Squad Logic
        // Decide Where Units In This Squad Should Go When Moving
        // TODO: Make Units Go To Squad Wapoint If Their Posts Are Bad
        public override void ApplyMovementFormation(int movementOrder) {
            switch(movementOrder) {
                case BehaviorFSM.BoxFormation:
                    int numUnits = squad.Units.Count;
                    int numFullRows = (int)Math.Floor(Math.Sqrt(numUnits / phi));
                    if(numFullRows <= 0)
                        return;
                    int numRows = (int)Math.Ceiling(Math.Sqrt(numUnits / phi));
                    int unitsPerFullRow = numUnits / numFullRows;

                    // Determine Spacing Bewteen Units In Formation
                    float spacing = float.MinValue;
                    foreach(var unit in squad.Units) {
                        if(unit.CollisionGeometry.BoundingRadius > spacing) {
                            spacing = unit.CollisionGeometry.BoundingRadius;
                        }
                    }
                    spacing *= 2;

                    // Special Spacing For The Last Row
                    float lastRowSpacing = spacing;
                    int unitsInLastRow = numUnits % numFullRows;
                    if(unitsInLastRow > 0)
                        lastRowSpacing = ((float)unitsPerFullRow)*spacing / ((float)unitsInLastRow);

                    // Calculate Formation As Offsets From Squad Waypoint
                    List<Vector2> formation = new List<Vector2>();
                    float rOffset = (unitsInLastRow > 0) ? numFullRows * spacing / 2.0f : (numFullRows - 1) * spacing / 2.0f;
                    float cOffset;
                    rOffset += spacing;
                    for(int r = 0; r < numFullRows; r++) {
                        rOffset -= spacing;
                        cOffset = -((unitsPerFullRow - 1) * spacing / 2.0f) - spacing;
                        for(int c = 0; c < unitsPerFullRow; c++) {
                            cOffset += spacing;
                            formation.Add(new Vector2(rOffset, cOffset));
                        }
                    }
                    if(unitsInLastRow > 0) {
                        rOffset -= spacing;
                        cOffset = -((unitsInLastRow - 1) * lastRowSpacing / 2.0f) - lastRowSpacing;
                        for(int c = 0; c < unitsInLastRow; c++) {
                            cOffset += lastRowSpacing;
                            formation.Add(new Vector2(rOffset, cOffset));
                        }
                    }

                    // Assign The Units To Posts In The Formation
                    bool[] assigned = new bool[formation.Count];
                    foreach(var unit in squad.Units) {
                        Vector2 pos = unit.GridPosition;
                        float minDistSq = float.MaxValue;
                        int assignment = 0;
                        for(int i = 0; i < formation.Count; i++) {
                            float distSq = Vector2.DistanceSquared(pos, formation[i]);
                            if(!assigned[i] && distSq < minDistSq) {
                                minDistSq = distSq;
                                assignment = i;
                            }
                        }
                        assigned[assignment] = true;
                        formationAssignments[unit.UUID] = formation[assignment];
                    }
                    break;
                case BehaviorFSM.CircleFormation:
                    // TODO: Implement
                    break;
                case BehaviorFSM.FreeFormation:
                    foreach(var unit in squad.Units) {
                        formationAssignments[unit.UUID] = Vector2.Zero;
                    }
                    break;
            }
        }
        // TODO: Implement
        // Decide Where Units In This Squad Should Go When Close To Their Target
        public override void CalculateTargetFormation() {

        }

        ///Unit Movement
        const float DECIDE_DIST = 1f;
        const float STOP_DIST = 0.5f;

        // TODO: Make The Unit Abandon His Post If It Is Collidable And Move Toward The Squad WP

        // Whether This Entity Has Decided To Move
        Dictionary<int,bool> doMove = new Dictionary<int,bool>();

        // The Waypoint To Which This Controller Has Decided To Send Its Entity
        Dictionary<int,Vector2> waypoints = new Dictionary<int,Vector2>();

        public override void DecideMoves(GameState g, float dt) {
            foreach(var unit in squad.Units) {
                if(unit.Target != null) {
                    switch(unit.CombatOrders) {
                        case BehaviorFSM.UseMeleeAttack:
                            waypoints[unit.UUID] = unit.Target.GridPosition;
                            Vector2 udisp = waypoints[unit.UUID] - unit.GridPosition;
                            float ur = unit.CollisionGeometry.BoundingRadius + unit.Target.CollisionGeometry.BoundingRadius;
                            ur *= 1.3f;
                            doMove[unit.UUID] = udisp.LengthSquared() > (ur * ur);
                            if(!doMove[unit.UUID] && unit.State == BehaviorFSM.Walking)
                                unit.State = BehaviorFSM.CombatMelee;
                            continue;
                        case BehaviorFSM.UseRangedAttack:
                            continue;
                    }
                }
                else if(Waypoints == null || Waypoints.Count == 0) {
                    // Pathfinding Has Not Finished: Make The Formation
                    Vector2 post = formationAssignments[unit.UUID];
                    waypoints[unit.UUID] = unit.GridPosition + post;
                    SetDoMove(unit);
                }
                else if(IsValid(CurrentWaypointIndices[unit.UUID])) {
                    // Find This Unit's Formation Post (Offset)
                    int prevWaypointIndex = CurrentWaypointIndices[unit.UUID] + 1;
                    Vector2 prevWaypoint = squad.GridPosition;
                    if(IsValid(prevWaypointIndex)) {
                        prevWaypoint = Waypoints[prevWaypointIndex];
                    }
                    Vector2 currWaypoint = squad.MovementController.Waypoints[CurrentWaypointIndices[unit.UUID]];
                    Vector2 disp = currWaypoint - prevWaypoint;
                    float a = (float)Math.Atan2(disp.Y, disp.X);
                    Vector2 post = RotateFormation(a)[unit.UUID];
                    // Use The Next Squad Waypoint And The Post To Assign A Waypoint
                    waypoints[unit.UUID] = currWaypoint + post;
                    CurrentWaypointIndices[unit.UUID]--;
                    SetDoMove(unit);
                }
                if(!doMove[unit.UUID] && ShouldRest(unit))
                    unit.State = BehaviorFSM.Rest;
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
                if(!doMove[unit.UUID]) continue;

                Vector2 change = waypoints[unit.UUID] - unit.GridPosition;
                if(change != Vector2.Zero) {
                    float magnitude = change.Length();
                    Vector2 scaledChange = (change / magnitude) * minDefaultMoveSpeed * dt;

                    // This Logic Prevents The Unit From Hovering Around Its Goal
                    if(magnitude < STOP_DIST) {
                        if(ShouldRest(unit))
                            unit.State = BehaviorFSM.Rest;
                        continue;
                    }
                    unit.State = BehaviorFSM.Walking;

                    if(scaledChange.LengthSquared() > magnitude * magnitude)
                        unit.Move(change);
                    else
                        unit.Move(scaledChange);
                }
                else if(ShouldRest(unit))
                    unit.State = BehaviorFSM.Rest;
            }
        }

        private bool ShouldRest(RTSUnit unit) {
            return unit.State != BehaviorFSM.CombatMelee && unit.State != BehaviorFSM.CombatRanged;
        }

        private void SetDoMove(RTSUnit unit) {
            Vector2 disp = waypoints[unit.UUID] - unit.GridPosition;
            doMove[unit.UUID] = disp.LengthSquared() > (DECIDE_DIST * DECIDE_DIST);
        }
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
            alRest = new AnimationLoop(60, 149);
            alRest.FrameSpeed = 30;
            alWalk = new AnimationLoop(0, 59);
            alWalk.FrameSpeed = 80;
            alCombat = new AnimationLoop(105, 109);
            alCombat.FrameSpeed = 30;

            SetAnimation(BehaviorFSM.None);
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

// TODO: Finish This Implementation
namespace RTS.Mech.Building {
    public class Action : ACBuildingActionController {
        public override void DecideAction(GameState g, float dt) {
        }
        public override void ApplyAction(GameState g, float dt) {
        }
    }
}
