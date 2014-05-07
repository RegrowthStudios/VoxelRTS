using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grey.Vox;
using Microsoft.Xna.Framework;

namespace Grey.Graphics {
    public class VGPCube6 : IVGeoProvider {
        public Color[] Colors {
            get;
            set;
        }
        public Vector4[] UVRects {
            get;
            set;
        }

        public VGPCube6() {
            Colors = new Color[] {
                Color.White,
                Color.White,
                Color.White,
                Color.White,
                Color.White,
                Color.White
            };
            UVRects = new Vector4[] {
                new Vector4(0, 0, 1, 1),
                new Vector4(0, 0, 1, 1),
                new Vector4(0, 0, 1, 1),
                new Vector4(0, 0, 1, 1),
                new Vector4(0, 0, 1, 1),
                new Vector4(0, 0, 1, 1)
            };
        }

        public void Append(List<VertexVoxel> lVerts, List<int> lInds, ref MeshedFace mf) {
            Vector3 v1 = new Vector3(mf.RXNYN.X, mf.RXNYN.Y, mf.RXNYN.Z);
            Vector3 v2 = new Vector3(mf.RXPYN.X, mf.RXPYN.Y, mf.RXPYN.Z);
            Vector3 v3 = new Vector3(mf.RXNYP.X, mf.RXNYP.Y, mf.RXNYP.Z);
            Vector3 v4 = new Vector3(mf.RXPYP.X, mf.RXPYP.Y, mf.RXPYP.Z);
            int vi = lVerts.Count;
            switch(mf.Face) {
                default:
                    lVerts.Add(new VertexVoxel(v1, new Vector2(0, 0), UVRects[mf.Face], Colors[mf.Face]));
                    lVerts.Add(new VertexVoxel(v2, new Vector2(mf.Size.X, 0), UVRects[mf.Face], Colors[mf.Face]));
                    lVerts.Add(new VertexVoxel(v3, new Vector2(0, mf.Size.Y), UVRects[mf.Face], Colors[mf.Face]));
                    lVerts.Add(new VertexVoxel(v4, new Vector2(mf.Size.X, mf.Size.Y), UVRects[mf.Face], Colors[mf.Face]));
                    lInds.Add(vi + 0);
                    lInds.Add(vi + 1);
                    lInds.Add(vi + 2);
                    lInds.Add(vi + 2);
                    lInds.Add(vi + 1);
                    lInds.Add(vi + 3);
                    break;
            }
        }
    }
}