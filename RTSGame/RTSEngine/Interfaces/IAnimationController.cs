using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;

namespace RTSEngine.Interfaces {
    public enum AnimationType {
        Rest,
        Walking,
        CombatRanged,
        CombatMelee,
        Death,
        Special0,
        Special1
    }

    public interface IAnimationController : IEntityController {
        AnimationType Animation {
            get;
            set;
        }
        float AnimationFrame {
            get;
        }

        void Update(GameState s, float dt);
    }
}