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

        // Events Triggered By This Entity
        event Action<IEntity, IEntity> OnNewTarget;
        event Action<IEntity, int> OnDamage;
        event Action<IEntity> OnDestruction;

        // This Entity's Controller Of Controllers
        ACUnitActionController ActionController { get; set; }
        ACUnitAnimationController AnimationController { get; set; }

        // Calls The Destruction Event
        void Destroy();

        // The Current Health Of The Entity
        int Health { get; }

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

    public interface ISquad {
        // This Squad's Team
        RTSTeam Team { get; }

        // An Enumerator For This Squad's Combatants
        IEnumerable<ICombatEntity> Combatants { get; }
        int EntityCount { get; }

        // The Average Position Of The Squad
        Vector2 GridPosition { get; }

        // Events When Squad Is Altered
        event Action<ISquad, ICombatEntity> OnCombatantAddition;
        event Action<ISquad, ICombatEntity> OnCombatantRemoval;

        // The Targetting Controller For This Squad
        ITargettingController TargettingController { get; set; }

        // Adds A Combatant To This Squad
        void AddCombatant(ICombatEntity e);

        // Removes All Combatants From This Squad That Match A Predicate
        void RemoveAll(Predicate<ICombatEntity> f);

        // Should Be Done At The Beginning Of Each Frame (Only Once)
        void RecalculateGridPosition();
    }
}