using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data {
    #region CollisionCircle Class
    public class CollisionCircle : ICollidable {
        // Collision Specifications
        public CollisionType CollisionType {
            get { return CollisionType.Circle; }
        }
        public bool IsStatic {
            get;
            set;
        }

        // Location Parameter
        public Vector2 Center {
            get;
            set;
        }

        // Size Parameters
        public float Radius {
            get;
            set;
        }
        public float InnerRadius {
            get { return Radius; }
        }
        public float BoundingRadius {
            get { return Radius; }
        }

        // Constructor
        public CollisionCircle(float r, Vector2 c, bool isStatic = false) {
            Radius = r;
            Center = c;
            IsStatic = IsStatic;
        }

        public object Clone() {
            return new CollisionCircle(Radius, Center, IsStatic);
        }
    }
    #endregion

    #region CollisionRect Class
    public class CollisionRect : ICollidable {
        public const float SQRT_2 = 1.415f;

        // Collision Specifications
        public CollisionType CollisionType {
            get { return CollisionType.Rectangle; }
        }
        public bool IsStatic {
            get;
            set;
        }

        // Location Parameter
        public Vector2 Center {
            get;
            set;
        }

        // Size Parameters
        private Vector2 size;
        public float Width {
            get { return size.X; }
            set { size.X = value; }
        }
        public float Height {
            get { return size.Y; }
            set { size.Y = value; }
        }
        public float InnerRadius {
            get { return Width < Height ? Width : Height; }
        }
        public float BoundingRadius {
            get { return Width == Height ? Width * SQRT_2 : size.Length(); }
        }

        // Constructor
        public CollisionRect(float w, float h, Vector2 c, bool isStatic = false) {
            Width = w;
            Height = h;
            Center = c;
            IsStatic = IsStatic;
        }

        public object Clone() {
            return new CollisionRect(Width, Height, Center, IsStatic);
        }
    }
    #endregion

    public static class CollisionController {
        private static readonly Random r = new Random();
        private const float OFFSET = 0.01f;

        // Process collision between two objects
        // Match the types of two objects before calling HandleCollision()
        public static void ProcessCollision(ICollidable o1, ICollidable o2) {
            switch(o1.CollisionType) {
                case (CollisionType.Circle):
                    switch(o2.CollisionType) {
                        case (CollisionType.Circle):
                            HandleCollision((CollisionCircle)o1, (CollisionCircle)o2);
                            break;
                        case (CollisionType.Rectangle):
                            HandleCollision((CollisionCircle)o1, (CollisionRect)o2);
                            break;
                    }
                    break;
                case (CollisionType.Rectangle):
                    switch(o2.CollisionType) {
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
            if(circle1.IsStatic && circle2.IsStatic) return;

            Vector2 d = circle1.Center - circle2.Center;
            float distance = d.Length();

            // If two circles collide
            if(circle1.Radius + circle2.Radius > distance) {
                if(distance == 0) {
                    d.X = r.Next(-200, 201);
                    d.Y = r.Next(-200, 201);
                    if(d != Vector2.Zero)
                        d.Normalize();
                }
                else {
                    d /= distance;
                }

                // Static objects do not move, so only move back the non-static one
                if(circle1.IsStatic) {
                    d *= -1;
                    float distToPush = circle1.Radius + circle2.Radius - distance + OFFSET;
                    circle2.Center += distToPush * d;
                }
                else if(circle2.IsStatic) {
                    Vector2 direction = d / distance;
                    float distToPush = circle1.Radius + circle2.Radius - distance + OFFSET;
                    circle1.Center += distToPush * d;
                }
                // If both objects are non-static, move them both back evenly
                else {
                    float distToPush = (circle1.Radius + circle2.Radius - distance) / 2 + OFFSET;
                    circle1.Center += distToPush * d;
                    circle2.Center -= distToPush * d;
                }
            }
        }

        // Detect circle-rectangle collision
        private static void HandleCollision(CollisionCircle circle, CollisionRect rect) {
            // Reference: stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection/402010#402010
            float dx = circle.Center.X - rect.Center.X;
            float dy = circle.Center.Y - rect.Center.Y;
            float cornerDistSqr = (float)(Math.Pow(dx - rect.Width / 2, 2) + Math.Pow(dy - rect.Height / 2, 2));

            // If circle and rectangle collide
            if(dx <= rect.Width / 2 ||
                dy <= rect.Height / 2 ||
                cornerDistSqr <= circle.Radius * circle.Radius) {
                // Create a collision boundary around the rectangle, in which collision occurs
                float top = rect.Center.Y + rect.Height / 2 + circle.Radius;
                float bottom = rect.Center.Y - rect.Height / 2 - circle.Radius;
                float left = rect.Center.X - rect.Width / 2 - circle.Radius;
                float right = rect.Center.X + rect.Width / 2 + circle.Radius;
                float distToTop = Math.Abs(circle.Center.Y - top);
                float distToBottom = Math.Abs(circle.Center.Y - bottom);
                float distToLeft = Math.Abs(circle.Center.X - left);
                float distToRight = Math.Abs(circle.Center.X - right);

                // Choose the closest distance to the collision boundary as the pushing direction
                float min = Math.Min(Math.Min(Math.Min(distToTop, distToBottom), distToLeft), distToRight);
                Vector2 pushAmount = new Vector2(); // How much should be pushed relative to circle
                pushAmount.Y += min == distToTop ? distToTop + OFFSET : 0;
                pushAmount.Y += min == distToBottom ? -distToBottom - OFFSET : 0;
                pushAmount.X += min == distToLeft ? -distToLeft - OFFSET : 0;
                pushAmount.X += min == distToRight ? distToRight + OFFSET : 0;

                // Only move the non-static object
                if(rect.IsStatic)
                    circle.Center += pushAmount;
                else if(circle.IsStatic)
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
            float bottom1 = rect1.Center.Y - rect1.Height / 2;
            float bottom2 = rect2.Center.Y - rect2.Height / 2;
            float top1 = rect1.Center.Y + rect1.Height / 2;
            float top2 = rect2.Center.Y + rect2.Height / 2;
            float left1 = rect1.Center.X - rect1.Width / 2;
            float left2 = rect2.Center.X - rect2.Width / 2;
            float right1 = rect1.Center.X + rect1.Width / 2;
            float right2 = rect2.Center.X + rect2.Width / 2;

            // If two rectangles collide
            if(top1 > bottom2 && top2 > bottom1 && left1 < right2 && left2 < right1) {
                Vector2 pushAmount = new Vector2();
                Vector2 dir = rect2.Center - rect1.Center;
                // If rect2 is on the right of rect1
                if(dir.X > 0) {
                    // If rect2 is below rect1
                    if(dir.Y < 0) {
                        if(right1 - left2 > top2 - bottom1)
                            pushAmount.Y = top2 - bottom1 + OFFSET; // rect1 pushed up
                        else
                            pushAmount.X = left2 - right1 - OFFSET; // rect1 pushed left
                    }
                    // If rect2 is above rect1
                    else {
                        if(right1 - left2 > top1 - bottom2)
                            pushAmount.Y = bottom2 - top1 - OFFSET; // rect1 pushed down
                        else
                            pushAmount.X = left2 - right1 - OFFSET; // rect1 pushed left
                    }
                }
                // If rect2 is on the left of rect1
                else {
                    // If rect2 is below rect1
                    if(dir.Y < 0) {
                        if(left1 - right2 > top2 - bottom1)
                            pushAmount.Y = top2 - bottom1 + OFFSET; // rect1 pushed up
                        else
                            pushAmount.X = right2 - left1 + OFFSET; // rect1 pushed right
                    }
                    // If rect2 is above rect1
                    else {
                        if(right2 - left1 > top1 - bottom2)
                            pushAmount.Y = bottom2 - top1 - OFFSET; // rect1 pushed down
                        else
                            pushAmount.X = right2 - left1 + OFFSET; // rect1 pushed right
                    }
                }
                // Push apart rectangles that are non-static
                if(rect1.IsStatic)
                    rect2.Center -= pushAmount;
                else if(rect2.IsStatic)
                    rect1.Center += pushAmount;
                else {
                    rect1.Center += pushAmount / 2;
                    rect2.Center -= pushAmount / 2;
                }
            }
        }
    }
}