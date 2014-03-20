using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    public class ActionController : ACUnitActionController {
        // Performs Decision Logic For The Entity
        public override void DecideAction(GameState g, float dt) {
            if(unit.MovementController != null)
                unit.MovementController.DecideMove(g, dt);
        }

        // Apply The Entity's Decision
        public override void ApplyAction(GameState g, float dt) {
            if(unit.MovementController != null)
                unit.MovementController.ApplyMove(g, dt);
            if(unit.CombatController != null)
                unit.CombatController.Attack(g, dt);
            unit.AnimationController.Update(g, dt);
        }
    }
}