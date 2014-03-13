using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data {
    #region Detection Objects Generated From A Camera
    public struct Frustum {

        public Frustum(Matrix mView, Matrix mProj) {
            // TODO: From An PerspectiveProjection
        }
    }
    public struct OBB {

        public OBB(Matrix mView, Matrix mProj) {
            // TODO: From An Orthographic Projection
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