using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data.Team;

namespace RTSEngine.Data.Controllers {
    public class CombatController : ICombatController {
        // The Entity That This CombatController Is Controlling
        private ICombatEntity entity;
        public IEntity Entity {
            get { return entity; }
        }

        // Random Object For Generating And Testing Crit Rolls
        private static Random CritRoller = new Random();
        
        public void Attack(GameState g, float dt) {
            RTSUnitInstance unit = entity as RTSUnitInstance;
            if(unit != null) {
                RTSUnitInstance target = entity.Target as RTSUnitInstance;
                if(target != null) {
                    float DistSq = (target.WorldPosition - unit.WorldPosition).LengthSquared();
                    float MinDistSq = (float)Math.Pow(unit.UnitData.BaseCombatData.MinRange,2); 
                    float MaxDistSq = (float)Math.Pow(unit.UnitData.BaseCombatData.MaxRange,2);
                    if(MinDistSq <= DistSq && DistSq <= MaxDistSq) {
                        target.Damage(unit.DealDamage(100 * CritRoller.NextDouble()));
                    }
                }
            }
        }

        // Constructor
        public CombatController(ICombatEntity entity) {
            this.entity = entity;
        }
    }
}
