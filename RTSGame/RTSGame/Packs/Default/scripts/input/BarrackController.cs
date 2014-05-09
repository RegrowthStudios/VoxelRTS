using RTS.Input;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTS.Packs.Default.scripts.input {
    class BarrackController {

        private RTSBuilding barrack;
        private List<IEntity> army;    
        private AI AIInput;
        private Random random;
        private RTSBuilding target;

        public void Init(AI input, RTSBuilding b) {
            army = new List<IEntity>();
            random = new Random();
            AIInput = input;
            barrack = b;
            AIInput.Team.OnUnitSpawn += OnUnitSpawn;
        }

        public void SpawnUnits() {
            if (army.Count > AIInput.spawnCap) { return; }
            int numSpawn = AIInput.spawnCap - army.Count;
            int[,] unitBatches = AIInput.unitBatches;
            int i = random.Next(unitBatches.GetLength(0));
            for (int j = 0; j < unitBatches.GetLength(1) && i <= numSpawn; j++) {
                AIInput.AddEvent(new SpawnUnitEvent(AIInput.TeamIndex, unitBatches[i, j], barrack.GridPosition));
            } 
        }

        public void OnUnitSpawn(RTSUnit u) {
            army.Add(u);
            u.OnDestruction += OnUnitDeath;
        }

        public void OnUnitDeath(IEntity e) {
            IEntity dead = null;
            foreach (var u in army) {
                if (e.UUID == u.UUID) {
                    dead = e;
                }
            }
            if (dead != null) {
                army.Remove(dead);
            }
        }

        public void DecideTarget() {
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
            if (army.Count != AIInput.spawnCap) { return; }
            AIInput.AddEvent(new SelectEvent(AIInput.TeamIndex, army));
            AIInput.AddEvent(new SetTargetEvent(AIInput.TeamIndex, target)); 
        }
    }
}
