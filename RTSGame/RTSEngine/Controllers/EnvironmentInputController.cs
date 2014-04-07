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

namespace RTSEngine.Controllers {
    public class EnvironmentInputController : InputController {
        Thread thread;
        private bool running;
        private bool paused;

        private Random random;
        private ImpactGrid grid;
        private int counter;

        private List<Point> treeLocations;
        public RTSBuildingData FloraData {
            get { return Team.race.Buildings[floraType]; }
        }
        public RTSBuildingData OreData {
            get { return Team.race.Buildings[oreType]; }
        }

        // Indices For Resources And Units
        private int floraType;
        private int oreType;
        private int minionType;
        private int tankType;
        private int titanType;
        // Time Intervals At Which Recovery And Spawning Happens (In Seconds)
        private int recoverTime;
        private int disasterTime;
        // Impact Levels For Level One, Level Two, Level Three
        private int L1Impact;
        private int L2Impact;
        private int L3Impact;
        // Impact Level At Which Region Will No Longer Recover
        private int noLongerRecoverImpact;
        // Maximum Number Of Units Per Region For Each Level
        private int[] spawnCaps;
        // Impact Recovered Per Region Per Recovery Phase
        private int recoverImpact;
        // Environment Unit Spawn Offset
        private int spawnOffset;
        // Minimum Number Of Units Spawned For Each Level
        private int[][] minNumSpawn;
        // Maximum Number Of Units Spawned For Each Level
        private int[][] maxNumSpawn;
        // Array Of Possible Spawn Types
        private int[] spawns;
        

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

        public void Init(FileInfo infoRace, FileInfo spawnImage) {
            EnvironmentInitData eid = EnvironmentDataParser.Parse(infoRace);
            floraType = eid.FloraType;
            oreType = eid.OreType;
            minionType = eid.MinionType;
            tankType = eid.TankType;
            titanType = eid.TitanType;
            spawns = new int[] { minionType, tankType, titanType };
            recoverTime = eid.RecoverTime;
            disasterTime = eid.DisasterTime;
            L1Impact = eid.L1Impact;
            L2Impact = eid.L2Impact;
            L3Impact = eid.L3Impact;
            noLongerRecoverImpact = eid.NoLongerRecoverImpact;
            spawnCaps = new int[] { eid.L1SpawnCap, eid.L2SpawnCap, eid.L3SpawnCap };
            recoverImpact = eid.RecoverImpact;
            spawnOffset = eid.SpawnOffset;
            minNumSpawn = new int[3][] { eid.L1MinNumSpawn, eid.L1MaxNumSpawn, eid.L2MinNumSpawn };
            maxNumSpawn = new int[3][] { eid.L2MaxNumSpawn, eid.L3MinNumSpawn, eid.L3MaxNumSpawn };
            treeLocations = new List<Point>();

            using (var bmp = System.Drawing.Bitmap.FromFile(spawnImage.FullName) as System.Drawing.Bitmap) {
                int w = bmp.Width;
                int h = bmp.Height;
                int[] colorValues = new int[w * h];
                System.Drawing.Imaging.BitmapData bmpData = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(bmpData.Scan0, colorValues, 0, colorValues.Length);
                bmp.UnlockBits(bmpData);

                int i = 0;
                for (int y = 0; y < h; y++) {
                    for (int x = 0; x < w; x++) {
                        Point p = new Point(x, y);
                        if (colorValues[i] == System.Drawing.Color.Green.ToArgb()) {
                            AddEvent(new SpawnBuildingEvent(TeamIndex, floraType, p));
                            treeLocations.Add(p);
                        }
                        else if (colorValues[i] == System.Drawing.Color.Red.ToArgb()) {
                            AddEvent(new SpawnBuildingEvent(TeamIndex, oreType, p));
                        }
                        i++;
                    }
                }
            }
        }

        private void WorkThread() {
            while(running) {
                if(paused) {
                    Thread.Sleep(1000);
                    continue;
                }
                UpdateUnitsInRegion();
                if(counter % disasterTime == 0) {
                    SpawnUnits();
                    SetInitTarget();
                }
                if(counter % recoverTime == 0) {
                    Recover();
                }
                counter++;
                counter = counter % (disasterTime + recoverTime);
                Thread.Sleep(1000);
            }
        }

        public void Start() {
            counter = 0;
            paused = false;
        }
        public override void Dispose() {
            running = false;
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
            foreach(var r in GameState.Regions) {
                if(r.RegionImpact < noLongerRecoverImpact && r.RegionImpact > 0) {
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
                                    AddEvent(new SpawnBuildingEvent(TeamIndex, floraType, newTreeC));
                                    r.AddToRegionImpact(-FloraData.Impact);
                                }
                            }
                        }
                    }

                    // Regenerate Ore Health
                    foreach(var c in r.Cells) {
                        foreach(var o in GameState.IGrid.ImpactGenerators[c.X, c.Y]) {
                            if(o.Data.FriendlyName.Equals(OreData.FriendlyName)) {
                                o.Health += recoverImpact;
                                r.AddToRegionImpact(-(OreData.Impact / recoverImpact));
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
                    // Find The Cell With The Largest Impact
                    Point p = r.Cells.First();
                    foreach(var c in r.Cells) {
                        if(grid.CellImpact[c.X, c.Y] > grid.CellImpact[p.X, p.Y]) {
                            p = c;
                        }
                    }
                    // Randomly Choose An Impact Generator In That Cell
                    ImpactGenerator g = null;
                    while(g == null || !g.Data.FriendlyName.Equals(OreData.FriendlyName) || !g.Data.FriendlyName.Equals(FloraData.FriendlyName)) {
                        int i = random.Next(grid.ImpactGenerators[p.X, p.Y].Count);
                        g = grid.ImpactGenerators[p.X, p.Y][i];
                    }

                    // Spawn Environmental Units
                    Vector2 spawnPos = g.GridPosition;
                    Vector2 offset;
                    int numSpawn;
                  //FIX
                    for(int i = 0; i < spawns.GetLength(1) && r.units.Count < spawnCaps[level - 1]; i++) {
                        numSpawn = random.Next(minNumSpawn[level - 1][i], maxNumSpawn[level - 1][i]);
                        offset.X = random.Next(spawnOffset);
                        offset.Y = random.Next(spawnOffset);
                        for(int j = 0; j < numSpawn; j++) {
                            AddEvent(new SpawnUnitEvent(TeamIndex, spawns[i], spawnPos + offset));
                        }
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

    }
}