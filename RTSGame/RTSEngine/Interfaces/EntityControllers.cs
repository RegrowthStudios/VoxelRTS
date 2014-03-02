using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Interfaces {
    public interface IMovementContoller {
        // List Of Waypoints To Move Each Target
        IEnumerable<Vector2> Waypoints;

        // Provides Controller With A New Move List
        void SetWaypoints(Vector2[] p);

        // Adds Entities To Be Moved
        void AddEntities(IMovingEntity[] entities);

        // Performs The Critical Logic Of This Controller
        void MoveTargets(float dt);
    }

    public interface IActionController {
        // Performs Decision Logic For The Entity
        void PerformDecision(IMovingEntity entity, float dt);
    }
}