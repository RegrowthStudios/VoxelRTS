using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEU.Physics;
using Microsoft.Xna.Framework;

namespace BEU.Data {
    public abstract class ACEntityData {
        public int Population;
        public int MaxPopulation;

        public int MaxHealth;
        public int OriginHealth;

        [ZXParse]
        public ICollidable ICollidableShape;
    }

    public class TankData : ACEntityData {
        public TankData() {
            // Defaults
            Population = 0;
            MaxPopulation = 100;
            MaxHealth = 100;
            OriginHealth = 100;

            SetCollisionCircle(0.5f);
        }

        public void SetCollisionCircle(float r) {
            ICollidableShape = new CollisionCircle(r, Vector2.Zero, false);
        }
    }
    public class ObstacleData : ACEntityData {
        [ZXParse]
        public Point GridSize;

        public ObstacleData() {
            // Defaults
            Population = 0;
            MaxPopulation = 100;
            MaxHealth = 1000;
            OriginHealth = 500;

            SetGridSize(1, 1);
        }

        public void SetGridSize(int w, int h) {
            GridSize = new Point(w, h);
            ICollidableShape = new CollisionRect(
                GridSize.X * Constants.CGRID_SIZE,
                GridSize.Y * Constants.CGRID_SIZE,
                Vector2.Zero, true);
        }
    }
}
