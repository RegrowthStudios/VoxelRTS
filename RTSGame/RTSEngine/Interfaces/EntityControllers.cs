using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Graphics;

namespace RTSEngine.Interfaces {
    public static class BehaviorFSM {
        // State Codes
        public const int None = 0;
        public const int Rest = None + 1;
        public const int Walking = Rest + 1;
        public const int PrepareCombatRanged = Walking + 1;
        public const int CombatRanged = PrepareCombatRanged + 1;
        public const int CombatMelee = CombatRanged + 1;
        public const int Death = CombatMelee + 1;
        public const int Special0 = Death + 1;
        public const int Special1 = Special0 + 1;

        // Targeting Order Codes - Will Influence Targeting Behavior
        public const int TargetPassively = 0;
        public const int TargetAggressively = TargetPassively + 1;

        // Combat Order Codes - Will Influence Behavior While In Combat
        public const int UseRangedAttack = 0;
        public const int UseMeleeAttack = UseRangedAttack + 1;

        // Movement Order Codes - Will Influence Movement Behavior
        public const int FreeFormation = 0;
        public const int BoxFormation = FreeFormation + 1;
        public const int CircleFormation = BoxFormation + 1;
        
        public static int GetState(int behaviorCode) {
            return GetByte(behaviorCode, 0);
        }
        public static int GetTargetingOrders(int behaviorCode) {
            return GetByte(behaviorCode, 1);
        }
        public static int GetCombatOrders(int behaviorCode) {
            return GetByte(behaviorCode, 2);
        }
        public static int GetMovementOrders(int behaviorCode) {
            return GetByte(behaviorCode, 3);
        }
        
        // Retrieve The Specified Byte From A 32-Bit Int
        public static int GetByte(int data, int index) {
            return (data >> 8*index) & 255;
        }

        // Set The Specified Byte In A 32-Bit Int
        public static int SetByte(int data, int b, int index) {
            return (data & ~(255 << 8*index)) | (b << 8*index);
        }
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
        // Index Of Squad Waypoint That This Controller's Unit Is Currently Attempting To Reach
        public int CurrentWaypointIndex { get; set; }
        
        // Has This Controller's Unit's Squad's Pathfinding Query Been Resolved?
        public bool HasPath { get; set; }
        
        // Does The Provided Index Point To A Valid Squad Waypoint?
        public bool IsValid(int idx) {
            if(unit.Squad == null || unit.Squad.MovementController == null || unit.Squad.MovementController.Waypoints == null)
                return false;
            else
                return idx >= 0 && idx < unit.Squad.MovementController.Waypoints.Count;
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

        public void SetBuilding(RTSBuilding b) {
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
}