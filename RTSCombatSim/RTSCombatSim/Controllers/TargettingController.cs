using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    public class TargettingController : ITargettingController {
        // The Entity That This TargettingController Is Controlling
        private IEntity entity;
        public IEntity Entity {
            get { return entity; }
        }

        // The Target Selected By Find Target
        private IEntity target;

        // Find The Closest Target On An Opposing Team
        public void FindTarget(GameState g, float dt) {
            foreach(var team in g.teams) {
                if(team != entity.Team) {
                    float minDist = float.MaxValue;
                    foreach(var unit in team.Units) {
                        float dist = (unit.GridPosition - entity.GridPosition).LengthSquared();
                        if(dist < minDist) {
                            minDist = dist;
                            target = unit;
                        }
                    }
                }
            }
        }

        public void ChangeTarget(GameState g, float dt) {
            entity.Target = target;
        }

        // Constructor
        public TargettingController(IEntity entity, IEntity target) {
            this.entity = entity;
            this.target = target;
        }

        public TargettingController(IEntity entity) {
            this.entity = entity;
            this.target = null;
        }
    }
}
