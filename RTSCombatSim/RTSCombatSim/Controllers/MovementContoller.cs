using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    public class MovementContoller : IMovementController {
        // TODO: store decided move

        // The Entity That This MovementController Is Moving
        private IMovingEntity entity;
        public IEntity Entity {
            get { return entity; }
        }

        // List Of Waypoints To Move Each Target
        private Vector2[] waypoints;
        public IEnumerable<Vector2> Waypoints {
            get { return waypoints; }
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

        public MovementContoller(Vector2[] p) {
            waypoints = p;
        }
    }
}
