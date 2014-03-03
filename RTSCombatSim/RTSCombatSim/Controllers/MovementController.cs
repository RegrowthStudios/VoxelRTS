using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    public class MovementController : IMovementController {

        // The Waypoint To Which This Controller Has Decided To Send Its Entity
        protected Vector2 waypoint;

        // The Entity That This MovementController Is Moving
        protected IMovingEntity entity;
        public IEntity Entity {
            get { return entity; }
        }

        // List Of Waypoints To Move Each Target
        protected Vector2[] waypoints;
        public IEnumerable<Vector2> Waypoints {
            get { return waypoints; }
        }

        // Provides Controller With A New Move List
        public void SetWaypoints(Vector2[] p) {
            waypoints = p;
        }

        // Constructs a MovementController to Move the Given Entity to the Waypoints
        public MovementController(IMovingEntity entity, Vector2[] waypoints) {
            this.entity = entity;
            this.waypoints = waypoints;
        }

        // Performs The Critical Logic Of This Controller
        public void DecideMove(GameState g, float dt) {
            waypoint = waypoints[waypoints.Length - 1];
        }

        public void ApplyMove(GameState g, float dt) {
            Vector2 change = waypoint - entity.GridPosition;
            if(change != Vector2.Zero) {
                float magnitude = change.Length();
                Vector2 scaledChange = (change / magnitude) * entity.MovementSpeed * dt;
                // This Logic Prevents The Unit From Hovering Around Its Goal
                if(scaledChange.LengthSquared() > magnitude * magnitude)
                    entity.Move(change);
                else
                    entity.Move(scaledChange);
            }
        }
    }
}