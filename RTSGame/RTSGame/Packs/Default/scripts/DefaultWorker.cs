using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Interfaces;

namespace RTS.Default.Worker
{
    public class Action : ACUnitActionController
    {
        int teamIndex;
        Action<GameState, float> fDecide, fApply;
        public override void DecideAction(GameState g, float dt) {
            fDecide(g, dt);
        }

        public override void ApplyAction(GameState g, float dt)
        {
            fApply(g, dt);
            if (unit.Target != null)
                unit.TurnToFace(unit.Target.GridPosition);
        }

        public override void Init(RTSEngine.Data.GameState s, RTSEngine.Controllers.GameplayController c)
        {
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
                
            }
        }

        void ASRest(GameState g, float dt) { 
            // Do nothing
        }

        void DSHarvest(GameState g, float dt) {

        }

        void FollowTarget(GameState g, float dt) {
            if (unit.Target == null) {
                unit.State = BehaviorFSM.Rest;
                fDecide = DSRest;
                fApply = ASRest;
                return;
            }

        }
    }

    public class Combat : ACUnitCombatController {

    }

    public class Animation : ACUnitAnimationController {

    }

    public class Movement : ACUnitMovementController {

    }
}
