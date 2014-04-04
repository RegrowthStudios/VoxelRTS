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
        // Decide Where Units In This Squad Should Go When Moving
        // TODO: Make Units Go To Squad Wapoint If Their Posts Are Bad
        public override void ApplyMovementFormation(int movementOrder) {
            switch(movementOrder) {
                case BehaviorFSM.BoxFormation:
                    // Determine Spacing Bewteen Units In Formation
                    float spacing = float.MinValue;
                    foreach(var unit in squad.Units) {
                        if(unit.CollisionGeometry.BoundingRadius > spacing) {
                            spacing = unit.CollisionGeometry.BoundingRadius;
                        }
                    }
                    spacing *= 2;
                    int numUnits = squad.Units.Count;
                    int numFullRows = (int)Math.Floor(Math.Sqrt(numUnits / phi));
                    int numRows = (int)Math.Ceiling(Math.Sqrt(numUnits / phi));
                    int unitsPerFullRow = numFullRows <= 0 ? numUnits : numUnits / numFullRows;

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
            // TODO: Real Behavior FSM
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
            alWalk.FrameSpeed = 50;
            alCombat = new AnimationLoop(120, 149);
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
        const float DECIDE_DIST = 1f;
        const float STOP_DIST = 0.5f;

        // TODO: Make The Unit Abandone His Post If It Is Collidable And Move Toward The Squad WP

        // Whether This Entity Has Decided To Move
        bool doMove;

        // The Waypoint To Which This Controller Has Decided To Send Its Entity
        protected Vector2 waypoint;

        public override void DecideMove(GameState g, float dt) {
            if(unit.Target != null) {
                switch(unit.CombatOrders) {
                    case BehaviorFSM.UseMeleeAttack:
                        waypoint = unit.Target.GridPosition;
                        Vector2 udisp = waypoint - unit.GridPosition;
                        float ur = unit.CollisionGeometry.BoundingRadius + unit.Target.CollisionGeometry.BoundingRadius;
                        ur *= 1.3f;
                        doMove = udisp.LengthSquared() > (ur * ur);
                        if(!doMove && unit.State == BehaviorFSM.Walking)
                            unit.State = BehaviorFSM.CombatMelee;
                        return;
                    case BehaviorFSM.UseRangedAttack:
                        return;
                }
            }
            else if(!HasPath) {
                // Pathfinding Has Not Finished: Make The Formation
                if (unit.Squad != null && unit.Squad.MovementController != null) {
                    Vector2 post = unit.Squad.MovementController.formationAssignments[unit.UUID];
                    waypoint = unit.GridPosition + post;
                    SetDoMove();
                }
            }
            else if(IsValid(CurrentWaypointIndex)) {
                // Find This Unit's Formation Post (Offset)
                int prevWaypointIndex = CurrentWaypointIndex + 1;
                Vector2 prevWaypoint = unit.Squad.GridPosition;
                if(IsValid(prevWaypointIndex)) {
                    prevWaypoint = unit.Squad.MovementController.Waypoints[prevWaypointIndex];
                }
                Vector2 currWaypoint = unit.Squad.MovementController.Waypoints[CurrentWaypointIndex];
                Vector2 disp = currWaypoint - prevWaypoint;
                float a = (float)Math.Atan2(disp.Y, disp.X);
                Vector2 post = unit.Squad.MovementController.RotateFormation(a)[unit.UUID];
                // Use The Next Squad Waypoint And The Post To Assign A Waypoint
                waypoint = currWaypoint + post;
                CurrentWaypointIndex--;
                SetDoMove();
            }
            if(!doMove && ShouldRest())
                unit.State = BehaviorFSM.Rest;
        }
        public override void ApplyMove(GameState g, float dt) {
            if(!doMove) return;

            Vector2 change = waypoint - unit.GridPosition;
            if(change != Vector2.Zero) {
                float magnitude = change.Length();
                Vector2 scaledChange = (change / magnitude) * unit.MovementSpeed * dt;

                // This Logic Prevents The Unit From Hovering Around Its Goal
                if(magnitude < STOP_DIST) {
                    if(ShouldRest())
                        unit.State = BehaviorFSM.Rest;
                    return;
                }
                unit.State = BehaviorFSM.Walking;

                if(scaledChange.LengthSquared() > magnitude * magnitude)
                    unit.Move(change);
                else
                    unit.Move(scaledChange);
            }
            else if(ShouldRest())
                unit.State = BehaviorFSM.Rest;
        }
        private bool ShouldRest() {
            return unit.State != BehaviorFSM.CombatMelee && unit.State != BehaviorFSM.CombatRanged;
        }
        private void SetDoMove() {
            Vector2 disp = waypoint - unit.GridPosition;
            doMove = disp.LengthSquared() > (DECIDE_DIST * DECIDE_DIST);
        }
    }
}

// TODO: Finish This Implementation
//namespace RTS.Mech.Building {
//    public class Action : ACBuildingActionController {
//        public int time = -1;
//        // Update Unit Build Time
//        public void Update(GameState g) {
//            if (Building.UnitQueue.Count > 0) {
//                // Initialize Timer If Building Is Just Starting To Produce
//                if (time == -1)
//                    time = Building.UnitQueue.Peek().UnitData.BuildTime;
//                else if (time == 0) {
//                    SpawnUnit(g);
//                    ApplyEnvImpact(g);
//                }
//                time--;
//            }
//        }

//        // Produce Unit
//        public void SpawnUnit (GameState g){
            
//        }

//        // Apply Environmental Impact
//        public void  ApplyEnvImpact(GameState g){
         
//        }
//    }
//}