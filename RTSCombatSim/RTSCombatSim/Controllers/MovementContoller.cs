using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace RTSCS.Controllers {
    class MovementContoller : IMovementController {
        // TODO

        // List Of Waypoints To Move Each Target
        private Vector2[] waypoints;
        public IEnumerable<Vector2> Waypoints {
            get { return waypoints; }
        }
       
        // Provides Controller With A New Move List
        public void SetWaypoints(Vector2[] p) {
            waypoints = p;
        }

        // Adds Entities To Be Moved
        public void AddEntities(IMovingEntity[] entities) {

        }

        // Performs The Critical Logic Of This Controller
        public void MoveTargets(float dt) {
            
        }

        public MovementContoller(Vector2[] p) {
            waypoints = p;
        }
    }
}
