﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BEU.Physics {
    public enum CollisionType {
        Circle = 1,
        Rectangle = 2
    }
    public interface ICollidable : ICloneable {
        // Circle Or Rectangle?
        CollisionType CollisionType { get; }

        // The Center Of The Geometry
        Vector2 Center { get; set; }
        float Height { get; set; }

        // The Encapsulating Radii For The Geometry
        float InnerRadius { get; }
        float BoundingRadius { get; }

        // Collision Controller Won't Move The Geometry If It Is Static
        bool IsStatic { get; set; }
    }

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
        public float Height {
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
            IsStatic = isStatic;
            Height = 0;
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
        public float Height {
            get;
            set;
        }

        // Size Parameters
        private Vector2 size;
        public float Width {
            get { return size.X; }
            set { size.X = value; }
        }
        public float Depth {
            get { return size.Y; }
            set { size.Y = value; }
        }
        public float InnerRadius {
            get { return Width < Depth ? Width * 0.5f : Depth * 0.5f; }
        }
        public float BoundingRadius {
            get { return Width == Depth ? Width * SQRT_2 * 0.5f : size.Length() * 0.5f; }
        }

        // Constructor
        public CollisionRect(float w, float h, Vector2 c, bool isStatic = false) {
            Width = w;
            Depth = h;
            Center = c;
            IsStatic = isStatic;
            Height = 0;
        }

        public object Clone() {
            return new CollisionRect(Width, Depth, Center, IsStatic);
        }
    }
    #endregion

    public static class CollisionController {
        private static readonly Random r = new Random();
        private const float OFFSET = 0.01f;

        // Process Collision Between Two Objects
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
        private static void HandleCollision(CollisionCircle circle, CollisionRect rect) {
            if(circle.IsStatic && rect.IsStatic)
                return;

            // Reference: stackoverflow.com/questions/401847/circle-rectangle-collision-detection-intersection/402010#402010
            Vector2 d = circle.Center - rect.Center;
            d = new Vector2(Math.Abs(d.X), Math.Abs(d.Y));
            Vector2 cd = new Vector2(d.X - rect.Width * 0.5f, d.Y - rect.Depth * 0.5f);
            float cornerDistSqr = cd.LengthSquared();

            // If circle and rectangle collide
            if(d.X <= rect.Width / 2 + circle.Radius && d.Y <= rect.Depth / 2 + circle.Radius
                || cornerDistSqr <= circle.Radius * circle.Radius) {
                // If circle and rectangle centers completely overlap,
                // slightly move one of the object's center so they don't completely overlap
                if(d.Length() == 0) {
                    d.X = r.Next(-200, 201);
                    d.Y = r.Next(-200, 201);
                    if(d != Vector2.Zero) {
                        d.Normalize();
                        d *= 0.1f;
                    }
                    if(circle.IsStatic)
                        rect.Center += d;
                    else
                        circle.Center += d;
                    HandleCollision(circle, rect);
                }

                // Create a collision boundary around the rectangle, in which collision occurs
                float top = rect.Center.Y + rect.Depth / 2 + circle.Radius;
                float bottom = rect.Center.Y - rect.Depth / 2 - circle.Radius;
                float left = rect.Center.X - rect.Width / 2 - circle.Radius;
                float right = rect.Center.X + rect.Width / 2 + circle.Radius;
                float distToTop = Math.Abs(circle.Center.Y - top);
                float distToBottom = Math.Abs(circle.Center.Y - bottom);
                float distToLeft = Math.Abs(circle.Center.X - left);
                float distToRight = Math.Abs(circle.Center.X - right);

                // Choose the closest distance to the collision boundary as the pushing direction
                Vector2 pushAmount = new Vector2(); // How much should be pushed relative to circle
                float min = Math.Min(Math.Min(Math.Min(distToTop, distToBottom), distToLeft), distToRight);
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
        private static void HandleCollision(CollisionRect rect1, CollisionRect rect2) {
            if(rect1.IsStatic && rect2.IsStatic)
                return;

            // bottom1 means the bottom Y coordinate of rect1
            // left2 means the left X coordinate of rect2
            float bottom1 = rect1.Center.Y - rect1.Depth / 2;
            float bottom2 = rect2.Center.Y - rect2.Depth / 2;
            float top1 = rect1.Center.Y + rect1.Depth / 2;
            float top2 = rect2.Center.Y + rect2.Depth / 2;
            float left1 = rect1.Center.X - rect1.Width / 2;
            float left2 = rect2.Center.X - rect2.Width / 2;
            float right1 = rect1.Center.X + rect1.Width / 2;
            float right2 = rect2.Center.X + rect2.Width / 2;

            // If two rectangles collide
            if(top1 > bottom2 && top2 > bottom1 && left1 < right2 && left2 < right1) {
                // If two rectangle centers completely overlap,
                // slightly move one of the object's center so they don't completely overlap
                Vector2 d = rect2.Center - rect1.Center;
                if(d.Length() == 0) {
                    d.X = r.Next(-200, 201);
                    d.Y = r.Next(-200, 201);
                    if(d != Vector2.Zero) {
                        d.Normalize();
                        d *= 0.1f;
                    }
                    if(rect1.IsStatic)
                        rect2.Center += d;
                    else
                        rect1.Center += d;
                    HandleCollision(rect1, rect2);
                }

                Vector2 pushAmount = new Vector2();
                // If rect2 is on the right of rect1
                if(d.X > 0) {
                    // If rect2 is below rect1
                    if(d.Y < 0) {
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
                    if(d.Y < 0) {
                        if(right2 - left1 > top2 - bottom1)
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