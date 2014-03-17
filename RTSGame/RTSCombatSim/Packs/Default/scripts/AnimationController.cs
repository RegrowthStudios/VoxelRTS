using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Interfaces;

namespace RTSCS.Controllers {
    public class AnimationController : IAnimationController {
        bool restart;

        public AnimationType anim;
        public AnimationType Animation {
            get {
                return anim;
            }
            set {
                anim = value;
                restart = true;
            }
        }

        public IEntity Entity {
            get;
            private set;
        }

        public float AnimationFrame {
            get { return alCurrent.CurrentFrame; }
        }

        private AnimationLoop alRest, alWalk;
        private AnimationLoop alCurrent;

        public AnimationController() {
            Entity = null;
            anim = AnimationType.Rest;
            alRest = new AnimationLoop(0, 59);
            alWalk = new AnimationLoop(60, 119);
            alCurrent = alRest;
            alCurrent.Restart();
        }

        public void SetEntity(IEntity e) {
            if(Entity != null && Entity != e)
                throw new InvalidOperationException("Controllers Can Only Have Entities Set Once");
            Entity = e;
        }

        public void Update(GameState s, float dt) {
            if(restart) {
                switch(anim) {
                    case AnimationType.Walking:
                        alCurrent = alWalk;
                        alCurrent.Restart(false);
                        break;
                    default:
                        alCurrent = alRest;
                        alCurrent.Restart(true);
                        break;
                }
                restart = false;
            }
            alCurrent.Step(dt);
        }
    }
}