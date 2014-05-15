using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework {
    public interface ITween {
        float GetValue(float a, float b, float t);
    }

    public class TweenLinear : ITween {
        public float GetValue(float a, float b, float t) {
            return t * b + (1 - t) * a;
        }
    }
    public class TweenQuadratic : ITween {
        public float GetValue(float a, float b, float t) {
            t *= t;
            return t * b + (1 - t) * a;
        }
    }
    public class TweenCubic : ITween {
        public float GetValue(float a, float b, float t) {
            t = t * t * t;
            return t * b + (1 - t) * a;
        }
    }
    public class TweenQuartic : ITween {
        public float GetValue(float a, float b, float t) {
            t *= t;
            t *= t;
            return t * b + (1 - t) * a;
        }
    }
    public class TweenEase : ITween {
        public float GetValue(float a, float b, float t) {
            if(t > 0.5) {
                t = 1 - t;
                t *= 2 * t;
                t = 1 - t;
            }
            else {
                t = 2 * t;
            }
            return t * b + (1 - t) * a;
        }
    }

    public static class Tweens {
        public static readonly TweenLinear LINEAR = new TweenLinear();
        public static readonly TweenQuadratic QUADRATIC = new TweenQuadratic();
        public static readonly TweenCubic CUBIC = new TweenCubic();
        public static readonly TweenQuartic QUARTIC = new TweenQuartic();
        public static readonly TweenEase EASE = new TweenEase();
    }

    public static class ITweenExt {
        public static Vector2 GetValue(this ITween c, Vector2 a, Vector2 b, float t) {
            return new Vector2(
                c.GetValue(a.X, b.X, t),
                c.GetValue(a.Y, b.Y, t)
                );
        }
        public static Vector3 GetValue(this ITween c, Vector3 a, Vector3 b, float t) {
            return new Vector3(
                c.GetValue(a.X, b.X, t),
                c.GetValue(a.Y, b.Y, t),
                c.GetValue(a.Z, b.Z, t)
                );
        }
        public static Vector4 GetValue(this ITween c, Vector4 a, Vector4 b, float t) {
            return new Vector4(
                c.GetValue(a.X, b.X, t),
                c.GetValue(a.Y, b.Y, t),
                c.GetValue(a.Z, b.Z, t),
                c.GetValue(a.W, b.W, t)
                );
        }
    }
}