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
        IEntity Target { get; }
    }

    public interface IMovingEntity : IEntity {
        // Speed Of Movement For The Entity
        float MovementSpeed { get; }
    }

    public interface IDestructibleEntity : IEntity {
        // The Current Health Of The Entity
        int Health { get; }

        // Applies Damage To Health
        void Damage(int d);

        // Event Triggered When This Entity Receives Damage
        event Action<IDestructibleEntity, int> OnDamage;
    }

    public interface ICombatEntity : IEntity {
        // Information About Whether This Entity Can Attack Yet
        bool CanAttack { get; }

        // Computes The Damage To Deal With Access To A Random Number
        int DealDamage(double rand);

        // Event Triggered When This Entity Find A New Attack Target (Null When Can't Find One)
        event Action<ICombatEntity, IDestructibleEntity> OnNewAttackTarget;
    }

}