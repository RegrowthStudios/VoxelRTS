using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;
using System.IO;

namespace RTSEngine.Controllers {
    // TODO: Make This A Better AI
    public class AIInputController : InputController {
        Thread t;
        bool running, paused;

        public AIInputController(GameState g, int ti)
            : base(g, ti, InputType.AI) {
            t = new Thread(WorkThread);
            t.IsBackground = true;
            running = true;
            paused = true;
        }
        public override void Begin() {
            t.Start();
            paused = false;
        }
        public override void Dispose() {
            running = false;
            paused = false;
            t.Join();
        }

        private void WorkThread() {
            Random r = new Random();
            while(running) {
                if(paused) {
                    Thread.Sleep(1000);
                    continue;
                }
                switch(r.Next(12)) {
                    case 0:
                        SpawnUnits(r);
                        break;
                    case 1:
                        MoveUnits(r);
                        break;
                }
                Thread.Sleep(1000);
            }
        }
        private void SpawnUnits(Random r) {
            int ui = r.Next(Team.Race.ActiveUnits.Length);
            int cc = Team.Units.Aggregate<RTSUnit, int>(0, (i, u) => {
                if(u.UnitData == Team.Race.ActiveUnits[ui].Data) return i + 1;
                else return i;
            });
            cc = Team.Race.ActiveUnits[ui].Data.MaxCount - cc;
            if(cc > 10) cc = 10;
            if(cc < 1) return;
            int uc = r.Next(1, cc);
            for(int i = 0; i < uc; i++) {
                AddEvent(new SpawnUnitEvent(
                    TeamIndex,
                    ui,
                    new Vector2(
                        r.Next((int)GameState.Map.Width - 20) + 10,
                        r.Next((int)GameState.Map.Depth - 20) + 10)
                    ));
            }
            //DevConsole.AddCommand(string.Format("spawn [{0}, {1}, {2}, {3}, {4}]",
            //    TeamIndex,
            //    ui,
            //    r.Next(1, cc),
            //    r.Next((int)GameState.Map.Width - 20) + 10,
            //    r.Next((int)GameState.Map.Depth - 20) + 10
            //    ));
        }
        private void MoveUnits(Random r) {
            var toMove = new List<IEntity>();
            foreach(var unit in Team.Units) {
                if(r.Next(100) > 80)
                    toMove.Add(unit);
            }
            if(toMove.Count < 1) return;
            AddEvent(new SelectEvent(TeamIndex, toMove));
            AddEvent(new SetWayPointEvent(
                TeamIndex,
                new Vector2(
                    r.Next((int)GameState.Map.Width - 20) + 10,
                    r.Next((int)GameState.Map.Depth - 20) + 10)
                ));
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }
}