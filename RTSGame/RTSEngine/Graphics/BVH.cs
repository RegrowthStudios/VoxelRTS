using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Graphics {
    public class BVHNode {
        public BoundingBox bound;
        public Vector3 MinBound {
            get { return bound.Min; }
        }
        public Vector3 MaxBound {
            get { return bound.Max; }
        }

        public BVHNode lChild, rChild;
        public bool IsLeaf {
            get {
                return lChild == null && rChild == null;
            }
        }

        public int surfaceIndexStart, surfaceIndexEnd;

        public float Volume {
            get {
                return
                    (bound.Max.X - bound.Min.X) *
                    (bound.Max.Y - bound.Min.Y) *
                    (bound.Max.Z - bound.Min.Z)
                    ;
            }
        }
        public IEnumerable<BVHNode> Children {
            get {
                yield return lChild;
                yield return rChild;
            }
        }

        public BVHNode() {
            bound = new BoundingBox();
            lChild = null;
            rChild = null;
            surfaceIndexStart = -1;
            surfaceIndexEnd = -1;
        }
        public BVHNode(Vector3 minBound, Vector3 maxBound, BVHNode leftChild, BVHNode rightChild, int start, int end) {
            bound = new BoundingBox(minBound, maxBound);
            lChild = leftChild;
            rChild = rightChild;
            surfaceIndexStart = start;
            surfaceIndexEnd = end;
        }

        public void GetAVD(ref Vector2 f) {
            float v = Volume;
            foreach(BVHNode c in Children) {
                if(c == null) continue;
                float cv = c.Volume;
                f.X += cv / v;
                f.Y++;
                c.GetAVD(ref f);
            }
        }

        public bool Intersects(Ray ray) {
            float tMin, tMax;
            bool hit = false;
            float tEntry = float.MaxValue, tExit = -float.MaxValue;

            // solve parametric equations to find each intersection point
            if(ray.Direction.X > 0) {
                tMin = (MinBound.X - ray.Position.X) / ray.Direction.X;
                tMax = (MaxBound.X - ray.Position.X) / ray.Direction.X;
                tEntry = tMin; tExit = tMax; hit = true;
            }
            else if(ray.Direction.X < 0) {
                tMax = (MinBound.X - ray.Position.X) / ray.Direction.X;
                tMin = (MaxBound.X - ray.Position.X) / ray.Direction.X;
                tEntry = tMin; tExit = tMax; hit = true;
            }
            if(ray.Direction.Y > 0) {
                tMin = (MinBound.Y - ray.Position.Y) / ray.Direction.Y;
                tMax = (MaxBound.Y - ray.Position.Y) / ray.Direction.Y;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            else if(ray.Direction.Y < 0) {
                tMax = (MinBound.Y - ray.Position.Y) / ray.Direction.Y;
                tMin = (MaxBound.Y - ray.Position.Y) / ray.Direction.Y;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            if(ray.Direction.Z > 0) {
                tMin = (MinBound.Z - ray.Position.Z) / ray.Direction.Z;
                tMax = (MaxBound.Z - ray.Position.Z) / ray.Direction.Z;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            else if(ray.Direction.Z < 0) {
                tMax = (MinBound.Z - ray.Position.Z) / ray.Direction.Z;
                tMin = (MaxBound.Z - ray.Position.Z) / ray.Direction.Z;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            return hit && (tEntry <= tExit);
        }
        public float IntersectTime(Ray ray) {
            float tMin, tMax;
            bool hit = false;
            float tEntry = float.MaxValue, tExit = -float.MaxValue;

            // Find Collision Point
            if(ray.Direction.X > 0) {
                tMin = (MinBound.X - ray.Position.X) / ray.Direction.X;
                tMax = (MaxBound.X - ray.Position.X) / ray.Direction.X;
                tEntry = tMin; tExit = tMax; hit = true;
            }
            else if(ray.Direction.X < 0) {
                tMax = (MinBound.X - ray.Position.X) / ray.Direction.X;
                tMin = (MaxBound.X - ray.Position.X) / ray.Direction.X;
                tEntry = tMin; tExit = tMax; hit = true;
            }
            if(ray.Direction.Y > 0) {
                tMin = (MinBound.Y - ray.Position.Y) / ray.Direction.Y;
                tMax = (MaxBound.Y - ray.Position.Y) / ray.Direction.Y;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            else if(ray.Direction.Y < 0) {
                tMax = (MinBound.Y - ray.Position.Y) / ray.Direction.Y;
                tMin = (MaxBound.Y - ray.Position.Y) / ray.Direction.Y;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            if(ray.Direction.Z > 0) {
                tMin = (MinBound.Z - ray.Position.Z) / ray.Direction.Z;
                tMax = (MaxBound.Z - ray.Position.Z) / ray.Direction.Z;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            else if(ray.Direction.Z < 0) {
                tMax = (MinBound.Z - ray.Position.Z) / ray.Direction.Z;
                tMin = (MaxBound.Z - ray.Position.Z) / ray.Direction.Z;
                if(hit) {
                    if(tEntry < tMin) tEntry = tMin;
                    if(tExit > tMax) tExit = tMax;
                }
                else {
                    tEntry = tMin; tExit = tMax; hit = true;
                }
            }
            if(hit && (tEntry <= tExit)) return tEntry;
            return float.NaN;
        }
    }

    public class Surface {
        public BoundingBox bounds;
        public Vector3 MinBound {
            get { return bounds.Min; }
        }
        public Vector3 MaxBound {
            get { return bounds.Max; }
        }

    }
    public struct IntersectionRecord {
        public float t;
    }

    public class BVH {
        private List<Surface> surfaces;
        BVHNode root;

        public BVH() {

        }

        public bool Intersect(ref IntersectionRecord outRecord, Ray rayIn) {
            if(!root.Intersects(rayIn)) return false;
            return IntersectHelper(root, ref outRecord, rayIn);
        }
        private bool IntersectHelper(BVHNode node, ref IntersectionRecord outRecord, Ray rayIn) {
            // TODO 17
            if(node.IsLeaf) {
                outRecord.t = float.PositiveInfinity;
                IntersectionRecord tempRec = new IntersectionRecord();
                for(int i = node.surfaceIndexStart; i < node.surfaceIndexEnd; i++) {
                   /* if(surfaces[i].intersect(tempRec, rayIn)) {
                        // check if current t value is smaller
                        if(tempRec.t < outRecord.t) {
                            outRecord = tempRec;
                        }
                    }*/
                }
                return false; /* !float.IsInfinite(outRecord.t); */
            }
            else {

                float t1 = node.lChild.IntersectTime(rayIn);
                float t2 = node.rChild.IntersectTime(rayIn);
                if(float.IsNaN(t1)) {
                    if(float.IsNaN(t2)) return false;
                    return IntersectHelper(node.rChild, ref outRecord, rayIn);
                }
                if(float.IsNaN(t2)) return IntersectHelper(node.lChild, ref outRecord, rayIn);

                // Need To Check Both
                IntersectionRecord lRec = new IntersectionRecord();
                IntersectionRecord rRec = new IntersectionRecord();
                bool lHit, rHit;
                lHit = IntersectHelper(node.lChild, ref lRec, rayIn);
                rHit = IntersectHelper(node.rChild, ref rRec, rayIn);
                if(lHit && rHit)
                    outRecord = lRec.t <= rRec.t ? lRec : rRec;
                else if(lHit)
                    outRecord = lRec;
                else if(rHit)
                    outRecord = rRec;
                else
                    return false;
                return true;
            }
        }

        public void Build(List<Surface> s) {
            surfaces = s;
            root = CreateTree(0, surfaces.Count);
        }
        private BVHNode CreateTree(int start, int end) {
            Vector3 minB = new Vector3(float.MaxValue);
            Vector3 maxB = new Vector3(-float.MaxValue);
            Vector3 sMin, sMax;
            for(int i = start; i < end; i++) {
                Surface s = surfaces[i];

                sMin = s.MinBound;
                sMax = s.MaxBound;

                // compute min bound
                minB.X = Math.Min(minB.X, sMin.X);
                minB.Y = Math.Min(minB.Y, sMin.Y);
                minB.Z = Math.Min(minB.Z, sMin.Z);

                // compute max bound
                maxB.X = Math.Max(maxB.X, sMax.X);
                maxB.Y = Math.Max(maxB.Y, sMax.Y);
                maxB.Z = Math.Max(maxB.Z, sMax.Z);
            }

            // ==== Step 2 ====
            // Check for the base case. 
            // If the range [start, end) is small enough, just return a new leaf node.

            int maxNumSurfaces = 1;
            if(end - start <= maxNumSurfaces) {
                return new BVHNode(minB, maxB, null, null, start, end);
            }

            // ==== Step 3 ====
            // Figure out the widest dimension (x or y or z).
            // If x is the widest, set widestDim = 0. If y, set widestDim = 1. If z, set widestDim = 2.

            int widestDim;
            Vector3 dim = maxB - minB;

            // ==== Step 4 ====
            // Sort surfaces according to the widest dimension.
            // You can also implement O(n) randomized splitting algorithm.
            if(dim.X >= dim.Y && dim.X >= dim.Z) {
                surfaces.Sort(SurfComparisonX);
            }
            else if(dim.Y >= dim.Z) {
                surfaces.Sort(SurfComparisonY);
            }
            else {
                surfaces.Sort(SurfComparisonZ);
            }

            // ==== Step 5 ====
            // Recursively create left and right children.

            int e = (start + end) / 2;
            BVHNode leftChild = CreateTree(start, e);
            BVHNode rightChild = CreateTree(e, end);

            return new BVHNode(minB, maxB, leftChild, rightChild, start, end);
        }
        static int SurfComparisonX(Surface s1, Surface s2) {
            float v1 = (s1.MaxBound.X + s1.MinBound.X) * 0.5f;
            float v2 = (s2.MaxBound.X + s2.MinBound.X) * 0.5f;
            return v1.CompareTo(v2);
        }
        static int SurfComparisonY(Surface s1, Surface s2) {
            float v1 = (s1.MaxBound.Y + s1.MinBound.Y) * 0.5f;
            float v2 = (s2.MaxBound.Y + s2.MinBound.Y) * 0.5f;
            return v1.CompareTo(v2);
        }
        static int SurfComparisonZ(Surface s1, Surface s2) {
            float v1 = (s1.MaxBound.Z + s1.MinBound.Z) * 0.5f;
            float v2 = (s2.MaxBound.Z + s2.MinBound.Z) * 0.5f;
            return v1.CompareTo(v2);
        }
    }
}