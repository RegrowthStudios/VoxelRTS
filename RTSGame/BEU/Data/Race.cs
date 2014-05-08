using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BEU.Data {
    public class Race {
        public const int MAX_TYPES = 32;

        public string Name;
        public readonly TankData[] TankTypes;

        public Race() {
            Name = "Unknown";
            TankTypes = new TankData[MAX_TYPES];
        }

        public void AddType(int i, TankData d) {
            TankTypes[i] = d;
        }
    }
}
