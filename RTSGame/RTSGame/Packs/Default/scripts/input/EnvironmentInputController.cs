using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Parsers;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.IO;
using RTSEngine.Controllers;

namespace RTS.Input {
    public class Environment : ACInputController {
        Thread thread;
        private bool running;
        private bool paused;

        private Random random;
        private ImpactGrid grid;
        private int counter;

        private List<Point> treeLocations;
        public RTSBuildingData FloraData {
            get { return Team.Race.Buildings[FloraType]; }
        }
        public RTSBuildingData OreData {
            get { return Team.Race.Buildings[OreType]; }
        }

        // Indices For Resources And Units
        private EnvironmentData eData;
        private int FloraType {
            get { return eData.FloraType; }
        }
        private int OreType {
            get { return eData.OreType; }
        }
        private int MinionType {
            get { return eData.MinionType; }
        }
        private int TankType {
            get { return eData.TankType; }
        }
        private int TitanType {
            get { return eData.TitanType; }
        }
        // Time Intervals At Which Recovery And Spawning Happens (In Seconds)
        private int RecoverTime {
            get { return eData.DisasterTime * 2; }
        }
        private int DisasterTime {
            get { return eData.DisasterTime; }
        }
        // Impact Levels For Level One, Level Two, Level Three
        private int L1Impact {
            get { return eData.L1Impact; }
        }
        private int L2Impact {
            get { return eData.L2Impact; }
        }
        private int L3Impact {
            get { return eData.L3Impact; }
        }
        // Impact Level At Which Region Will No Longer Recover
        private int PointOfNoReturn {
            get { return eData.NoLongerRecoverImpact; }
        }
        // Maximum Number Of Units Per Region For Each Level
        private IEnumerable<int> SpawnCaps {
            get {
                yield return eData.L1SpawnCap;
                yield return eData.L2SpawnCap;
                yield return eData.L3SpawnCap;
            }
        }
        // Impact Recovered Per Region Per Recovery Phase
        private int RecoverImpact {
            get { return eData.RecoverImpact; }
        }
        // Environment Unit Spawn Offset
        private int SpawnOffset {
            get { return eData.SpawnOffset; }
        }
        // Minimum Number Of Units Spawned For Each Level
        private int[][] minNumSpawn;
        // Maximum Number Of Units Spawned For Each Level
        private int[][] maxNumSpawn;
        // Array Of Possible Spawn Types
        private IEnumerable<int> Spawns {
            get {
                yield return MinionType;
                yield return TankType;
                yield return TitanType;
            }
        }

        public Environment()
            : base() {
            Type = RTSInputType.Environment;
        }

        public override void Init(GameState s, int ti) {
            base.Init(s, ti);
            grid = GameState.IGrid;
            random = new Random();
            thread = new Thread(WorkThread);
            thread.IsBackground = true;
            running = true;
            paused = true;
            eData = EnvironmentDataParser.Parse(Team.Race.InfoFile);
            minNumSpawn = new int[3][] { eData.L1MinNumSpawn, eData.L1MaxNumSpawn, eData.L2MinNumSpawn };
            maxNumSpawn = new int[3][] { eData.L2MaxNumSpawn, eData.L3MinNumSpawn, eData.L3MaxNumSpawn };

            for(int i = 0; i < Team.Buildings.Count; i++) {
                Team.Buildings[i].OnDamage += OnBuildingDamage;
            }
            Team.OnBuildingSpawn += (b) => {
                b.OnDamage += OnBuildingDamage;
            };
        }

        private void WorkThread() {
            DevConsole.AddCommand("Began");
            while(running) {
                if(paused) {
                    Thread.Sleep(1000);
                    continue;
                }
                DevConsole.AddCommand("Tick");
                DevConsole.AddCommand("DT: " + (counter % DisasterTime));
                UpdateUnitsInRegion();
                if(counter % DisasterTime == 0) {
                    SpawnUnits();
                    SetInitTarget();
                }
                if(counter % RecoverTime == 0) {
                    Recover();
                }
                counter++;
                counter = counter % (RecoverTime);
                Thread.Sleep(1000);
            }
            DevConsole.AddCommand("Exited");
        }

        public override void Begin() {
            counter = 0;
            thread.Start();
            paused = false;
        }
        public override void Dispose() {
            running = false;
            paused = false;
            thread.Join();
        }

        private void UpdateUnitsInRegion() {
            foreach(var r in GameState.Regions) {
                r.units = new List<IEntity>();
            }
            foreach(var u in Team.Units) {
                Point unitI = HashHelper.Hash(u.GridPosition, grid.numCells, grid.size);
                grid.Region[unitI.X, unitI.Y].units.Add(u);
            }
        }

