using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;

namespace RTSEngine.Interfaces {
    public interface IEntityController {
        // The Entity That This Controller is Controlling
        IEntity Entity { get; }
    }

    public interface IMovementController : IEntityController {
        // List Of Waypoints To Move Each Target
        IEnumerable<Vector2> Waypoints { get; }

        // Provides Controller With A New Move List
        void SetWaypoints(Vector2[] p);

        // Performs The Critical Logic Of This Controller
        void Move(GameState g, float dt);
    }

    public interface IActionController : IEntityController {
        // Performs Decision Logic (Eg., Attack Or Move?) For The Entity
        void PerformDecision(GameState g, float dt);
    }

    public interface ITargettingController : IEntityController { 
        // Find And Set A Target For This Controller's Entity
        void FindTarget(GameState g, float dt);
    }

    public interface ICombatController : IEntityController {
        // Attack This Controller's Entity's Target, If Possible
        void Attack(GameState g, float dt);
    }
}