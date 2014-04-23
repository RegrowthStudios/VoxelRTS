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

namespace RTS.Default.Worker {
    public class Action : ACUnitActionController {
        int teamIndex;
        Action<GameState, float> fDecide, fApply;
        private RTSBuilding targetResource;
        private IEntity target;
        private bool headingDepository;
        private bool harvesting;

        public override void DecideAction(GameState g, float dt) {
            fDecide(g, dt);
        }
        public override void ApplyAction(GameState g, float dt) {
            fApply(g, dt);
            if(unit.Target != null)
                unit.TurnToFace(unit.Target.GridPosition);
        }

        public override void Init(RTSEngine.Data.GameState s, RTSEngine.Controllers.GameplayController c) {
            unit.State = BehaviorFSM.Rest;
            unit.TargetingOrders = BehaviorFSM.TargetPassively; // Never changes
            unit.CombatOrders = BehaviorFSM.CombatMelee; // Never changes
            unit.MovementOrders = 0;
            targetResource = null;
            target = null;
            headingDepository = false;
            harvesting = false;

            fDecide = DSRest;
            fApply = ASRest;

            teamIndex = unit.Team.Index;
        }

        void DSRest(GameState g, float dt) {
            if (unit.Target != null || target != null) {
                if (target == null) target = unit.Target;
                Vector2 dir = target.GridPosition - unit.GridPosition;
                float dl = dir.Length();

                // If the unit is far away from target, walk there
                if (dl > unit.Data.BaseCombatData.MinRange + target.CollisionGeometry.BoundingRadius) {
                    unit.State = BehaviorFSM.Walking;
                    fDecide = DSWalk;
                    fApply = ASWalk;
                }
                // If target is close
                else {
                    unit.State = BehaviorFSM.CombatRanged;
                    fDecide = DSCombatRanged;
                    fApply = ASCombatRanged; 
                } 
            }
        }

        void ASRest(GameState g, float dt) { /* Do nothing */ }

        void DSWalk(GameState g, float dt) {
            // If target does not exist anymore
            if (target == null){
                if (headingDepository)
                    target = GetClosestDepository();
                Rest();
                return;
            }
            Vector2 dir = target.GridPosition - unit.GridPosition;
            float dl = dir.Length();
            
            // If target is close enough, switch to CombatRanged
            if (dl <= unit.Data.BaseCombatData.MinRange + target.CollisionGeometry.BoundingRadius) {
                unit.State = BehaviorFSM.CombatRanged;
                fDecide = DSCombatRanged;
                fApply = ASCombatRanged;
            }
        }

        void ASWalk(GameState g, float dt) {
            if (target == null) return;
            // Move unit to target
            Vector2 dir = target.GridPosition - unit.GridPosition;
            float dl = dir.Length();
            dir /= dl;
            float m = unit.MovementSpeed * dt;
            if (m > dl)
                unit.Move(dir * dl);
            else
                unit.Move(dir * m);     
        }

        void DSCombatRanged(GameState g, float dt) {
            // If target does not exist, rest
            if (target == null)
                Rest();
            // If target is on the same team
            else if (target.Team.Index == teamIndex) {
                // If target is a building
                if (target is RTSBuilding) {
                    RTSBuilding targetB = (RTSBuilding)target;
                    // If target is player's depositable building
                    if (targetB.Data.Depositable) {
                        fDecide = DSDeposit;
                        fApply = ASDeposit;
                    }
                    // If target is the player's undepositable building, repair
                    else {
                        unit.State = BehaviorFSM.Repair;
                        fDecide = DSRepair;
                        fApply = ASRepair;
                    }
                }
            }
            // If target is on different team
            else {
                // If target is a building type
                if (target is RTSBuilding) {
                    RTSBuilding targetB = (RTSBuilding)target;
                    // If target is resource
                    if (targetB.IsResource) {
                        harvesting = true;
                        unit.State = BehaviorFSM.Harvest;
                        fDecide = DSHarvest;
                        fApply = ASHarvest;
                    }

                }
                // If enemy target is not resource, switch to combat state
                else {
                    unit.State = BehaviorFSM.CombatMelee;
                    fDecide = DSCombatMelee;
                    fApply = ASCombatMelee;
                }
            }
        }

