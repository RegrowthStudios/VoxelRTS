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

namespace RTS.Default.Unit {
    public class Action : ACUnitActionController {
        int teamIndex;
        Action<GameState, float> fDecide, fApply;
        ACUnitCombatController cc;
        ACUnitMovementController mc;

        // Targeting Behavior State Info
        const int PF_COOLDOWN = 10; // Number Of ChaseTarget Calls Before A New Path Query Is Sent
        int pfCounter = 0;
        Point targetCellPrev = Point.Zero;
        IEntity prevTarget = null;
        bool attackMoving = false;
        bool passivelyTargeting = false;
        Vector2 origin;
        float prepareCooldown;

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

        public override void Reset() {
            attackMoving = false;
            passivelyTargeting = false;
            targetCellPrev = Point.Zero;
            prevTarget = null;
            fDecide = DSMain;
        }

        public override void Init(GameState s, GameplayController c, object args) {
            cc = unit.CombatController;
            mc = unit.MovementController;

            unit.TargetingOrders = BehaviorFSM.TargetPassively;
            unit.CombatOrders = BehaviorFSM.UseRangedAttack;
            unit.MovementOrders = BehaviorFSM.JustMove;
            SetState(BehaviorFSM.Rest);

            teamIndex = unit.Team.Index;
            origin = unit.GridPosition;
        }

        private void SetState(int state) {
            switch(state) {
                case BehaviorFSM.Rest:
                    fDecide = DSMain;
                    fApply = ASRest;
                    break;
                case BehaviorFSM.Walking:
                    fDecide = DSMain;
                    fApply = mc.ApplyMove;
                    break;
                case BehaviorFSM.PrepareCombatRanged:
                    if(unit.State != state) {
                        cc.Reset();
                        fDecide = DSPrepareCombatRanged;
                        fApply = ASPrepareCombatRanged;
                        prepareCooldown = unit.Data.BaseCombatData.SetupTimer;
                    }
                    break;
                case BehaviorFSM.CombatRanged:
                    if(unit.State != state) cc.Reset();
                    fDecide = DSCombatRanged;
                    fApply = ASCombatRanged;
                    break;
                case BehaviorFSM.CombatMelee:
                    if(unit.State != state) cc.Reset();
                    fDecide = DSCombatMelee;
                    fApply = ASCombatMelee;
                    break;
            }
            // Update The Unit's State
            unit.State = state;
        }

        void DSMain(GameState g, float dt) {
            // Default: Rest
            SetState(BehaviorFSM.Rest);
            if(mc == null || !unit.IsAlive) return; // Units Must Have A Movement Controller And Be Living
            // Check For A-Move
            if(unit.Target == null) {
                if(unit.MovementOrders == BehaviorFSM.AttackMove) {
                    FindTarget(g, dt);
                    if(unit.Target != null) {
                        attackMoving = true;
                        DSChaseTarget(g, dt, true);
                    }
                    else if(attackMoving) {
                        unit.Team.Input.AddEvent(new SetWayPointEvent(teamIndex, mc.Goal));
                        attackMoving = false;
                    }
                }
            }
            else {
                bool inFog = g.CGrid.GetFogOfWar(unit.Target.GridPosition, teamIndex) != FogOfWar.Active;
                if(!unit.Target.IsAlive || inFog)
                    unit.Target = null;
            }
            mc.DecideMove(g, dt);
            var doMove = mc.doMove;
            if(doMove) {
                if(unit.Target != null)  // This Is A User-Set Target
                    DSChaseTarget(g, dt, true);
                else
                    SetState(BehaviorFSM.Walking);
            }
            else {
                if(unit.Target == null) FindTarget(g, dt);
                DSChaseTarget(g, dt, false); 
                //DSPassiveTarget(g, dt); // Currently Broken
            }
        }
        void ASRest(GameState g, float dt) {
            // Do Nothing
        }

