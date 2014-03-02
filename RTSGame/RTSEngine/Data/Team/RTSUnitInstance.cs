using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team
{
    // TODO: Implement IDestructibleEntity, ICombatEntity, IMovingEntity

    class RTSUnitInstance : IDestructibleEntity, ICombatEntity, IMovingEntity
    {

        // RTSUnit Data of The Unit
        protected RTSUnit UnitData;

        // MovementController of The Unit
        protected IMovementController MovementController;

        // ActionController of The Unit
        protected IActionController ActionController;

        // TargettingController of The Unit
        protected ITargettingController TargettingController;

        // CombatController of The Unit
        protected ICombatController CombatController;

        protected RTSTeam RTSTeam;
        protected Vector3 Position;
        protected ICollidable Shape;
        protected IEntity CurrentTarget;
        protected Boolean AbleToAttack;

        // The Entity's Team
        public RTSTeam Team
        {
            get { return RTSTeam; }
        }

        // Location In The World
        public Vector3 WorldPosition
        {
            get { return Position; }
        }

        // Collision Geometry
        public ICollidable CollisionGeometry
        {
            get { return Shape; }
        }

        // Targetting Information 
        public IEntity Target
        {
            get { return CurrentTarget; }
            set 
            {
                CurrentTarget = value;
                if (OnNewAttackTarget != null)
                    OnNewAttackTarget(this, (IDestructibleEntity) CurrentTarget); 
            }
        }

        // Speed Of Movement For The Entity
        public float MovementSpeed
        {
            get { return UnitData.MovementSpeed; }
        }

        // The Current Health Of The Entity
        public int Health
        {
            get { return UnitData.Health; }
        }

        // Information About Whether This Entity Can Attack Yet
        public bool CanAttack
        {
            get { return AbleToAttack; }
        }

        public RTSUnitInstance(RTSTeam team, Vector3 position, ICollidable shape)
        {
            this.RTSTeam = team;
            this.Position = position;
            this.Shape = shape;
        }

        // Computes The Damage To Deal With Access To A Random Number
        public int DealDamage(double rand)
        {
            return UnitData.BaseCombatData.ComputeDamageDealt(rand);
        }

        // Applies Damage To Health
        public void Damage(int d)
        {
            if (OnDamage != null)
                OnDamage(this, d);

            UnitData.Health -= d;
        }

        // Event Triggered When This Entity Receives Damage
        public event Action<IDestructibleEntity, int> OnDamage;

        // Event Triggered When This Entity Find A New Attack Target (Null When Can't Find One)
        public event Action<ICombatEntity, IDestructibleEntity> OnNewAttackTarget;

    }
}
