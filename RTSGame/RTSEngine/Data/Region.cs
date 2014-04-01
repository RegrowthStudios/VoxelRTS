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
        public int RegionImpact { get; set; }
        public List<ImpactCell> cells { get; set; } 

        public Region(List<ImpactCell> cellList) {
            cells = cellList;
            RegionImpact = 0;
            foreach (var c in cells) {
                c.IncreaseImpact += AddToRegionImpact;
                RegionImpact += c.CellImpact;
            }
        }

        public void AddToRegionImpact(int amount) {
            RegionImpact += amount; 
        }
    }
}
