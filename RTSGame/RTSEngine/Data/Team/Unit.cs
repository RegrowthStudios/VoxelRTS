using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class Unit /*: IDestructibleEntity, ICombatEntity*/ {
        // Health Of The Unit
        public int Health;

        // Speed Of The Unit
        public float MovementSpeed;
    }
}