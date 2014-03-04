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
        // Circle Or Rectangle?
        CollisionType CollisionType { get; }

        // The Center Of The Geometry
        Vector2 Center { get; set; }

        // The Encapsulating Radii For The Geometry
        float InnerRadius { get; }
        float BoundingRadius { get; }

        // Collision Controller Won't Move The Geometry If It Is Static
        bool IsStatic { get; }
    }
}