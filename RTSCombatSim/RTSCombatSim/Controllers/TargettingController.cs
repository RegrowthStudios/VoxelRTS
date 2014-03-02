using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    public class TargettingController : ITargettingController {
        // TODO
        // The Entity That This TargettingController Is Controlling
        private IEntity entity;
        public IEntity Entity {
            get { return entity; }
        }

        public void FindTarget(GameState g, float dt) {

        }

        public void ChangeTarget(GameState g, float dt) {

        }

    }
}
