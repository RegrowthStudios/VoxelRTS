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
using RTSEngine.Graphics;
using Grey.Vox;
using Grey.Graphics;

namespace RTS.Input {
    public class Environment : ACInputController {
        Thread thread;
        private bool running;
        private bool paused;

        private Random random;
        private ImpactGrid grid;
        private int counter;
        private int playerIndex;

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

        public override void Init(GameState s, int ti, object args) {
            base.Init(s, ti, args);
            grid = GameState.IGrid;
            random = new Random();
            thread = new Thread(WorkThread);
            thread.IsBackground = true;
            running = true;
            paused = true;
            eData = EnvironmentData.Default;
            minNumSpawn = new int[3][] { eData.L1MinNumSpawn, eData.L1MaxNumSpawn, eData.L2MinNumSpawn };
            maxNumSpawn = new int[3][] { eData.L2MaxNumSpawn, eData.L3MinNumSpawn, eData.L3MaxNumSpawn };
            treeLocations = new List<Point>();
            treeLocations.Add(new Point(1, 1));
            FireStarts = new List<Point>();

            for(int i = 0; i < Team.Buildings.Count; i++) {
                Team.Buildings[i].OnDamage += OnBuildingDamage;
                grid.AddImpactGenerator(Team.Buildings[i]);
                if(Team.Buildings[i].Data.FriendlyName.Equals(FloraData.FriendlyName)) {
                    Point treeC = HashHelper.Hash(Team.Buildings[i].GridPosition, GameState.CGrid.numCells, GameState.CGrid.size);
                    treeLocations.Add(treeC);
                }
            }


            // Open File
            FileInfo fi = new FileInfo(s.LevelGrid.Directory.FullName + @"\env.dat");
            if(fi.Exists) {
                BinaryReader r = new BinaryReader(fi.OpenRead());

                while(true) {
                    int type = r.ReadInt32();
                    if(type == -1) break;
                    int x = r.ReadInt32();
                    int z = r.ReadInt32();
                    AddEvent(new SpawnBuildingEvent(TeamIndex, type == 0 ? FloraType : OreType, new Point(x, z), true));
                }
                r.BaseStream.Dispose();
            }

            Team.OnBuildingSpawn += (b) => {
                b.OnDamage += OnBuildingDamage;
                grid.AddImpactGenerator(b);
            };

            // Find Player's Index 
            for (int i = 0; i < GameState.activeTeams.Length; i++) {
                var at = GameState.activeTeams[i];
                if (at.Team.Type == RTSInputType.Player) {
                    playerIndex = at.Team.Index;
                }
            }
            Team.OnUnitSpawn += OnUnitSpawn;
            
            AddEvent(new SpawnBuildingEvent(TeamIndex, FloraType, new Point(60,60)));
            treeLocations.Add(new Point(60, 60));
        }

        private void WorkThread() {
            while(running) {
                if(paused) {
                    Thread.Sleep(1000);
                    continue;
                }
                foreach (var r in GameState.Regions) {
                    //SetInitTarget(r);
                }
                if(counter % DisasterTime == 0) {
                    DevConsole.AddCommand("disaster");
                    CreateDisaster();
                }
                if(counter % RecoverTime == 0) {
                    DevConsole.AddCommand("recover");
                    Recover();
                }
                counter++;
                counter = counter % (RecoverTime);
                Thread.Sleep(3000);
            }
        }

