using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Data {
    public enum FogOfWar : uint {
        Nothing = 0x00,
        Passive = 0x01,
        Active = 0x02,
        All = 0x03
    }

    public struct GridInfo {
        // Fog Of War Information For All Players
        private uint fog;

        

        // Access For Fog Of War Information
        public FogOfWar GetFogOfWar(int p) {
            return (FogOfWar)((fog >> (p << 2)) & 0x03);
        }
        public void SetFogOfWar(int p, FogOfWar f) {
            p <<= 2;
            fog &= ~(0x03u << p);
            fog |= ((uint)f << p);
        }
    }

    public class LevelGrid {

    }
}
