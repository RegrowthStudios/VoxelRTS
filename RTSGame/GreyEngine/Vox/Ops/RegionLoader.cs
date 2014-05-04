using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Grey.Engine;

namespace Grey.Vox {
    public class RegionLoader : IRegionWorker {
        private Region region;

        public RegionLoader(Region r) {
            region = r;
        }

        public void DoWork() {
            // Can't Load A Null Region
            if(region == null) return;

            // TODO: Load A Region

            // TODO: Generate A Region Correctly
            Random r = new Random();

            // Surface Blocks
            Vector3I v = Vector3I.Zero;
            for(v.Z = 0; v.Z < Region.DEPTH; v.Z++) {
                for(v.X = 0; v.X < Region.WIDTH; v.X++) {
                    for(v.Y = 0; v.Y < Region.HEIGHT / 2; v.Y++) {
                        region.SetVoxel(v.X, v.Y, v.Z, 1);
                    }
                }
            }

            // Place It In The World Now
            region.LoadComplete();
            if(!region.World.PlaceRegion(region)) return;

            // Faces Need To Be Calculated
            region.NotifyFacesChanged();

            // Check For Neighboring Regions
            int rx = region.loc.X;
            int rz = region.loc.Y;
            if(region.rNX != null) region.rNX.NotifyFacesChanged();
            else region.World.state.AddEvent(new VEWorldMod(rx - 1, rz, VEWMType.RegionAdd));
            if(region.rPX != null) region.rPX.NotifyFacesChanged();
            else region.World.state.AddEvent(new VEWorldMod(rx + 1, rz, VEWMType.RegionAdd));
            if(region.rNZ != null) region.rNZ.NotifyFacesChanged();
            else region.World.state.AddEvent(new VEWorldMod(rx, rz - 1, VEWMType.RegionAdd));
            if(region.rPZ != null) region.rPZ.NotifyFacesChanged();
            else region.World.state.AddEvent(new VEWorldMod(rx, rz + 1, VEWMType.RegionAdd));
        }
    }
}