        public void FindTarget(GameState g, float dt) {
            IEntity target = null;
            // If enemy unit is around, target him automatically
            float minDistSq = unit.Data.BaseCombatData.MaxRange;
            minDistSq *= minDistSq;
            foreach (IndexedTeam t in g.activeTeams.ToArray()) {
                RTSTeam team = t.Team;
                if(teamIndex != t.Index && team.Input.Type != RTSInputType.Environment) { // Enemy team check
                    foreach (RTSUnit enemy in team.Units.ToArray()) {
                        if(g.CGrid.GetFogOfWar(enemy.GridPosition, teamIndex) != FogOfWar.Active)
                            continue;
                        if(!enemy.IsAlive) continue;
                        float d = (enemy.GridPosition - unit.GridPosition).LengthSquared();
                        if (d <= minDistSq) {
                            target = enemy;
                            minDistSq = d;
                        }
                    }
                }
            }
            // If no unit target was found, search for building target
            if(target == null) {
                minDistSq = unit.Data.BaseCombatData.MaxRange;
                minDistSq *= minDistSq;
                foreach(IndexedTeam t in g.activeTeams.ToArray()) {
                    RTSTeam team = t.Team;
                    if(teamIndex != t.Index && team.Input.Type != RTSInputType.Environment) { // Enemy team check
                        foreach(RTSBuilding enemy in g.teams[t.Index].Buildings.ToArray()) {
                            if(g.CGrid.GetFogOfWar(enemy.GridPosition, teamIndex) != FogOfWar.Active)
                                continue;
                            if(!enemy.IsAlive) continue;
                            float d = (enemy.GridPosition - unit.GridPosition).LengthSquared();
                            if (d <= minDistSq) {
                                target = enemy;
                                minDistSq = d;
                            }
                        }
                    }
                }
            }
            unit.Target = target;
        }

