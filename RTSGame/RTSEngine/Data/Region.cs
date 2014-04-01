using Microsoft.Xna.Framework;
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
        public List<Point> Cells { get; private set; }

        public Region(List<Point> cellList) {
            Cells = cellList;
            RegionImpact = 0;
        }

        public void AddToRegionImpact(int amount) {
            RegionImpact += amount; 
        }
    }
}
