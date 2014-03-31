using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team
{
    public class RTSBuilding : IEntity
    {
        public int Health;
        public int CapitalCost { get; private set; }
        public RTSTeam Team { get; private set; }
        public Vector2 GridPosition;
        public float Height;
        public ICollidable ICollidableShape;
        public BoundingBox BBox;

        public Vector3 WorldPosition {
            get { return new Vector3(GridPosition.X, Height, GridPosition.Y); }
        }

        public RTSBuilding() {
            
        }

    }
}
