using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Team;

namespace RTSEngine.Interfaces {
    public interface IEntity {
        // The Entity's Team
        RTSTeam Team { get; }

        // Location In The World
        Vector3 WorldPosition { get; }

        // Collision Geometry
        ICollidable CollisionGeometry { get; }

        // Targetting Information
        IEntity Target { get; set; }

        // These Can Change During The Entity's Lifecycle But Will Always Be There
        IActionController ActionController { get; set; }
        ITargettingController TargettingController { get; set; }
    }

    public interface IMovingEntity : IEntity {
        // Speed Of Movement For The Entity
        float MovementSpeed { get; }

        // This Can Change During The Entity's Lifecycle But Will Always Be There
        IMovementController MovementController { get; set; }
        
        // Changes the Position of the Unit by the Change
        void Move (Vector3 change);
    }

    public interface IDestructibleEntity : IEntity {
        // The Current Health Of The Entity
        int Health { get; }

        // Applies Damage To Health
        void Damage(int d);

        // Event Triggered When This Entity Receives Damage
        event Action<IDestructibleEntity, int> OnDamage;
    }

    public interface ICombatEntity : IDestructibleEntity, IMovingEntity {

        // Computes The Damage To Deal With Access To A Random Number
        int DealDamage(double rand);

        // Event Triggered When This Entity Find A New Attack Target (Null When Can't Find One)
        event Action<ICombatEntity, IDestructibleEntity> OnNewAttackTarget;

        // This Can Change During The Entity's Lifecycle But Will Always Be There
        ICombatController CombatController { get; set; }
    }

}