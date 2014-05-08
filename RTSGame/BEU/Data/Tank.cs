using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BEU.Data {
    public class Tank : ACEntity {
        public Team Team {
            get;
            private set;
        }

        public TankData Data {
            get;
            private set;
        }
        public override ACEntityData EntityData {
            get { return Data; }
            protected set { Data = value as TankData; }
        }

        public Tank(Team t, TankData d, Vector2 pos)
            : base(d) {
            Position = pos;
            Team = t;
        }
    }
}
