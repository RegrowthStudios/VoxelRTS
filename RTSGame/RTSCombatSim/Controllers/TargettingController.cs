using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    public class TargettingController : ITargettingController {
        private static readonly Random rand = new Random();

        // The Squad That This TargettingController Is Controlling
        public ISquad Squad {
            get;
            private set;
        }

        // The Target Selected By Find Target
        private IEntity target;

        // Constructors
        public TargettingController() {
            Squad = null;
            target = null;
        }

        // Set Entity Only Once
        public void SetSquad(ISquad e) {
            if(Squad != null && Squad != e)
                throw new InvalidOperationException("Controllers Can Only Have Squads Set Once");
            Squad = e;
        }

        // Find The Closest Target On An Opposing Team
        public void FindTarget(GameState g, float dt) {
            float minDist = float.MaxValue;
            foreach(var team in g.Teams) {
                if(team != Squad.Team) {
                    foreach(var unit in team.Units) {
                        if(!unit.IsAlive) return;
                        float dist = (unit.GridPosition - Squad.GridPosition).LengthSquared();
                        if(dist < minDist || (dist == minDist && rand.NextDouble() < 0.5)) {
                            minDist = dist;
                            target = unit;
                        }
                    }
                }
            }
        }
        public void ChangeTarget(GameState g, float dt) {
            foreach(var u in Squad.Combatants) {
                u.Target = target;
            }
        }
    }
}
