using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Grey.Graphics {
    public class VGPCustom : IVGeoProvider {
        public static IEnumerable<MeshedFace> Decomposed(MeshedFace mf) {
            for(int v = 0; v < mf.Size.Y; v++) {
                for(int u = 0; u < mf.Size.X; u++) {
                    Vector3I aU = MeshedFace.FACE_AXES[mf.Face, 0];
                    Vector3I aV = MeshedFace.FACE_AXES[mf.Face, 1];
                    yield return new MeshedFace(
                        new Vector3I(
                            mf.RXNYN.X + aU.X * u + aV.X * v,
                            mf.RXNYN.Y + aU.Y * u + aV.Y * v,
                            mf.RXNYN.Z + aU.Z * u + aV.Z * v
                            ),
                        aU, aV,
                        new Point(1, 1),
                        mf.ID, mf.Face
                        );
                }
            }
        }

        public VertexVoxel[][] CustomVerts;
        public int[][] CustomInds;

        public VGPCustom() {
            CustomVerts = new VertexVoxel[6][];
            for(int i = 0; i < 6; i++) CustomVerts[i] = new VertexVoxel[0];
            CustomInds = new int[6][];
            for(int i = 0; i < 6; i++) CustomInds[i] = new int[0];
        }

        public void Append(List<VertexVoxel> lVerts, List<int> lInds, ref MeshedFace mf) {
            VertexVoxel vv;
            foreach(var mfd in Decomposed(mf)) {
                Vector3 v1 = new Vector3(mfd.RXNYN.X, mfd.RXNYN.Y, mfd.RXNYN.Z);
                int vc = lVerts.Count;
                foreach(var vert in CustomVerts[mf.Face]) {
                    vv = vert;
                    vv.Position += v1;
                    lVerts.Add(vv);
                }
                foreach(var ind in CustomInds[mf.Face]) {
                    lInds.Add(ind + vc);
                }
            }
        }
    }
}
