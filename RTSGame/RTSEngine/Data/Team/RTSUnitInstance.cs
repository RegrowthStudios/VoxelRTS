using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    // TODO: Implement IDestructibleEntity, ICombatEntity, IMovingEntity

    public class RTSUnitInstance : ICombatEntity {

        // RTSUnit Data of The Unit
        protected RTSUnit UnitData;

        // RTSTeam of The Unit
        protected RTSTeam RTSTeam;

        // Position of The Unit
        protected Vector3 Position;

        // Target of the Unit
        protected IEntity CurrentTarget;

        // The Entity's Team
        public RTSTeam Team {
            get { return RTSTeam; }
        }

        // Location In The World
        public Vector3 WorldPosition {
            get { return Position; }
        }

        // Collision Geometry
        public ICollidable CollisionGeometry {
            get { return UnitData.ICollidableShape; }
        }

        // Targetting Information 
        public IEntity Target {
            get { return CurrentTarget; }
            set {
                CurrentTarget = value;
                if (OnNewAttackTarget != null)
                    OnNewAttackTarget(this, (IDestructibleEntity)CurrentTarget);
            }
        }

        // Speed Of Movement For The Entity
        public float MovementSpeed {
            get { return UnitData.MovementSpeed; }
        }

        // The Current Health Of The Entity
        public int Health {
            get { return UnitData.Health; }
        }

        // MovementController of The Unit
        public IMovementController MovementController {
            get;
            set;
        }

        // ActionController of The Unit
        public IActionController ActionController {
            get;
            set;
        }

        // TargettingController of The Unit
        public ITargettingController TargettingController {
            get;
            set;
        }

        // CombatController of The Unit
        public ICombatController CombatController {
            get;
            set;
        }

        // Creates a New RTSUnitInstance on the Given Team with the Given Data at the Given Position
        public RTSUnitInstance(RTSTeam team, RTSUnit data, Vector3 position) {
            this.RTSTeam = team;
            this.UnitData = data;
            this.Position = position;
        }

        // Computes The Damage To Deal With Access To A Random Number
        public int DealDamage(double rand) {
            return UnitData.BaseCombatData.ComputeDamageDealt(rand);
        }

        // Applies Damage To Health
        public void Damage(int d) {
            if (OnDamage != null)
                OnDamage(this, d);

            UnitData.Health -= d;
        }

        // Changes the Position of the Unit by Change
        public void Move(Vector3 change) {
            float x = Position.X - change.X;
            float y = Position.Y - change.Y;
            float z = Position.Z - change.Z;
            Position = new Vector3(x, y, z);
        }

        // Event Triggered When This Entity Receives Damage
        public event Action<IDestructibleEntity, int> OnDamage;

        // Event Triggered When This Entity Find A New Attack Target (Null When Can't Find One)
        public event Action<ICombatEntity, IDestructibleEntity> OnNewAttackTarget;

    }
}
