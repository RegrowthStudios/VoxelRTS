using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data;
using RTSEngine.Data.Team;

namespace RTSCS.Controllers {
    public class CombatController : ICombatController {
        // The Entity That This CombatController Is Controlling
        private ICombatEntity entity;
        public IEntity Entity {
            get { return entity; }
        }

        // Random Object For Generating And Testing Crit Rolls
        private static Random critRoller = new Random();

        // The Amount Of Time Remaining Before This Controller's Entity Can Attack Again
        private float attackCooldown;

        // Constructor
        public CombatController() {
            entity = null;
        }

        // Set Entity Only Once
        public void SetEntity(IEntity e) {
            if(Entity != null && Entity != e)
                throw new InvalidOperationException("Controllers Can Only Have Entities Set Once");
            entity = e as ICombatEntity;
        }

        public void Attack(GameState g, float dt) {
            if(attackCooldown <= 0) {
                RTSUnitInstance unit = entity as RTSUnitInstance;
                attackCooldown = unit.UnitData.BaseCombatData.AttackTimer;
                if(unit != null) {
                    RTSUnitInstance target = entity.Target as RTSUnitInstance;
                    if(target != null) {
                        float distSquared = (target.WorldPosition - unit.WorldPosition).LengthSquared();
                        float minDistSquared = unit.UnitData.BaseCombatData.MinRange * unit.UnitData.BaseCombatData.MinRange;
                        float maxDistSquared = unit.UnitData.BaseCombatData.MaxRange * unit.UnitData.BaseCombatData.MaxRange;

                        // Lose Pursuit If It Is Out Of Range
                        if(distSquared > maxDistSquared) {
                            entity.Target = null;
                            return;
                        }

                        if(minDistSquared <= distSquared) {
                            unit.DamageTarget(critRoller.NextDouble());
                            if(target.Health <= 0) entity.Target = null;
                        }
                    }
                }
            }
            else attackCooldown -= dt;
        }
    }
}