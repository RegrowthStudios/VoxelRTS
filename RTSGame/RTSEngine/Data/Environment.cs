using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Data {
    public class Environment {

        public List<Region> regions {
            get;
            private set;
        }

        public Environment() {
            regions = new List<Region>();

        }

    }
}
