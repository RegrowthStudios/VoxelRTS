using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;

namespace RTS.Default.Worker
{
    public class Action : ACUnitActionController
    {
        int teamIndex;
        Action<GameState, float> fDecide, fApply;
        private RTSBuilding targetResource;

        public override void DecideAction(GameState g, float dt) {
            fDecide(g, dt);
        }

        public override void ApplyAction(GameState g, float dt) {
            fApply(g, dt);
            if (unit.Target != null)
                unit.TurnToFace(unit.Target.GridPosition);
        }

        public override void Init(RTSEngine.Data.GameState s, RTSEngine.Controllers.GameplayController c) {
            unit.State = BehaviorFSM.Rest;
            unit.TargetingOrders = BehaviorFSM.TargetPassively;
            unit.CombatOrders = BehaviorFSM.CombatRanged;
            unit.MovementOrders = 0;

            fDecide = DSRest;
            fApply = ASRest;

            teamIndex = unit.Team.Index;
        }

        void DSRest(GameState g, float dt) {
            unit.State = BehaviorFSM.Rest;
            if (unit.Target != null) {
                Vector2 dir = unit.Target.GridPosition - unit.GridPosition;
                float dl = dir.Length();
                
                if (unit.Target.Team.Index == unit.Team.Index) {
                    if (unit.Target is RTSBuilding) {
                        RTSBuilding target = (RTSBuilding)unit.Target;
                        // If target is resource, enter harvest mode
                        if (target.IsResource) {
                            unit.State = BehaviorFSM.Harvest;
                            targetResource = target;
                            // If the unit is far away from target resource, walk there
                            if (dl > 0.001) {
                                fDecide = DSFollowTarget;
                                fApply = ASFollowTarget;
                            }
                            // If the unit is near target resource
                            else {
                                // If the unit can still carry resources, harvest 
                                if (unit.Resources < unit.Data.CarryingCapacity) {
                                    unit.CombatController.Attack(g, dt);
                                }
                                // If the unit cannot carry resources anymore,
                                // set target as the nearest depositable building
                                else {
                                    fDecide = DSFindDepository;
                                    fApply = ASFindDepository;
                                }
                            }
                        }
                        // If target is player's depositable building
                        else if (target.Data.Depositable) {
                            // If unit is far away from building, walk there
                            if (dl > 0.001) {
                                fDecide = DSFollowTarget;
                                fApply = ASFollowTarget;
                            }
                            // If unit is near building, deposit resources
                            else {
                                fDecide = DSDeposit;
                                fApply = ASDeposit;
                            }
                        }
                        // If target is the player's undepositable building, repair
                        else {
                            unit.CombatController.Attack(g, dt);
                        }
                    }
                }
                // If target is on different team and is a unit, initiate combat
                else {
                    if (unit.Target is RTSUnit) {
                        RTSUnit target = (RTSUnit)unit.Target;
                        // If target unit is far away, walk there
                        if (dl > unit.Data.BaseCombatData.MaxRange) {
                            fDecide = DSFollowTarget;
                            fApply = ASFollowTarget;
                        }
                        else {
                            unit.CombatController.Attack(g, dt);
                        }
                    }
                }
            }
        }

        void ASRest(GameState g, float dt) {}

        void DSFollowTarget(GameState g, float dt) {}

        void ASFollowTarget(GameState g, float dt) {
            // If target does not exist anymore
            if (unit.Target == null) {
                // If unit is in depository searching mode, find another depository if any
                if (unit.State == BehaviorFSM.FindDepository){
                    DSFindDepository(g, dt);
                }
                else
                    unit.State = BehaviorFSM.Rest;
                fDecide = DSRest;
                fApply = ASRest;
            }
            else {
                // Move unit to target
                Vector2 dir = unit.Target.GridPosition - unit.GridPosition;
                float dl = dir.Length();
                if (dl > 0.001) {
                    dir /= dl;
                    float m = unit.MovementSpeed * dt;
                    if (m > dl)
                        unit.Move(dir * dl);
                    else
                        unit.Move(dir * m);
                }
                else {
                    fDecide = DSRest;
                    fApply = ASRest;
                }
            }
        }

