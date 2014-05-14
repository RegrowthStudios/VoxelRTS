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
        Thread thread;
        private bool running;
        private bool paused;

        public Random random;
        public int spawnCap;
        public int[] unitSpawnP;
        List<BarrackController> barrackControllers;
        public RTSTeam player;
        public int playerIndex;
        public List<List<IEntity>> squads;
       
        public AI()
            : base() {
            Type = RTSInputType.AI;
        }

        public override void Init(GameState s, int ti, object args) {
            base.Init(s, ti, args);

            Team.OnBuildingSpawn += OnBuildingSpawn;
            random = new Random();
            spawnCap = 1;
            unitSpawnP = new int[] { 33, 33, 34 };
            barrackControllers = new List<BarrackController>();
            squads = new List<List<IEntity>>();

         
            foreach (var b in Team.Buildings) {
                DevConsole.AddCommand("added barracks");
                barrackControllers.Add(new BarrackController(this, b));
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

        public void OnBuildingSpawn(RTSBuilding b) {
    
            DevConsole.AddCommand("added barracks");
            barrackControllers.Add(new BarrackController(this, b));
            Team.Buildings.Add(b);
            b.OnDestruction += OnBuildingDestruction;

        }

        public void OnBuildingDestruction(IEntity b) {
            BarrackController destroyed = null;
            
            foreach (var bc in barrackControllers) {
                if (bc.barrack.UUID == b.UUID) {
                    destroyed = bc;
                }
            }
            if (destroyed != null) {
                destroyed.Dispose();
                barrackControllers.Remove(destroyed);
            }

            RTSBuilding destroyedB = null;
            foreach (var bb in Team.Buildings) {
                if (bb.UUID == b.UUID) {
                    destroyedB = bb;
                }
            }
            if (destroyedB != null) {
                Team.Buildings.Remove(destroyedB);
            }
        }

        private void WorkThread() {
            while (running) {
                
                if (paused) {
                    Thread.Sleep(1000);
                    continue;
                }
                DevConsole.AddCommand("thread");
                foreach (var bc in barrackControllers) {
                    bc.SpawnUnits();
                    bc.DecideTarget();
                    bc.ApplyTarget();
                }
                
                Thread.Sleep(2000);
            }
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