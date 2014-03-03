using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Interfaces {

    public enum CollisionType {
        Circle = 1,
        Rectangle = 2
    }
    public interface ICollidable : ICloneable {
        CollisionType CollisionType { get; }

        Vector2 Center{ get; set;}

        bool IsStatic { get; }
    }
}
