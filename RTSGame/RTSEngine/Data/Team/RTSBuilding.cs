using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSBuilding : IEntity, ImpactGenerator {
        public static void Serialize(BinaryWriter s, RTSBuilding e) {
            s.Write(e.BuildingData.Index);
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
            if(e.ActionController != null) {
                s.Write(true);
                // TODO: Custom Serialize
            }
            else {
                s.Write(false);
            }
        }
        public static RTSBuilding Deserialize(BinaryReader s, GameState state) {
            // TODO: Implement
            return null;
        }

        // Common Data
        public RTSBuildingData BuildingData {
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
                    BuildingData.BBox.Min + WorldPosition,
                    BuildingData.BBox.Max + WorldPosition
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

        public event Action<Vector2, int> GenerateImpact;

        // Constructor
        public RTSBuilding(RTSTeam team, RTSBuildingData data, Vector2 position) {
            // Identification
            UUID = UUIDGenerator.GetUUID();
            Team = team;
            gridPos = position;

            BuildingData = data;
            height = 0;
            Health = BuildingData.Health;
            CollisionGeometry = BuildingData.ICollidableShape.Clone() as ICollidable;
            ViewDirection = Vector2.UnitX;
            CollisionGeometry.Center += GridPosition;
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
    }
}