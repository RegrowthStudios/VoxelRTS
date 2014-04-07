using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;

namespace RTSEngine.Interfaces {
    // Base Controller Functionality
    public abstract class ACSquadController {
        // The Squad That Is Being Controlled
        protected RTSSquad squad;
        public RTSSquad Squad {
            get { return squad; }
        }

        // Will Set Once And Then Fail On Later Occurrences
        public virtual void SetSquad(RTSSquad s) {
            if(s == null) return;
            if(squad != null)
                throw new ArgumentException("Cannot Rebind This Controller To Another Squad");
            squad = s;
            return;
        }
        public T SetSquad<T>(RTSSquad s) where T : ACSquadController {
            SetSquad(s);
            return this as T;
        }
    }

    // A Super Controller Called By The Gameplay Controller
    public abstract class ACSquadActionController : ACSquadController {
        // Scripted Super-Controller Logic
        public abstract void DecideAction(GameState g, float dt);
        public abstract void ApplyAction(GameState g, float dt);
    }

    // The Movement Controller That Dictates The General Movement Behavior Of Units In The Squad
    public abstract class ACSquadMovementController : ACSquadController {
        // Box Formation Will Follow The Golden Ratio Phi
        protected const float phi = 1.61803398875f;

        // The Constants Used In Flow Field Calculations
        protected const float rForce =  1f;
        protected const float aForce = -10f;

        // Waypoints That Units In This Squad Will Generally Follow
        private List<Vector2> waypoints = new List<Vector2>();
        public List<Vector2> Waypoints {
            get { return waypoints; }
            set { waypoints = value; }
        }

        // Units' Displacements From The Squad's Waypoints At Origin
        private List<Vector2> formation = new List<Vector2>();
        public List<Vector2> Formation {
            get { return formation; }
            set { formation = value; }
        }

        // The Index Of The Current Waypoint Each Unit In This Squad Is Supposed To Head Toward
        // Key: UUID; Value: CurrentWaypointIndex
        private Dictionary<int, int> currentWaypointIndices = new Dictionary<int, int>();
        public Dictionary<int, int> CurrentWaypointIndices {
            get { return currentWaypointIndices; }
            set { currentWaypointIndices = value; }
        }

        // The Net Force On Each Unit In This Squad
        // Key: UUID; Value: Net Force
        private Dictionary<int, Vector2> netForces = new Dictionary<int, Vector2>();
        public Dictionary<int,Vector2> NetForces {
            get { return netForces; }
            set { netForces = value; }
        }

        // Does The Provided Index Point To A Valid Squad Waypoint?
        public bool IsValid(int idx) {
            return Waypoints != null && idx >= 0 && idx < Waypoints.Count;
        }

        // Given An Angle, Rotate The Formation Assignments
        public List<Vector2> RotateFormations(float a) {
            var rfa = new List<Vector2>();
            Matrix mr = Matrix.CreateRotationZ(a);
            foreach(var post in formation) {
                rfa.Add(Vector2.TransformNormal(post, mr));
            }
            return rfa;
        }

        // Calculate The Force Between Two Locations
        public Vector2 Force(Vector2 a, Vector2 b) {
            Vector2 diff = a - b;
            float denom = diff.LengthSquared();
            return diff.X != 0 && diff.Y != 0 ? 1 / denom * Vector2.Normalize(diff) : Vector2.Zero;
        }

        // Calculate The Force Between Two IEntities
        public Vector2 Force(IEntity e1, IEntity e2) {
            return rForce*Force(e1.GridPosition, e2.GridPosition);
        }

        // Calculate The Force Between An IEntity And A Waypoint
        public Vector2 Force(IEntity e, Vector2 wp) {
            return aForce*Force(e.GridPosition, wp);
        }
 
        // Decide Where Units In This Squad Should Tend To Go While Moving
        // Sets Formation Field/Property
        public abstract void ApplyMovementFormation(int movementOrder);
        
        // Scripted Logic For Movement
        public abstract void DecideMoves(GameState g, float dt);
        public abstract void ApplyMoves(GameState g, float dt);
    }

    // Controls The Targetting That A Squad Performs
    public abstract class ACSquadTargetingController : ACSquadController {
        // A Squad Target
        protected RTSSquad targetSquad;

        // A Unit Target
        protected RTSUnit targetUnit;
        public RTSUnit Target {
            get { return targetUnit; }
            set { 
                targetUnit = value;
                targetSquad = targetUnit != null ? targetUnit.Squad : null;
            }
        }

        // Find And Setting A Target For This Squad
        public abstract void DecideTarget(GameState g, float dt);
        public abstract void ApplyTarget(GameState g, float dt);
    }
}