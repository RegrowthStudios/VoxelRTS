using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSBuilding : IEntity {
        public static void Serialize(BinaryWriter s, RTSBuilding e) {
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
            for(int i = 0; i < GameState.MAX_PLAYERS; i++) {
                s.Write(e.viewedInfo.Get(i));
            }
            if(e.ActionController != null) {
                s.Write(true);
                e.ActionController.Serialize(s);
            }
            else {
                s.Write(false);
            }
        }
        public static RTSBuilding Deserialize(BinaryReader s, RTSTeam team, out int? target) {
            int type = s.ReadInt32();
            RTSBuilding e = team.AddBuilding(type, Vector2.Zero);
            if(e == null) throw new Exception("Could Not Create A Building That Was Previously Created");
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
            for(int i = 0; i < GameState.MAX_PLAYERS; i++) {
                e.viewedInfo.Set(i, s.ReadBoolean());
            }
            if(s.ReadBoolean()) {
                if(e.ActionController != null) e.ActionController.Deserialize(s);
            }
            else {
                e.ActionController = null;
            }
            return e;
        }

        // Common Data
        public RTSBuildingData Data {
            get;
            private set;
        }

        // Unique ID
        public int UUID {
            get;
            private set;
        }

        // Building's Team
        public RTSTeam Team {
            get;
            private set;
        }

        // State Information
        public int State {
            get;
            set;
        }
        private BitArray viewedInfo;

        // View Direction
        public Vector2 ViewDirection {
            get;
            private set;
        }

        // 2D Position
        private Vector2 gridPos;
        public Vector2 GridPosition {
            get { return gridPos; }
            set {
                gridPos = value;
                CollisionGeometry.Center = Data.ICollidableShape.Center + gridPos;
            }
        }
        public Vector2 GridStartPos {
            get {
                Vector2 gs = GridPosition;
                gs.X -= (Data.GridSize.X - 1);
                gs.Y -= (Data.GridSize.Y - 1);
                return gs;
            }
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

        // This Building's Current Health
        public int Health {
            get;
            set;
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

        // Is It A Resource?
        public bool IsResource {
            get { return Data.IsResource; }
        }

        // Building Information
        private int bAmount;
        public int BuildAmountLeft {
            get { return bAmount; }
            set {
                if(!IsBuilt) {
                    bAmount = value;
                    if(IsBuilt && OnBuildingFinished != null) {
                        OnBuildingFinished(this);
                    }
                }
            }
        }
        public bool IsBuilt {
            get { return BuildAmountLeft <= 0; }
        }
        private float BuildRatio {
            get { return IsBuilt ? 1f : ((float)(Data.BuildAmount - BuildAmountLeft) / (float)Data.BuildAmount); }
        }

        // Damaging Events
        public event Action<IEntity, int> OnDamage;
        public event Action<IEntity> OnDestruction;
        public event Action<RTSBuilding> OnBuildingFinished;
        public event Action<RTSUnit> OnUnitSpawn;

        public void SpawnUnit(RTSUnit u) {
            OnUnitSpawn(u);
        }

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

        private ACBuildingActionController aController;
        public ACBuildingActionController ActionController {
            get { return aController; }
            set {
                aController = value;
                if(aController != null) aController.SetBuilding(this);
            }
        }
        private List<ACBuildingButtonController> bControllers;
        public List<ACBuildingButtonController> ButtonControllers {
            get { return bControllers; }
        }

        // Constructor
        public RTSBuilding(RTSTeam team, RTSBuildingData data, Vector2 position) {
            // Identification
            UUID = UUIDGenerator.GetUUID();
            Team = team;
            gridPos = position;
            viewedInfo = new BitArray(GameState.MAX_PLAYERS);
            viewedInfo.SetAll(false);

            Data = data;
            gridPos.X += (Data.GridSize.X - 1);
            gridPos.Y += (Data.GridSize.Y - 1);
            height = 0;
            Health = Data.Health;
            bAmount = Data.BuildAmount;
            CollisionGeometry = Data.ICollidableShape.Clone() as ICollidable;
            ViewDirection = Vector2.UnitX;
            CollisionGeometry.Center += GridPosition;
            bControllers = new List<ACBuildingButtonController>();
        }

        public void SetViewedInfo(int p, bool b) {
            viewedInfo.Set(p, b);
        }
        public bool GetViewedInfo(int p) {
            return viewedInfo.Get(p);
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

        public void AddButton(ACBuildingButtonController b) {
            if(b != null) b.SetBuilding(this);
            bControllers.Add(b);
        }
    }
}