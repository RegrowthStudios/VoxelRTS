using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    public class ActionController : IActionController {
        // The Entity That This ActionController Is Controlling
        private IEntity entity;
        public IEntity Entity {
            get { return entity; }
        }

        // Performs Decision Logic For The Entity
        public void PerformDecision(GameState g, float dt) {
            if (entity is ICombatEntity) {
                ICombatEntity centity = entity as ICombatEntity;
                if (centity != null && centity.Target != null) {
                    centity.CombatController.Attack(g, dt);
                }
                else {
                    centity.TargettingController.FindTarget(g, dt);
                }
            }
            if (entity is IMovingEntity) {
                IMovingEntity mentity = entity as IMovingEntity;
                if (mentity != null) mentity.MovementController.Move(g, dt);
            }


        }
    }
}
