using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSUnitInstance : ICombatEntity {
        // RTSUnit Data Of The Unit
        public RTSUnit UnitData { get; private set; }

        // RTSTeam Of The Unit
        public RTSTeam Team { get; private set; }

        // Unit's View Direction
        public Vector2 ViewDirection { get; private set; }

        // 3-D Position Of The Unit
        protected Vector3 worldPosition;
        public Vector3 WorldPosition {
            get { return worldPosition; }
        }

        // 2-D Position Of The Unit
        public Vector2 GridPosition {
            get {
                return new Vector2(worldPosition.X, worldPosition.Y);
            }
            set {
                worldPosition.X = value.X;
                worldPosition.Y = value.Y;
            }
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

        // Event Triggered When This Entity Find A New Attack Target (Null When Can't Find One)
        public event Action<ICombatEntity, IDestructibleEntity> OnNewAttackTarget;

        // This Unit's Current Health
        public int Health { get; private set; }

        // Event Triggered When This Entity Receives Damage
        public event Action<IDestructibleEntity, int> OnDamage;

        // Collision Geometry
        public ICollidable CollisionGeometry {
            get;
            private set;
        }

        // Speed Of Movement For The Entity
        public float MovementSpeed {
            get { return UnitData.MovementSpeed; }
        }

        // MovementController of The Unit
        private IMovementController mController;
        public IMovementController MovementController {
            get { return mController; }
            set {
                mController = value;
                if(mController != null)
                    mController.SetEntity(this);
            }
        }

        // ActionController of The Unit
        private IActionController aController;
        public IActionController ActionController {
            get { return aController; }
            set {
                aController = value;
                if(aController != null)
                    aController.SetEntity(this);
            }
        }

        // TargettingController of The Unit
        private ITargettingController tController;
        public ITargettingController TargettingController {
            get { return tController; }
            set {
                tController = value;
                if(tController != null)
                    tController.SetEntity(this);
            }
        }

        // CombatController of The Unit
        private ICombatController cController;
        public ICombatController CombatController {
            get { return cController; }
            set {
                cController = value;
                if(cController != null)
                    cController.SetEntity(this);
            }
        }

        // Creates a New RTSUnitInstance on the Given Team with the Given Data at the Given Position
        public RTSUnitInstance(RTSTeam team, RTSUnit data, Vector3 position) {
            Team = team;
            UnitData = data;
            worldPosition = position;
            Health = UnitData.Health;
            CollisionGeometry = UnitData.ICollidableShape.Clone() as ICollidable;
        }

        // Computes The Damage To Deal With Access To A Random Number
        public int DealDamage(double rand) {
            return UnitData.BaseCombatData.ComputeDamageDealt(rand);
        }

        // Applies Damage To Health
        public void Damage(int d) {
            Health -= d;
            if(OnDamage != null)
                OnDamage(this, d);
        }

        // Changes the Position of the Unit by Change
        public void Move(Vector2 change) {
            worldPosition.X += change.X;
            worldPosition.Y += change.Y;
            ViewDirection = Vector2.Normalize(change);
        }
    }
}
