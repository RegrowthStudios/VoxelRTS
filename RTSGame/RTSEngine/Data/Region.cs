using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Data
{
    public class Region
    {
        public int RegionImpact { get; private set; }


        public Region() {

            RegionImpact = 0;

        }

        public void AddToRegionImpact(int amount) {
            RegionImpact += amount; 
        }
    }
}
