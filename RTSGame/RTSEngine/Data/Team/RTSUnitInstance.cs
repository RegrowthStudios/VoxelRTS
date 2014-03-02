using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team
{
    // TODO: Implement IDestructibleEntity, ICombatEntity, IMovingEntity

    class RTSUnitInstance : IDestructibleEntity, ICombatEntity, IMovingEntity
    {

        //RTSUnit Data of The Unit
        protected RTSUnit UnitData;

        //MovementController of The Unit
        protected IMovementContoller MovementController;

        //ActionController of The Unit
        protected IActionController ActionController;

        //TargettingController of The Unit
        protected ITargettingController TargettingController;

        //CombatController of The Unit
        protected ICombatController CombatController;

            
        protected RTSTeam RTSTeam;
        protected Vector3 Position;
        protected ICollidable Shape;
        protected IEntity CurrentTarget;
        protected Boolean AbleToAttack;   

        // The Entity's Team
        public RTSTeam Team { 
            get { return RTSTeam; }
        }

        // Location In The World
        public Vector3 WorldPosition { 
            get { return Position; } 
        }

        // Collision Geometry
        public ICollidable CollisionGeometry {
            get { return Shape; }
        }

        // Targetting Information 
        public IEntity Target { 
            get { return CurrentTarget;}
        }

        // Speed Of Movement For The Entity
        public float MovementSpeed {
            get { return UnitData.MovementSpeed; }
        }

        // The Current Health Of The Entity
        public int Health {
            get { return UnitData.Health ; }
        }

        // Information About Whether This Entity Can Attack Yet
        public bool CanAttack { 
            get { return AbleToAttack; }
        }



    }
}
