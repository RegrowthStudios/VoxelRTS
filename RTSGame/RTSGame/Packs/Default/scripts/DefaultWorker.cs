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
using RTSEngine.Graphics;
using RTSEngine.Interfaces;

namespace RTS.Default.Worker {
    public class Action : ACUnitActionController {
        int teamIndex;
        Action<GameState, float> fDecide, fApply;
        Combat cc;
        Movement mc;

        // Targeting Behavior State Info
        Point targetCellPrev = Point.Zero;
        IEntity prevTarget = null;

        // Worker Specific Stuff
        private RTSBuilding targetResource;
        private RTSBuilding targetDistro;
        bool depositing;

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

        public override void Init(RTSEngine.Data.GameState s, RTSEngine.Controllers.GameplayController c) {
            cc = unit.CombatController as Combat;
            mc = unit.MovementController as Movement;

            unit.TargetingOrders = BehaviorFSM.TargetPassively;
            unit.CombatOrders = BehaviorFSM.UseMeleeAttack;
            unit.MovementOrders = BehaviorFSM.JustMove;
            SetState(BehaviorFSM.Rest);

            teamIndex = unit.Team.Index;

            // Worker Specific Stuff
            targetResource = null;
            targetDistro = null;
            depositing = false;
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
                case BehaviorFSM.Harvest:
                case BehaviorFSM.Build:    
                case BehaviorFSM.Repair:
                case BehaviorFSM.CombatMelee:
                    fDecide = DSMain;
                    fApply = ApplyWorkerAction;
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
                    if(unit.Target != null)  {// This Is A User-Set Target
                        DSChaseTarget(g, dt);
                    }
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

        void ASRest(GameState g, float dt) { /* Do nothing */ }

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
                        // Melee Attack Will Be Used For Many Special Worker Functions
                        case BehaviorFSM.UseMeleeAttack:
                            if(dBetween <= unit.CollisionGeometry.InnerRadius * 0.2f) {
                                DecideWorkerAction(g, dt);
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

        void DecideWorkerAction(GameState g, float dt) {
            if (unit.Target == null)
                SetState(BehaviorFSM.Rest);
            RTSBuilding targetB = unit.Target as RTSBuilding;
            // If Target Is On The Same Team -> Might Be A Depositable Building, Etc.
            if (unit.Target.Team.Index == teamIndex) {
                if(targetB != null) {
                    // Prioritize Building Unbuilt buildings
                    if(!targetB.IsBuilt) {
                        SetState(BehaviorFSM.Build);
                    }
                    // If Target Is Player's Depositable Building
                    else if(targetB.Data.Depositable) {
                        targetDistro = targetB;
                        SetState(BehaviorFSM.Harvest); // TODO: Make This Harvest Only Once
                        depositing = true;
                    }
                    // If Target Is The Player's Undepositable Building, Repair
                    else {
                        SetState(BehaviorFSM.Repair);
                    }
                }
            }
            // If Target Is On Another Team
            else {
                if(targetB != null) {
                    if (targetB.IsResource) {
                        SetState(BehaviorFSM.Harvest);
                        depositing = false;
                    }
                }
                else {
                    SetState(BehaviorFSM.CombatMelee);
                }
            }
        }

        void ApplyWorkerAction(GameState g, float dt) {
            if(unit.Target == null || cc == null) {
                SetState(BehaviorFSM.Rest);
                return;
            }
            RTSBuilding targetB = unit.Target as RTSBuilding;
            switch(unit.State) {
                case BehaviorFSM.Build:
                    cc.Attack(g, dt);
                    if(targetB != null && targetB.BuildAmountLeft <= 0) {
                        SetState(BehaviorFSM.Rest);
                    }
                    break;
                case BehaviorFSM.Repair:
                    cc.Attack(g, dt);
                    if(targetB != null && targetB.Health >= targetB.Data.Health) {
                        SetState(BehaviorFSM.Rest);
                    }
                    break;
                case BehaviorFSM.Harvest:
                    if(depositing)
                        ASDeposit(g, dt);
                    else {
                        cc.Attack(g, dt);
                        SetState(BehaviorFSM.Harvest);
                    }
                    break;
                case BehaviorFSM.CombatMelee:
                    cc.Attack(g, dt);
                    break;
            }
        }

        // Worker-Specific Things
        // TODO: Why /2?
        void ASDeposit(GameState g, float dt) {
            if (unit.Resources == 0) return;
            unit.Team.Input.AddEvent(new CapitalEvent(teamIndex, unit.Resources/2));
            unit.Resources = 0;
            depositing = false;
        }

        void DSHarvest(GameState g, float dt) {
            // Look For A Distro
            if(unit.Resources > unit.Data.CarryingCapacity) {
                // Always Go To Nearest Distro
                targetDistro = GetClosestDepository();
                unit.Target = targetDistro;
                SetState(BehaviorFSM.Rest); 
            }
            else if(unit.Target == null || !unit.Target.IsAlive) {
                // Look For More Harvestable Stuff
                if(targetResource == null) // Only Target A New Resource If The Old One Is Bad
                    targetResource = GetClosestResource(g);
                unit.Target = GetClosestResource(g);
                if(unit.Target == null) { // No Resources Found
                    targetDistro = GetClosestDepository();
                    unit.Target = targetDistro;
                }
                SetState(BehaviorFSM.Rest);        
            }
            depositing = unit.Target == targetDistro;
        }

        // Helper Method For Finding The Closest Depositable Building
        private RTSBuilding GetClosestDepository() {
            float minDistSq = float.MaxValue;
            RTSBuilding depository = null;
            for (int i = 0; i < unit.Team.Buildings.Count; i++) {
                var building = unit.Team.Buildings[i];
                if(!building.Data.Depositable)
                    continue;
                float distSq = (building.GridPosition - unit.GridPosition).LengthSquared();
                if(distSq < minDistSq) {
                    minDistSq = distSq;
                    depository = building;
                }
            }
            return depository;
        }

        // Helper Method For Finding The Closest Resource In Region
        // TODO: Decide If Worker Should Only Work "In-Region"
        private RTSBuilding GetClosestResource(GameState g) {
            float minDistSq = float.MaxValue;
            RTSBuilding resource = null;
            for(int ti = 0; ti < g.activeTeams.Length; ti++) {
                // Don't Automatically Self-Target
                if(g.activeTeams[ti].Index == teamIndex)
                    continue;
                // TODO: Ignore Teams That Aren't On The Environment
                RTSTeam team = g.activeTeams[ti].Team;
                for(int i = 0; i < team.Buildings.Count; i++) {
                    RTSBuilding b = team.Buildings[i];
                    // This Check Makes Sure The Candidate Target Is In Range Of The Squad
                    if(g.CGrid.GetFogOfWar(b.GridPosition, teamIndex) != FogOfWar.Active)
                        continue;
                    // Make Sure This Building Is A Resource
                    if(!b.IsResource)
                        continue;
                    float d = (b.GridPosition - unit.GridPosition).LengthSquared();
                    if(d < minDistSq) {
                        resource = b;
                        minDistSq = d;
                    }
                }
            }
            return resource;
        }

        public override void Deserialize(System.IO.BinaryReader s) {
            // TODO
        }
        public override void Serialize(System.IO.BinaryWriter s) {
            // TODO
        }
    }

    // Combat & Harvesting Are Controlled Here
    public class Combat : ACUnitCombatController {
        // Random Object For Generating And Testing Critical Hit Rolls
        private static Random critRoller = new Random();
        private float attackCooldown;

        public override void Init(GameState s, GameplayController c) {}

        public override void Attack(GameState g, float dt) {
            if(attackCooldown > 0)
                attackCooldown -= dt;
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

                // TODO: Verify Damage:Resource & Damage:Capital & Damage:BuildAmt Ratios
                if(attackCooldown <= 0) {
                    attackCooldown = unit.Data.BaseCombatData.AttackTimer;
                    int damage = unit.Data.BaseCombatData.AttackDamage;
                    if(minDistSquared <= distSquared) {
                        RTSBuilding bTarget = unit.Target as RTSBuilding;
                        switch(unit.State) {
                            case BehaviorFSM.Build:
                                if(bTarget != null && !bTarget.IsBuilt) { // Only Build Unbuilt Buildings
                                    bTarget.BuildAmountLeft -= damage;
                                    if(bTarget.BuildAmountLeft < 0) bTarget.BuildAmountLeft = 0;
                                }
                                break;
                            case BehaviorFSM.Harvest:
                                if(bTarget != null && bTarget.IsResource) { // Only Harvest Resources
                                    bTarget.Damage(damage);
                                    unit.Resources += damage;
                                }
                                break;
                            case BehaviorFSM.Repair:
                                if(bTarget != null) { // Only Repair Buildings
                                    // Negative Damage = Heal
                                    bTarget.Damage(-damage);
                                    // Clamp Repaired Health To Target's Max Health
                                    if(bTarget.Health > bTarget.Data.Health)
                                        bTarget.Health = bTarget.Data.Health;
                                    unit.Team.Input.AddEvent(new CapitalEvent(unit.Team.Index, -damage));
                                }
                                break;
                            case BehaviorFSM.CombatMelee:
                                unit.DamageTarget(critRoller.NextDouble());
                                break;
                        }
                    }
                }
                if(!unit.Target.IsAlive) unit.Target = null;
            }
        }

        public override void Deserialize(System.IO.BinaryReader s) {
            // TODO
        }
        public override void Serialize(System.IO.BinaryWriter s) {
            // TODO
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
            NetForce = pForce*UnitForce(unit.GridPosition, waypoint);
            Point unitCell = HashHelper.Hash(unit.GridPosition, cg.numCells, cg.size);
            // Apply Forces From Other Units In This One's Cell
            foreach(var otherUnit in cg.EDynamic[unitCell.X, unitCell.Y]) {
                NetForce += dForce*UnitForce(unit.GridPosition, otherUnit.GridPosition);
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
                cont = () => {return a.X < b.X && a.Y < b.Y; };
            }
            else if(a.X < b.X && a.Y > b.Y) {
                cont = () => {return a.X < b.X && a.Y > b.Y; };
            }
            else if(a.X > b.X && a.Y < b.Y) {
                cont = () => {return a.X > b.X && a.Y < b.Y; };
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

        public override void Init(GameState s, GameplayController c) {

        }

        public override void SetUnit(RTSUnit u) {
            base.SetUnit(u);
            if (unit != null) {
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
            switch (state) {
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
                case BehaviorFSM.Harvest: //TODO: Add harvest animation
                    alCurrent = alCombat;
                    alCurrent.Restart(true);
                    break;
                case BehaviorFSM.Repair: //TODO: Add repair animation
                    alCurrent = alCombat;
                    alCurrent.Restart(true);
                    break;
                default:
                    alCurrent = null;
                    return;
            }
        }
        public override void Update(GameState s, float dt) {
            if (lastState != unit.State) {
                // A New Animation State If Provided
                SetAnimation(unit.State);
                if (lastState == BehaviorFSM.None) {
                    rt = r.Next(120, 350) / 10f;
                }
            }

            // Save Last State
            lastState = unit.State;

            // Step The Current Animation
            if (alCurrent != null) {
                alCurrent.Step(dt);
                AnimationFrame = alCurrent.CurrentFrame;
            }

            if (lastState == BehaviorFSM.None) {
                // Check For A Random Animation
                if (alCurrent == null) {
                    rt -= dt;
                    if (rt <= 0) {
                        rt = r.Next(120, 350) / 10f;
                        alCurrent = alRest;
                        alCurrent.Restart(false);
                    }
                }
                else {
                    // Check If At The End Of The Loop
                    if (AnimationFrame == alCurrent.EndFrame) {
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