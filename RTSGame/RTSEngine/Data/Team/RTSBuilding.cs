using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSBuilding : IEntity {
        # region Properties
        public RTSBuildingData BuildingData { get; private set; }
        // Unique ID
        public int UUID {
            get;
            private set;
        }

        // This Unit's Current Health
        public int Health { get; private set; }

        public RTSTeam Team { get; private set; }

        // Activity Of This Building
        public Queue<RTSUnit> UnitQueue { get; private set; }

        // Event Triggered When This Entity Receives Damage
        public event Action<IEntity, int> OnDamage;

        // Destruction Event
        public event Action<IEntity> OnDestruction;

        public bool IsAlive {
            get { return Health > 0; }
            set {
                if(!value)
                    Destroy();
                else if(!IsAlive)
                    throw new InvalidOperationException("Cannot Bring Back Units From The Dead");
            }
        }

        private ACBuildingActionController aController;
        public ACBuildingActionController ActionController {
            get { return aController; }
            set {
                aController = value;
                if(aController != null) aController.setBuilding(this);
            }
        }
        #endregion

        #region Spacial Properties
        // Collision Geometry
        public ICollidable CollisionGeometry {
            get;
            private set;
        }
        public BoundingBox BBox {
            get {
                return new BoundingBox(
                    BuildingData.BBox.Min + WorldPosition,
                    BuildingData.BBox.Max + WorldPosition
                    );
            }
        }

        // 2-D Position Of The Building
        private Vector2 gridPos;
        public Vector2 GridPosition {
            get { return gridPos; }
            set { gridPos = value; }
        }

        // 3-D Position Of The Building
        private float height;
        public float Height {
            get { return height; }
            set { height = value; }
        }
        public Vector3 WorldPosition {
            get { return new Vector3(gridPos.X, height, gridPos.Y); }
        }

        // Target Of The Building
        protected Vector2 targetPos;
        public Vector2 TargetPos {
            get { return targetPos; }
            set { targetPos = value; }
        }
        #endregion

        // Constructor
        public RTSBuilding(RTSTeam team, RTSBuildingData data, Vector2 position) {
            Team = team;
            BuildingData = data;
            gridPos = position;
            height = 0;
            Health = BuildingData.Health;
            CollisionGeometry = BuildingData.ICollidableShape.Clone() as ICollidable;
            UnitQueue = new Queue<RTSUnit>();
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

        // Enqueue New Unit
        public void EnqueueUnit(RTSUnit unit) {
            UnitQueue.Enqueue(unit);
        }

        // TODO: Implement Missing Interface Elements
        #region AUTO-IMPLEMENTED MISSING INTERFACE ELEMENTS
        public IEntity Target {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public event Action<IEntity, IEntity> OnNewTarget;

        ACUnitActionController IEntity.ActionController {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public ACUnitAnimationController AnimationController {
            get {
                throw new NotImplementedException();
            }
            set {
                throw new NotImplementedException();
            }
        }

        public int State {
            get;
            set;
        }
        #endregion
    }
}
