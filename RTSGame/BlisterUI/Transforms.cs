using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlisterUI {
    public struct WidgetFrame {
        #region Static
        /// <summary>
        /// The Identity Transformation
        /// </summary>
        public static WidgetFrame Identity {
            get { return new WidgetFrame(Vector2.Zero, 0, Vector2.One, 0); }
        }

        /// <summary>
        /// Transformation Multiplication (Non-Commutative)
        /// </summary>
        /// <param name="baseFrame">The Parent Transformation</param>
        /// <param name="localFrameCoords">The Transformation To Be Placed</param>
        /// <returns>Places A Transformation Into Another</returns>
        public static WidgetFrame Multiply(WidgetFrame baseFrame, WidgetFrame localFrameCoords) {
            return new WidgetFrame(
                baseFrame.Position
                    + baseFrame.Scaling.X * baseFrame.LocalUnitX * localFrameCoords.Position.X
                    + baseFrame.Scaling.Y * baseFrame.LocalUnitY * localFrameCoords.Position.Y,
                baseFrame.Depth + localFrameCoords.Depth,
                baseFrame.Scaling * localFrameCoords.Scaling,
                baseFrame.Rotation + localFrameCoords.Rotation
                );
        }
        public static WidgetFrame operator *(WidgetFrame baseFrame, WidgetFrame localFrameCoords) {
            return new WidgetFrame(
                baseFrame.Position
                    + baseFrame.Scaling.X * baseFrame.LocalUnitX * localFrameCoords.Position.X
                    + baseFrame.Scaling.Y * baseFrame.LocalUnitY * localFrameCoords.Position.Y,
                baseFrame.Depth + localFrameCoords.Depth,
                baseFrame.Scaling * localFrameCoords.Scaling,
                baseFrame.Rotation + localFrameCoords.Rotation
                );
        }
        /// <summary>
        /// Point Transformation
        /// </summary>
        /// <param name="p">A Point In The Identity Frame</param>
        /// <param name="frame">A Transformed Set Of Coordinate Axes</param>
        /// <returns>A Point Transformed Into The Specified Frame</returns>
        public static Vector2 Transform(Vector2 p, WidgetFrame frame) {
            return frame.Position + frame.Scaling.X * frame.LocalUnitX * p.X + frame.Scaling.Y * frame.LocalUnitY * p.Y;
        }
        public static Vector2 TransformNormal(Vector2 n, WidgetFrame frame) {
            return frame.Scaling.X * frame.LocalUnitX * n.X + frame.Scaling.Y * frame.LocalUnitY * n.Y;
        }
        /// <summary>
        /// Inverse Point Transformation
        /// </summary>
        /// <param name="p">A Point In The Specified Frame</param>
        /// <param name="frame">A Transformed Set Of Coordinate Axes</param>
        /// <returns>A Point Transformed Into The Identity Frame</returns>
        public static Vector2 InvTransform(Vector2 p, WidgetFrame frame) {
            Vector2 lux = frame.LocalUnitX;
            Vector2 luy = frame.LocalUnitY;

            float y = (lux.X * (p.Y - frame.Position.Y) - lux.Y * (p.X - frame.Position.X)) / (frame.Scaling.Y * (lux.X * luy.Y - luy.X * lux.Y));
            float x = (p.X - frame.Position.X - frame.Scaling.Y * luy.X * y) / (frame.Scaling.X * lux.X);

            return new Vector2(x, y);
        }
        #endregion

        // Translational Transformation
        public Vector2 Position;
        public float Depth;

        // Linear Transformation
        public Vector2 Scaling;
        public float Rotation;

        /// <summary>
        /// Direction Of The Rotated X-Axis
        /// </summary>
        public Vector2 LocalUnitX {
            get {
                return new Vector2(
                    (float)Math.Cos(Rotation),
                    (float)Math.Sin(Rotation)
                    );
            }
        }
        /// <summary>
        /// Direction Of The Rotated Y-Axis
        /// </summary>
        public Vector2 LocalUnitY {
            get {
                return new Vector2(
                    (float)-Math.Sin(Rotation),
                    (float)Math.Cos(Rotation)
                    );
            }
        }

        // All Of The Different Initializers
        public WidgetFrame(Vector2 p, float d, Vector2 s, float r) {
            Position = p;
            Depth = d;
            Scaling = s;
            Rotation = r;
        }
        public WidgetFrame(Vector2 p, float d, Vector2 s) : this(p, d, s, 0) { }
        public WidgetFrame(Vector2 p, Vector2 s, float r) : this(p, 1, s, r) { }
        public WidgetFrame(Vector2 p, float d, float r) : this(p, d, Vector2.One, r) { }
        public WidgetFrame(Vector2 p, float d) : this(p, d, Vector2.One, 0) { }
    }

    public class WidgetTransform {
        public WidgetFrame Frame;
        public Vector2 RectSize;
    }
}
