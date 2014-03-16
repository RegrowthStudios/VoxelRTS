using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    class SquadMovementController{

        // The Waypoint To Which This Controller Has Decided To Send Its Entity
        protected Vector2 waypoint;

         // The Index Of The Waypoint The Squad Is Currently Moving To
        protected int current;

        // The Entity That This MovementController Is Moving
        protected ISquad squad;
        public ISquad Squad {
            get { return squad; }
        }

        // List Of Waypoints To Move Each Target
        protected Vector2[] waypoints;
        public IEnumerable<Vector2> Waypoints {
            get { return waypoints; }
        }

        // Constructs a MovementController to Move the Given Entity to the Waypoints
        public SquadMovementController() {
            squad = null;
            waypoints = null;
        }

        // Set Squad Only Once
        public void SetSquad(ISquad s) {
            if(Squad != null && Squad != s)
                throw new InvalidOperationException("Controllers Can Only Have Entities Set Once");
            squad = s;
        }

        // Provides Controller With A New Move List
        public void SetWaypoints(Vector2[] p) {
            waypoints = p;
            current = waypoints.Length - 1;
        }

        // Performs The Critical Logic Of This Controller
        public void DecideMove(GameState g, float dt) {
            waypoint = waypoints[current];
            current--;
        }

        public void ApplyMove(GameState g, float dt) {
            foreach(var c in Squad.Combatants){
                Vector2 change = waypoint - c.GridPosition;
                if(change != Vector2.Zero) {
                    float magnitude = change.Length();
                    Vector2 scaledChange = (change / magnitude) * c.MovementSpeed * dt;
                    // This Logic Prevents The Unit From Hovering Around Its Goal
                    if(scaledChange.LengthSquared() > magnitude * magnitude)
                        c.Move(change);
                    else
                        c.Move(scaledChange);
                }
            }            
        }
    }
}
