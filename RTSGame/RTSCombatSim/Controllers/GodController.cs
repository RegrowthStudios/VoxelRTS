using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;

namespace RTSCS.Controllers {
    public class GodController : IActionController, ICombatController, IMovementController {
        RTSUnitInstance unit;
        public IEntity Entity {
            get { return unit; }
        }

        public IEnumerable<Vector2> Waypoints {
            get { throw new NotImplementedException(); }
        }

        public GodController() {
            unit = null;
        }

        public void DecideAction(GameState g, float dt) {
        }
        public void ApplyAction(GameState g, float dt) {
            if(unit.Health < 100000)
                unit.Damage(unit.Health - 100000);
            ApplyMove(g, dt);
            Attack(g, dt);
        }

        public void SetEntity(IEntity e) {
            unit = e as RTSUnitInstance;
        }
        public void Attack(GameState g, float dt) {
            foreach(var t in g.teams) {
                if(t == unit.Team)
                    continue;
                foreach(var u in t.Units) {
                    u.Damage(100000);
                }
            }
        }

        public void SetWaypoints(Vector2[] p) {
        }
        public void DecideMove(GameState g, float dt) {
        }
        public void ApplyMove(GameState g, float dt) {
            unit.Move(Vector2.Zero - unit.GridPosition);
        }

        public void FindTarget(GameState g, float dt) {
        }
        public void ChangeTarget(GameState g, float dt) {
        }
    }
}
