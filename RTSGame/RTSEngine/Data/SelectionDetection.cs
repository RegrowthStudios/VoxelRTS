using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data {
    #region Detection Objects Generated From A Camera
    public struct Frustum {
        // 0=near, 1=far, 2=bottom, 3=top, 4=left, 5=right
        public Plane[] planes = new Plane[6];

        public Frustum(Matrix mView, Matrix mProj, Vector2 min, Vector2 max) {
            // TODO: From An PerspectiveProjection
            BoundingFrustum f = new BoundingFrustum(mView * mProj);
            planes[0] = f.Near;
            planes[1] = f.Far;
            planes[2] = f.Bottom;
            planes[3] = f.Top;
            planes[4] = f.Left;
            planes[5] = f.Right;
        }
    }

    public struct OBB {
        public Vector3 min;
        public Vector3 max;
        public OBB(Matrix mView, Matrix mProj, Vector2 min, Vector2 max) {
            // TODO: From An Orthographic Projection
            BoundingBox b = new BoundingBox();
            this.min = b.Min;
            this.max = b.Max;
        }
    }
    #endregion

    // Has To Detect If The Camera View Intersects With A Box
    public static class SelectionDetection {
        public static bool Intersects(ref Frustum f, ref BoundingBox box) {
            // TODO: Detect

            return false;
        }
        public static bool Intersects(ref OBB b, ref BoundingBox box) {
            // TODO: Detect
            return false;
        }
    }
}