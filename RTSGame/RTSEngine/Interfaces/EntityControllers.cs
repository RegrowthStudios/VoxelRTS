using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;

namespace RTSEngine.Interfaces {
    public enum AnimationType {
        None,
        Rest,
        Walking,
        PrepareCombatRanged,
        CombatRanged,
        CombatMelee,
        Death,
        Special0,
        Special1
    }

    // Base Controller Functionality
    public abstract class ACUnitController {
        // The Unit That Is Being Controlled
        protected RTSUnit unit;
        public RTSUnit Unit {
            get { return Unit; }
        }

        public void SetUnit(RTSUnit u) {
            if(u == null) return;
            if(unit != null)
                throw new ArgumentException("Cannot Rebind This Controller To Another Unit");
            unit = u;
            return;
        }
        public T SetUnit<T>(RTSUnit u) where T : ACUnitController {
            SetUnit(u);
            return this as T;
        }
    }

    // A Super Controller Called By The Gameplay Controller
    public abstract class ACUnitActionController : ACUnitController {
        // Scripted Super-Controller Logic
        public abstract void DecideAction(GameState g, float dt);
        public abstract void ApplyAction(GameState g, float dt);
    }

    // Steps Animations And May Send Particle Events
    public abstract class ACUnitAnimationController : ACUnitController {
        // FSM
        protected AnimationType animation;
        public AnimationType Animation {
            get { return animation; }
        }

        // This Is The Value Read By The Renderer
        public float AnimationFrame {
            get;
            protected set;
        }

        // Scripted Animation
        public abstract void SetAnimation(AnimationType t);
        public abstract void Update(GameState s, float dt);
    }

    // Reasons About How Combat Damage Should Be Performed To A Unit's Target
    public abstract class ACUnitCombatController : ACUnitController {
        // Scripted Logic For Attacking
        public abstract void Attack(GameState g, float dt);
    }

    // Special Movement Mechanics
    public abstract class ACUnitMovementController : ACUnitController {
        protected readonly List<Vector2> waypoints = new List<Vector2>();
        public List<Vector2> Waypoints {
            get { return waypoints; }
        }

        // Copies Over Waypoints From The List
        public virtual void SetWaypoints(List<Vector2> wp) {
            waypoints.Clear();
            if(wp != null)
                waypoints.AddRange(wp);
        }

        // Scripted Logic For Movement
        public abstract void DecideMove(GameState g, float dt);
        public abstract void ApplyMove(GameState g, float dt);
    }
}