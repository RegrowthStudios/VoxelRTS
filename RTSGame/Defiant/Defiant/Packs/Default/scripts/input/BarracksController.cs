﻿using Microsoft.Xna.Framework;
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
        public RTSBuilding target;

        public BarracksController(AI input, RTSBuilding b) {
            army = new List<IEntity>();
            sentArmy = new List<IEntity>();
            AIInput = input;
            barracks = b;
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
            if (target != null) { return; }
            float minDist = float.MaxValue;
            foreach (var b in AIInput.player.Buildings) {
                float dist = (b.GridPosition - barracks.GridPosition).Length();
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
            DecideTarget();
            if (army.Count > 0 && target != null) {
                AIInput.AddEvent(new SelectEvent(AIInput.TeamIndex, army));
                AIInput.AddEvent(new SetTargetEvent(AIInput.TeamIndex, target));
            }
            foreach (var u in army) {
                u.OnDestruction -= OnUnitDeath;
            }
        }
    }
}