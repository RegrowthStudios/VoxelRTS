using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Interfaces {
    public enum CollisionType {
        Circle = 1,
        Rectangle = 2
    }
    public interface ICollidable {
        CollisionType CollisionType { get; }
    }
}
