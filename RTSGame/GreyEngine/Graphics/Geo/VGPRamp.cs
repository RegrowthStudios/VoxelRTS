using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grey.Vox;
using Microsoft.Xna.Framework;

namespace Grey.Graphics {
    public class VGPRamp : IVGeoProvider {
        public Color Color {
            get;
            set;
        }
        public Vector4 UVRect {
            get;
            set;
        }
        public Vector4 UVRectTop {
            get;
            set;
        }
        public int Orientation {
            set {
                switch(value) {
                    case 0:
                        faceBack = Voxel.FACE_NX;
                        subs[1] = subs[3] = Vector3.UnitY;
                        subs[0] = subs[2] = Vector3.Zero;
                        faceSide = Voxel.FACE_NZ;
                        sideVerts = new int[] {
                            1, 3, 2,
                            0, 3, 2
                        };
                        break;
                    case 1:
                        faceBack = Voxel.FACE_PZ;
                        subs[1] = subs[0] = Vector3.UnitY;
                        subs[3] = subs[2] = Vector3.Zero;
                        faceSide = Voxel.FACE_NX;
                        sideVerts = new int[] {
                            1, 3, 2,
                            0, 3, 2
                        };
                        break;
                    case 2:
                        faceBack = Voxel.FACE_PX;
                        subs[1] = subs[3] = Vector3.Zero;
                        subs[0] = subs[2] = Vector3.UnitY;
                        faceSide = Voxel.FACE_PZ;
                        sideVerts = new int[] {
                            1, 3, 2,
                            0, 3, 2
                        };
                        break;
                    default:
                        faceBack = Voxel.FACE_NZ;
                        subs[1] = subs[0] = Vector3.Zero;
                        subs[3] = subs[2] = Vector3.UnitY;
                        faceSide = Voxel.FACE_PX;
                        sideVerts = new int[] {
                            1, 3, 2,
                            0, 3, 2
                        };
                        break;
                }
            }
        }

        int faceBack, faceSide;
        Vector3[] subs;
        int[] sideVerts;

        public VGPRamp() {
            Color = Color.White;
            UVRect = new Vector4(0, 0, 1, 1);
            UVRectTop = UVRect;
            subs = new Vector3[4];
            Orientation = 0;
        }

        public void Append(List<VertexVoxel> lVerts, List<int> lInds, ref MeshedFace mf) {
            Vector3 v1 = new Vector3(mf.RXNYN.X, mf.RXNYN.Y, mf.RXNYN.Z);
            Vector3 v2 = new Vector3(mf.RXPYN.X, mf.RXPYN.Y, mf.RXPYN.Z);
            Vector3 v3 = new Vector3(mf.RXNYP.X, mf.RXNYP.Y, mf.RXNYP.Z);
            Vector3 v4 = new Vector3(mf.RXPYP.X, mf.RXPYP.Y, mf.RXPYP.Z);
            Vector3[] verts = new Vector3[] { v1, v2, v3, v4 };
            int vi = lVerts.Count;
            if(mf.Face == faceBack) {
                lVerts.Add(new VertexVoxel(v1, new Vector2(0, 0), UVRect, Color));
                lVerts.Add(new VertexVoxel(v2, new Vector2(mf.Size.X, 0), UVRect, Color));
                lVerts.Add(new VertexVoxel(v3, new Vector2(0, mf.Size.Y), UVRect, Color));
                lVerts.Add(new VertexVoxel(v4, new Vector2(mf.Size.X, mf.Size.Y), UVRect, Color));
                lInds.Add(vi + 0);
                lInds.Add(vi + 1);
                lInds.Add(vi + 2);
                lInds.Add(vi + 2);
                lInds.Add(vi + 1);
                lInds.Add(vi + 3);
                return;
            }
            else if(mf.Face == (faceBack ^ 0x01)) {
                return;
            }
            else if(mf.Face == faceSide) {
                lVerts.Add(new VertexVoxel(verts[sideVerts[0]], new Vector2(0, 0), UVRect, Color));
                lVerts.Add(new VertexVoxel(verts[sideVerts[1]], new Vector2(mf.Size.X, 0), UVRect, Color));
                lVerts.Add(new VertexVoxel(verts[sideVerts[2]], new Vector2(0, mf.Size.Y), UVRect, Color));
                lInds.Add(vi + 0);
                lInds.Add(vi + 1);
                lInds.Add(vi + 2);
                return;
            }
            else if(mf.Face == (faceSide ^ 0x01)) {
                lVerts.Add(new VertexVoxel(verts[sideVerts[3]], new Vector2(0, 0), UVRect, Color));
                lVerts.Add(new VertexVoxel(verts[sideVerts[4]], new Vector2(mf.Size.X, 0), UVRect, Color));
                lVerts.Add(new VertexVoxel(verts[sideVerts[5]], new Vector2(0, mf.Size.Y), UVRect, Color));
                lInds.Add(vi + 0);
                lInds.Add(vi + 1);
                lInds.Add(vi + 2);
                return;
            }
            else if(mf.Face == Voxel.FACE_PY) {
                lVerts.Add(new VertexVoxel(v1 - subs[0], new Vector2(0, 0), UVRectTop, Color));
                lVerts.Add(new VertexVoxel(v2 - subs[1], new Vector2(mf.Size.X, 0), UVRectTop, Color));
                lVerts.Add(new VertexVoxel(v3 - subs[2], new Vector2(0, mf.Size.Y), UVRectTop, Color));
                lVerts.Add(new VertexVoxel(v4 - subs[3], new Vector2(mf.Size.X, mf.Size.Y), UVRectTop, Color));
                lInds.Add(vi + 0);
                lInds.Add(vi + 1);
                lInds.Add(vi + 2);
                lInds.Add(vi + 2);
                lInds.Add(vi + 1);
                lInds.Add(vi + 3);
                return;
            }
            //switch(mf.Face) {
            //    case Voxel.FACE_PY:

            //        break;
            //    default:
            //        lVerts.Add(new VertexVoxel(v1, new Vector2(0, 0), UVRect, Color));
            //        lVerts.Add(new VertexVoxel(v2, new Vector2(mf.Size.X, 0), UVRect, Color));
            //        lVerts.Add(new VertexVoxel(v3, new Vector2(0, mf.Size.Y), UVRect, Color));
            //        lVerts.Add(new VertexVoxel(v4, new Vector2(mf.Size.X, mf.Size.Y), UVRect, Color));
            //        lInds.Add(vi + 0);
            //        lInds.Add(vi + 1);
            //        lInds.Add(vi + 2);
            //        lInds.Add(vi + 2);
            //        lInds.Add(vi + 1);
            //        lInds.Add(vi + 3);
            //        break;
            //}
        }
    }
}