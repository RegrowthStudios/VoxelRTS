using RTS.Input;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTS.Packs.Default.scripts.input {
    class BarrackController {

        public RTSBuilding barrack;
        public List<IEntity> army;
        public List<IEntity> sentArmy;
        private AI AIInput;
        public RTSBuilding target;

        public BarrackController(AI input, RTSBuilding b) {
            army = new List<IEntity>();
            sentArmy = new List<IEntity>();
            AIInput = input;
            barrack = b;
            b.OnUnitSpawn += OnUnitSpawn;
        }

        public void SpawnUnits() {
            if (army.Count > AIInput.spawnCap) { DevConsole.AddCommand("full");  return; }
            int numSpawn = AIInput.spawnCap - army.Count;
            int type;
            for (int i = 0; i < numSpawn; i++) {
                int j = AIInput.random.Next(100);
                if (j > AIInput.unitSpawnP[0] + AIInput.unitSpawnP[1]) {
                    type = 2;
                }
                else if (j > AIInput.unitSpawnP[1]) {
                    type = 1;
                }
                else {
                    type = 0;
                }
                AIInput.AddEvent(new SpawnUnitEvent(AIInput.TeamIndex, type, barrack.GridPosition, barrack.UUID));
            } 
        }

        public void OnUnitSpawn(RTSUnit u) {
            DevConsole.AddCommand("spawn");
            army.Add(u);
            u.OnDestruction += OnUnitDeath;
        }

        public void OnUnitDeath(IEntity e) {
            DevConsole.AddCommand("death");
            bool removedArmy = army.Remove(e);
            bool removedSentArmy = sentArmy.Remove(e);
        }

        public void DecideTarget() {
            if (target != null) { return; }
            float minDist = float.MaxValue;
            foreach (var b in AIInput.player.Buildings) {
                float dist = (b.GridPosition - barrack.GridPosition).Length();
                if (dist < minDist) {
                    minDist = dist;
                    target = b;
                }
            }
        }

        public void ApplyTarget() {
            if (army.Count < AIInput.spawnCap || sentArmy.Count > 0 || target == null) { return; }
            foreach (var u in army) {
                sentArmy.Add(u);
            }
            army.Clear();

            AIInput.AddEvent(new SelectEvent(AIInput.TeamIndex, sentArmy));
            AIInput.AddEvent(new SetTargetEvent(AIInput.TeamIndex, target));
            
        }

        public void Dispose() {
            if (army.Count > 0 && target == null) {
                AIInput.AddEvent(new SelectEvent(AIInput.TeamIndex, army));
                AIInput.AddEvent(new SetTargetEvent(AIInput.TeamIndex, target));
            }
            foreach (var u in army) {
                u.OnDestruction -= OnUnitDeath;
            }
            barrack.OnUnitSpawn -= OnUnitSpawn;
        }
    }
}
