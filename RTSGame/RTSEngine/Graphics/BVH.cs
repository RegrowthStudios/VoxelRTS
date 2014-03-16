using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public struct Triangle {
        public Vector3 MinBound {
            get { return Vector3.Min(Vector3.Min(P1, P2), P3); }
        }
        public Vector3 MaxBound {
            get { return Vector3.Max(Vector3.Max(P1, P2), P3); }
        }

        public Vector3 P1, P2, P3;
        public Vector3 N;

        public Triangle(Vector3 p1, Vector3 p2, Vector3 p3) {
            P1 = p1;
            P2 = p2;
            P3 = p3;
            N = Vector3.Normalize(Vector3.Cross(P3 - P1, P2 - P1));
        }

        public bool Intersect(ref IntersectionRecord rec, Ray r) {
            Plane p = new Plane(P1, P2, P3);
            float? v = r.Intersects(p);
            if(!v.HasValue) return false;



            //// Find Ray's Normal Distance From The Triangle
            //Vector3 disp = r.Position - P1;
            //float dotDisp = Vector3.Dot(N, disp);

            //// Check If Ray Is Behind Triangle
            //if(dotDisp < 0) return false;

            //// Find Ray's Direction Relative To Normal
            //float sp = -Vector3.Dot(r.Direction, N);
            //if(sp <= 0) return false;

            // Find Time And Intersection With Plane
            //float t = dotDisp / sp;
            float t = v.Value;
            Vector3 hit = r.Position + r.Direction * t;
            Vector3 relHit = hit - P1;

            // Find UV In Triangle
            Vector3 axU = P3 - P1;
            Vector3 axV = P2 - P1;
            float distU = axU.Length();
            float distV = axV.Length();
            float pU = Vector3.Dot(relHit, axU / distU);
            if(pU < 0 || pU > distU) return false;
            float pV = Vector3.Dot(relHit, axV / distV);
            if(pV < 0 || pV > distV) return false;

            rec.T = t;
            return true;
        }
    }

    public struct IntersectionRecord {
        public float T;
    }

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

    public class BVH {
        private static readonly TriAxisSorterX F_SORT_X = new TriAxisSorterX();
        private static readonly TriAxisSorterY F_SORT_Y = new TriAxisSorterY();
        private static readonly TriAxisSorterZ F_SORT_Z = new TriAxisSorterZ();

        private List<Triangle> tris;
        private BVHNode root;

        public BVH() {

        }

        public bool Intersect(ref IntersectionRecord outRecord, Ray rayIn) {
            if(!root.Intersects(rayIn)) return false;
            return IntersectHelper(root, ref outRecord, rayIn);
        }
        private bool IntersectHelper(BVHNode node, ref IntersectionRecord outRecord, Ray rayIn) {
            if(node.IsLeaf) {
                outRecord.T = float.PositiveInfinity;
                IntersectionRecord tempRec = new IntersectionRecord();
                for(int i = node.surfaceIndexStart; i < node.surfaceIndexEnd; i++) {
                    if(tris[i].Intersect(ref tempRec, rayIn)) {
                        // check if current t value is smaller
                        if(tempRec.T < outRecord.T) {
                            outRecord = tempRec;
                        }
                    }
                }
                return !float.IsPositiveInfinity(outRecord.T);
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
                    outRecord = lRec.T <= rRec.T ? lRec : rRec;
                else if(lHit)
                    outRecord = lRec;
                else if(rHit)
                    outRecord = rRec;
                else
                    return false;
                return true;
            }
        }

        public void Build(VertexPositionTexture[] verts, int[] inds) {
            tris = new List<Triangle>(inds.Length / 3);
            for(int i = 0; i < inds.Length; ) {
                int i1 = inds[i++];
                int i2 = inds[i++];
                int i3 = inds[i++];
                tris.Add(new Triangle(
                    verts[i1].Position,
                    verts[i2].Position,
                    verts[i3].Position
                    ));
            }
            root = CreateTree(0, tris.Count);
        }
        private BVHNode CreateTree(int start, int end) {
            Vector3 minB = new Vector3(float.MaxValue);
            Vector3 maxB = new Vector3(-float.MaxValue);
            for(int i = start; i < end; i++) {
                minB = Vector3.Min(tris[i].MinBound, minB);
                maxB = Vector3.Max(tris[i].MaxBound, maxB);
            }

            // Check For Leaf Node Condition
            if(end - start <= 1) {
                return new BVHNode(minB, maxB, null, null, start, end);
            }

            // Sort On Widest Dimension
            Vector3 dim = maxB - minB;
            if(dim.X >= dim.Y && dim.X >= dim.Z) tris.Sort(start, end - start, F_SORT_X);
            else if(dim.Y >= dim.Z) tris.Sort(start, end - start, F_SORT_Y);
            else tris.Sort(start, end - start, F_SORT_Z);

            // Create Children
            int e = (start + end) / 2;
            BVHNode leftChild = CreateTree(start, e);
            BVHNode rightChild = CreateTree(e, end);

            return new BVHNode(minB, maxB, leftChild, rightChild, start, end);
        }

        class TriAxisSorterX : IComparer<Triangle> {
            public int Compare(Triangle x, Triangle y) {
                float v1 = (x.MaxBound.X + x.MinBound.X) * 0.5f;
                float v2 = (y.MaxBound.X + y.MinBound.X) * 0.5f;
                return v1.CompareTo(v2);
            }
        }
        class TriAxisSorterY : IComparer<Triangle> {
            public int Compare(Triangle x, Triangle y) {
                float v1 = (x.MaxBound.Y + x.MinBound.Y) * 0.5f;
                float v2 = (y.MaxBound.Y + y.MinBound.Y) * 0.5f;
                return v1.CompareTo(v2);
            }
        }
        class TriAxisSorterZ : IComparer<Triangle> {
            public int Compare(Triangle x, Triangle y) {
                float v1 = (x.MaxBound.Z + x.MinBound.Z) * 0.5f;
                float v2 = (y.MaxBound.Z + y.MinBound.Z) * 0.5f;
                return v1.CompareTo(v2);
            }
        }
    }
}