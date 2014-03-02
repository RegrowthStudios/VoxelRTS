using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Controllers {
    public class CombatController : ICombatController {
        // The Entity That This CombatController Is Controlling
        private ICombatEntity entity;
        public IEntity Entity {
            get { return entity; }
        }

        public void Attack(GameState g, float dt) {

        }
    }
}
