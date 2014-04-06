using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Controllers;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSBuildingData {
        // The Friendly Name
        public string FriendlyName;

        // Health Of The Building
        public int Health;

        // The Capital Cost Of The Building
        public int CapitalCost;

        // Maximum Number Of These Buildings Supported
        public int MaxCount;

        // Environment Impact
        public int Impact;

        // Time To Build The Building
        public int BuildTime;

        // View Radius
        public int SightRadius;

        // ICollidableShape of The Building
        public Point GridSize;
        public ICollidable ICollidableShape;
        public BoundingBox BBox;

        public ReflectedBuildingController DefaultActionController;
    }
}