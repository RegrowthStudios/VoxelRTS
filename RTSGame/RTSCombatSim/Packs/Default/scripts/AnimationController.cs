using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Interfaces;

namespace RTSCS.Controllers {
    public class AnimationController : ACUnitAnimationController {
        private AnimationLoop alRest, alWalk;
        private AnimationLoop alCurrent;

        public AnimationController() {
            animation = AnimationType.Rest;
            alRest = new AnimationLoop(0, 59);
            alWalk = new AnimationLoop(60, 119);
            alCurrent = alRest;
            alCurrent.Restart();
        }

        public override void SetAnimation(AnimationType t) {
            switch(t) {
                case AnimationType.Walking:
                    alCurrent = alWalk;
                    alCurrent.Restart(true);
                    break;
                case AnimationType.Rest:
                    alCurrent = alRest;
                    alCurrent.Restart(true);
                    break;
                default:
                    return;
            }
            animation = t;
        }

        public override void Update(GameState s, float dt) {
            alCurrent.Step(dt);
            AnimationFrame = alCurrent.CurrentFrame;
        }
    }
}