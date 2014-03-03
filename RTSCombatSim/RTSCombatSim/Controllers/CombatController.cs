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

        public void Attack(GameState g, float dt) {
            if(attackCooldown <= 0) {
                RTSUnitInstance unit = entity as RTSUnitInstance;
                if(unit != null) {
                    RTSUnitInstance target = entity.Target as RTSUnitInstance;
                    if(target != null) {
                        float distSquared = (target.WorldPosition - unit.WorldPosition).LengthSquared();
                        float minDistSquared = unit.UnitData.BaseCombatData.MinRange * unit.UnitData.BaseCombatData.MinRange;
                        float maxDistSquared = unit.UnitData.BaseCombatData.MaxRange * unit.UnitData.BaseCombatData.MaxRange;
                        if(minDistSquared <= distSquared && distSquared <= maxDistSquared) {
                            target.Damage(unit.DealDamage(critRoller.NextDouble()));
                            if(target.Health <= 0) entity.Target = null;
                        }
                    }
                }
                attackCooldown = unit.UnitData.BaseCombatData.AttackTimer;;
            }
            else attackCooldown -= dt;
        }

        // Constructor
        public CombatController(ICombatEntity entity) {
            this.entity = entity;
        }
    }
}
