using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NovaLibrary.Object
{
    public abstract class NovaMovingObject : NovaObject, INovaMotion
    {
        protected Vector2 velocity = Vector2.Zero;
        protected float friction = 1f;
        public Vector2 Velocity
        {
            get
            {
                return velocity;
            }
            set
            {
                velocity = value;
            }
        }

        public void accelerate(Vector2 vAdd)
        {
            Velocity += vAdd;
        }
        public void accelerate(Vector2 acc, float dTime)
        {
            accelerate(acc * dTime);
        }
        public virtual void Move(float dTime)
        {
            Center += velocity;
            velocity /= friction;
        }
    }
}
