﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Algorithms;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Graphics;
using RTSEngine.Interfaces;

namespace RTS.Default.Unit {
    public class Action : ACUnitActionController {
        int teamIndex;
        Action<GameState, float> fDecide, fApply;
        Combat cc;
        Movement mc;

        // Targeting Behavior State Info
        Point targetCellPrev = Point.Zero;
        IEntity prevTarget = null;
        Vector2 origin = Vector2.Zero;
        bool moveToOrigin = false;

        public override void SetUnit(RTSUnit u) {
            base.SetUnit(u);
            if(unit != null) {
                // Prevent Units From Running Toward The Location Of A Killed Target
                unit.OnNewTarget += (S, T) => {
                    if(mc != null && T == null)
                        mc.Waypoints = null;
                };
            }
        }

        public override void DecideAction(GameState g, float dt) {
            fDecide(g, dt);
        }
        public override void ApplyAction(GameState g, float dt) {
            fApply(g, dt);
        }

        public override void Init(GameState s, GameplayController c) {
            cc = unit.CombatController as Combat;
            mc = unit.MovementController as Movement;

            unit.TargetingOrders = BehaviorFSM.TargetPassively;
            unit.CombatOrders = BehaviorFSM.UseRangedAttack;
            unit.MovementOrders = BehaviorFSM.JustMove;
            SetState(BehaviorFSM.Rest);

            teamIndex = unit.Team.Index;
        }

        private void SetState(int state) {
            unit.State = state;
            switch(unit.State) {
                case BehaviorFSM.Rest:
                    fDecide = DSMain;
                    fApply = ASRest;
                    break;
                case BehaviorFSM.Walking:
                    fDecide = DSMain;
                    fApply = mc.ApplyMove;
                    break;
                case BehaviorFSM.CombatRanged:
                    fDecide = DSCombatRanged;
                    fApply = ASCombatRanged;
                    break;
                case BehaviorFSM.CombatMelee:
                    fDecide = DSCombatMelee;
                    fApply = ASCombatMelee;
                    break;
            }
        }

        void DSMain(GameState g, float dt) {
            // Default: Rest
            SetState(BehaviorFSM.Rest);
            if(mc != null) {
                mc.DecideMove(g, dt);
                var doMove = mc.doMove;
                if(doMove) {
                    if(unit.Target != null)  // This Is A User-Set Target
                        DSChaseTarget(g, dt);
                    else
                        SetState(BehaviorFSM.Walking);
                }
                else {
                    if(unit.Target != null) { // This Is A SquadTC-Set Target
                        switch(unit.TargetingOrders) {
                            case BehaviorFSM.TargetPassively:
                                // TODO: Implement/Verify
                                break;
                            case BehaviorFSM.TargetAggressively:
                                DSChaseTarget(g, dt);
                                break;
                        }
                    }
                }
            }
        }
        void ASRest(GameState g, float dt) {
            // Do Nothing
        }

