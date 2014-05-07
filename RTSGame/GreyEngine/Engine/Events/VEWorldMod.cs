using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grey.Engine {
    public enum VEWMType {
        RegionAdd,
        RegionRemove
    }

    public class VEWorldMod : VEvent {
        public int X, Z;
        public VEWMType WMType;

        public VEWorldMod(int x, int z, VEWMType t) : base() {
            X = x;
            Z = z;
            VEType = VEventType.WorldModification;
            WMType = t;
        }
    }
}
