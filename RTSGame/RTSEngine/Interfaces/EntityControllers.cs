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
        void DecideMove(GameState g, float dt);

        void ApplyMove(GameState g, float dt);
    }

    public interface IActionController : IEntityController {
        // Performs Decision Logic For The Entity
        void DecideAction(GameState g, float dt);

        // Apply The Entity's Decision
        void ApplyAction(GameState g, float dt);
    }

    public interface ITargettingController : IEntityController { 
        // Find A Target For This Controller's Entity
        void FindTarget(GameState g, float dt);

        // Set A Target For This Controller's Entity
        void ChangeTarget(GameState g, float dt);
    }

    public interface ICombatController : IEntityController {
        // Attack This Controller's Entity's Target, If Possible
        void Attack(GameState g, float dt);
    }
}