using Microsoft.Xna.Framework;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Data {
    public class ImpactRegion {
        public int RegionImpact { get;  private set; }
        public List<Point> Cells { get; private set; }
        public List<IEntity> Selected { get; set; }
        public int PopCount { get; set; }

        public ImpactRegion(List<Point> cellList) {
            Cells = cellList;
            RegionImpact = 0;
            Selected = new List<IEntity>();
            PopCount = 0;
        }

        public void AddToRegionImpact(int amount) {
            RegionImpact += amount;
        }
    }
}