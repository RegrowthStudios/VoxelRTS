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
        // Box Formation Follows The Golden Ratio Phi
        protected const float phi = 1.61803398875f;

        // Waypoints That Units In This Squad Will Generally Follow
        private List<Vector2> waypoints = new List<Vector2>();
        public List<Vector2> Waypoints {
            get { return waypoints; }
            set { waypoints = value; }
        }        
    
        // Units' Displacements From The Squad's Waypoints At Origin
        public readonly Dictionary<int, Vector2> formationAssignments = new Dictionary<int, Vector2>();

        // Units' Rotated Displacements From The Squad's Waypoints At Origin
        public Dictionary<int, Vector2> RotatedFormationAssignments { get; set; }

        // Given An Angle, Rotate The Formation Assignments
        protected void RotateFormation(float a) {
            Matrix mr = Matrix.CreateRotationZ(a);
            foreach(var fa in formationAssignments) {
                int uuid = fa.Key;
                Vector2 post = fa.Value;
                if(!RotatedFormationAssignments.ContainsKey(uuid)) {
                    RotatedFormationAssignments[uuid] = Vector2.TransformNormal(post, mr);
                }
                else {
                    RotatedFormationAssignments.Add(uuid,Vector2.TransformNormal(post, mr));
                }
            }
        }

        // Decide Where Units In This Squad Should Go When Moving
        public abstract void ApplyMovementFormation(int movementOrder);

        // Decide Where Units In This Squad Should Go When Close To Their Target
        public abstract void CalculateTargetFormation();
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