        public override void Begin() {
            counter = 1;
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
                    // Randomly Choose The Location Of A Starting Tree In Region
                    List<Point> treesInRegion = new List<Point>();
                    foreach (var tl in treeLocations) {
                        ImpactRegion rr = FindRegion(tl);
                        if (rr == r) {
                            treesInRegion.Add(tl);
                        }
                    }
                    // Spawn Trees Around The Starting Tree
                    if (treesInRegion.Count > 0) {
                        int i = random.Next(treesInRegion.Count);
                        Point treeC = treesInRegion[i];
                       
                        for (int x = -1; x <= 1; x++) {
                            for (int y = -1; y <= 1; y++) {

                                Point newTreeC = new Point(treeC.X + x, treeC.Y + y);
                                bool noBuilding = GameState.CGrid.EStatic[newTreeC.X, newTreeC.Y] == null;
                                bool noUnits = GameState.CGrid.EDynamic[newTreeC.X, newTreeC.Y].Count == 0;
                                if (noBuilding && noUnits && r.RegionImpact > 0) {
                                    DevConsole.AddCommand("recover4");
                                    AddEvent(new SpawnBuildingEvent(TeamIndex, FloraType, newTreeC));
                                    Vector2 newTreePos = new Vector2(newTreeC.X, newTreeC.Y) * GameState.CGrid.cellSize + Vector2.One;
                                    grid.AddImpact(newTreePos, -1 * (FloraData.Impact * FloraData.Health));
                                }
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

                if(level > 0) {
                    DevConsole.AddCommand("has level");
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

            
            if (FireThread != null) {
                FireRunning = false;
                FireThread.Join();
            }
            if (FireStarts.Count > 0) {
                FireThread = new Thread(FireWorkThread);
                FireThread.IsBackground = true;
                FireRunning = true;
                List<Point> fires = new List<Point>();
                foreach (var f in FireStarts) {
                    fires.Add(f);
                }
                FireStarts.Clear();
                FireThread.Start(fires);
            }
        }
        
        private void FireWorkThread(Object l) {
            DevConsole.AddCommand("started");

            List<Point> fires = l as List<Point>;
            HashSet<Point> hitCells = new HashSet<Point>();
            List<FireParticle> particles = new List<FireParticle>();
            int hitChance = 80;
            bool canSee;

            while(FireRunning) {
                if (fires.Count < 1) {
                    DevConsole.AddCommand("return");
                    return;
                }
                
                foreach(var f in fires) {
                    // Apply Fire Damage To Units and Buildings In Fire
                    Point c = new Point(f.X * 2, f.Y * 2);
                    for (int x = 0; x < 2 && c.X + x < GameState.CGrid.numCells.X; x++) {
                        for (int y = 0; y < 2 && c.Y + y < GameState.CGrid.numCells.Y; y++) {
                            foreach (var u in GameState.CGrid.EDynamic[c.X + x, c.Y + y]) {                               
                                if (u.Team.Index != Team.Index) {
                                    bool takeDamage = (random.Next(100) <= FireHitUnitP);
                                    if (true) {
                                        u.Damage(FireUnitDamage);
                                        canSee = GameState.CGrid.GetFogOfWar(u.GridPosition, playerIndex) == FogOfWar.Active;
                                        if (canSee) {
                                            particles.Add(new FireParticle(u.WorldPosition, 3, 2, 2, 7));
                                        }
                                    }
                                }
                            }            
                            RTSBuilding b = GameState.CGrid.EStatic[c.X, c.Y];
                            if (b != null && b.Team.Index != Team.Index) {
                                bool takeDamage = (random.Next(100) <= FireHitBuildingP);
                                if (true) {
                                    //b.Damage(FireBuildingDamage);
                                    canSee = GameState.CGrid.GetFogOfWar(b.GridPosition, playerIndex) == FogOfWar.Active;
                                    if (canSee) {
                                        particles.Add(new FireParticle(b.WorldPosition, 5, 1, 2, 6));
                                    }
                                }
                            }
                        }
                    }
                    // Add Fire Particle If Not In Player's Fog Of War
                    Vector2 p = new Vector2(f.X, f.Y) * grid.cellSize + Vector2.One;
                    canSee = GameState.CGrid.GetFogOfWar(p, playerIndex) == FogOfWar.Active;
                    if (canSee) {
                        Vector3 pos = new Vector3(p.X, GameState.CGrid.HeightAt(p), p.Y);
                        particles.Add(new FireParticle(pos, 4, 3, 10, 9));
                    }
                    // Fire Spreading
                    for(int x = -1; x < 2 ; x++) {
                        if (f.X + x < grid.numCells.X && f.X + x >= 0) {  
                            for(int y = -1; y < 2; y++) {
                                if (f.Y + y < grid.numCells.Y && f.Y + y >= 0) {
                                    bool isHit = (random.Next(100) <= hitChance);
                                    if (isHit) {
                                        bool added = hitCells.Add(new Point(f.X + x, f.Y + y));
                                    }
                                }
                            }
                        }
                    }
                }
                hitChance = (int)((float)hitChance * 0.7);
                fires.Clear();
                foreach (var hc in hitCells) {
                    fires.Add(hc);
                }
                hitCells.Clear();   
                GameState.AddParticles(particles);
                particles.Clear();
                Thread.Sleep(3000);
            }   
        }
        
        

        // Natural Disaster That Damages Both Units And Buildings
        private void CreateFire(ImpactRegion r) {
            
            // Find Impact Cell With Highest Impact
            int i = random.Next(r.Cells.Count); 
            Point highest = r.Cells.ElementAt(i);
            foreach (var ic in r.Cells) {
                if (grid.CellImpact[highest.X, highest.Y] > grid.CellImpact[ic.X, ic.Y]) {
                    highest = ic;
                }
            }
            // Use That Impact Cell As A Fire Starting Point 
            FireStarts.Add(highest);
                 
        }

        // Natural Disaster That Damages Units
        private void CreateLightning(ImpactRegion r) {
            List<LightningParticle> particles = new List<LightningParticle>();
            foreach(var ic in r.Cells) {
                Point c = new Point(ic.X * 2, ic.Y * 2);
                for(int x = 0; x < 2 && c.X + x < GameState.CGrid.numCells.X; x++) {
                    for(int y = 0; y < 2 && c.Y + y < GameState.CGrid.numCells.Y; y++) {
                        foreach(var u in GameState.CGrid.EDynamic[c.X + x, c.Y + y]) {
                            if(u.Team.Index != Team.Index) {
                                bool takeDamage = (random.Next(100) <= LightningHitP);
                                if(takeDamage) {
                                    u.Damage(LightningDamage);
                                    bool canSee = GameState.CGrid.GetFogOfWar(u.GridPosition, playerIndex) == FogOfWar.Active;
                                    if (canSee) {
                                        particles.Add(new LightningParticle(u.WorldPosition, 1, 7, 1, 0.6f, 1, Color.BlueViolet));
                                    }
                                }
                            }
                        }
                    }
                }
            }
            GameState.AddParticles(particles);
        }

        // Natural Disaster That Damages Buildings
        private void CreateEarthquake(ImpactRegion r) {
            List<LightningParticle> particles = new List<LightningParticle>();
            foreach(var ic in r.Cells) {
                Point c = new Point(ic.X * 2, ic.Y * 2);
                for(int x = 0; x < 2 && c.X + x < GameState.CGrid.numCells.X; x++) {
                    for(int y = 0; y < 2 && c.Y + y < GameState.CGrid.numCells.Y; y++) {
                        RTSBuilding b = GameState.CGrid.EStatic[c.X + x, c.Y + y];
                        if(b != null && b.Team.Index != Team.Index) {
                            bool takeDamage = (random.Next(100) <= EarthquakeHitP);
                            if(true) {
                                b.Damage(EarthquakeDamage);
                                bool canSee = GameState.CGrid.GetFogOfWar(b.GridPosition, playerIndex) == FogOfWar.Active;
                                if (canSee) {
                                    particles.Add(new LightningParticle(b.WorldPosition, 3, 10, 2, 0.6f, 1, Color.Red));
                                }
                            }
                        }

                    }
                }
            }
            GameState.AddParticles(particles);
        }

        private void SpawnUnits(ImpactRegion r, int level) {
            // Decide Spawn Cap
            int spawnCap;
            if (level == 1) {
                spawnCap = L1SpawnCap;
            }
            else if (level == 2) {
                spawnCap = L2SpawnCap;
            }
            else if (level == 3) {
                spawnCap = L3SpawnCap;
            }
            else {
                spawnCap = 0;
            }

            // Return If Population Count Of Region Is Greater Than The Spawn Cap For The Region
            if (r.PopCount >= spawnCap) { 
                return; 
            }
        
            // Decide On A Starting Point
            Point start = new Point(-1, -1);
            foreach (var u in GameState.activeTeams[playerIndex].Team.Units) {
                Point cc = HashHelper.Hash(u.GridPosition, GameState.CGrid.numCells, GameState.CGrid.size);
                start = cc;
            }

            // Find Possible Spawn Points
            List<Point> spawnPoints = new List<Point>();
            Queue<Point> unvisited = new Queue<Point>();
            int width = GameState.CGrid.numCells.X;
            int height = GameState.CGrid.numCells.Y;
            bool[,] visited = new bool[width, height];
            for (int x = 0; x < width; x++) {
                for (int y = 0; y < height; y++) {
                    visited[x, y] = false;
                }
            }
            if (start.X > 0 && start.Y > 0) {
                unvisited.Enqueue(start);
                visited[start.X, start.Y] = true;
            }

            while (unvisited.Count > 0) {
                
                Point at = unvisited.Dequeue();   
                
                for (int x = -1; x < 2; x++) {
                    for (int y = -1; y < 2; y++) {
                        Point p = new Point(at.X + x, at.Y + y);
                        if ((p.X != at.X || p.Y != at.Y) && p.X > 0 && p.Y > 0 && p.X < width && p.Y < height) {
                            ImpactRegion pr = FindRegion(p);
                            RTSBuilding b = GameState.CGrid.EStatic[p.X, p.Y];
                            if (pr.Cells.First() == r.Cells.First() && b != null) {
                                bool isOre = b.Data.FriendlyName.Equals(OreData.FriendlyName);
                                bool isTree = b.Data.FriendlyName.Equals(FloraData.FriendlyName);
                                if (isOre || isTree) {
                                    spawnPoints.Add(at);
                                }
                            }
                            else if (pr.Cells.First() == r.Cells.First() && b == null && !visited[p.X, p.Y]) {
                                unvisited.Enqueue(p);
                                visited[p.X, p.Y] = true;
                            }
                        }
                    }
                }
            }

            if (spawnPoints.Count > 0) {
                // Choose A Random Spawn Point
                int ii = random.Next(spawnPoints.Count);
                Point spawnPoint = spawnPoints.ElementAt(ii);
                Vector2 spawnPos = new Vector2(spawnPoint.X, spawnPoint.Y) * GameState.CGrid.cellSize + Vector2.One;

                // Spawn Environmental Units
                int numSpawn;
                int ti = 0;
                foreach (var spawnType in Spawns) {
                    numSpawn = random.Next(minNumSpawn[level - 1][ti], maxNumSpawn[level - 1][ti]);
                    for (int j = 0; j < numSpawn && r.PopCount < spawnCap; j++) {
                        AddEvent(new SpawnUnitEvent(TeamIndex, spawnType, spawnPos));
                        r.PopCount++;
                    }
                    ti++;
                }
            }
            SetInitTarget(r);
            
        }

        private void OnUnitSpawn(RTSUnit u) {
            ImpactRegion r = FindRegion(u);
            r.Selected.Add(u);
            r.PopCount++;
            u.OnDestruction += OnUnitDeath;
        }

        private void OnUnitDeath(IEntity e) {
            ImpactRegion r = FindRegion(e);
            r.PopCount--;
            IEntity dead = null;
            foreach (var u in r.Selected) {
                if (e.UUID == u.UUID) {
                    dead = e;
                }
            }
            if (dead != null) {
                r.Selected.Remove(dead);
            }
        }

        // Find The Region An Entity Is Located In
        private ImpactRegion FindRegion(IEntity e) {
            Point ic = HashHelper.Hash(e.GridPosition, grid.numCells, grid.size);
            ImpactRegion r = grid.Region[ic.X, ic.Y];
            return r;
        }

        // Find The Region A Collision Cell Is Located In
        private ImpactRegion FindRegion(Point cc) {
            Vector2 pos = new Vector2(cc.X, cc.Y) * GameState.CGrid.cellSize + Vector2.One;
            Point ic = HashHelper.Hash(pos, grid.numCells, grid.size);
            ImpactRegion r = grid.Region[ic.X, ic.Y];
            return r;
        }

        private void SetInitTarget(ImpactRegion r) {
            List<IEntity> selected = r.Selected;
            // Select Units Not In A Squad
            AddEvent(new SelectEvent(TeamIndex, selected));
            // Set The Target For Those Units
            IEntity target = null;
            Vector2 sumPos = Vector2.Zero;
            foreach(var s in selected)
                sumPos += s.GridPosition;
            Vector2 averagePos = new Vector2(sumPos.X / selected.Count, sumPos.Y / selected.Count);
            foreach(var t in GameState.activeTeams)
                if(t.Index != TeamIndex)
                    foreach(var u in t.Team.Units)
                        if(target == null || Vector2.Distance(u.GridPosition, averagePos) < Vector2.Distance(u.GridPosition, target.GridPosition))
                            target = u;
            AddEvent(new SetTargetEvent(TeamIndex, target));

        }

        #region Level Editor
        List<LEVoxel> voxels;
        ushort minID, maxID;
        public override List<LEVoxel> CreateVoxels(VoxAtlas atlas) {
            float duv = 1f / 8f;
            voxels = new List<LEVoxel>(2);
            LEVoxel lev;
            VGPCube vgp;

            // Create Flora Voxel
            lev = new LEVoxel("Flora", atlas);
            lev.VData.FaceType.SetAllTypes(0x00000001u);
            lev.VData.FaceType.SetAllMasks(0xfffffffeu);
            vgp = new VGPCube();
            vgp.Color = Color.Khaki;
            vgp.UVRect = new Vector4(duv * 0, duv * 1, duv, duv);
            lev.VData.GeoProvider = vgp;
            voxels.Add(lev);

            // Create Ore Voxel
            lev = new LEVoxel("Ore", atlas);
            lev.VData.FaceType.SetAllTypes(0x00000001u);
            lev.VData.FaceType.SetAllMasks(0xfffffffeu);
            vgp = new VGPCube();
            vgp.Color = Color.Purple;
            vgp.UVRect = new Vector4(duv * 1, duv * 1, duv, duv);
            lev.VData.GeoProvider = vgp;
            voxels.Add(lev);
         
            minID = voxels[0].VData.ID;
            maxID = voxels[voxels.Count - 1].VData.ID;
            return voxels;
        }
        public override void LESave(VoxWorld world, int w, int h, DirectoryInfo dir) {
            // Create File
            FileInfo fi = new FileInfo(dir.FullName + @"\env.dat");
            BinaryWriter s = new BinaryWriter(fi.Create());

            // Search Through Columns
            Vector3I loc = Vector3I.Zero;
            for(loc.Z = 0; loc.Z < h; loc.Z++) {
                for(loc.X = 0; loc.X < w; loc.X++) {
                    loc.Y = 0;
                    VoxLocation vl = new VoxLocation(loc);
                    Region r = world.regions[vl.RegionIndex];

                    // Search Through The Region
                    int type;
                    for(; vl.VoxelLoc.Y < Region.HEIGHT; vl.VoxelLoc.Y++) {
                        ushort id = r.voxels[vl.VoxelIndex].ID;
                        if(id < minID || id > maxID) continue;

                        // Write Team And Type
                        type = id - minID;

                        s.Write(type);
                        s.Write(loc.X);
                        s.Write(loc.Z);
                        break;
                    }
                }
            }
            s.Write(-1);

            // Flush And Close
            s.Flush();
            s.BaseStream.Dispose();
        }
        #endregion

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
            //DevConsole.AddCommand("Impact Added " + imp);
        }
    }
}