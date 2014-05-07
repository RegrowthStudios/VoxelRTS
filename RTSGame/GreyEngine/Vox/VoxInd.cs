using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Grey.Vox {
    public struct VoxLocation {
        public Point RegionLoc;
        public int RegionIndex {
            get { return VoxWorld.ToIndex(RegionLoc); }
        }
        public Vector3I VoxelLoc;
        public int VoxelIndex {
            get { return Region.ToIndex(VoxelLoc); }
        }

        public VoxLocation(Vector3I loc) {
            RegionLoc = new Point(loc.X >> Region.XZ_SHIFT, loc.Z >> Region.XZ_SHIFT);
            VoxelLoc = new Vector3I(
                loc.X & ((0x01 << Region.XZ_SHIFT) - 1),
                loc.Y,
                loc.Z & ((0x01 << Region.XZ_SHIFT) - 1)
                );
        }
    }

}
