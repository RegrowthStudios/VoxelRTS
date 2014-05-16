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

        // Entity's Icon Key
        string IconKey { get; }

        // State For Deciding Actions
        int State { get; set; }

        // The Current Health Of The Entity
        int Health { get; }
        int MaxHealth { get; }

        // Whether It Is Alive Or Not
        bool IsAlive { get; set; }

        // Location In The World
        Vector2 GridPosition { get; set; }
        float Height { get; }
        Vector3 WorldPosition { get; }

        // View Direction
        Vector2 ViewDirection { get; }

        // Collision Geometry
        ICollidable CollisionGeometry { get; }
        BoundingBox BBox { get; }

        // Targetting Information
        IEntity Target { get; set; }

        // Events
        event Action<IEntity, IEntity> OnNewTarget;
        event Action<IEntity, int> OnDamage;
        event Action<IEntity> OnDestruction;

        // Calls The Destruction Event
        void Destroy();

        // Applies Damage To Health
        void Damage(int d);
    }

    public interface ICombatEntity : IEntity {
        // Orders To Govern Combat Behavior
        int CombatOrders { get; set; }

        // Orders To Govern Movement Behavior
        int MovementOrders { get; set; }

        // Event From Combat
        event Action<ICombatEntity, IEntity> OnAttackMade;

        // This Entity's Combat Controller
        ACUnitCombatController CombatController { get; set; }

        // Computes The Damage To Deal With Access To A Random Number
        int ComputeDamage(double rand);

        // Actually Damages A Target
        void DamageTarget(double rand);
    }

    public static class EntityExt {
        public static float GetHealthRatio(this IEntity e) {
            return (float)e.Health / (float)e.MaxHealth;
        }
    }
}