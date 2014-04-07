using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Graphics;

namespace RTSEngine.Interfaces {
    public static class FSMState {
        public const int None = 0;
        public const int Rest = None + 1;
        public const int Walking = Rest + 1;
        public const int PrepareCombatRanged = Walking + 1;
        public const int CombatRanged = PrepareCombatRanged + 1;
        public const int CombatMelee = CombatRanged + 1;
        public const int Death = CombatMelee + 1;
        public const int Special0 = Death + 1;
        public const int Special1 = Special0 + 1;
    }

    // Base Controller Functionality
    public abstract class ACUnitController {
        // The Unit That Is Being Controlled
        protected RTSUnit unit;
        public RTSUnit Unit {
            get { return Unit; }
        }

        public virtual void SetUnit(RTSUnit u) {
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
        // This Is The Value Read By The Renderer
        public float AnimationFrame {
            get;
            protected set;
        }

        // Local List Of Particles For The Controller
        private List<Particle> particles = new List<Particle>();
        public bool HasParticles {
            get { return particles.Count > 0; }
        }

        protected void AddParticle(Particle p) {
            particles.Add(p);
        }
        public void GetParticles(List<Particle> lOut) {
            if(particles.Count > 0) {
                lOut.AddRange(particles);
                particles.Clear();
            }
        }

        // Scripted Animation
        public abstract void Update(GameState s, float dt);
    }

    // Reasons About How Combat Damage Should Be Performed To A Unit's Target
    public abstract class ACUnitCombatController : ACUnitController {
        // Scripted Logic For Attacking
        public abstract void Attack(GameState g, float dt);
    }

    // Special Movement Mechanics
    public abstract class ACUnitMovementController : ACUnitController {
        private List<Vector2> waypoints = new List<Vector2>();
        public List<Vector2> Waypoints {
            get { return waypoints; }
            set { waypoints = value; }
        }

        // Scripted Logic For Movement
        public abstract void DecideMove(GameState g, float dt);
        public abstract void ApplyMove(GameState g, float dt);
    }

    // Base Controller Functionality
    public abstract class ACBuildingController {
        // The Building That Is Being Controlled
        protected RTSBuilding building;
        public RTSBuilding Building {
            get { return building; }
        }

        public virtual void SetBuilding(RTSBuilding b) {
            if(b == null) return;
            if(building != null)
                throw new ArgumentException("Cannot Rebind This Controller To Another Building");
            building = b;
            return;
        }
        public T SetBuilding<T>(RTSBuilding b) where T : ACUnitController {
            SetBuilding(b);
            return this as T;
        }
    }

    // A Super Controller Called By The Gameplay Controller
    public abstract class ACBuildingActionController : ACBuildingController {
        // Scripted Super-Controller Logic
        public abstract void DecideAction(GameState g, float dt);
        public abstract void ApplyAction(GameState g, float dt);
    }

    // A Button That Will Be Shown
    public abstract class ACRTSButton {
        public abstract void Apply(GameState s);
    }
}