        void DSChaseTarget(GameState g, float dt) {
            if(unit.Target == null) {
                SetState(BehaviorFSM.Rest);
                return;
            }
            FogOfWar f = g.CGrid.GetFogOfWar(unit.Target.GridPosition, teamIndex);
            switch(f) {
                case FogOfWar.Active:
                    float mr = unit.Data.BaseCombatData.MaxRange;
                    float d = (unit.Target.GridPosition - unit.GridPosition).Length();
                    float dBetween = d - unit.CollisionGeometry.BoundingRadius - unit.Target.CollisionGeometry.BoundingRadius;
                    switch(unit.CombatOrders) {
                        case BehaviorFSM.UseRangedAttack:
                            if(d <= mr * 0.75) {
                                SetState(BehaviorFSM.CombatRanged);
                                return;
                            }
                            break;
                        case BehaviorFSM.UseMeleeAttack:
                            if(dBetween <= unit.CollisionGeometry.InnerRadius * 0.2f) {
                                SetState(BehaviorFSM.CombatMelee);
                                return;
                            }
                            break;
                    }
                    Point targetCellCurr = HashHelper.Hash(unit.Target.GridPosition, g.CGrid.numCells, g.CGrid.size);
                    bool sameTarget = prevTarget == unit.Target;
                    // If The Target Has Changed Cells And Is Out Of Range, We Need To Pathfind To It
                    if(sameTarget && (targetCellCurr.X != targetCellPrev.X || targetCellCurr.Y != targetCellPrev.Y)) {
                        mc.Query = mc.Pathfinder.ReissuePathQuery(mc.Query, unit.GridPosition, unit.Target.GridPosition, unit.Team.Index);
                        SetState(BehaviorFSM.Rest);
                    }
                    else {
                        SetState(BehaviorFSM.Walking);
                    }
                    prevTarget = unit.Target;
                    targetCellPrev = targetCellCurr;
                    break;
            }
        }
        void DSPassiveTarget(GameState g, float dt) {
            unit.State = BehaviorFSM.Rest;
            Vector2 d = unit.GridPosition - origin;
            float mr = unit.Data.BaseCombatData.MaxRange;
            if(d.LengthSquared() > mr * mr) {
                moveToOrigin = true;
            }
            else {
                DSChaseTarget(g, dt);
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
                //ASChaseTarget(g, dt);
            }
        }

        void DSCombatRanged(GameState g, float dt) {
            if(unit.Target == null || cc == null) {
                SetState(BehaviorFSM.Rest);
                return;
            }
            SetState(BehaviorFSM.CombatRanged);
        }
        void ASCombatRanged(GameState g, float dt) {
            if(unit.Target == null || unit.CombatOrders != BehaviorFSM.UseRangedAttack) {
                SetState(BehaviorFSM.Rest);
                return;
            }
            cc.Attack(g, dt);
        }

        // TODO: Actually Implement Melee
        void DSCombatMelee(GameState g, float dt) {
            if(unit.Target == null || cc == null) {
                SetState(BehaviorFSM.Rest);
                return;
            }
            SetState(BehaviorFSM.CombatMelee);
        }
        void ASCombatMelee(GameState g, float dt) {
            if(unit.Target == null || unit.CombatOrders != BehaviorFSM.UseMeleeAttack) {
                SetState(BehaviorFSM.Rest);
                return;
            }
            cc.Attack(g, dt);
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Serialize        
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Deserialize
        }
    }

    public class Combat : ACUnitCombatController {
        // Random Object For Generating And Testing Critical Hit Rolls
        private static Random critRoller = new Random();

        // The Amount Of Time Remaining Before This Controller's Entity Can Attack Again
        private float attackCooldown;

        public override void Init(GameState s, GameplayController c) {

        }

