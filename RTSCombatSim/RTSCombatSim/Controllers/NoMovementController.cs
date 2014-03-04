using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    public class NoMovementController : IMovementController {
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

        // Constructs a MovementController to Move the Given Entity to the Waypoints
        public NoMovementController() {
            entity = null;
            waypoints = null;
        }

        // Set Entity Only Once
        public void SetEntity(IEntity e) {
            if(Entity != null && Entity != e)
                throw new InvalidOperationException("Controllers Can Only Have Entities Set Once");
            entity = e as IMovingEntity;
        }

        // Provides Controller With A New Move List
        public void SetWaypoints(Vector2[] p) {
            waypoints = p;
        }

        // Performs The Critical Logic Of This Controller
        public void DecideMove(GameState g, float dt) {

        }

        public void ApplyMove(GameState g, float dt) {

        }
    }
}