using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grey.Engine;
using Microsoft.Xna.Framework;

namespace Grey.Vox {
    public class VoxWorld {
        public const int XZ_SHIFT = 5;
        public const int INDZ_SHIFT = XZ_SHIFT;
        public const int WIDTH = 0x01 << XZ_SHIFT;
        public const int DEPTH = 0x01 << XZ_SHIFT;
        public const int REGION_COUNT = WIDTH * DEPTH;

        public const int WORLD_VOXBYTE_SIZE = REGION_COUNT * Region.REGION_VOXBYTE_SIZE;

        // Precondition That All Value Fall Within Range
        public static int ToIndex(int x, int z) {
            return (z << INDZ_SHIFT) | x;
        }
        public static int ToIndex(Point p) {
            return ToIndex(p.X, p.Y);
        }

        // The Voxel Atlas
        public VoxAtlas Atlas {
            get;
            set;
        }

        // The Beginning Location Of The World
        public Point worldMin;

        // All Indexed Regions
        public readonly Region[] regions;
        public readonly RegionPager pager;
        public event Action<VoxWorld, Region> OnRegionAddition;
        public event Action<VoxWorld, Region> OnRegionDeletion;

        // Access To The State
        public readonly VoxState state;

        public VoxWorld(VoxState s) {
            // Reference The State
            state = s;

            // Start With World Near The Center
            worldMin = new Point(-WIDTH / 2, -DEPTH / 2);

            // No Regions To Add Yet
            regions = new Region[REGION_COUNT];
            Array.Clear(regions, 0, REGION_COUNT);
            pager = new RegionPager();

            // Create An Empty Atlas
            Atlas = new VoxAtlas();
        }

        public bool IsInArray(int x, int z, out int ax, out int az) {
            ax = x - worldMin.X;
            az = z - worldMin.Y;
            return ax >= 0 && ax < WIDTH && az >= 0 && az < DEPTH;
        }
        public Region TryCreateRegion(int x, int z) {
            Region r = new Region(this);
            r.loc = new Point(x, z);
            if(pager.TryAdd(r))
                return r;
            else
                return null;
        }

        public bool PlaceRegion(Region region) {
            int x, z;
            if(!IsInArray(region.loc.X, region.loc.Y, out x, out z)) {
                // TODO: Send To Unloader
                return false;
            }

            int i = ToIndex(x, z);

            // Check Old Region
            if(regions[i] != null) {
                regions[i] = null;
                if(OnRegionDeletion != null)
                    OnRegionDeletion(this, regions[i]);
            }

            // Add To Array
            regions[i] = region;
            if(OnRegionAddition != null)
                OnRegionAddition(this, region);

            return true;
        }
    }
}