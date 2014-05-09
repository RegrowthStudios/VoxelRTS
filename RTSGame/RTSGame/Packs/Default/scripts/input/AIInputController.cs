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

namespace RTS.Input {

    public enum AggressionLevel {
        LOW,
        MODERATE,
        HIGH
    }

    public class AI : ACInputController {
        private static readonly Random AI_SEEDER = new Random();
        private const int RareUnitCount = 2; // Within each batch, there are a few rare units that have a different type
        private int MaxUnit; // Maximum number of units I can spawn
        private int[,] unitBatches = new int[3, 10]; // Unit patterns to spawn
        private AggressionLevel aggressionLevel; // How angry I am

        Thread t;
        bool running, paused;

        Random r = new Random(AI_SEEDER.Next());
        private int playerIndex; // Player index
        private RTSTeam player; // Player
        private float playerDistance; // How far is player unit from my barracks?
        private RTSUnit targetUnit; // Target unit
        private float spawnCoolDown; // 
        

        public AI()
            : base() {
            Type = RTSInputType.AI;
        }

        public override void Init(GameState s, int ti) {
            base.Init(s, ti);

            t = new Thread(WorkThread);
            t.IsBackground = true;
            running = true;
            paused = true;
            for (int i = 0; i < s.activeTeams.Length; i++) {
                if (s.activeTeams[i].Team.Input.Type == RTSInputType.Player)
                    playerIndex = s.activeTeams[i].Team.Index;
            }
            player = s.teams[playerIndex];
            InitializeUnitBatches();
            MaxUnit = Team.Buildings.Count * 10;
            aggressionLevel = AggressionLevel.LOW;
            playerDistance = float.MaxValue;
            spawnCoolDown = 0;
        }

        public void DecideAction(GameState g, float dt) {
            spawnCoolDown -= dt;
            // Update aggression level
            if (g.TotalGameTime > 60) {
                aggressionLevel = AggressionLevel.HIGH;
            }
            else if (g.TotalGameTime > 30) {
                aggressionLevel = AggressionLevel.MODERATE;
            }

            // Update player distance
            foreach (RTSUnit unit in player.Units) {
                foreach (RTSBuilding building in Team.Buildings) {
                    if (building.Data.FriendlyName == "Barracks") {
                        float dist = (building.GridPosition - unit.GridPosition).Length();
                        if (dist < playerDistance) {
                            playerDistance = dist;
                            targetUnit = unit;
                        }
                    }
                }
            }
        }

        public void ApplyAction(GameState g, float dt) {
            if (spawnCoolDown < 0) {
                SpawnUnits(g);
                spawnCoolDown = 30;
            }
            MoveUnits(g);
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
            while(running) {
                //if(paused) {
                //    Thread.Sleep(1000);
                //    continue;
                //}
                //switch(r.Next(12)) {
                //    case 0:
                //        SpawnUnits(r);
                //        break;
                //    case 1:
                //        MoveUnits(r);
                //        break;
                //}
                Thread.Sleep(1000);
            }
        }
        private void SpawnUnits(GameState g) {
            
            for (int i = 0; i < Team.Buildings.Count; i++) {
                // For each building spawn a batch of units 
                for (int j = 0; j < unitBatches.GetLength(1); j++) {
                    AddEvent(new SpawnUnitEvent(TeamIndex,
                        unitBatches[i%unitBatches.GetLength(0),j],
                        Team.Buildings[i].GridPosition));
                }
            }

            /*int ui = r.Next(Team.Race.ActiveUnits.Length);
            int cc = Team.Units.Aggregate<RTSUnit, int>(0, (i, u) => {
                if(u.Data == Team.Race.ActiveUnits[ui]) return i + 1;
                else return i;
            });
            cc = Team.Race.ActiveUnits[ui].MaxCount - cc;
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
            }*/
            //DevConsole.AddCommand(string.Format("spawn [{0}, {1}, {2}, {3}, {4}]",
            //    TeamIndex,
            //    ui,
            //    r.Next(1, cc),
            //    r.Next((int)GameState.Map.Width - 20) + 10,
            //    r.Next((int)GameState.Map.Depth - 20) + 10
            //    ));
        }

        private void MoveUnits(GameState g) {
            switch (aggressionLevel) {
                case AggressionLevel.LOW:
                    // If player distance is pretty close
                    if (playerDistance < g.CGrid.numCells.X * 0.1) {
                        // Recruit 5 (or fewer) units to attack target unit
                        int unitsToRecruit = Math.Min(Team.Units.Count, 5);
                        for (int i = 0; i < unitsToRecruit; i++) 
                            Team.Units[i].Target = targetUnit;
                    }
                    break;
                case AggressionLevel.MODERATE:
                    if(playerDistance < g.CGrid.numCells.X * 0.2) {
                        // Recruit 10 (or fewer) units to attack target unit
                        int unitsToRecruit = Math.Min(Team.Units.Count, 10);
                        for (int i = 0; i < unitsToRecruit; i++)
                            Team.Units[i].Target = targetUnit;
                    }
                    break;
                case AggressionLevel.HIGH:
                    // Recruit 5 (or fewer) units to attack target unit
                    int toRecruit = Math.Min(Team.Units.Count, 5);
                    for (int i = 0; i < toRecruit; i++)
                        Team.Units[i].Target = targetUnit;

                    // Recruit 5 (or fewer) units to attack HQ
                    RTSBuilding targetB = null;
                    foreach (RTSBuilding building in player.Buildings) {
                        if (building.Data.FriendlyName == "Headquarters")
                            targetB = building;
                    }
                    for (int i = toRecruit; i < Team.Units.Count; i++) {
                        Team.Units[i].Target = targetB;
                    }
                    break;
            }
            /*var toMove = new List<IEntity>();
            for(int i = 0; i < Team.Units.Count; i++) {
                var unit = Team.Units[i];
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
                ));*/
        }

        private void InitializeUnitBatches() {
            // Initialize unit batches. Each batch holds one type of units with a few rare type.
            for (int i = 0; i < unitBatches.GetLength(0); i++) {
                for (int j = 0; j < unitBatches.GetLength(1); j++) {
                    switch (i) {
                        case 0:
                            if (j < RareUnitCount)
                                unitBatches[i, j] = 1;
                            else
                                unitBatches[i, j] = 0;
                            break;
                        case 1:
                            if (j < RareUnitCount)
                                unitBatches[i, j] = 2;
                            else
                                unitBatches[i, j] = 1;
                            break;
                        case 2:
                            if (j < RareUnitCount)
                                unitBatches[i, j] = 0;
                            else
                                unitBatches[i, j] = 2;
                            break;
                    }
                }
            }
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }
}