using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grey.Engine {
    public enum VEventType {
        Unknown,
        WorldModification
    }

    public class VEvent {
        public VEventType VEType;

        public VEvent() {
            VEType = VEventType.Unknown;
        }
    }
}