        void DSChaseTarget(GameState g, float dt, bool allowChase) {
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
                    // Ignore Same Team And Environment
                    if(unit.Target.Team.Index != teamIndex && unit.Target.Team.Input.Type != RTSInputType.Environment) {
                        switch(unit.CombatOrders) {
                            case BehaviorFSM.UseRangedAttack:
                                if(d <= mr * 0.75) {
                                    SetState(BehaviorFSM.PrepareCombatRanged);
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
                    }
                    if(!allowChase) return;
                    Point targetCellCurr = HashHelper.Hash(unit.Target.GridPosition, g.CGrid.numCells, g.CGrid.size);
                    bool sameTarget = prevTarget == unit.Target;
                    bool considerPF = pfCounter >= PF_COOLDOWN && sameTarget;
                    if(!sameTarget || pfCounter >= PF_COOLDOWN) {
                        pfCounter = 0;
                    }
                    else if(sameTarget) {
                        pfCounter += 1;
                    }
                    bool reachedGoalNotTarget = mc.IsValid(mc.CurrentWaypointIndex) && mc.CurrentWaypointIndex == 0;
                    reachedGoalNotTarget &= unit.Target.Team.Index != teamIndex; // Treat Allies As Open Space
                    // If The Target Has Changed Cells And Is Out Of Range, We Might Need To Pathfind To It
                    if(considerPF && (targetCellCurr.X != targetCellPrev.X || targetCellCurr.Y != targetCellPrev.Y) || reachedGoalNotTarget) {
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

        // TODO: Fix This So We Can Have Local Target Chasing About An Origin
        void DSPassiveTarget(GameState g, float dt) {
            if(!passivelyTargeting) origin = unit.GridPosition;
            Vector2 d = unit.GridPosition - origin;
            float mr = unit.Data.BaseCombatData.MaxRange;
            if(passivelyTargeting && d.LengthSquared() > mr * mr) {
                unit.Team.Input.AddEvent(new SetWayPointEvent(teamIndex, origin));
            }
            else {
                if(unit.Target == null) FindTarget(g, dt);
                if(unit.Target != null) passivelyTargeting = true;
                DSChaseTarget(g, dt, true);
            }
        }

        void DSPrepareCombatRanged(GameState g, float dt) {
            // Just Wait
        }
        void ASPrepareCombatRanged(GameState g, float dt) {
            prepareCooldown -= dt;
            if(prepareCooldown < 0) {
                SetState(BehaviorFSM.CombatRanged);
            }
            if(unit.Target != null) {
                unit.TurnToFace(unit.Target.GridPosition);
            }
        }

        void DSCombatRanged(GameState g, float dt) {
            // Check If Target Is Null
            if(unit.Target == null || cc == null) {
                SetState(BehaviorFSM.Rest);
                return;
            }
            // Check If Target Is Out Of Range
            else if(unit.Target != null) {
                float mr = unit.Data.BaseCombatData.MaxRange;
                float d2 = (unit.Target.GridPosition - unit.GridPosition).LengthSquared();
                if(d2 > mr * mr) {
                    SetState(BehaviorFSM.Rest);
                    return;
                }
            }
            SetState(BehaviorFSM.CombatRanged);
        }
        void ASCombatRanged(GameState g, float dt) {
            if(unit.Target == null || unit.CombatOrders != BehaviorFSM.UseRangedAttack || cc == null) {
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

        public override void Init(GameState s, GameplayController c, object args) {
        }

        public override void Reset() {
            attackCooldown = unit.Data.BaseCombatData.AttackTimer;
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
        protected const float dForce = 0f;
        protected const float sForce = 0f;
        protected const float pForce = -200f;

        // The Unit Thinks It Is Stuck
        bool stuck;
        // The Unit Was Recently Stuck
        bool wasStuck;
        // The Current Path Is Invalid
        bool invalid;

        // Calculate The Unit Force Between Two Locations
        public Vector2 UnitForce(Vector2 a, Vector2 b) {
            Vector2 diff = a - b;
            float mag = diff.LengthSquared();
            return diff.X != 0 && diff.Y != 0 ? diff / mag : Vector2.Zero;
        }

        protected const int historySize = 5;
        // The Last Few Locations This Unit Has Been To
        private Queue<Vector2> unitHistory = new Queue<Vector2>();
        public Queue<Vector2> UnitHistory {
            get { return unitHistory; }
            set { unitHistory = value; }
        }

        public void AddToHistory(Vector2 location) {
            if(UnitHistory.Count >= historySize)
                UnitHistory.Dequeue();
            UnitHistory.Enqueue(location);
        }

        // How Many Waypoints This Unit Should Lookahead When Updating Its PF Query
        protected const int lookahead = 2;

        public override void Init(GameState s, GameplayController c, object args) {
            Pathfinder = c.pathfinder;
            NetForce = Vector2.Zero;
        }

        public override void DecideMove(GameState g, float dt) {
            if(!unit.IsAlive) {
                doMove = false;
                return;
            }
            stuck = false;
            if(unitHistory.Count >= historySize) {
                Vector2 delta = Vector2.Zero;
                while(unitHistory.Count > 1) {
                    Vector2 oldest = unitHistory.Dequeue();
                    Vector2 oldest2 = unitHistory.Dequeue();
                    delta += oldest2 - oldest;
                }
                delta += unit.GridPosition - unitHistory.Dequeue();
                delta /= unitHistory.Count + 1;
                float r = unit.CollisionGeometry.BoundingRadius;
                wasStuck = stuck = Waypoints != null && Waypoints.Count > 0 && delta.LengthSquared() < 1.2 * r * 1.2 * r;
            }
            AddToHistory(unit.GridPosition);
            doMove = IsValid(CurrentWaypointIndex) && (Query == null || Query.IsComplete);
            if(!doMove) return;
            // If The Old Path Has Become Invalidated, Send A New Query
            // Note: These Invalidation Checks Ignore Fog, But The Idea Is To Look Only A Small Distance Ahead (Within Sight)
            invalid = false;
            int end = Math.Max(CurrentWaypointIndex - lookahead, 0);
            for(int i = CurrentWaypointIndex; i > end; i--) {
                Vector2 wp2 = Waypoints[i];
                Point cell2 = HashHelper.Hash(wp2, g.CGrid.numCells, g.CGrid.size);
                if(g.CGrid.GetCollision(cell2.X, cell2.Y)) {
                    invalid = true;
                    break;
                }
                // Check For Walls Too
                Point cell1 = HashHelper.Hash(unit.GridPosition, g.CGrid.numCells, g.CGrid.size);
                if(IsValid(i + 1)) {
                    Vector2 wp1 = Waypoints[i + 1];
                    cell1 = HashHelper.Hash(wp1, g.CGrid.numCells, g.CGrid.size);
                }
                if(!g.CGrid.CanMoveFrom(cell1, cell2)) {
                    invalid = true;
                    break;
                }

            }
            if(stuck || invalid && (Query == null || Query != null && Query.IsOld)) {
                Vector2 goal = Waypoints[0];
                Query = Pathfinder.ReissuePathQuery(Query, unit.GridPosition, goal, unit.Team.Index);
            }
            SetNetForceAndWaypoint(g);
        }
        public override void ApplyMove(GameState g, float dt) {
            if(!unit.IsAlive) return;
            if(NetForce != Vector2.Zero) {
                float magnitude = NetForce.Length();
                Vector2 scaledChange = (NetForce / magnitude) * unit.Squad.MinDefaultMoveSpeed() * dt;
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
#if DEBUG
            Vector2 first = Waypoints[Waypoints.Count - 1];
            Vector3 oFirst = new Vector3(first.X, g.CGrid.HeightAt(first), first.Y);
            g.AddParticle(new AlertParticle(oFirst, 1f, Color.Red, oFirst + Vector3.Up, 0.2f, Color.Green, g.TotalGameTime, 1f));
            Vector3 oWP = new Vector3(waypoint.X, g.CGrid.HeightAt(waypoint), waypoint.Y);
            g.AddParticle(new AlertParticle(oWP, 1f, Color.Blue, oWP + Vector3.Up, 0.2f, Color.Purple, g.TotalGameTime, 3f));
#endif
            if(Query != null && !Query.IsOld && Query.IsComplete) {
                Query.IsOld = true; // Only Do This Once Per Query
                Waypoints = Query.waypoints;
                CurrentWaypointIndex = Waypoints.Count - 1;
            }
            // Set Net Force...
            NetForce = pForce * UnitForce(unit.GridPosition, waypoint);
            Point unitCell = HashHelper.Hash(unit.GridPosition, cg.numCells, cg.size);
            
            //// Apply Forces From Other Units In This One's Cell
            //foreach(var otherUnit in cg.EDynamic[unitCell.X, unitCell.Y]) {
            //    NetForce += dForce * UnitForce(unit.GridPosition, otherUnit.GridPosition);
            //}
            //// Apply Forces From Buildings And Other Units Near This One
            //foreach(Point n in Pathfinder.Neighborhood(unitCell)) {
            //    RTSBuilding b = cg.EStatic[n.X, n.Y];
            //    if(b != null)
            //        NetForce += sForce * UnitForce(unit.GridPosition, b.GridPosition);
            //    foreach(var otherUnit in cg.EDynamic[n.X, n.Y]) {
            //        NetForce += sForce * UnitForce(unit.GridPosition, otherUnit.GridPosition);
            //    }
            //}
            
            // Set Waypoint...
            Point currWaypointCell = HashHelper.Hash(waypoint, cg.numCells, cg.size);
            float sqr2 = unit.Squad.Radius();
            sqr2 *= sqr2;
            bool inGoalCell = unitCell.X == currWaypointCell.X && unitCell.Y == currWaypointCell.Y;
            bool withinCellDistSq = (waypoint - unit.GridPosition).LengthSquared() < cg.cellSize;
            bool withinSquad = (waypoint - unit.GridPosition).LengthSquared() < 1.5 * sqr2;
            if(inGoalCell || (!wasStuck && (withinSquad || withinCellDistSq))) {
                CurrentWaypointIndex--;
                if(CurrentWaypointIndex < 0)
                    Waypoints = null;
                wasStuck = false;
            }
        }

        // TODO: Unstupify This Function
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

    public class AnimationInitArgs {
        public static readonly AnimationInitArgs Default = new AnimationInitArgs();

        public Point[] Splices;
        public float[] Speeds;

        public Color BulletTint;
        public Vector3 BulletOffset;
        public Vector3 BulletDirection;

        public Color BloodTint;

        public AnimationInitArgs() {
            Splices = new Point[] {
                new Point(0, 59),
                new Point(60, 119),
                new Point(120, 149),
                new Point(150, 179),
                new Point(180, 199),
                new Point(200, 259)
            };
            Speeds = new float[] {
                30,
                80,
                30,
                50,
                60,
                40
            };
            BulletTint = Color.Yellow;
            BulletOffset = new Vector3(-0.1f, 1f, 0.2f);
            BulletDirection = new Vector3(0, 0, 1);

            BloodTint = Color.Red;
        }
    }
    public class Animation : ACUnitAnimationController {
        private static Random r = new Random();
        private GameState state;
        AnimationInitArgs initArgs;

        private AnimationLoop alRest, alWalk, alMelee, alPrepareFire, alFire, alDeath;
        private AnimationLoop alCurrent;

        private float rt;
        private int lastState;
        int isInit;
        bool addBlood;

        public Animation() {
            isInit = 0;
        }

        public override void Init(GameState s, GameplayController c, object args) {
            state = s;

            initArgs = args == null ? null : args as AnimationInitArgs;
            if(initArgs == null) initArgs = AnimationInitArgs.Default;

            alRest = new AnimationLoop(initArgs.Splices[0].X, initArgs.Splices[0].Y);
            alRest.FrameSpeed = initArgs.Speeds[0];
            alWalk = new AnimationLoop(initArgs.Splices[1].X, initArgs.Splices[1].Y);
            alWalk.FrameSpeed = initArgs.Speeds[1];
            alMelee = new AnimationLoop(initArgs.Splices[2].X, initArgs.Splices[2].Y);
            alMelee.FrameSpeed = initArgs.Speeds[2];
            alPrepareFire = new AnimationLoop(initArgs.Splices[3].X, initArgs.Splices[3].Y);
            alPrepareFire.FrameSpeed = initArgs.Speeds[3];
            alFire = new AnimationLoop(initArgs.Splices[4].X, initArgs.Splices[4].Y);
            alFire.FrameSpeed = initArgs.Speeds[4];
            alDeath = new AnimationLoop(initArgs.Splices[5].X, initArgs.Splices[5].Y);
            alDeath.FrameSpeed = initArgs.Speeds[5];
            SetAnimation(BehaviorFSM.None);

            System.Threading.Interlocked.Exchange(ref isInit, 1);
        }

        public override void SetUnit(RTSUnit u) {
            base.SetUnit(u);
            if(unit != null) {
                unit.OnAttackMade += unit_OnAttackMade;
                unit.OnDestruction += unit_OnDestruction;
                unit.OnDamage += unit_OnDamage;
            }
        }

        void unit_OnAttackMade(ICombatEntity arg1, IEntity arg2) {
            Vector2 cpvd = new Vector2(arg1.ViewDirection.Y, -arg1.ViewDirection.X);

            Vector2 o2 = cpvd * initArgs.BulletOffset.X + arg1.ViewDirection * initArgs.BulletOffset.Z;
            Vector3 o = new Vector3(o2.X, initArgs.BulletOffset.Y, o2.Y);
            o += arg1.WorldPosition;

            Vector2 d2 = cpvd * initArgs.BulletDirection.X + arg1.ViewDirection * initArgs.BulletDirection.Z;
            Vector3 d = new Vector3(d2.X, initArgs.BulletDirection.Y, d2.Y);
            d.Normalize();

            BulletParticle bp = new BulletParticle(o, d, 0.05f, 1.4f, 0.1f);
            bp.Tint = initArgs.BulletTint;
            AddParticle(bp);
        }
        void unit_OnDamage(IEntity arg1, int v) {
            addBlood = true;
        }
        void Splurt(float ct) {
            Vector3 o = unit.WorldPosition + Vector3.Up;
            AddParticle(new BloodParticle(o, initArgs.BloodTint, 0.1f, 1f, ct, 1f));
        }
        void unit_OnDestruction(IEntity e) {
            alDeath.Restart(false);
            System.Threading.Interlocked.Exchange(ref alCurrent, alDeath);
        }

        private void SetAnimation(int state) {
            switch(state) {
                case BehaviorFSM.None:
                    alCurrent = null;
                    AnimationFrame = 0;
                    break;
                case BehaviorFSM.Walking:
                    alCurrent = alWalk;
                    alCurrent.Restart(true);
                    break;
                case BehaviorFSM.PrepareCombatRanged:
                    alCurrent = alPrepareFire;
                    alCurrent.Restart(false);
                    break;
                case BehaviorFSM.CombatRanged:
                    alCurrent = alFire;
                    alCurrent.Restart(false);
                    break;
                case BehaviorFSM.CombatMelee:
                    alCurrent = alMelee;
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
            if(System.Threading.Interlocked.CompareExchange(ref isInit, 1, 1) == 0)
                return;

            if(addBlood) {
                Splurt(s.TotalGameTime);
                addBlood = false;
            }

            // Animate Death
            if(!unit.IsAlive) {
                if(AnimationFrame == alDeath.EndFrame)
                    return;
                alDeath.Step(dt);
                AnimationFrame = alDeath.CurrentFrame;
                return;
            }

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