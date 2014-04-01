using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team
{
    public class RTSBuildingData
    {
        // The Friendly Name
        public string FriendlyName;

        // Health Of The Building
        public int Health;

        // The Capital Cost Of The Building
        public int CapitalCost;

        // Environment Impact
        public int Impact;

        // ICollidableShape of The Building
        public ICollidable ICollidableShape;
        public BoundingBox BBox;
    }
}
