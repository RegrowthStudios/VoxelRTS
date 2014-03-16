using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data {
    #region Detection Objects Generated From A Camera
    public struct Frustum {
        // 0=near, 1=far, 2=bottom, 3=top, 4=left, 5=right
        public Plane[] planes;

        public Frustum(Matrix mView, Matrix mProj, Vector2 min, Vector2 max) {
            // TODO: From An PerspectiveProjection

            // Construct matrix to transform screen to selection box
            Vector2 selectCenter = (min + max) / 2;
            Matrix translate = Matrix.CreateTranslation(selectCenter.X, selectCenter.Y, 0f);
            Vector2 scaleAmount = (max - min) / new Vector2(2,2);
            Matrix scale = Matrix.CreateScale(scaleAmount.X, scaleAmount.Y, 1);

            BoundingFrustum f = new BoundingFrustum(mView * mProj);
            planes = new Plane[6];
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
            // TODO: Detect

            return false;
        }
        public static bool Intersects(ref OBB b, ref BoundingBox box) {
            // TODO: Detect
            return false;
        }
    }
}