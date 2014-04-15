using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NovaLibrary.Object
{
    public interface INovaMotion
    {
        Vector2 Velocity { get; set; }

        void accelerate(Vector2 vAdd);
        void accelerate(Vector2 acc, float dTime);
        void Move(float dTime);
    }
}
