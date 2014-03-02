using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data
{
    #region CollisionCircle Class
    public class CollisionCircle : ICollidable {

        public CollisionType type;
        public float radius;
        public Vector2 location;

        public CollisionType CollisionType {
            get { return type; }
        }

        // Constructor
        public CollisionCircle(float radius, Vector2 location){
            type = CollisionType.Circle;
            this.radius = radius;
            this.location = location;
        }
    }
    #endregion

    #region CollisionRect Class
        public class CollisionRect : ICollidable {

            public CollisionType type;
            public float width;
            public float height;
            public Vector2 location;

            public CollisionType CollisionType {
                get { return type; }
            }

            // Constructor
            public CollisionRect(float width, float height, Vector2 location) {
                type = CollisionType.Rectangle;
                this.width = width;
                this.height = height;
                this.location = location;
            }
        }
    #endregion

    #region CollisionController Class
        public class CollisionController {

            // Constructor
            public CollisionController() { }

            // Process collision between two objects
            // Match the types of two objects before calling HandleCollision()
            public void ProcessCollision(ICollidable o1, ICollidable o2) {
                switch (o1.CollisionType) {
                    case (CollisionType.Circle):
                        switch (o2.CollisionType) {
                            case (CollisionType.Circle):
                                HandleCollision((CollisionCircle)o1, (CollisionCircle)o2);
                                break;
                            case (CollisionType.Rectangle):
                                HandleCollision((CollisionCircle)o1, (CollisionRect)o2);
                                break;
                        }
                        break;
                    case (CollisionType.Rectangle):
                        switch (o2.CollisionType){
                            case (CollisionType.Circle):
                                HandleCollision((CollisionCircle)o2, (CollisionRect)o1);
                                break;
                            case (CollisionType.Rectangle):
                                HandleCollision((CollisionRect)o1, (CollisionRect)o2);
                                break;
                        }
                        break;
                }
            }

            protected void HandleCollision(CollisionCircle circle1, CollisionCircle circle2) {

            }

            protected void HandleCollision(CollisionCircle circle, CollisionRect rect) {

            }
            protected void HandleCollision(CollisionRect rect1, CollisionRect rect2) {

            }
        }

    #endregion
}