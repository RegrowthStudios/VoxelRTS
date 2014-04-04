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
        public void SetSquad(RTSSquad s) {
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

        // Waypoints That Units In This Squad Will Generally Follow
        private List<Vector2> waypoints = new List<Vector2>();
        public List<Vector2> Waypoints {
            get { return waypoints; }
            set { waypoints = value; }
        }

        // The Index Of The Current Waypoint Each Unit In This Squad Is Supposed To Head Toward
        // Key: UUID; Value: CurrentWaypointIndex
        private Dictionary<int, int> currentWaypointIndices = new Dictionary<int, int>();
        public Dictionary<int, int> CurrentWaypointIndices {
            get { return currentWaypointIndices; }
            set { currentWaypointIndices = value; }
        }

        // Does The Provided Index Point To A Valid Squad Waypoint?
        public bool IsValid(int idx) {
            return Waypoints != null && idx >= 0 && idx < Waypoints.Count;
        }

        // Units' Displacements From The Squad's Waypoints At Origin
        public readonly Dictionary<int, Vector2> formationAssignments = new Dictionary<int, Vector2>();

        // Given An Angle, Rotate The Formation Assignments
        public Dictionary<int, Vector2> RotateFormation(float a) {
            var rfa = new Dictionary<int, Vector2>(); 
            Matrix mr = Matrix.CreateRotationZ(a);
            foreach(var fa in formationAssignments) {
                int uuid = fa.Key;
                Vector2 post = fa.Value;
                rfa[uuid] = Vector2.TransformNormal(post, mr);
            }
            return rfa;
        }
 
        // Decide Where Units In This Squad Should Go When Moving
        public abstract void ApplyMovementFormation(int movementOrder);

        // Decide Where Units In This Squad Should Go When Close To Their Target
        public abstract void CalculateTargetFormation();
        
        // Scripted Logic For Movement
        public abstract void DecideMoves(GameState g, float dt);
        public abstract void ApplyMoves(GameState g, float dt);
    }

    // Controls The Targetting That A Squad Performs
    public abstract class ACSquadTargettingController : ACSquadController {
        // A Squad Target
        protected RTSSquad targetSquad;

        // A Unit Target
        protected RTSUnit targetUnit;
        public RTSUnit Target {
            get { return targetUnit; }
            set { targetUnit = value; }
        }

        // Find And Setting A Target For This Squad
        public abstract void DecideTarget(GameState g, float dt);
        public abstract void ApplyTarget(GameState g, float dt);
    }
}