using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;

namespace RTSEngine.Interfaces {
    // Bitfield Flags For Entity Controller Type
    public enum EntityControllerType {
        None = 0x00,
        Action = 0x01,
        Movement = 0x02,
        Targetting = 0x04,
        Combat = 0x08
    }

    public interface IEntityController {
        // The Entity That This Controller is Controlling
        IEntity Entity { get; }

        // Will Set Once And Then Fail On Later Occurences
        void SetEntity(IEntity e);
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

    public interface ICombatController : IEntityController {
        // Attack This Controller's Entity's Target, If Possible
        void Attack(GameState g, float dt);
    }
}