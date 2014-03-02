using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    // TODO: Implement IDestructibleEntity, ICombatEntity, IMovingEntity

    public class RTSUnitInstance : ICombatEntity {

        // RTSUnit Data Of The Unit
        public RTSUnit UnitData { get; private set; }

        // RTSTeam Of The Unit
        public RTSTeam Team { get; private set; }

        // Position Of The Unit
        protected Vector3 worldPosition;
        public Vector3 WorldPosition {
            get { return worldPosition; }
        }

        // Target Of The Unit
        protected IEntity target;
        public IEntity Target {
            get { return target; }
            set {
                target = value;
                if(OnNewAttackTarget != null)
                    OnNewAttackTarget(this, (IDestructibleEntity)target);
            }
        }

        // This Unit's Current Health
        public int Health { get; private set; }

        // Collision Geometry
        public ICollidable CollisionGeometry {
            get { return UnitData.ICollidableShape; }
        }

        // Speed Of Movement For The Entity
        public float MovementSpeed {
            get { return UnitData.MovementSpeed; }
        }

        // MovementController of The Unit
        public IMovementController MovementController { get; set; }

        // ActionController of The Unit
        public IActionController ActionController { get; set; }

        // TargettingController of The Unit
        public ITargettingController TargettingController { get; set; }

        // CombatController of The Unit
        public ICombatController CombatController { get; set; }

        // Creates a New RTSUnitInstance on the Given Team with the Given Data at the Given Position
        public RTSUnitInstance(RTSTeam team, RTSUnit data, Vector3 position) {
            this.Team = team;
            this.UnitData = data;
            this.worldPosition = position;
            this.Health = UnitData.Health;
        }

        // Computes The Damage To Deal With Access To A Random Number
        public int DealDamage(double rand) {
            return UnitData.BaseCombatData.ComputeDamageDealt(rand);
        }

        // Applies Damage To Health
        public void Damage(int d) {
            if (OnDamage != null)
                OnDamage(this, d);

            Health -= d;
        }

        // Changes the Position of the Unit by Change
        public void Move(Vector3 change) {
            float x = worldPosition.X - change.X;
            float y = worldPosition.Y - change.Y;
            float z = worldPosition.Z - change.Z;
            worldPosition = new Vector3(x, y, z);
        }

        // Event Triggered When This Entity Receives Damage
        public event Action<IDestructibleEntity, int> OnDamage;

        // Event Triggered When This Entity Find A New Attack Target (Null When Can't Find One)
        public event Action<ICombatEntity, IDestructibleEntity> OnNewAttackTarget;

    }
}
