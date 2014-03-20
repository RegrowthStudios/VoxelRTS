using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;

namespace RTSEngine.Controllers {
    public class AIInputController : InputController {
        Thread t;
        bool running;
        int teamIndex;


        public AIInputController(GameState g, RTSTeam t, int ti)
            : base(g, t) {
            teamIndex = ti;
        }

        public override void Dispose() {
        }

        private void WorkThread() {
            Random r = new Random();
            while(running) {
                switch(r.Next(2)) {
                    case 0: SpawnUnits(r); break;
                    case 1: MoveUnits(r); break;
                }
                Thread.Sleep(1000);
            }
        }
        private void SpawnUnits(Random r) {
            int ui = r.Next(3);
            int cc = Team.units.Aggregate<RTSUnit, int>(0, (i, u) => {
                if(u.UnitData == Team.unitData[ui]) return i + 1;
                else return i;
            });
            cc = Team.unitData[ui].MaxCount - cc;
            if(cc > 10) cc = 10;
            if(cc < 1) return;
            DevConsole.AddCommand(string.Format("spawn [{0}, {1}, {2}, {3}, {4}]",
                teamIndex,
                ui,
                r.Next(1, cc),
                r.Next((int)GameState.Map.Width - 20) + 10,
                r.Next((int)GameState.Map.Depth - 20) + 10
                ));
        }
        private void MoveUnits(Random r) {
            var toMove = new List<IEntity>();
            foreach(var unit in Team.units) {
                if(r.Next(100) > 80)
                    toMove.Add(unit);
            }
            if(toMove.Count < 1) return;
            //AddEvent(new SelectEvent(toMove));
        }
    }
}
