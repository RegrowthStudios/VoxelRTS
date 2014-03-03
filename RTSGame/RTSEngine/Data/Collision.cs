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

        protected CollisionType type;
        public float radius;
        private Vector2 center;
        private bool isStatic;

        public CollisionType CollisionType {
            get { return type; }
        }

        public Vector2 Center {
            get { return center; }
            set { center = value; }
        }

        public bool IsStatic {
            get { return isStatic; }
        }

        // Constructor
        public CollisionCircle(float radius, Vector2 center){
            type = CollisionType.Circle;
            this.radius = radius;
            this.center = center;
        }
    }
    #endregion

    #region CollisionRect Class
        public class CollisionRect : ICollidable {

            private CollisionType type;
            public float width;
            public float height;
            private bool isStatic;
            private Vector2 center;

            public CollisionType CollisionType {
                get { return type; }
            }

            public Vector2 Center {
                get { return center;}
                set { center = value; }
            }

            public bool IsStatic {
                get { return isStatic; }
            }

            // Constructor
            public CollisionRect(float width, float height, Vector2 center) {
                type = CollisionType.Rectangle;
                this.width = width;
                this.height = height;
                this.center = center;
            }
        }
    #endregion

    #region CollisionController Class
        public static class CollisionController {

            private const float OFFSET = 0.01f;

            // Process collision between two objects
            // Match the types of two objects before calling HandleCollision()
            public static void ProcessCollision(ICollidable o1, ICollidable o2) {
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

            // Detect circle-circle collision
            private static void HandleCollision(CollisionCircle circle1, CollisionCircle circle2) {
                float dx = circle1.Center.X - circle2.Center.X;
                float dy = circle1.Center.Y - circle2.Center.Y;
                // How far apart two circles are
                float distance = (float)Math.Sqrt(dx * dx + dy * dy);
                // How much two circles are overlapping
                float overlapDist = circle1.radius + circle2.radius - distance;

                // If two circles collide
                if (circle1.radius + circle2.radius > distance) {
                    // Static objects do not move, so only move back the non-static one
                    if (circle1.IsStatic) {
                        Vector2 direction = circle2.Center - circle1.Center;
                        direction.Normalize();
                        float distToPush = circle1.radius + circle2.radius - distance + OFFSET;
                        circle2.Center += distToPush * direction;
                    }
                    else if (circle2.IsStatic) {
                        Vector2 direction = circle1.Center - circle2.Center;
                        direction.Normalize();
                        float distToPush = circle1.radius + circle2.radius - distance + OFFSET;
                        circle1.Center += distToPush * direction;
                    }
                    // If both objects are non-static, move them both back evenly
                    else {
                        Vector2 direction = circle1.Center - circle2.Center;
                        direction.Normalize();
                        float distToPush = (circle1.radius + circle2.radius - distance) / 2 + OFFSET;
                        circle1.Center += distToPush * direction;
                        circle2.Center -= distToPush * direction;
                    }
                }
            }

            // Detect circle-rectangle collision
            private static void HandleCollision(CollisionCircle circle, CollisionRect rect) {
                // Reference: stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection/402010#402010
                float dx = circle.Center.X - rect.Center.X;
                float dy = circle.Center.Y - rect.Center.Y;
                float cornerDistSqr = (float)(Math.Pow(dx - rect.width/2,2) + Math.Pow(dy-rect.height/2,2));

                // If circle and rectangle collide
                if (dx <= rect.width / 2 ||
                    dy <= rect.height / 2 ||
                    cornerDistSqr <= circle.radius * circle.radius) {
                    // Create a collision boundary around the rectangle, in which collision occurs
                    float top = rect.Center.Y + rect.height / 2 + circle.radius;
                    float bottom = rect.Center.Y - rect.height / 2 - circle.radius;
                    float left = rect.Center.X - rect.width / 2 - circle.radius;
                    float right = rect.Center.X + rect.width / 2 + circle.radius;
                    float distToTop = Math.Abs(circle.Center.Y - top);
                    float distToBottom = Math.Abs(circle.Center.Y - bottom);
                    float distToLeft = Math.Abs(circle.Center.X - left);
                    float distToRight = Math.Abs(circle.Center.X - right);

                    // Choose the closest distance to the collision boundary as the pushing direction
                    float min = Math.Min(Math.Min(Math.Min(distToTop, distToBottom), distToLeft), distToRight);
                    Vector2 pushAmount = new Vector2(); // How much should be pushed relative to circle
                    pushAmount.Y = min == distToTop ? distToTop : 0;
                    pushAmount.Y = min == distToBottom ? -distToBottom : 0;
                    pushAmount.X = min == distToLeft ? -distToLeft : 0;
                    pushAmount.X = min == distToRight ? distToRight : 0;

                    // Only move the non-static object
                    if (rect.IsStatic)
                        circle.Center += pushAmount;
                    else if (circle.IsStatic)
                        rect.Center -= pushAmount;
                    else {
                        circle.Center += pushAmount / 2;
                        rect.Center -= pushAmount / 2;
                    }
                }
            }

            // Detect rectangle-rectangle collision
            private static void HandleCollision(CollisionRect rect1, CollisionRect rect2) {
                // bottom1 means the bottom Y coordinate of rect1
                // left2 means the left X coordinate of rect2
                float bottom1 = rect1.Center.Y - rect1.height/2;
                float bottom2 = rect2.Center.Y - rect2.height/2;
                float top1 = rect1.Center.Y + rect1.height/2;
                float top2 = rect2.Center.Y + rect2.height/2;
                float left1 = rect1.Center.X - rect1.width/2;
                float left2 = rect2.Center.X - rect2.width/2;
                float right1 = rect1.Center.X + rect1.width/2;
                float right2 = rect2.Center.X + rect2.width/2;

                // If two rectangles collide
                if (top1 > bottom2 && top2 > bottom1 && left1 < right2 && left2 < right1) {
                    Vector2 pushAmount = new Vector2();
                    Vector2 dir = rect2.Center - rect1.Center;
                    // If rect2 is on the right of rect1
                    if (dir.X > 0){
                        // If rect2 is below rect1
                        if (dir.Y < 0){
                            if (right1 - left2 > top2 - bottom1)
                                pushAmount.Y = top2 - bottom1; // rect1 pushed up
                            else
                                pushAmount.X = left2 - right1; // rect1 pushed left
                        }
                        // If rect2 is above rect1
                        else {
                            if (right1 - left2 > top1 - bottom2)
                                pushAmount.Y = bottom2 - top1; // rect1 pushed down
                            else
                                pushAmount.X = left2 - right1; // rect1 pushed left
                        }
                    }
                    // If rect2 is on the left of rect1
                    else {
                        // If rect2 is below rect1
                        if (dir.Y < 0){
                            if (left1 - right2 > top2 - bottom1)
                                pushAmount.Y = top2 - bottom1; // rect1 pushed up
                            else 
                                pushAmount.X = right2 - left1; // rect1 pushed right
                        }
                        // If rect2 is above rect1
                        else {
                            if (right2 - left1 > top1 - bottom2)
                                pushAmount.Y = bottom2 - top1; // rect1 pushed down
                            else
                                pushAmount.X = right2 - left1; // rect1 pushed right
                        }
                    }
                    // Push apart rectangles that are non-static
                    if (rect1.IsStatic)
                        rect2.Center -= pushAmount;
                    else if (rect2.IsStatic)
                        rect1.Center += pushAmount;
                    else {
                        rect1.Center += pushAmount / 2;
                        rect2.Center -= pushAmount / 2;
                    }      
                }
            }
        }

    #endregion
}