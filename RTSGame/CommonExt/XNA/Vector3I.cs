using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework {
    public struct Vector3I {
        private static readonly Vector3I vZero = new Vector3I(0);
        public static Vector3I Zero { get { return vZero; } }
        private static readonly Vector3I vOne = new Vector3I(1);
        public static Vector3I One { get { return vOne; } }

        public int X;
        public int Y;
        public int Z;

        public Vector3I(int x, int y, int z) {
            X = x;
            Y = y;
            Z = z;
        }
        public Vector3I(Point p, int z) {
            X = p.X;
            Y = p.Y;
            Z = z;
        }
        public Vector3I(int v) {
            X = Y = Z = v;
        }
    }
}
