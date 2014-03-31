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
        public int Impact { get; set; }
        public List<ImpactCell> cells { get; set; } 

        public Region(List<ImpactCell> cellList) {
            cells = cellList;
            foreach (var c in cells) {

            }
        }

        public void IncreaseImpact(int amount) {
        
        }
    }
}
