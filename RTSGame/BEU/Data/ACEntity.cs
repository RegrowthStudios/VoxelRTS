using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEU.Physics;
using Microsoft.Xna.Framework;

namespace BEU.Data {
    public abstract class ACEntity {
        private static int curUUID = 0;

        // Entity's Unique ID
        public int UUID {
            get;
            private set;
        }

        // Entity's Data
        public abstract ACEntityData EntityData {
            get;
            protected set;
        }

        // The Current Health Of The Entity
        private int health;
        public int Health {
            get { return health; }
            protected set {
                health = value;
                if(!IsAlive)
                    Destroy();
            }
        }

        // Whether It Is Alive Or Not
        public bool IsAlive {
            get { return Health > 0; }
            set {
                if(!value) {
                    Health = 0;
                }
            }
        }
        public bool IsDestroyed {
            get;
            private set;
        }

        // Location In The World
        public Vector2 Position {
            get;
            set;
        }

        // View Direction
        public Vector2 ViewDirection {
            get;
            protected set;
        }

        // Collision Geometry
        public ICollidable CollisionGeometry {
            get;
            protected set;
        }

        // Events
        public event Action<ACEntity, int> OnDamage;
        public event Action<ACEntity> OnDestruction;

        // NOTE: Not Thread-safe, And Will Not Check Population
        public ACEntity(ACEntityData data) {
            // Set Data
            EntityData = data;
            
            // Increment Population
            EntityData.Population++;

            // Give Health
            Health = EntityData.OriginHealth;
            IsDestroyed = false;

            // Give A UUID
            curUUID++;
            UUID = curUUID;

            // Default Position
            Position = Vector2.Zero;
            ViewDirection = Vector2.UnitX;

            // Geometry
            CollisionGeometry = EntityData.ICollidableShape.Clone() as ICollidable;
        }

        // Calls The Destruction Event
        public void Destroy() {
            if(IsDestroyed) return;
            IsDestroyed = true;
            EntityData.Population--;
            health = 0;
            if(OnDestruction != null)
                OnDestruction(this);
        }

        // Applies Damage To Health
        void Damage(int d) {
            Health -= d;
            if(OnDamage != null)
                OnDamage(this, d);
        }
    }
}