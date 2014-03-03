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
        public void DecideAction(GameState g, float dt) {
            ICombatEntity cEntity = entity as ICombatEntity;
            if(cEntity != null && cEntity.Target == null) {
                cEntity.TargettingController.FindTarget(g, dt);
            }
            IMovingEntity mEntity = entity as IMovingEntity;
            if(mEntity != null) mEntity.MovementController.DecideMove(g, dt);  
        }

        // Apply The Entity's Decision
        public void ApplyAction(GameState g, float dt) {
            ICombatEntity cEntity = entity as ICombatEntity;
            if(cEntity != null && cEntity.Target != null) {
                cEntity.CombatController.Attack(g, dt);
            }
            else {
                cEntity.TargettingController.ChangeTarget(g, dt);
            }
            IMovingEntity mEntity = entity as IMovingEntity;
            if(mEntity != null) mEntity.MovementController.ApplyMove(g, dt);
        }

        // Constructor
        public ActionController(IEntity entity) {
            this.entity = entity;
        }
    }
}
