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
using Grey.Vox;
using RTS.Packs.Default.scripts.input;
using RTSEngine.Controllers;

namespace RTS.Input {

    public class AI : ACInputController {
        public enum AggressionLevel {
            None,
            VeryLow,
            Low,
            Medium,
            High,
            VeryHigh
        };

        private int numActive;
        private int timeElapsed;
        private int dt;
        private AggressionLevel level;

        Thread thread;
        private bool running;
        private bool paused;

        public Random random;
        public int spawnCap;
        public int[] unitSpawnP;
        List<BarracksController> barracksControllers;
        public RTSTeam player;
        public int playerIndex;
        public List<List<IEntity>> squads;
       
        public AI()
            : base() {
            Type = RTSInputType.AI;
        }

        public override void Init(GameState s, int ti, object args) {
            base.Init(s, ti, args);
            Team.Capital = int.MaxValue/2;
            Team.PopulationCap = int.MaxValue/2;
            Team.OnBuildingSpawn += OnBuildingSpawn;
            Team.OnUnitSpawn += OnUnitSpawn;
            random = new Random();
            spawnCap = 0;
            unitSpawnP = new int[] { 33, 33, 34 };
            barracksControllers = new List<BarracksController>();
            squads = new List<List<IEntity>>();
            numActive = 0;
            timeElapsed = 0;
            level = AggressionLevel.None;
            dt = 2; //5

            foreach (var b in Team.Buildings) {
                DevConsole.AddCommand("added barracks");
                barracksControllers.Add(new BarracksController(this, b));
            }
          
            for (int i = 0; i < s.activeTeams.Length; i++) {
                if (s.activeTeams[i].Team.Input.Type == RTSInputType.Player)
                    playerIndex = s.activeTeams[i].Team.Index;
            }
            player = s.teams[playerIndex];
            
            thread = new Thread(WorkThread);
            thread.IsBackground = true;
            running = true;
            paused = true;
             
        }

        private void IncreaseActive(int n) {
            int numIncrease = n;
            if (n + numActive > barracksControllers.Count) {
                numIncrease = barracksControllers.Count - numActive;
            }
            if (numIncrease == 0) { return; }
            numActive += numIncrease;
            foreach (var bc in barracksControllers) {
                if (!bc.active && numIncrease > 0) {
                    bc.active = true;
                    DevConsole.AddCommand("new active");
                    numIncrease--;
                }
            }
        }

        public void OnUnitSpawn(RTSUnit u) {
            DevConsole.AddCommand("spawn");
            Point cc = HashHelper.Hash(u.GridPosition, GameState.CGrid.numCells, GameState.CGrid.size);
            RTSBuilding b = GameState.CGrid.EStatic[cc.X, cc.Y];
            if (b != null) {
                foreach (var bc in barracksControllers) {
                    if (b.UUID == bc.barracks.UUID) {
                        bc.army.Add(u);
                        u.OnDestruction += bc.OnUnitDeath;
                    }
                }
            }
        }

        public void OnUnitDeath(IEntity u) {
            List<List<IEntity>> toRemove = new List<List<IEntity>>();
            foreach (var s in squads) {
                s.Remove(u);
                if (s.Count == 0) {
                    toRemove.Add(s);
                }
            }
            foreach (var s in toRemove) {
                squads.Remove(s);
            }
        }

        public void OnBuildingSpawn(RTSBuilding b) {
    
            DevConsole.AddCommand("added barracks");
            barracksControllers.Add(new BarracksController(this, b));
            Team.Buildings.Add(b);
            b.OnDestruction += OnBuildingDestruction;

        }

        public void OnBuildingDestruction(IEntity b) {
            DevConsole.AddCommand("building destroyed");
            BarracksController destroyed = null;
            
            foreach (var bc in barracksControllers) {
                if (bc.barracks.UUID == b.UUID) {
                    destroyed = bc;
                }
            }
            if (destroyed != null) {
                foreach (var u in destroyed.army) {
                    u.OnDestruction += OnUnitDeath;
                }
                foreach (var u in destroyed.sentArmy) {
                    u.OnDestruction += OnUnitDeath;
                }
                if (destroyed.active) {
                    numActive--;
                }
                destroyed.Dispose();  
                barracksControllers.Remove(destroyed);
            }
            RTSBuilding bb = b as RTSBuilding;
            Team.Buildings.Remove(bb);
        }

