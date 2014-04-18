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
            s.Write(e.Data.Index);
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
                e.ActionController.Serialize(s);
            }
            else {
                s.Write(false);
            }
            if(e.CombatController != null) {
                s.Write(true);
                e.CombatController.Serialize(s);
            }
            else {
                s.Write(false);
            }
            if(e.MovementController != null) {
                s.Write(true);
                e.MovementController.Serialize(s);
            }
            else {
                s.Write(false);
            }
            if(e.AnimationController != null) {
                s.Write(true);
                e.AnimationController.Serialize(s);
            }
            else {
                s.Write(false);
            }
        }
        public static RTSUnit Deserialize(BinaryReader s, RTSTeam team, out int? target) {
            int type = s.ReadInt32();
            RTSUnit e = team.AddUnit(type, Vector2.Zero);
            if(e == null) throw new Exception("Could Not Create A Unit That Was Previously Created");
            e.UUID = s.ReadInt32();
            e.State = s.ReadInt32();
            e.ViewDirection = s.ReadVector2();
            e.GridPosition = s.ReadVector2();
            e.Height = s.ReadSingle();
            if(s.ReadBoolean()) {
                target = s.ReadInt32();
            }
            else {
                target = null;
            }
            e.Health = s.ReadInt32();
            e.MovementMultiplier = s.ReadSingle();
            if(s.ReadBoolean()) {
                if(e.ActionController != null) e.ActionController.Deserialize(s);
            }
            else {
                e.ActionController = null;
            }
            if(s.ReadBoolean()) {
                if(e.CombatController != null) e.CombatController.Deserialize(s);
            }
            else {
                e.CombatController = null;
            }
            if(s.ReadBoolean()) {
                if(e.MovementController != null) e.MovementController.Deserialize(s);
            }
            else {
                e.MovementController = null;
            }
            if(s.ReadBoolean()) {
                if(e.AnimationController != null) e.AnimationController.Deserialize(s);
            }
            else {
                e.AnimationController = null;
            }
            return e;
        }

        // Common Data
        public RTSUnitData Data {
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

        // The Unit's Behavior Code
        private int BehaviorCode;

        // The Unit's State
        public int State {
            get { return BehaviorFSM.GetByte(BehaviorCode, 0); }
            set { BehaviorCode = BehaviorFSM.SetByte(BehaviorCode, value, 0); }
        }

        // The Unit's Targeting Orders
        public int TargetingOrders {
            get { return BehaviorFSM.GetByte(BehaviorCode, 1); }
            set { BehaviorCode = BehaviorFSM.SetByte(BehaviorCode, value, 1); }
        }

        // The Unit's Combat Orders
        public int CombatOrders {
            get { return BehaviorFSM.GetByte(BehaviorCode, 2); }
            set { BehaviorCode = BehaviorFSM.SetByte(BehaviorCode, value, 2); }
        }

        // The Unit's Movement Orders
        public int MovementOrders {
            get { return BehaviorFSM.GetByte(BehaviorCode, 3); }
            set { BehaviorCode = BehaviorFSM.SetByte(BehaviorCode, value, 3); }
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
                    Data.BBox.Min + WorldPosition,
                    Data.BBox.Max + WorldPosition
                    );
            }
        }

        // Speed Of Movement For The Entity
        public float MovementSpeed {
            get { return Data.MovementSpeed * MovementMultiplier; }
        }
        public float MovementMultiplier {
            get;
            set;
        }

        // How Much Resources This Worker Is Carrying
        public int Resources { get; set; }

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
            Data = data;
            Health = Data.Health;

            // Default Information
            height = 0;
            ViewDirection = Vector2.UnitX;
            CollisionGeometry = Data.ICollidableShape.Clone() as ICollidable;
            MovementMultiplier = 1f;
            Resources = 0;
        }

        // Computes The Damage To Deal With Access To A Random Number And A Target
        public int ComputeDamage(double rand) {
            RTSUnit t = Target as RTSUnit;
            int dmg = Data.BaseCombatData.ComputeDamageDealt(rand);
            if(t != null) dmg = t.Data.BaseCombatData.ComputeDamageReceived(dmg);
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

        public void TurnToFace(Vector2 pos) {
            Vector2 dir = pos - GridPosition;
            float dl = dir.Length();
            if(dl > 0.001) {
                ViewDirection = dir / dl;
            }
        }
    }
}