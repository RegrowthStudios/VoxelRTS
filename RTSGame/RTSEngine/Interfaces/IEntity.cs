using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Team;

namespace RTSEngine.Interfaces {
    public interface IEntity {
        // This Entity's Team
        RTSTeam Team { get; }

        // Entity's Unique ID
        int UUID { get; }

        // The Current Health Of The Entity
        int Health { get; }

        // Whether It Is Alive Or Not
        bool IsAlive { get; set; }

        // Location In The World
        Vector2 GridPosition { get; set; }
        float Height { get; }
        Vector3 WorldPosition { get; }

        // Collision Geometry
        ICollidable CollisionGeometry { get; }
        BoundingBox BBox { get; }

        // Targetting Information
        IEntity Target { get; set; }

        // Events
        event Action<IEntity, IEntity> OnNewTarget;
        event Action<IEntity, int> OnDamage;
        event Action<IEntity> OnDestruction;

        // Controllers
        ACUnitActionController ActionController { get; set; }
        ACUnitAnimationController AnimationController { get; set; }

        // Calls The Destruction Event
        void Destroy();

        // Applies Damage To Health
        void Damage(int d);
    }

    public interface ICombatEntity : IEntity {
        // Event From Combat
        event Action<ICombatEntity, IEntity> OnAttackMade;

        // This Entity's Combat Controller
        ACUnitCombatController CombatController { get; set; }

        // Computes The Damage To Deal With Access To A Random Number
        int ComputeDamage(double rand);

        // Actually Damages A Target
        void DamageTarget(double rand);
    }
}