using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grey.Vox {
    public class RegionPager {
        public int INITIAL_CAPACITY = 4;

        public List<Region>[] rLists;

        public RegionPager() {
            rLists = new List<Region>[0x01 << 10];
            Array.Clear(rLists, 0, rLists.Length);
        }

        public bool TryAdd(Region r) {
            int h = Hash(r);
            List<Region> rl;
            bool b = true;
            if(rLists[h] == null) {
                rl = new List<Region>(INITIAL_CAPACITY);
                rLists[h] = rl;
            }
            rl = rLists[h];
            for(int i = 0; i < rl.Count; i++) {
                if(rl[i].loc == r.loc) b = false;
            }
            if(b) rl.Add(r);
            return b;
        }
        public void Remove(Region r) {
            int h = Hash(r);
            var rl = rLists[h];
            if(rl != null) {
                rl.Remove(r);
                if(rl.Count < 1)
                    rLists[h] = null;
            }
        }

        public Region Obtain(int x, int z) {
            Region r = null;
            int h = Hash(x, z);
            var rl = rLists[h];
            if(rl != null) {
                for(int i = 0; i < rl.Count; i++) {
                    if(rl[i].loc.X == x && rl[i].loc.Y == z) {
                        r = rl[i];
                        break;
                    }
                }
            }
            return r;
        }

        public int Hash(Region r) {
            return Hash(r.loc.X, r.loc.Y);
        }
        public int Hash(int x, int z) {
            return ((z & 0x1f) << 5) | (x & 0x1f);
        }
    }
}