        public override void Attack(GameState g, float dt) {
            if(attackCooldown > 0)
                attackCooldown -= dt;
            if(!(unit.State == BehaviorFSM.CombatMelee || unit.State == BehaviorFSM.CombatRanged))
                return;
            if(unit.Target != null) {
                if(!unit.Target.IsAlive) {
                    unit.Target = null;
                    return;
                }
                unit.TurnToFace(unit.Target.GridPosition);

                float minDistSquared = unit.Data.BaseCombatData.MinRange * unit.Data.BaseCombatData.MinRange;
                float distSquared = (unit.Target.GridPosition - unit.GridPosition).LengthSquared();
                float maxDistSquared = unit.Data.BaseCombatData.MaxRange * unit.Data.BaseCombatData.MaxRange;
                if(distSquared > maxDistSquared) return;

                if(attackCooldown <= 0) {
                    attackCooldown = unit.Data.BaseCombatData.AttackTimer;
                    if(minDistSquared <= distSquared) {
                        unit.DamageTarget(critRoller.NextDouble());
                        if(!unit.Target.IsAlive) {
                            unit.Target = null;
                        }
                    }
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

    public class Movement : ACUnitMovementController {
        // The Constants Used In Flow Field Calculations
        protected const float dForce = 2f;
        protected const float sForce = 10f;
        protected const float pForce = -200f;

        // Calculate The Unit Force Between Two Locations
        public Vector2 UnitForce(Vector2 a, Vector2 b) {
            Vector2 diff = a - b;
            float mag = diff.LengthSquared();
            return diff.X != 0 && diff.Y != 0 ? diff / mag : Vector2.Zero;
        }

        // How Many Waypoints This Unit Should Lookahead When Updating Its PF Query
        protected const int lookahead = 2;

        public override void Init(GameState s, GameplayController c) {
            Pathfinder = c.pathfinder;
            NetForce = Vector2.Zero;
        }

        // This Unit Movement Controller's Current PathQuery
        public PathQuery Query { get; set; }

        public override void DecideMove(GameState g, float dt) {
            doMove = IsValid(CurrentWaypointIndex) && (Query == null || Query.IsComplete);
            if(!doMove) return;
            // If The Old Path Has Become Invalidated, Send A New Query
            bool invalid = false;
            int end = Math.Max(CurrentWaypointIndex - lookahead, 0);
            for(int i = CurrentWaypointIndex; i > end; i--) {
                Vector2 wp = Waypoints[i];
                Point wpCell = HashHelper.Hash(wp, g.CGrid.numCells, g.CGrid.size);
                if(g.CGrid.GetCollision(wpCell.X, wpCell.Y)) {
                    invalid = true;
                    break;
                }
            }
            if(invalid && (Query == null || Query != null && Query.IsOld)) {
                Vector2 goal = Waypoints[0];
                Query = Pathfinder.ReissuePathQuery(Query, unit.GridPosition, goal, unit.Team.Index);
            }
            SetNetForceAndWaypoint(g);
        }
        public override void ApplyMove(GameState g, float dt) {
            if(NetForce != Vector2.Zero) {
                float magnitude = NetForce.Length();
                Vector2 scaledChange = (NetForce / magnitude) * unit.Squad.MovementController.MinDefaultMoveSpeed * dt;
                // TODO: Make Sure We Don't Overshoot The Goal But Otherwise Move At Max Speed
                if(scaledChange.LengthSquared() > magnitude * magnitude)
                    unit.Move(NetForce);
                else
                    unit.Move(scaledChange);
            }
        }

        private void SetNetForceAndWaypoint(GameState g) {
            CollisionGrid cg = g.CGrid;
            // TODO: Make The Routine Below Fast Enough To Use
            //int tempIdx = 0;
            //Vector2 waypoint = Waypoints[tempIdx];
            //float r = unit.CollisionGeometry.BoundingRadius;
            //while(IsValid(tempIdx)) {
            //    // Get The Waypoint Closest To The Goal That This Unit Can Straight-Shot
            //    if(CoastIsClear(unit.GridPosition, waypoint, r, r, cg)) {
            //        CurrentWaypointIndex = tempIdx;
            //        break;
            //    }
            //    tempIdx++;
            //}
            Vector2 waypoint = Waypoints[CurrentWaypointIndex];
            if(Query != null && !Query.IsOld && Query.IsComplete) {
                Query.IsOld = true; // Only Do This Once Per Query
                Waypoints = Query.waypoints;
                CurrentWaypointIndex = Waypoints.Count - 1;
            }
            // Set Net Force...
            NetForce = pForce * UnitForce(unit.GridPosition, waypoint);
            Point unitCell = HashHelper.Hash(unit.GridPosition, cg.numCells, cg.size);
            // Apply Forces From Other Units In This One's Cell
            foreach(var otherUnit in cg.EDynamic[unitCell.X, unitCell.Y]) {
                NetForce += dForce * UnitForce(unit.GridPosition, otherUnit.GridPosition);
            }
            // Apply Forces From Buildings And Other Units Near This One
            foreach(Point n in Pathfinder.Neighborhood(unitCell)) {
                RTSBuilding b = cg.EStatic[n.X, n.Y];
                if(b != null)
                    NetForce += sForce * UnitForce(unit.GridPosition, b.GridPosition);
                foreach(var otherUnit in cg.EDynamic[n.X, n.Y]) {
                    NetForce += sForce * UnitForce(unit.GridPosition, otherUnit.GridPosition);
                }
            }
            // Set Waypoint...
            Point currWaypointCell = HashHelper.Hash(waypoint, cg.numCells, cg.size);
            bool inGoalCell = unitCell.X == currWaypointCell.X && unitCell.Y == currWaypointCell.Y;
            float SquadRadiusSquared = unit.Squad.MovementController.SquadRadiusSquared;
            if(inGoalCell || (waypoint - unit.GridPosition).LengthSquared() < 1.5 * SquadRadiusSquared) {
                CurrentWaypointIndex--;
            }
        }

        // There Is A Straight-Line Path From A To B That Intersects No Collidable Objects (Ignores Dynamic Entities)
        private bool CoastIsClear(Vector2 a, Vector2 b, float stepSize, float radius, CollisionGrid cg) {
            Vector2 diff = b - a;
            float mag = diff.X != 0 && diff.Y != 0 ? diff.LengthSquared() : 1.0f;
            diff /= mag;
            Vector2 step = stepSize * diff;
            float root2 = 1.41421356237f;
            Func<bool> cont;
            if(a.X < b.X && a.Y < b.Y) {
                cont = () => { return a.X < b.X && a.Y < b.Y; };
            }
            else if(a.X < b.X && a.Y > b.Y) {
                cont = () => { return a.X < b.X && a.Y > b.Y; };
            }
            else if(a.X > b.X && a.Y < b.Y) {
                cont = () => { return a.X > b.X && a.Y < b.Y; };
            }
            else {
                cont = () => { return a.X > b.X && a.Y > b.Y; };
            }
            Vector2[] offsets = {   new Vector2(radius, 0),
                                    new Vector2(radius / 2.0f, 0),
                                    new Vector2(-radius, 0),
                                    new Vector2(-radius / 2.0f, 0),
                                    new Vector2(0, radius),
                                    new Vector2(0, radius / 2.0f),
                                    new Vector2(0, -radius),
                                    new Vector2(0, -radius / 2.0f),
                                    (new Vector2(1, 1) / root2) * radius, 
                                    (new Vector2(1, 1) / root2) * radius / 2.0f,
                                    (new Vector2(-1, 1) / root2) * radius,
                                    (new Vector2(-1, 1) / root2) * radius / 2.0f,
                                    (new Vector2(1, -1) / root2) * radius,
                                    (new Vector2(1, -1) / root2) * radius / 2.0f,
                                    (new Vector2(-1, -1) / root2) * radius,
                                    (new Vector2(-1, -1) / root2) * radius / 2.0f   };
            while(cont()) {
                bool collision = cg.GetCollision(a);
                foreach(var offset in offsets) {
                    collision |= cg.GetCollision(a + offset);
                }
                if(collision)
                    return false;
                a += step;
            }
            return true;
        }

        public override void Serialize(BinaryWriter s) {

        }

        public override void Deserialize(BinaryReader s) {

        }
    }

    public class Animation : ACUnitAnimationController {
        private static Random r = new Random();
        private GameState state;

        private AnimationLoop alRest, alWalk, alMelee, alPrepareFire, alFire, alDeath;
        private AnimationLoop alCurrent;

        private float rt;
        private int lastState;

        public Animation() {
            alRest = new AnimationLoop(0, 59);
            alRest.FrameSpeed = 30;
            alWalk = new AnimationLoop(60, 119);
            alWalk.FrameSpeed = 80;
            alMelee = new AnimationLoop(120, 149);
            alMelee.FrameSpeed = 30;
            alPrepareFire = new AnimationLoop(150, 179);
            alPrepareFire.FrameSpeed = 50;
            alFire = new AnimationLoop(180, 199);
            alFire.FrameSpeed = 60;
            alDeath = new AnimationLoop(200, 259);
            alDeath.FrameSpeed = 40;

            SetAnimation(BehaviorFSM.None);
        }

        public override void Init(GameState s, GameplayController c) {
            state = s;
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
                    alCurrent = alMelee;
                    alCurrent.Restart(false);
                    break;
                case BehaviorFSM.CombatRanged:
                    alCurrent = alFire;
                    alCurrent.Restart(false);
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

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }
}