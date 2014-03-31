using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;
using RTSEngine.Controllers;

namespace RTSEngine.Data.Team {
    public class RTSUnitData {
        // The Friendly Name
        public string FriendlyName;

        // Health Of The Unit
        public int Health;

        // The Time In Seconds It Takes To Produce The Unit
        public int BuildTime;

        // The Capital Cost Of The Unit
        public int CapitalCost;
        // The Population Cost Of The Unit
        public int PopulationCost;

        // Maximum Number Of These Units Supported
        public int MaxCount;

        // Speed Of The Unit
        public float MovementSpeed;

        // How Long It Takes For The Building To Make This Unit
        public int BuildTime;

        // BaseCombatData of The Unit
        public BaseCombatData BaseCombatData;

        // ICollidableShape of The Unit
        public ICollidable ICollidableShape;
        public BoundingBox BBox;

        public ReflectedUnitController DefaultMoveController;
        public ReflectedUnitController DefaultActionController;
        public ReflectedUnitController DefaultAnimationController;
        public ReflectedUnitController DefaultCombatController;
    }
}