        void ASCombatRanged(GameState g, float dt) { /* Do nothing */ }

        void DSDeposit(GameState g, float dt) {
            if (harvesting && targetResource != null && targetResource.IsAlive)
                target = targetResource;
            else 
                target = null;
            Rest();
        }

        void ASDeposit(GameState g, float dt) {
            if (unit.Resources == 0) return;
            unit.Team.Input.AddEvent(new CapitalEvent(teamIndex, unit.Resources/2));
            unit.Resources = 0;
            headingDepository = false;
        }

        void DSHarvest(GameState g, float dt) {
            // If unit cannot carry resources anymore or resource is exhausted, find depository
            if (unit.Resources > unit.Data.CarryingCapacity || target == null || !target.IsAlive) {
                if (target == null || !target.IsAlive) {
                    harvesting = false;
                    targetResource = null;
                }
                headingDepository = true;
                target = GetClosestDepository();
                Rest();
            }
        }

        void ASHarvest(GameState g, float dt) {
            unit.Target = target;
            // Apply damage to resource and add capital
            unit.CombatController.Attack(g, dt);
        }

        void DSCombatMelee(GameState g, float dt) {
            // If target does not exist, rest
            if (target == null)
                Rest();
        }

        void ASCombatMelee(GameState g, float dt) {
            unit.Target = target;
            unit.CombatController.Attack(g, dt);
        }

        void DSRepair(GameState g, float dt) {
            if (target == null)
                Rest();
            else if (target is RTSBuilding){
                RTSBuilding targetB = (RTSBuilding) target;
                if (targetB.Health >= targetB.Data.Health)
                    Rest();
            }
        }

        void ASRepair(GameState g, float dt) {
            unit.Target = target;
            unit.CombatController.Attack(g, dt);
        }

        // Helper method to apply rest state
        private void Rest() {
            unit.State = BehaviorFSM.Rest;
            fDecide = DSRest;
            fApply = ASRest;
        }

        // Helper method for finding closest depositable building
        private RTSBuilding GetClosestDepository() {
            float minDist = float.MaxValue;
            RTSBuilding depository = null;
            for (int i = 0; i < unit.Team.Buildings.Count; i++) {
                var building = unit.Team.Buildings[i];
                if (building.Data.Depositable) {
                    float dist = (building.GridPosition - unit.GridPosition).Length();
                    if (minDist > dist) {
                        minDist = dist;
                        depository = building;
                    }
                }
            }
            return depository;
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

        public override void Init(GameState s, RTSEngine.Controllers.GameplayController c) {}

        public override void Attack(GameState g, float dt) {
            if(attackCooldown > 0)
                attackCooldown -= dt;
            //f(unit.State != BehaviorFSM.None)
             //   return;
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
                    int damage = unit.Data.BaseCombatData.AttackDamage;                    
                    switch (unit.State) {
                        case BehaviorFSM.Harvest:
                            unit.Target.Damage(damage);
                            unit.Resources += damage; // Unit gets resources
                            if(!unit.Target.IsAlive) unit.Target = null;
                            break;
                        case BehaviorFSM.Repair:
                            // Negative damage = heal
                            unit.Target.Damage(-damage);
                            unit.Team.Input.AddEvent(new CapitalEvent(unit.Team.Index, -damage));
                            break;
                        case BehaviorFSM.CombatMelee:
                            if(minDistSquared <= distSquared) {
                                unit.DamageTarget(critRoller.NextDouble());
                                if(!unit.Target.IsAlive) unit.Target = null;
                            }
                            break;
                    }
                }
            }
        }

        public override void Deserialize(System.IO.BinaryReader s) {
            // TODO
        }
        public override void Serialize(System.IO.BinaryWriter s) {
            // TODO
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