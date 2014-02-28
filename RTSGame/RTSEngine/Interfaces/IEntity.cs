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

    public interface IEntity {
        public float X { get; }
        public float Y { get; }

        public float Radius { get; }

        // Updates Whatever It Needs To Update On A Frame
        void Update(float dt);
    }
}