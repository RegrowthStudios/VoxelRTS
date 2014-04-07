using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSUnit : ICombatEntity {
        public static void Serialize(BinaryWriter s, RTSUnit e) {
            s.Write(e.UnitData.Index);
            s.Write(e.UUID);
            s.Write(e.State);
            s.Write(e.ViewDirection);
            s.Write(e.GridPosition);
            s.Write(e.Height);
            if(e.Target != null) {
                s.Write(true);
                s.Write(e.Target.UUID);
            }
            else {
                s.Write(false);
            }
            s.Write(e.Health);
            s.Write(e.MovementMultiplier);
            if(e.ActionController != null) {
                s.Write(true);
                // TODO: Custom Serialize
            }
            else {
                s.Write(false);
            }
            if(e.CombatController != null) {
                s.Write(true);
                // TODO: Custom Serialize
            }
            else {
                s.Write(false);
            }
            if(e.MovementController != null) {
                s.Write(true);
                // TODO: Custom Serialize
            }
            else {
                s.Write(false);
            }
            if(e.AnimationController != null) {
                s.Write(true);
                // TODO: Custom Serialize
            }
            else {
                s.Write(false);
            }
        }
        public static RTSUnit Deserialize(BinaryReader s, GameState state) {
            // TODO: Implement
            return null;
        }

        // Common Data
        public RTSUnitData UnitData {
            get;
            private set;
        }

        // Unique ID
        public int UUID {
            get;
            private set;
        }

        // Unit's Team And Squad
        public RTSTeam Team {
            get;
            private set;
        }
        public RTSSquad Squad {
            get;
            set;
        }

        // State Information
        public int State {
            get;
            set;
        }

        // View Direction
        public Vector2 ViewDirection {
            get;
            private set;
        }

        // 2D Position
        private Vector2 gridPos;
        public Vector2 GridPosition {
            get { return gridPos; }
            set { gridPos = value; }
        }

        // 3D Position
        private float height;
        public float Height {
            get { return height; }
            set { height = value; }
        }
        public Vector3 WorldPosition {
            get { return new Vector3(gridPos.X, height, gridPos.Y); }
        }

        // Target
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

        // Event Triggered When New Target Found (Null When Can't Find One)
        public event Action<IEntity, IEntity> OnNewTarget;

        // This Unit's Current Health
        public int Health {
            get;
            private set;
        }
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

        // Damaging Events
        public event Action<IEntity, int> OnDamage;
        public event Action<IEntity> OnDestruction;

        // Collision Geometry
        public ICollidable CollisionGeometry {
            get;
            private set;
        }
        public BoundingBox BBox {
            get {
                return new BoundingBox(
                    UnitData.BBox.Min + WorldPosition,
                    UnitData.BBox.Max + WorldPosition
                    );
            }
        }

        // Speed Of Movement For The Entity
        public float MovementSpeed {
            get { return UnitData.MovementSpeed * MovementMultiplier; }
        }
        public float MovementMultiplier {
            get;
            set;
        }

        // Action Controller
        private ACUnitActionController aController;
        public ACUnitActionController ActionController {
            get { return aController; }
            set {
                aController = value;
                if(aController != null)
                    aController.SetUnit(this);
            }
        }

        // Combat Controller
        private ACUnitCombatController cController;
        public ACUnitCombatController CombatController {
            get { return cController; }
            set {
                cController = value;
                if(cController != null)
                    cController.SetUnit(this);
            }
        }

        // Movement Controller
        private ACUnitMovementController mController;
        public ACUnitMovementController MovementController {
            get { return mController; }
            set {
                mController = value;
                if(mController != null)
                    mController.SetUnit(this);
            }
        }

        // Animation Controller
        private ACUnitAnimationController anController;
        public ACUnitAnimationController AnimationController {
            get { return anController; }
            set {
                anController = value;
                if(anController != null)
                    anController.SetUnit(this);
            }
        }

        // Combat Event
        public event Action<ICombatEntity, IEntity> OnAttackMade;

        // Creates a New RTSUnitInstance on the Given Team with the Given Data at the Given Position
        public RTSUnit(RTSTeam team, RTSUnitData data, Vector2 position) {
            // Identification
            UUID = UUIDGenerator.GetUUID();
            Team = team;
            Squad = null;
            gridPos = position;

            // Set From Common Data
            UnitData = data;
            Health = UnitData.Health;

            // Default Information
            height = 0;
            ViewDirection = Vector2.UnitX;
            CollisionGeometry = UnitData.ICollidableShape.Clone() as ICollidable;
            MovementMultiplier = 1f;
        }

        // Computes The Damage To Deal With Access To A Random Number And A Target
        public int ComputeDamage(double rand) {
            RTSUnit t = Target as RTSUnit;
            int dmg = UnitData.BaseCombatData.ComputeDamageDealt(rand);
            if(t != null) dmg = t.UnitData.BaseCombatData.ComputeDamageReceived(dmg);
            return dmg;
        }

        // Applies Damage
        public void DamageTarget(double rand) {
            if(Target == null) return;
            IEntity t = target as IEntity;
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
            if(!IsAlive)
                Destroy();
        }

        // Destroys This Entity
        public void Destroy() {
            Health = 0;
            if(OnDestruction != null)
                OnDestruction(this);
        }

        // Changes the Position of the Unit by Change
        public void Move(Vector2 change) {
            if(change.X != 0 || change.Y != 0) {
                gridPos += change;
                ViewDirection = Vector2.Normalize(change);
            }
        }
    }
}