using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    public class MovementController : IMovementController {

        protected Vector2 move;

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
            move = waypoints[waypoints.Length - 1];
        }

        public void ApplyMove(GameState g, float dt) {
            Vector3 change;
            Vector2 position = new Vector2(entity.WorldPosition.X, entity.WorldPosition.Y);
            Vector2 unitChange = new Vector2(move.X - position.X, move.Y - position.Y);
            unitChange.Normalize();
            change = new Vector3(unitChange.X * entity.MovementSpeed * dt,unitChange.Y * entity.MovementSpeed * dt,0f);
            entity.Move(change);
        }

    }
}
