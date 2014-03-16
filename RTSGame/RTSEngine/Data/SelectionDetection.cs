using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data {
    #region Detection Objects Generated From A Camera
    public struct Frustum {
        public readonly BoundingFrustum frustum;

        public Frustum(Matrix mView, Matrix mProj, Vector2 min, Vector2 max) {
            // Transform Projection Matrix
            Vector2 s = (max - min) * 0.5f;
            Matrix ms = Matrix.CreateScale(1f / s.X, 1f / s.Y, 1);
            Vector2 o = (max + min) * 0.5f;
            Matrix mt = Matrix.CreateTranslation(-o.X, -o.Y, 0);
            frustum = new BoundingFrustum(mView * mProj * mt * ms);
        }
    }

    public struct OBB {
        public Vector3 min;
        public Vector3 max;
        public OBB(Matrix mView, Matrix mProj, Vector2 min, Vector2 max) {
            // TODO: From An Orthographic Projection

            // Construct matrix to transform screen to selection box
            Vector2 selectCenter = (min + max) / 2;
            Matrix translate = Matrix.CreateTranslation(selectCenter.X, selectCenter.Y, 0f);
            Vector2 scaleAmount = (max - min) / new Vector2(2, 2);
            Matrix scale = Matrix.CreateScale(scaleAmount.X, scaleAmount.Y, 1);

            BoundingBox b = new BoundingBox();
            this.min = b.Min;
            this.max = b.Max;
        }
    }
    #endregion

    // Has To Detect If The Camera View Intersects With A Box
    public static class SelectionDetection {
        public static bool Intersects(ref Frustum f, ref BoundingBox box) {
            ContainmentType ct;
            f.frustum.Contains(ref box, out ct);
            return ct != ContainmentType.Disjoint;
        }

        public static bool Intersects(ref OBB obb, ref BoundingBox box) {
            // Overlap detection
            if (obb.max.X < box.Min.X) return false; // obb is left of box
            if (obb.min.X > box.Max.X) return false; // obb is right of box
            if (obb.max.Y < box.Min.Y) return false; // obb is above box
            if (obb.min.Y > box.Max.Y) return false; // obb is below box
            return true; // boxes overlap
        }
    }
}