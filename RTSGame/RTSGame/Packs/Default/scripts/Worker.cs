using RTSEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTS.Squad {
    
    public class Action : ACSquadActionController {

        public override void DecideAction(RTSEngine.Data.GameState g, float dt) {
            throw new NotImplementedException();
        }

        public override void ApplyAction(RTSEngine.Data.GameState g, float dt) {
            throw new NotImplementedException();
        }
    }

    public class Movement : ACSquadMovementController {

    }

    public class Target : ACSquadTargettingController {

        public override void DecideTarget(RTSEngine.Data.GameState g, float dt) {
            throw new NotImplementedException();
        }

        public override void ApplyTarget(RTSEngine.Data.GameState g, float dt) {
            throw new NotImplementedException();
        }
    }

}

namespace RTS.Worker {

    public class Action : ACUnitActionController {

        public override void DecideAction(RTSEngine.Data.GameState g, float dt) {
            throw new NotImplementedException();
        }

        public override void ApplyAction(RTSEngine.Data.GameState g, float dt) {
            throw new NotImplementedException();
        }
    }

    public class Combat : ACUnitCombatController {

        public override void Attack(RTSEngine.Data.GameState g, float dt) {
            throw new NotImplementedException();
        }
    }


    public class Movement : ACUnitMovementController {

        public override void DecideMove(RTSEngine.Data.GameState g, float dt) {
            throw new NotImplementedException();
        }

        public override void ApplyMove(RTSEngine.Data.GameState g, float dt) {
            throw new NotImplementedException();
        }
    }

    public class Animation : ACUnitAnimationController {

        public override void Update(RTSEngine.Data.GameState s, float dt) {
            throw new NotImplementedException();
        }
    }

}
