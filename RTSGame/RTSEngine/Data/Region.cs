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
        public List<RTSUnit> EnemyUnits { get; set; }
        public List<IEntity> EnemyBuildings { get; set; }
        public List<RTSUnit> FriendlyUnits { get; set; }
        public List<IEntity> Resources { get; set; }

        public Region() {

        }
    }
}