        // Recovery Phase
        private void Recover() {
            if(treeLocations == null || treeLocations.Count < 1) return;

            foreach(var r in GameState.Regions) {
                if(r.RegionImpact < PointOfNoReturn && r.RegionImpact > 0) {
                    // Randomly Choose The Location Of A Starting Tree
                    int tp = random.Next(treeLocations.Count);
                    Point treeC = treeLocations[tp];

                    // Spawn Trees Around The Starting Tree
                    for(int x = -1; x <= 1; x += 2) {
                        for(int y = -1; y <= 1; y += 2) {
                            Point newTreeC = new Point(treeC.X + x, treeC.Y + y);
                            Vector2 newTreePos = new Vector2(newTreeC.X * GameState.CGrid.size.X, newTreeC.Y * GameState.CGrid.size.Y);
                            Point newTreeI = HashHelper.Hash(newTreePos, grid.numCells, grid.size);
                            foreach(var t in GameState.IGrid.ImpactGenerators[newTreeI.X, newTreeI.Y]) {
                                Point tc = HashHelper.Hash(t.GridPosition, GameState.CGrid.numCells, GameState.CGrid.size);
                                if(!Point.Equals(tc, newTreeC) && tc.X > -1 && tc.Y > -1 && tc.X < GameState.CGrid.numCells.X && tc.Y < GameState.CGrid.numCells.Y) {
                                    AddEvent(new SpawnBuildingEvent(TeamIndex, FloraType, newTreeC));
                                    r.AddToRegionImpact(-FloraData.Impact);
                                }
                            }
                        }
                    }

                    // Regenerate Ore Health
                    foreach(var c in r.Cells) {
                        foreach(var o in GameState.IGrid.ImpactGenerators[c.X, c.Y]) {
                            if(o.BuildingData.FriendlyName.Equals(OreData.FriendlyName)) {
                                o.Health += RecoverImpact;
                                r.AddToRegionImpact(-(OreData.Impact / RecoverImpact));
                            }
                        }
                    }

                }
            }
        }

        // Disaster Phase
        private void SpawnUnits() {
            foreach(var r in GameState.Regions) {
                // Decide Level
                int level;
                if(r.RegionImpact > L3Impact)
                    level = 3;
                else if(r.RegionImpact > L2Impact)
                    level = 2;
                else if(r.RegionImpact > L1Impact)
                    level = 1;
                else
                    level = 0;
                if(level > 0) {
                    DevConsole.AddCommand("Has Level");

                    // Find The Cell With The Largest Impact
                    Point p = r.Cells.First();
                    foreach(var c in r.Cells) {
                        if(grid.CellImpact[c.X, c.Y] > grid.CellImpact[p.X, p.Y]) {
                            p = c;
                        }
                    }
                    // Randomly Choose An Impact Generator In That Cell
                    ImpactGenerator g = null;

                    // TODO: Choose Random
                    int rpi = random.Next(r.Cells.Count);
                    Vector2 spawnPos = new Vector2(r.Cells[rpi].X, r.Cells[rpi].Y) * grid.cellSize + Vector2.One;

                    if(grid.ImpactGenerators[p.X, p.Y].Count != 0) {
                        var ig = grid.ImpactGenerators[p.X, p.Y];
                        int igi = random.Next(ig.Count);
                        for(int i = 0; i < ig.Count; i++) {
                            if(ig[i].BuildingData.FriendlyName.Equals(OreData.FriendlyName) ||
                                ig[i].BuildingData.FriendlyName.Equals(FloraData.FriendlyName)) {
                                igi--;
                                if(igi == 0)
                                    g = ig[i];
                            }
                        }
                        spawnPos = g.GridPosition;
                    }

                    // Spawn Environmental Units
                    Vector2 offset;
                    int numSpawn;
                    //FIX
                    int ti = 0;
                    foreach(var spawnType in Spawns) {
                        numSpawn = random.Next(minNumSpawn[level - 1][ti], maxNumSpawn[level - 1][ti]);
                        offset.X = random.Next(SpawnOffset);
                        offset.Y = random.Next(SpawnOffset);
                        for(int j = 0; j < numSpawn; j++) {
                            AddEvent(new SpawnUnitEvent(TeamIndex, spawnType, spawnPos + offset));
                        }
                        ti++;
                    }
                }
            }
        }

        private void SetInitTarget() {
            foreach(var r in GameState.Regions) {
                // Select Units Not In A Squad
                List<IEntity> squad = new List<IEntity>();
                foreach(RTSUnit u in r.units) {
                    if(u.Squad.Units.Count == 1)
                        squad.Add(u);
                }
                AddEvent(new SelectEvent(TeamIndex, squad));
                // Set The Target For Those Units
                IEntity target = null;
                Vector2 sumPos = Vector2.Zero;
                foreach(var u2 in squad)
                    sumPos += u2.GridPosition;
                Vector2 averagePos = new Vector2(sumPos.X / squad.Count, sumPos.Y / squad.Count);
                foreach(var t2 in GameState.activeTeams)
                    if(t2.Index != TeamIndex)
                        foreach(var u3 in t2.Team.Units)
                            if(target == null || Vector2.Distance(u3.GridPosition, averagePos) < Vector2.Distance(u3.GridPosition, target.GridPosition))
                                target = u3;
                AddEvent(new SetTargetEvent(TeamIndex, target));
            }
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }

        private void OnBuildingDamage(IEntity e, int d) {
            RTSBuilding building = e as RTSBuilding;
            Point p = HashHelper.Hash(e.GridPosition, grid.numCells, grid.size);
            int imp = building.BuildingData.Impact * d;
            grid.Region[p.X, p.Y].AddToRegionImpact(imp);
            DevConsole.AddCommand("Impact Added " + imp);
        }
    }
}