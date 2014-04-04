using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace RTSEngine.Controllers {
    public class EnvironmentInputController : InputController {
        Thread thread;
        private bool running;
        private bool paused;

        private Random random;
        private ImpactGrid grid;
        private int counter;
        private Point[] treePositions;

        private IndexedBuildingType tree;
        private IndexedBuildingType ore;
        private IndexedUnitType walkingTree1;
        private IndexedUnitType walkingTree2;
        private IndexedUnitType rockGolem1;
        private IndexedUnitType rockGolem2;


        private const int LEVEL_ONE = 10;
        private const int LEVEL_TWO = 20;
        private const int RECOVER_IMPACT = 20;
        private const int MAX_TREE_SPAWN = 10;
        private const int MAX_OFFSET = 10;
        private const int MIN_RECOVER = 5;
        private const int MAX_RECOVER = 10;
        private const int MAX_UNIT_SPAWN = 10;
        private const int RECOVER_TIME = 30;
        private const int SPAWN_TIME = 60;

        public EnvironmentInputController(GameState g, int ti)
            : base(g, ti) {
            grid = g.IGrid;
            random = new Random();
            thread = new Thread(WorkThread);
            thread.IsBackground = true;
            running = true;
            paused = true;
            thread.Start();
        }

        public void Init(IndexedUnitType wt1, IndexedUnitType wt2, IndexedUnitType rg1, IndexedUnitType rg2,
            IndexedBuildingType t, IndexedBuildingType o, Point[] treePos, Point[] orePos) {
            walkingTree1 = wt1;
            walkingTree2 = wt2;
            rockGolem1 = rg1;
            rockGolem2 = rg2;
            tree = t;
            ore = o;
            treePositions = treePos;
            for(int i = 0; i < treePositions.Length; i++) {
                AddEvent(new SpawnBuildingEvent(TeamIndex, tree.Index, treePositions[i]));
            }
            for(int j = 0; j < orePos.Length; j++) {
                AddEvent(new SpawnBuildingEvent(TeamIndex, ore.Index, orePos[j]));
            }
        }

        private void WorkThread() {
            while(running) {
                if(paused) {
                    Thread.Sleep(1000);
                    continue;
                }
                if(counter % SPAWN_TIME == 0) {
                    SpawnUnits();
                    counter = counter % (SPAWN_TIME + RECOVER_TIME);
                }
                if(counter % RECOVER_TIME == 0) {
                    Recover();
                    counter = counter % (SPAWN_TIME + RECOVER_TIME);
                }
                counter++;
                Thread.Sleep(1000);
            }
        }

        private void Recover() {
            foreach(var r in GameState.Regions) {
                if(r.RegionImpact < RECOVER_IMPACT && r.RegionImpact > 0) {
                    // Randomly Choose The Location Of A Starting Tree
                    int tp = random.Next(treePositions.Length);
                    Point treeLocation = treePositions[tp];
                    Point offset;

                    // Spawn A Random Number Of Trees 
                    int numTrees = random.Next(MAX_TREE_SPAWN);
                    for(int i = 0; i < numTrees; i++) {
                        offset.X = random.Next(MAX_OFFSET);
                        offset.Y = random.Next(MAX_OFFSET);
                        AddEvent(new SpawnBuildingEvent(TeamIndex, tree.Index, new Point(treeLocation.X + offset.X, treeLocation.Y + offset.Y)));
                        r.AddToRegionImpact(-tree.Data.Impact);
                    }

                    // Regenerate Ore Health
                    foreach(var c in r.Cells) {
                        foreach(var o in GameState.IGrid.ImpactGenerators[c.X, c.Y]) {
                            if(o.Data.FriendlyName.Equals(ore.Data.FriendlyName)) {
                                int recover = random.Next(MIN_RECOVER, MAX_RECOVER);
                                o.Health += recover;
                                r.AddToRegionImpact(-(ore.Data.Impact / recover));
                            }
                        }
                    }
                }
            }
        }

        private void SpawnUnits() {
            foreach(var r in GameState.Regions) {
                if(r.RegionImpact > LEVEL_ONE) {
                    // Find The Cell With The Largest Impact
                    Point p = r.Cells.First();
                    foreach(var c in r.Cells) {
                        if(grid.CellImpact[c.X, c.Y] > grid.CellImpact[p.X, p.Y]) {
                            p = c;
                        }
                    }
                    // Randomly Choose An Impact Generator In That Cell
                    ImpactGenerator g = null;
                    while(g == null || !g.Data.FriendlyName.Equals(ore.Data.FriendlyName) || !g.Data.FriendlyName.Equals(tree.Data.FriendlyName)) {
                        int i = random.Next(grid.ImpactGenerators[p.X, p.Y].Count);
                        g = grid.ImpactGenerators[p.X, p.Y][i];
                    }
                    // Spawn Environmental Units In That Cell
                    IndexedUnitType spawnData;
                    if(g.Data.FriendlyName.Equals(ore.Data.FriendlyName)) {
                        if(r.RegionImpact > LEVEL_TWO)
                            spawnData = rockGolem2;
                        else
                            spawnData = rockGolem1;
                    }
                    else {
                        if(r.RegionImpact > LEVEL_TWO)
                            spawnData = walkingTree2;
                        else
                            spawnData = walkingTree1;
                    }
                    Vector2 spawnPos = g.Position;
                    Vector2 offset;
                    int numUnits = random.Next(MAX_UNIT_SPAWN);
                    for(int j = 0; j < numUnits; j++) {
                        offset.X = random.Next(MAX_OFFSET);
                        offset.Y = random.Next(MAX_OFFSET);
                        AddEvent(new SpawnUnitEvent(TeamIndex, spawnData.Index, spawnPos + offset));
                    }
                }
            }
        }

        public void Start() {
            counter = 0;
            paused = false;
        }
        public override void Dispose() {
            running = false;
        }
    }
}