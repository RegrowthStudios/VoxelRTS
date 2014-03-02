using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;

namespace RTSEngine.Data {
    public class CollisionCircle : ICollidable {

        public CollisionType CollisionType {
            get { throw new NotImplementedException(); }
        }
    }
    public class CollisionRect : ICollidable {

        public CollisionType CollisionType {
            get { throw new NotImplementedException(); }
        }
    }
}