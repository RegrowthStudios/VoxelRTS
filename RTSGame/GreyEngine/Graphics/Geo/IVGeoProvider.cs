using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grey.Vox;
using Microsoft.Xna.Framework;

namespace Grey.Graphics {
    public struct MeshedFace {
        // Rectangle Locations
        public Vector3I RXNYN;
        public Vector3I RXPYN;
        public Vector3I RXNYP;
        public Vector3I RXPYP;

        // Voxel Geo Helpers
        public int ID;
        public int Face;
        public Point Size;

        public MeshedFace(Vector3I rmin, Vector3I axisX, Vector3I axisY, Point size, int id, int face) {
            RXNYN = rmin;
            Size = size;

            RXPYN = rmin;
            RXPYN.X += Size.X * axisX.X;
            RXPYN.Y += Size.X * axisX.Y;
            RXPYN.Z += Size.X * axisX.Z;

            RXNYP = rmin;
            RXNYP.X += Size.Y * axisY.X;
            RXNYP.Y += Size.Y * axisY.Y;
            RXNYP.Z += Size.Y * axisY.Z;

            RXPYP = RXPYN;
            RXPYP.X += Size.Y * axisY.X;
            RXPYP.Y += Size.Y * axisY.Y;
            RXPYP.Z += Size.Y * axisY.Z;

            ID = id;
            Face = face;
        }
    }

    public interface IVGeoProvider {
        void Append(List<VertexVoxel> lVerts, List<int> lInds, ref MeshedFace mf);
    }
}