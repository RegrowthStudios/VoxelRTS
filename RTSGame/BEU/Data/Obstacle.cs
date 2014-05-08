using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BEU.Data {
    public class Obstacle : ACEntity {
        public ObstacleData Data {
            get;
            private set;
        }
        public override ACEntityData EntityData {
            get { return Data; }
            protected set { Data = value as ObstacleData; }
        }

        public Vector2 StartPos {
            get {
                Vector2 gs = Position;
                gs.X -= (Data.GridSize.X / 2);
                gs.Y -= (Data.GridSize.Y / 2);
                return gs;
            }
        }

        public Obstacle(ObstacleData d) : base(d) { }
    }
}