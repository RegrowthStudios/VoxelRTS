using Microsoft.Xna.Framework;
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
    class BarracksController {

        public RTSBuilding barracks;
        public List<IEntity> army;
        public List<IEntity> sentArmy;
        private AI AIInput;
        public IEntity target;
        public int? currentTarget;

        public BarracksController(AI input, RTSBuilding b) {
            army = new List<IEntity>();
            sentArmy = new List<IEntity>();
            AIInput = input;
            barracks = b;
            currentTarget = null;
        }

        public void SpawnUnits() {
            if (army.Count > AIInput.spawnCap) { return; }
            int numSpawn = AIInput.spawnCap - army.Count;
            for (int i = 0; i < numSpawn; i++) {
                int type = AIInput.random.Next(barracks.ButtonControllers.Count);
                barracks.ButtonControllers[type].OnQueueFinished(AIInput.GameState);                
            } 
        }

        public void OnUnitDeath(IEntity e) {
            DevConsole.AddCommand("death");
            bool removedArmy = army.Remove(e);
            bool removedSentArmy = sentArmy.Remove(e);
        }

        public void DecideTarget() {
            if (target != null && target.IsAlive) { return; }
            // Set Closest Building As Target
            float minDist = float.MaxValue;
            foreach (var b in AIInput.player.Buildings) {
                float dist = (b.GridPosition - barracks.GridPosition).Length();
                if (dist < minDist && b.IsAlive) {
                    minDist = dist;
                    target = b;
                }
            }
            // If Player Has No More Buildings, Set Closest Unit As Target
            if (target != null && target.IsAlive) { DevConsole.AddCommand("found building target");  return; }
            minDist = float.MaxValue;
            foreach (var u in AIInput.player.Units) {
                float dist = (u.GridPosition - barracks.GridPosition).Length();
                if (dist < minDist && u.IsAlive) {
                    minDist = dist;
                    target = u;
                }
            }
            DevConsole.AddCommand("found unit target");
        }

        public void ApplyTarget() {
            if (target == null || !target.IsAlive) { DevConsole.AddCommand("return");  return; }
            if (sentArmy.Count == 0 && army.Count == AIInput.spawnCap) {
                sentArmy.Clear();
                foreach (var u in army) {
                    DevConsole.AddCommand("added unit");
                    sentArmy.Add(u);
                }
                army.Clear();
                DevConsole.AddCommand("targetting 1");
                SetTarget(sentArmy);
                currentTarget = target.UUID;
            }
            else if (sentArmy.Count > 0 && (!currentTarget.HasValue || currentTarget != target.UUID)) {
                DevConsole.AddCommand("targetting 2");
                SetTarget(sentArmy);
                currentTarget = target.UUID;
            }
        }

        private void SetTarget(List<IEntity> units) {
            AIInput.AddEvent(new SelectEvent(AIInput.TeamIndex, units));
            foreach (var u in units) {
                AIInput.AddEvent(new SetOrdersEvent(AIInput.TeamIndex, u.UUID, BehaviorFSM.AttackMove, 3));
            }
            AIInput.AddEvent(new SetWayPointEvent(AIInput.TeamIndex, target.GridPosition));
        }

        public void Dispose() {
            DecideTarget();
            if (army.Count > 0 && target != null) {
                SetTarget(army);
                AIInput.squads.Add(army);
            }
            if (sentArmy.Count > 0) {
                AIInput.squads.Add(sentArmy);
            }
            foreach (var u in army) {
                u.OnDestruction -= OnUnitDeath;
            }
        }
    }
}
