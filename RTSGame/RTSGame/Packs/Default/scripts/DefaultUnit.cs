using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Graphics;
using RTSEngine.Interfaces;

namespace RTS.Default.Unit {
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

            teamIndex = unit.Team.Index;
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
                        float mr = unit.Data.BaseCombatData.MaxRange;
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
                    float mr = unit.Data.BaseCombatData.MaxRange;
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
            float mr = unit.Data.BaseCombatData.MaxRange;
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

            float mr = unit.Data.BaseCombatData.MaxRange;
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

            float mr = unit.Data.BaseCombatData.MaxRange;
            float d = (unit.Target.GridPosition - unit.GridPosition).Length();
            etCombat += dt;
            if(d < mr) {
                if(etCombat > unit.Data.BaseCombatData.AttackTimer) {
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
            if(unit.State != BehaviorFSM.None)
                return;
            if(unit.Target != null) {
                if(!unit.Target.IsAlive) {
                    unit.Target = null;
                    return;
                }
                float minDistSquared = unit.Data.BaseCombatData.MinRange * unit.Data.BaseCombatData.MinRange;
                float distSquared = (unit.Target.WorldPosition - unit.WorldPosition).LengthSquared();
                float maxDistSquared = unit.Data.BaseCombatData.MaxRange * unit.Data.BaseCombatData.MaxRange;
                if(distSquared > maxDistSquared) return;

                if(attackCooldown <= 0) {
                    attackCooldown = unit.Data.BaseCombatData.AttackTimer;
                    unit.State = BehaviorFSM.CombatMelee;
                    if(minDistSquared <= distSquared) {
                        unit.DamageTarget(critRoller.NextDouble());
                        if(!unit.Target.IsAlive) unit.Target = null;
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

    // TODO: Decide If We Need This
    public class Movement : ACUnitMovementController {
        public override void Init(GameState s, GameplayController c) {

        }

        public override void Serialize(BinaryWriter s) {

        }

        public override void Deserialize(BinaryReader s) {

        }
    }

    public class Animation : ACUnitAnimationController {
        private static Random r = new Random();
        private GameState state;

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

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }
}