        private void UpdateLevel() {

            AggressionLevel newLevel = level;
            if (timeElapsed > 30 * dt) {
                newLevel = AggressionLevel.VeryHigh;
            }
            else if (timeElapsed > 20 * dt) {
                newLevel = AggressionLevel.High;
            }
            else if (timeElapsed > 12 * dt) {
                newLevel = AggressionLevel.Medium;
            }
            else if (timeElapsed > 6 * dt) {
                newLevel = AggressionLevel.Low;
            }
            else if (timeElapsed > 2 * dt) {
                newLevel = AggressionLevel.VeryLow;
            }

            if (newLevel != level) {
                level = newLevel;
                switch (level) {
                    case AggressionLevel.VeryLow:
                        DevConsole.AddCommand("very low level");
                        IncreaseActive(1);
                        spawnCap += 1;
                        break;
                    case AggressionLevel.Low:
                        DevConsole.AddCommand("low level");
                        IncreaseActive(1);
                        spawnCap += 1;
                        break;
                    case AggressionLevel.Medium:
                        DevConsole.AddCommand("medium level");
                        IncreaseActive(barracksControllers.Count / 3);
                        spawnCap += 2;
                        break;
                    case AggressionLevel.High:
                        DevConsole.AddCommand("high level");
                        IncreaseActive(barracksControllers.Count / 2);
                        spawnCap += 2;
                        break;
                    case AggressionLevel.VeryHigh:
                        DevConsole.AddCommand("very high level");
                        IncreaseActive(barracksControllers.Count);
                        spawnCap += 3;
                        break;
                    default:
                        break;
                }
            }
        }

        private void WorkThread() {
            while (running) {  
                if (paused) {
                    Thread.Sleep(1000);
                    continue;
                }
                UpdateLevel();

                foreach (var bc in barracksControllers) {
                    bc.SpawnUnits();
                    if (bc.active) {
                        bc.DecideTarget();
                        bc.ApplyTarget();
                    }
                }

                foreach (var s in squads) {
                    IEntity target = s.First().Target;
                    if (target == null || !target.IsAlive) {
                        target = GetClosestTarget(s);
                        if (target != null) {
                            SetTarget(s, target);
                        }
                    }
                }
                timeElapsed++;
                DevConsole.AddCommand("tick");
                Thread.Sleep(2000);
            }
        }

        private IEntity GetClosestTarget(List<IEntity> squad) {
            IEntity target = null;
            Vector2 pos = GetSquadPos(squad);
            float minDist = float.MaxValue;
            foreach (var b in player.Buildings) {
                float dist = (b.GridPosition - pos).Length();
                if (dist < minDist && b.IsAlive) {
                    minDist = dist;
                    target = b;
                }
            }
            if (target != null && target.IsAlive) { return target; }
            minDist = float.MaxValue;
            foreach (var u in player.Units) {
                float dist = (u.GridPosition - pos).Length();
                if (dist < minDist && u.IsAlive) {
                    minDist = dist;
                    target = u;
                }
            }
            return target;
        }

        private Vector2 GetSquadPos(List<IEntity> squad) {
            Vector2 sum = Vector2.Zero;
            foreach (var u in squad) {
                sum.X += u.GridPosition.X;
                sum.Y += u.GridPosition.Y;
            }
            return new Vector2(sum.X / squad.Count, sum.Y / squad.Count);
        }

        public override void Begin() {
            thread.Start();
            paused = false;
        }

        public override void Dispose() {
            running = false;
            paused = false;
            thread.Join();
        }

        public void SetTarget(List<IEntity> units, IEntity t) {
            AddEvent(new SelectEvent(TeamIndex, units));
            foreach (var u in units) {
                AddEvent(new SetOrdersEvent(TeamIndex, u.UUID, BehaviorFSM.AttackMove, 3));
            }
            AddEvent(new SetWayPointEvent(TeamIndex, t.GridPosition));
        }


     
        #region Level Editor
        public override List<LEVoxel> CreateVoxels(VoxAtlas atlas) {
            return null;
        }
        public override void LESave(VoxWorld world, int w, int h, DirectoryInfo dir) {
            return;
        }
        #endregion

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }
}