        // Deposit resources to building
        void DSDeposit(GameState g, float dt) {
            unit.Team.Capital += unit.Resources;
            unit.Resources = 0;
            if (unit.State == BehaviorFSM.FindDepository)
                unit.State = BehaviorFSM.Harvest;
            if (unit.State == BehaviorFSM.Harvest && targetResource != null && targetResource.IsAlive) {
                unit.Target = targetResource;
            }
            else {
                unit.Target = null;
                unit.State = BehaviorFSM.Rest;
            }
        }

        void ASDeposit(GameState g, float dt) {
            fDecide = DSRest;
            fApply = ASRest;
        }

        // Find the nearest depositable building
        void DSFindDepository(GameState g, float dt) {
            float minDist = float.MaxValue;
            RTSBuilding depository = null;
            foreach (RTSBuilding building in unit.Team.Buildings) {
                if (building.Data.Depositable) {
                    float dist = (building.GridPosition - unit.GridPosition).Length();
                    if (minDist > dist) {
                        minDist = dist;
                        depository = building;
                    }
                }
            }
            unit.Target = depository;
        }

        void ASFindDepository(GameState g, float dt) {
            fDecide = DSRest;
            fApply = ASRest;
        }

        public override void Deserialize(System.IO.BinaryReader s)
        {
            throw new NotImplementedException();
        }
        public override void Serialize(System.IO.BinaryWriter s)
        {
            throw new NotImplementedException();
        }
    }

    // Combat & Harvesting Are Controlled Here
    public class Combat : ACUnitCombatController {
        // Random Object For Generating And Testing Critical Hit Rolls
        private static Random critRoller = new Random();
        private float attackCooldown;

        public override void Init(GameState s, RTSEngine.Controllers.GameplayController c) {}

        public override void Attack(GameState g, float dt) {
            if (attackCooldown > 0)
                attackCooldown -= dt;
            if (unit.State != BehaviorFSM.None)
                return;
            if (unit.Target != null){
                if (!unit.Target.IsAlive) {
                    unit.Target = null;
                    return;
                }
                float minDistSquared = unit.Data.BaseCombatData.MinRange * unit.Data.BaseCombatData.MinRange;
                float distSquared = (unit.Target.WorldPosition - unit.WorldPosition).LengthSquared();
                float maxDistSquared = unit.Data.BaseCombatData.MaxRange * unit.Data.BaseCombatData.MaxRange;
                if (distSquared > maxDistSquared) return;

                if (attackCooldown <= 0) {
                    attackCooldown = unit.Data.BaseCombatData.AttackTimer;
                    // Harvest
                    RTSBuilding target = (RTSBuilding)unit.Target;
                    if (target.IsResource) {
                        unit.Target.Damage(unit.Data.BaseCombatData.AttackDamage);
                        unit.Team.Capital += unit.Data.BaseCombatData.AttackDamage;
                        if (!unit.Target.IsAlive) unit.Target = null;
                    }
                    // Repair
                    else if (target.Team.Index == unit.Team.Index) {
                        // Negative damage = heal
                        if (unit.Team.Capital > 0) {
                            target.Damage(-unit.Data.BaseCombatData.AttackDamage);
                            unit.Team.Capital -= unit.Data.BaseCombatData.AttackDamage;
                        }
                    }
                    // Combat
                    else {
                        unit.State = BehaviorFSM.CombatMelee;
                        if (minDistSquared <= distSquared)
                        {
                            unit.DamageTarget(critRoller.NextDouble());
                            if (!unit.Target.IsAlive) unit.Target = null;
                        }
                    }
                }
            }
        }

        public void Harvest(GameState g, float dt) {
           
            if (!unit.Target.IsAlive)
                unit.Target = null;
            if (unit.State == BehaviorFSM.None || unit.Target != null)
                return;

            if (attackCooldown > 0)
                attackCooldown -= dt;

            
        }

        public override void Deserialize(System.IO.BinaryReader s)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(System.IO.BinaryWriter s)
        {
            throw new NotImplementedException();
        }
    }

    public class Animation : ACUnitAnimationController {
        public override void Init(GameState s, RTSEngine.Controllers.GameplayController c)
        {
            throw new NotImplementedException();
        }
        public override void Update(GameState s, float dt)
        {
            throw new NotImplementedException();
        }
        public override void Deserialize(System.IO.BinaryReader s)
        {
            throw new NotImplementedException();
        }

        public override void Serialize(System.IO.BinaryWriter s)
        {
            throw new NotImplementedException();
        }
    }
}
