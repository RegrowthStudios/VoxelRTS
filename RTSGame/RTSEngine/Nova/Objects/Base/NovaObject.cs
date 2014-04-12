using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace NovaLibrary.Object
{
    public abstract class NovaObject : INovaObject
    {
        protected NOVA_TYPE type = NOVA_TYPE.NONE;
        protected Texture2D texture = null;
        protected float textureSize = 1f;
        protected Color color = Color.White;
        protected float rotation = 0f;
        protected float scale = 1f;
        protected float layerDepth = 0f;

        protected Vector2 center = Vector2.One;
        protected Vector2 centerOffset = Vector2.One;
        protected float radius = 1f;
        protected float mass = 1f;

        public NOVA_TYPE Type
        {
            get
            {
                return type;
            }
        }
        public Vector2 Center
        {
            get
            {
                return center;
            }
            set
            {
                center = value;
            }
        }
        public float Rotation
        {
            get
            {
                return rotation;
            }
            set
            {
                rotation = value;
            }
        }
        public float Radius
        {
            get
            {
                return radius;
            }
            set
            {
                radius = value;
                scale = 2f * radius / textureSize;
            }
        }
        public float Mass
        {
            get
            {
                return mass;
            }
            set
            {
                mass = value;
            }
        }

        protected virtual void setTexture(Texture2D t)
        {
            texture = t;
            textureSize = MathHelper.Max(t.Width, t.Height);
            scale = 2f * radius / textureSize;
            centerOffset = new Vector2(
                texture.Width / 2f,
                texture.Height / 2f
                );
        }
        public abstract void Spawn();
        public virtual void draw(SpriteBatch batch)
        {
            batch.Draw(texture, center, null, color, rotation, centerOffset, scale, SpriteEffects.None, layerDepth);
        }

        public object Clone()
        {
            return this.MemberwiseClone();
        }

        public abstract void Update(GameTime time);
        public abstract void OnCollision(NovaObject o);
    }
}
