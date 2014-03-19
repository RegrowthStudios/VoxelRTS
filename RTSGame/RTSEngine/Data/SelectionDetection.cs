using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data {
    // Has To Detect If The Camera View Intersects With A Box
    public static class SelectionDetection {
        public static BoundingFrustum FromSelectionRect(Matrix mView, Matrix mProj, Vector2 min, Vector2 max) {
            // Transform Projection Matrix
            Vector2 s = (max - min) * 0.5f;
            Matrix ms = Matrix.CreateScale(1f / s.X, 1f / s.Y, 1);
            Vector2 o = (max + min) * 0.5f;
            Matrix mt = Matrix.CreateTranslation(-o.X, -o.Y, 0);
            return new BoundingFrustum(mView * mProj * mt * ms);
        }

        public static bool Intersects(BoundingFrustum f, ref BoundingBox box) {
            ContainmentType ct;
            f.Contains(ref box, out ct);
            return ct != ContainmentType.Disjoint;
        }
    }
}