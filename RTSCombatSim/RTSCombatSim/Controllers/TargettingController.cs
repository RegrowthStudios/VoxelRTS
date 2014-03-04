using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    public class TargettingController : ITargettingController {
        // The Entity That This TargettingController Is Controlling
        public IEntity Entity {
            get;
            private set;
        }

        // The Target Selected By Find Target
        private IEntity target;

        // Constructors
        public TargettingController() {
            Entity = null;
            target = null;
        }

        // Set Entity Only Once
        public void SetEntity(IEntity e) {
            if(Entity != null)
                throw new InvalidOperationException("Controllers Can Only Have Entities Set Once");
            Entity = e;
        }

        // Find The Closest Target On An Opposing Team
        public void FindTarget(GameState g, float dt) {
            foreach(var team in g.teams) {
                if(team != Entity.Team) {
                    float minDist = float.MaxValue;
                    foreach(var unit in team.Units) {
                        float dist = (unit.GridPosition - Entity.GridPosition).LengthSquared();
                        if(dist < minDist) {
                            minDist = dist;
                            target = unit;
                        }
                    }
                }
            }
        }
        public void ChangeTarget(GameState g, float dt) {
            Entity.Target = target;
        }
    }
}
