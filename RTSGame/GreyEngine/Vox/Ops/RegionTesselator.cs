using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grey.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Grey.Vox {
    public class RegionTesselator : IRegionWorker {
        private Region region;
        private VoxelRenderer renderer;

        public RegionTesselator(Region r, VoxelRenderer vg) {
            region = r;
            renderer = vg;
        }

        public void DoWork() {
            var mFaces = RegionGreedyMesher.Mesh(region);

            var lVerts = new List<VertexVoxel>(128);
            var lInds = new List<int>(128);
            MeshedFace mf;
            for(int i = 0; i < mFaces.Count; i++) {
                mf = mFaces[i];
                region.World.Atlas[(ushort)Math.Abs(mf.ID)].GeoProvider.Append(lVerts, lInds, ref mf);
            }

            // Build The Geometry
            if(lVerts.Count < 1) return;
            VoxGeo geo = new VoxGeo();
            geo.Region = region;
            geo.VB = new VertexBuffer(renderer.G, VertexVoxel.Declaration, lVerts.Count, BufferUsage.WriteOnly);
            geo.VB.SetData(lVerts.ToArray());
            geo.IB = new IndexBuffer(renderer.G, IndexElementSize.ThirtyTwoBits, lInds.Count, BufferUsage.WriteOnly);
            geo.IB.SetData(lInds.ToArray());

            // TODO: Use The Geometry
            renderer.AddRegionGeo(geo);
        }
    }
}