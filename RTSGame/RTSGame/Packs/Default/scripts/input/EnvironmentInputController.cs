﻿using Microsoft.Xna.Framework;
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
using RTSEngine.Graphics;

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
            get { return 2 * eData.DisasterTime; }
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
        private int L1SpawnCap {
            get { return eData.L1SpawnCap; }
        }
        private int L2SpawnCap {
            get { return eData.L2SpawnCap; }
        }
        private int L3SpawnCap {
            get { return eData.L3SpawnCap; }
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

        private Thread FireThread;
        private List<Point> FireStarts;
        private bool FireRunning;

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
            treeLocations.Add(new Point(1, 1));

            for(int i = 0; i < Team.Buildings.Count; i++) {
                Team.Buildings[i].OnDamage += OnBuildingDamage;
                grid.AddImpactGenerator(Team.Buildings[i]);
                if(Team.Buildings[i].Data.FriendlyName.Equals(FloraData.FriendlyName)) {
                    Point treeC = HashHelper.Hash(Team.Buildings[i].GridPosition, GameState.CGrid.numCells, GameState.CGrid.size);
                    treeLocations.Add(treeC);
                }
            }

            using(var bmp = System.Drawing.Bitmap.FromFile(@"Packs\Default\maps\0\Resources.png") as System.Drawing.Bitmap) {
                int w = bmp.Width;
                int h = bmp.Height;
                float[] hd = new float[w * h];
                byte[] col = new byte[w * h * 4];
                int i = 0, ci = 0;

                // Convert Bitmap
                System.Drawing.Imaging.BitmapData bd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, col, 0, bd.Stride * bd.Height);
                bmp.UnlockBits(bd);
                // TODO: Make Pixels As Floating Point Values
                for(int y = 0; y < h; y++) {
                    for(int x = 0; x < w; x++) {
                        if(col[ci + 2] > 128) {
                            AddEvent(new SpawnBuildingEvent(TeamIndex, OreType, new Microsoft.Xna.Framework.Point(x, y), true));
                        }
                        else if(col[ci + 1] > 128) {
                            AddEvent(new SpawnBuildingEvent(TeamIndex, FloraType, new Microsoft.Xna.Framework.Point(x, y), true));
                        }
                        ci += 4;
                    }
                }
            }

            Team.OnBuildingSpawn += (b) => {
                b.OnDamage += OnBuildingDamage;
                grid.AddImpactGenerator(b);
            };
            //FireStarts = new List<Point>();
            //FireStarts.Add(new Point(0, 0));
            //FireThread = new Thread(FireWorkThread);
            //FireThread.IsBackground = true;
            //FireRunning = true;
            //FireThread.Start(FireStarts);
        }

        private void WorkThread() {
            while(running) {
                if(paused) {
                    Thread.Sleep(1000);
                    continue;
                }

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
                    for(int x = -1; x <= 1; x++) {
                        for(int y = -1; y <= 1; y++) {
                            Point newTreeC = new Point(treeC.X + x, treeC.Y + y);
                            AddEvent(new SpawnBuildingEvent(TeamIndex, FloraType, newTreeC));
                            Vector2 newTreePos = new Vector2(newTreeC.X, newTreeC.Y) * GameState.CGrid.cellSize + Vector2.One;
                            grid.AddImpact(newTreePos, -1 * (FloraData.Impact * FloraData.Health));
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
            //FireStarts = new List<Point>();
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

                //level = 3;

                if(level > 0) {
                    DevConsole.AddCommand("Has Level");

                    // Decide disaster type
                    int type = random.Next(2);

                    type = 1;

                    // Create the appropriate disaster
                    if(type == 0) {
                        SpawnUnits(r, level);
                    }
                    else {
                        if(level == 1) {
                            CreateLightning(r);
                        }
                        else if(level == 2) {
                            CreateEarthquake(r);
                        }
                        else {
                            CreateFire(r);
                        }
                    }
                }
            }

            /*
            if (FireThread != null) {
                FireRunning = false;
                FireThread.Join();
            }
            FireThread = new Thread(FireWorkThread);
            FireThread.IsBackground = true;
            FireRunning = true;
            FireThread.Start(FireStarts);
             */
        }

       /*
        private void FireWorkThread(Object l) {
            DevConsole.AddCommand("started");
            List<Point> fires = (List<Point>) l;
            List<Point> hitCells = new List<Point>();
            List<FireParticle> particles = new List<FireParticle>();
            while(FireRunning) {
                if(fires.Count < 1) return;

                hitCells.Clear();
                particles.Clear();

                foreach(var f in fires) {

                    foreach(var u in GameState.CGrid.EDynamic[f.X, f.Y]) {
                        if(u.Team.Index != Team.Index) {
                            bool takeDamage = (random.Next(100) <= FireHitUnitP);
                            if(takeDamage) {
                                u.Damage(FireUnitDamage);
                                particles.Add(new FireParticle(u.WorldPosition, 1, 1, 1, 5));
                            }
                        }
                    }
                    RTSBuilding b = GameState.CGrid.EStatic[f.X, f.Y];
                    if(b != null && b.Team.Index != Team.Index) {
                        bool takeDamage = (random.Next(100) <= FireHitBuildingP);
                        if(takeDamage) {
                            b.Damage(FireBuildingDamage);
                            particles.Add(new FireParticle(b.WorldPosition, 5, 4, 5, 5));
                        }
                    }

                    Vector2 p = new Vector2(f.X, f.Y) * GameState.CGrid.cellSize + Vector2.One;
                    Vector3 pos = new Vector3(p.X, GameState.Map.HeightAt(p.X, p.Y), p.Y);
                    particles.Add(new FireParticle(pos, 3, 3, 5, 10));

                    for(int x = -1; x < 2 && f.X + x < GameState.CGrid.numCells.X && f.X + x > 0; x++) {
                        for(int y = -1; y < 2 && f.Y + y < GameState.CGrid.numCells.Y && f.Y + y > 0; y++) {
                            bool isHit = (random.Next(100) <= 100);

                            if (true) {

                                hitCells.Add(new Point(f.X + x, f.Y + y));
                            }
                        }
                    }

                }
                fires = hitCells;
                
                GameState.AddParticles(particles);
                Thread.Sleep(100);
            }
        }

        */

        // Natural Disaster That Damages Both Units And Buildings
        private void CreateFire(Region r) {
            /*
            // Find Impact Cell With Highest Impact
            //int i = random.Next(r.Cells.Count); 
            //Point highest = r.Cells.ElementAt(i);
            Point highest = r.Cells.First();
            foreach (var ic in r.Cells) {
                if (grid.CellImpact[highest.X, highest.Y] > grid.CellImpact[ic.X, ic.Y]) {
                    highest = ic;
                }
            }
            // Use A Random Collision Cell In That Impact Cell As A Starting Point 
            Point start = new Point(highest.X * 2 + random.Next(2), highest.Y * 2 + random.Next(2));
            FireStarts.Add(start);
            */

            
            List<Point> hitCells = new List<Point>();
            // Choose Subareas In The Region To Hit
            foreach (var ic in r.Cells) {
                Point c = new Point(ic.X * 2, ic.Y * 2);
                for (int x = 0; x < 2 && c.X + x < GameState.CGrid.numCells.X; x++) {
                    for (int y = 0; y < 2 && c.Y + y < GameState.CGrid.numCells.Y; y++) {
                        bool isHit = (random.Next(100) <= 100);
                        if (isHit) {
                            hitCells.Add(new Point(c.X + x, c.Y + y));
                        }
                    }
                }
            }
            // Apply Fire Damage To Chosen Subareas
            List<FireParticle> particles = new List<FireParticle>();
            foreach(var c in hitCells) {
                Vector2 hitGridPos = new Vector2(c.X, c.Y) * GameState.CGrid.cellSize + Vector2.One;
                Vector3 hitPos = new Vector3(hitGridPos.X, GameState.Map.HeightAt(hitGridPos.X, hitGridPos.Y),hitGridPos.Y);
                //particles.Add(new FireParticle(hitPos, 3, 2, 5, 5));
                foreach(var u in GameState.CGrid.EDynamic[c.X, c.Y]) {
                    if(u.Team.Index != Team.Index) {
                        bool takeDamage = (random.Next(100) <= FireHitUnitP);
                        if(takeDamage) {
                            u.Damage(FireUnitDamage);
                            particles.Add(new FireParticle(u.WorldPosition, 1, 1, 1, 5));
                        }
                    }
                }
                RTSBuilding b = GameState.CGrid.EStatic[c.X, c.Y];
                if(b != null && b.Team.Index != Team.Index) {
                    bool takeDamage = (random.Next(100) <= FireHitBuildingP);
                    if(takeDamage) {
                        b.Damage(FireBuildingDamage);
                        particles.Add(new FireParticle(b.WorldPosition, 5, 4, 5, 5));
                    }
                }
            }
            GameState.AddParticles(particles);
             
        }

        // Natural Disaster That Damages Units
        private void CreateLightning(Region r) {
            foreach(var ic in r.Cells) {
                Point c = new Point(ic.X * 2, ic.Y * 2);
                for(int x = 0; x < 2 && c.X + x < GameState.CGrid.numCells.X; x++) {
                    for(int y = 0; y < 2 && c.Y + y < GameState.CGrid.numCells.Y; y++) {
                        foreach(var u in GameState.CGrid.EDynamic[c.X + x, c.Y + y]) {
                            if(u.Team.Index != Team.Index) {
                                bool takeDamage = (random.Next(100) <= LightningHitP);
                                if(takeDamage) {
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
            foreach(var ic in r.Cells) {
                Point c = new Point(ic.X * 2, ic.Y * 2);
                for(int x = 0; x < 2 && c.X + x < GameState.CGrid.numCells.X; x++) {
                    for(int y = 0; y < 2 && c.Y + y < GameState.CGrid.numCells.Y; y++) {
                        RTSBuilding b = GameState.CGrid.EStatic[c.X + x, c.Y + y];
                        if(b != null && b.Team.Index != Team.Index) {
                            bool takeDamage = (random.Next(100) <= EarthquakeHitP);
                            if(takeDamage) {
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
            foreach(var ic in r.Cells) {
                if((p.X < 0 && p.Y < 0) || grid.CellImpact[ic.X, ic.Y] > grid.CellImpact[p.X, p.Y]) {
                    Point c = new Point(ic.X * 2, ic.Y * 2);
                    res = new List<Point>();
                    for(int x = 0; x < 2 && c.X + x < GameState.CGrid.numCells.X; x++) {
                        for(int y = 0; y < 2 && c.Y + y < GameState.CGrid.numCells.Y; y++) {
                            RTSBuilding b = GameState.CGrid.EStatic[c.X + x, c.Y + y];
                            if(b != null && b.Team.Index == Team.Index) {
                                hasResource = true;
                                res.Add(new Point(c.X + x, c.Y + y));
                            }
                        }
                    }
                    if(hasResource) {
                        p = ic;
                        resources = res;
                    }
                }
            }

              // Decide Spawn Cap
            int spawnCap;
            if (level == 1){
                spawnCap =  L1SpawnCap;
            }
            else if (level == 2){
                spawnCap =  L2SpawnCap;
            }
            else {
                spawnCap =  L3SpawnCap;
            }

            if(resources.Count > 0 && r.num < spawnCap) {

              


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
                foreach(var spawnType in Spawns) {
                    numSpawn = random.Next(minNumSpawn[level - 1][ti], maxNumSpawn[level - 1][ti]);
                    offset.X = random.Next(SpawnOffset);
                    offset.Y = random.Next(SpawnOffset);
                    for(int j = 0; j < numSpawn; j++) {
                        AddEvent(new SpawnUnitEvent(TeamIndex, spawnType, spawnPos + offset));
                        r.num = r.num +1;
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