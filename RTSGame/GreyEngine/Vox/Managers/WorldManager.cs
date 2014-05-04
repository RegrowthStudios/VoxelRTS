using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grey.Engine;

namespace Grey.Vox.Managers {
    public class WorldManager {
        private readonly VoxState state;
        private VoxWorld World {
            get { return state.World; }
        }

        public WorldManager(VoxState s) {
            state = s;
        }

        public void Update() {
            UpdateQueue();
        }

        public void UpdateQueue() {
            var queue = state.GetEvents();
            for(int i = 0; i < queue.Count; i++) {
                VEvent e = queue[i];
                switch(e.VEType) {
                    case VEventType.WorldModification:
                        Apply(e as VEWorldMod);
                        break;
                    default:
                        break;
                }
            }
        }
        public void Apply(VEWorldMod e) {
            Region r, rN;
            switch(e.WMType) {
                case VEWMType.RegionAdd:
                    // Check If The Region Needs To Be Loaded
                    r = World.TryCreateRegion(e.X, e.Z);
                    if(r == null) return;

                    // Look For Neighbors
                    rN = World.pager.Obtain(e.X - 1, e.Z);
                    if(rN != null) { r.rNX = rN; rN.rPX = r; }
                    rN = World.pager.Obtain(e.X + 1, e.Z);
                    if(rN != null) { r.rPX = rN; rN.rNX = r; }
                    rN = World.pager.Obtain(e.X, e.Z - 1);
                    if(rN != null) { r.rNZ = rN; rN.rPZ = r; }
                    rN = World.pager.Obtain(e.X, e.Z + 1);
                    if(rN != null) { r.rPZ = rN; rN.rNZ = r; }

                    state.VWorkPool.AddWork(new RegionLoader(r));

                    break;
                case VEWMType.RegionRemove:
                    // TODO: Remove Regions
                    break;
            }
        }
    }
}