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
        // Amount Of Health Ore Recovers Per Recovery Phase
        private int OreRecoverHealth {
            get { return eData.OreRecoverHealth; }
        }
        // Environment Unit Spawn Offset
        private int SpawnOffset {
            get { return eData.SpawnOffset; }
        }

        // Probability That Lightning Hits A Unit
        private int LightningHitP {
            get { return eData.LightningHitP; }
        }
        // Probability That Earthquake Hits A Building
        private int EarthquakeHitP {
            get { return eData.EarthquakeHitP; }
        }
        // Probability That Fire Hits An Unit
        private int FireHitUnitP {
            get { return eData.FireHitUnitP; }
        }
        //Probability That Fire Hits A Building
        private int FireHitBuildingP {
            get { return eData.FireHitBuildingP; }
        }
        // Damage Done By Lightning
        private int LightningDamage {
            get { return eData.LightningDamage; }
        }
        // Damage Done By Earthquakes
        private int EarthquakeDamage {
            get { return eData.EarthquakeDamage; }
        }
        //Damage Done To Units By Fire
        private int FireUnitDamage {
            get { return eData.FireUnitDamage; }
        }
        //Damage Done To Buildings By Fire
        private int FireBuildingDamage {
            get { return eData.FireBuildingDamage; }
        }
        private int FireSpreadP {
            get { return eData.FireSpreadP; }
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
            treeLocations = new List<Point>();

            for(int i = 0; i < Team.Buildings.Count; i++) {
                Team.Buildings[i].OnDamage += OnBuildingDamage;
                grid.AddImpactGenerator(Team.Buildings[i]);
                if (Team.Buildings[i].Data.FriendlyName.Equals(FloraData.FriendlyName)) {
                    Point treeC = HashHelper.Hash(Team.Buildings[i].GridPosition, GameState.CGrid.numCells, GameState.CGrid.size);
                    treeLocations.Add(treeC);
                }
            }
            Team.OnBuildingSpawn += (b) => {
                b.OnDamage += OnBuildingDamage;
                grid.AddImpactGenerator(b);
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
                
                if(counter % DisasterTime == 0) {
                    CreateDisaster();
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
                            bool inbounds = (newTreeC.X > -1 && newTreeC.Y > -1 && newTreeC.X < GameState.CGrid.numCells.X && newTreeC.Y < GameState.CGrid.numCells.Y);
                            if (inbounds && GameState.CGrid.EStatic[newTreeC.X, newTreeC.Y] == null) {
                                AddEvent(new SpawnBuildingEvent(TeamIndex, FloraType, newTreeC));
                                Vector2 newTreePos = new Vector2(newTreeC.X * GameState.CGrid.size.X, newTreeC.Y * GameState.CGrid.size.Y);
                                grid.AddImpact(newTreePos, FloraData.Impact * FloraData.Health);
                            }
                        }
                    }

                    // Regenerate Ore Health
                    foreach(var c in r.Cells) {
                        foreach(var o in GameState.IGrid.ImpactGenerators[c.X, c.Y]) {
                            if(o.Data.FriendlyName.Equals(OreData.FriendlyName)) {
                                o.Health += OreRecoverHealth;
                                r.AddToRegionImpact(-(OreData.Impact * OreRecoverHealth));
                            }
                        }
                    }

                }
            }
        }

        // Disaster Phase
        private void CreateDisaster() {
            
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

                level = 3; //testing

                if(level > 0) {
                    DevConsole.AddCommand("Has Level");

                    // Decide disaster type
                    int type = random.Next(2);
                    type = 0;


                    // Create the appropriate disaster
                    if (type == 0) {
                        SpawnUnits(r, level);
                    }
                    else {
                        if (level == 1) {
                            CreateLightning(r);
                        }
                        else if (level == 2) {
                            CreateEarthquake(r);
                        }
                        else {
                            CreateFire(r);
                        }
                    }
                }
            }
        }

        // Natural Disaster That Damages Both Units And Buildings
        private void CreateFire(Region r) {
            List<Point> hitCells = new List<Point>();
            // Choose Subareas In The Region To Hit
            foreach (var ic in r.Cells) {
                Point c = new Point(ic.X * 2, ic.Y * 2);
                for (int x = 0; x < 2 && c.X + x < GameState.CGrid.numCells.X; x++) {
                    for (int y = 0; y < 2 && c.Y + y < GameState.CGrid.numCells.Y; y++) {
                        bool isHit = (random.Next(100) <= FireSpreadP);
                        if (isHit) {
                            hitCells.Add(new Point(c.X + x, c.Y + y));
                        }
                    }
                }
            }
            // Apply Fire Damage To Chosen Subareas
            foreach (var c in hitCells) {
               foreach (var u in GameState.CGrid.EDynamic[c.X, c.Y]) {
                   if (u.Team.Index != Team.Index) {
                        bool takeDamage = (random.Next(100) <= FireHitUnitP);
                        if (takeDamage) {
                            u.Damage(FireUnitDamage);
                        }
                   }
               }
               RTSBuilding b = GameState.CGrid.EStatic[c.X, c.Y];
               if (b != null && b.Team.Index != Team.Index) {
                   bool takeDamage = (random.Next(100) <= FireHitBuildingP);
                   if (takeDamage) {
                       b.Damage(FireBuildingDamage);
                   }
                }
            }    
        }

        // Natural Disaster That Damages Units
        private void CreateLightning(Region r) {
            foreach (var ic in r.Cells) {
                Point c = new Point(ic.X * 2, ic.Y * 2);
                for (int x = 0; x < 2 && c.X + x < GameState.CGrid.numCells.X; x++) {
                    for (int y = 0; y < 2 && c.Y + y < GameState.CGrid.numCells.Y; y++) {
                        foreach (var u in GameState.CGrid.EDynamic[c.X + x, c.Y + y]) {
                            if (u.Team.Index != Team.Index) {
                                bool takeDamage = (random.Next(100) <= LightningHitP);
                                if (takeDamage) {
                                    u.Damage(LightningDamage);
                                }
                            }
                        }
                    }
                }
            }
        }


        // Natural Disaster That Damages Buildings
        private void CreateEarthquake(Region r) {
            foreach (var ic in r.Cells) {
                Point c = new Point(ic.X * 2, ic.Y * 2);
                for (int x = 0; x < 2 && c.X + x < GameState.CGrid.numCells.X; x++) {
                    for (int y = 0; y < 2 && c.Y + y < GameState.CGrid.numCells.Y; y++) {
                        RTSBuilding b = GameState.CGrid.EStatic[c.X + x, c.Y + y];
                        if (b != null && b.Team.Index != Team.Index) {
                            bool takeDamage = (random.Next(100) <= EarthquakeHitP);
                            if (takeDamage) {
                                b.Damage(EarthquakeDamage);
                            }
                        }
                        
                    }
                }
            }
        }


        private void SpawnUnits(Region r, int level) {
            // Find The Cell With The Largest Impact Which Still Contains Trees Or Ore
            Point p = new Point(-1, -1);
            bool hasResource = false;
            List<Point> resources = new List<Point>();
            List<Point> res;
            foreach (var ic in r.Cells) {
                if ((p.X < 0 && p.Y < 0) || grid.CellImpact[ic.X, ic.Y] > grid.CellImpact[p.X, p.Y]) {
                    Point c = new Point(ic.X * 2, ic.Y * 2);
                    res = new List<Point>();
                    for (int x = 0; x < 2 && c.X + x < GameState.CGrid.numCells.X; x++) {
                        for (int y = 0; y < 2 && c.Y + y < GameState.CGrid.numCells.Y; y++) {
                            RTSBuilding b = GameState.CGrid.EStatic[c.X + x, c.Y + y];
                            if (b != null && b.Team.Index == Team.Index) {
                                hasResource = true;
                                res.Add(new Point(c.X + x, c.Y + y));
                            }
                        }
                    }
                    if (hasResource) {
                        p = ic;
                        resources = res;
                    }
                }
            }

            if (resources.Count > 0) {
                
                // Decide Spawn Cap
                
                
                // Count Number Of Units Currently In Region

                // Randomly Choose An Impact Generator In That Cell
                int i = random.Next(resources.Count);
                Point resourcePos = resources[i];
                RTSBuilding resource = GameState.CGrid.EStatic[resourcePos.X, resourcePos.Y];

                // Spawn Environmental Units
                Vector2 spawnPos = new Vector2(resourcePos.X, resourcePos.Y) * GameState.CGrid.cellSize + Vector2.One;
                Vector2 offset;
                int numSpawn;

                int ti = 0;
                foreach (var spawnType in Spawns) {
                    numSpawn = random.Next(minNumSpawn[level - 1][ti], maxNumSpawn[level - 1][ti]);
                    offset.X = random.Next(SpawnOffset);
                    offset.Y = random.Next(SpawnOffset);
                    for (int j = 0; j < numSpawn; j++) {
                        AddEvent(new SpawnUnitEvent(TeamIndex, spawnType, spawnPos + offset));
                    }
                    ti++;
                }

                SetInitTarget(r);
            }
        }


        private void SetInitTarget(Region r) {
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
       
         
        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }

        private void OnBuildingDamage(IEntity e, int d) {
            RTSBuilding building = e as RTSBuilding;
            int imp = building.Data.Impact * d;
            grid.AddImpact(e.GridPosition, imp);
            DevConsole.AddCommand("Impact Added " + imp);
        }
    }
}