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
                if(target != value) {
                    target = value;
                    if(OnNewTarget != null)
                        OnNewTarget(this, target);
                }
            }
        }

        // Event Triggered When This Entity Find A New Attack Target (Null When Can't Find One)
        public event Action<IEntity, IEntity> OnNewTarget;

        // This Unit's Current Health
        public int Health { get; private set; }
        public bool IsAlive {
            get {
                return Health > 0;
            }
            set {
                if(!value)
                    Destroy();
                else if(!IsAlive)
                    throw new InvalidOperationException("Cannot Bring Back Units From The Dead");
            }
        }

        // Event Triggered When This Entity Receives Damage
        public event Action<IDestructibleEntity, int> OnDamage;

        // Destruction Event
        public event Action<IEntity> OnDestruction;

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

        // Combat Event
        public event Action<ICombatEntity, IDestructibleEntity> OnAttackMade;

        // Creates a New RTSUnitInstance on the Given Team with the Given Data at the Given Position
        public RTSUnitInstance(RTSTeam team, RTSUnit data, Vector3 position) {
            Team = team;
            UnitData = data;
            worldPosition = position;
            ViewDirection = Vector2.UnitX;
            Health = UnitData.Health;
            CollisionGeometry = UnitData.ICollidableShape.Clone() as ICollidable;
        }

        // Computes The Damage To Deal With Access To A Random Number
        public int ComputeDamage(double rand) {
            return UnitData.BaseCombatData.ComputeDamageDealt(rand);
        }

        // Applies Damage
        public void DamageTarget(double rand) {
            if(Target == null) return;
            IDestructibleEntity t = target as IDestructibleEntity;
            if(t == null) return;

            // Damage
            t.Damage(ComputeDamage(rand));
            if(OnAttackMade != null)
                OnAttackMade(this, t);
        }

        // Applies Damage To Health
        public void Damage(int d) {
            Health -= d;
            if(OnDamage != null)
                OnDamage(this, d);
        }

        // Destroys This Entity
        public void Destroy() {
            Health = 0;
            if(OnDestruction != null)
                OnDestruction(this);
        }

        // Changes the Position of the Unit by Change
        public void Move(Vector2 change) {
            worldPosition.X += change.X;
            worldPosition.Y += change.Y;
            if(change.X != 0 || change.Y != 0)
                ViewDirection = Vector2.Normalize(change);
        }
    }
}