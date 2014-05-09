using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;

namespace RTSEngine.Data {
    public enum FogOfWar : uint {
        Nothing = 0x00,
        Passive = 0x01,
        Active = 0x02
    }

    public static class FogOfWarHelper {
        public static FogOfWar GetFogOfWar(uint fog, int p) {
            return (FogOfWar)((fog >> (p << 2)) & 0x03);
        }
        public static void SetFogOfWar(ref uint fog, int p, FogOfWar f) {
            p <<= 2;
            fog &= ~(0x03u << p);
            fog |= ((uint)f << p);
        }
    }

    public struct HeightTile {
        public float XNZN, XPZN, XNZP, XPZP;
    }

    public class CollisionGrid {
        public static class Direction {
            public const byte XN = 0x01;
            public const byte XP = 0x02;
            public const byte ZN = 0x04;
            public const byte ZP = 0x08;
            public const byte XNZN = 0x10;
            public const byte XPZN = 0x20;
            public const byte XNZP = 0x40;
            public const byte XPZP = 0x80;
        }

        // Full Size Of The Map
        public readonly Vector2 size;

        // Number And Size Of Cells
        public readonly Point numCells;
        public readonly float cellSize;

        public List<RTSUnit>[,] EDynamic {
            get;
            private set;
        }
        public RTSBuilding[,] EStatic {
            get;
            private set;
        }
        public CollisionRect[,][] Walls {
            get;
            private set;
        }
        public byte[,] WallInformation {
            get;
            private set;
        }
        public uint[,] Fog {
            get;
            private set;
        }
        public bool[,] Collision {
            get;
            private set;
        }
        private HeightTile[,] heights;

        // Locations In The Grid Which Contain Units
        public List<Point> ActiveGrids {
            get;
            private set;
        }

        public event Action<int, int, int, FogOfWar> OnFOWChange;

        public CollisionGrid(int w, int h) {
            cellSize = RTSConstants.CGRID_SIZE;
            numCells = new Point(w, h);
            size = new Vector2(w, h) * cellSize;

            EDynamic = new List<RTSUnit>[numCells.X, numCells.Y];
            EStatic = new RTSBuilding[numCells.X, numCells.Y];
            for(int x = 0; x < numCells.X; x++) {
                for(int y = 0; y < numCells.Y; y++) {
                    EDynamic[x, y] = new List<RTSUnit>();
                }
            }
            ActiveGrids = new List<Point>();
            Fog = new uint[numCells.X, numCells.Y];
            Collision = new bool[numCells.X, numCells.Y];
            Walls = new CollisionRect[numCells.X, numCells.Y][];
            WallInformation = new byte[numCells.X, numCells.Y];
            heights = new HeightTile[numCells.X, numCells.Y];
        }

        public void Add(RTSUnit o) {
            Point p = HashHelper.Hash(o.CollisionGeometry.Center, numCells, size);

            // Check If Active
            if(EDynamic[p.X, p.Y].Count < 1)
                ActiveGrids.Add(p);
            EDynamic[p.X, p.Y].Add(o);
        }
        public bool CanAddBuilding(Vector2 pos, Point gs) {
            // Check All The Cells
            Point p = HashHelper.Hash(pos, numCells, size);

            // Check If Building Fits In Map
            if(p.X + gs.X >= numCells.X || p.Y + gs.Y >= numCells.Y)
                return false;

            for(int y = 0; y < gs.Y; y++) {
                for(int x = 0; x < gs.X; x++) {
                    if(EStatic[p.X + x, p.Y + y] != null)
                        return false;
                }
            }
            return true;
        }
        public void Add(RTSBuilding b) {
            b.OnDestruction += OnBuildingDestruction;

            // Add To All The Cells
            Point p = HashHelper.Hash(b.GridStartPos, numCells, size);
            for(int y = 0; y < b.Data.GridSize.Y; y++) {
                for(int x = 0; x < b.Data.GridSize.X; x++) {
                    EStatic[p.X + x, p.Y + y] = b;
                }
            }
        }
        public void AddWalls(int x, int y, params byte[] directions) {
            byte dir = 0x00;
            for(int i = 0; i < directions.Length; i++) dir |= directions[i];
            AddWalls(x, y, dir);
        }
        public void AddWalls(int x, int y, byte directions) {
            byte mi = 0x00;
            CollisionRect[] pWalls = new CollisionRect[4];
            int c = 0;
            if((directions & Direction.XN) != 0 && pWalls[0] == null) {
                mi |= Direction.XN | Direction.XNZN | Direction.XNZP;
                pWalls[0] = new CollisionRect(
                    RTSConstants.CGRID_WALL_SIZE,
                    RTSConstants.CGRID_SIZE,
                    new Vector2(
                        x * RTSConstants.CGRID_SIZE + RTSConstants.CGRID_WALL_SIZE * 0.5f,
                        y * RTSConstants.CGRID_SIZE + RTSConstants.CGRID_SIZE * 0.5f
                        ),
                    true
                    );
                c++;
            }
            if((directions & Direction.XP) != 0 && pWalls[1] == null) {
                mi |= Direction.XP | Direction.XPZN | Direction.XPZP;
                pWalls[1] = new CollisionRect(
                    RTSConstants.CGRID_WALL_SIZE,
                    RTSConstants.CGRID_SIZE,
                    new Vector2(
                        (x + 1) * RTSConstants.CGRID_SIZE - RTSConstants.CGRID_WALL_SIZE * 0.5f,
                        y * RTSConstants.CGRID_SIZE + RTSConstants.CGRID_SIZE * 0.5f
                        ),
                    true
                    );
                c++;
            }
            if((directions & Direction.ZN) != 0 && pWalls[2] == null) {
                mi |= Direction.ZN | Direction.XNZN | Direction.XPZN;
                pWalls[2] = new CollisionRect(
                    RTSConstants.CGRID_SIZE,
                    RTSConstants.CGRID_WALL_SIZE,
                    new Vector2(
                        x * RTSConstants.CGRID_SIZE + RTSConstants.CGRID_SIZE * 0.5f,
                        y * RTSConstants.CGRID_SIZE + RTSConstants.CGRID_WALL_SIZE * 0.5f
                        ),
                    true
                    );
                c++;
            }
            if((directions & Direction.ZP) != 0 && pWalls[3] == null) {
                mi |= Direction.ZP | Direction.XNZP | Direction.XPZP;
                pWalls[3] = new CollisionRect(
                    RTSConstants.CGRID_SIZE,
                    RTSConstants.CGRID_WALL_SIZE,
                    new Vector2(
                        x * RTSConstants.CGRID_SIZE + RTSConstants.CGRID_SIZE * 0.5f,
                        (y + 1) * RTSConstants.CGRID_SIZE - RTSConstants.CGRID_WALL_SIZE * 0.5f
                        ),
                    true
                    );
                c++;
            }
            CollisionRect[] walls = new CollisionRect[c];
            c = 0;
            if(pWalls[0] != null) walls[c++] = pWalls[0];
            if(pWalls[1] != null) walls[c++] = pWalls[1];
            if(pWalls[2] != null) walls[c++] = pWalls[2];
            if(pWalls[3] != null) walls[c++] = pWalls[3];
            Add(walls, mi, x, y);
        }
        public void Add(CollisionRect[] walls, byte mi, int x, int y) {
            Walls[x, y] = new CollisionRect[walls.Length];
            WallInformation[x, y] |= mi;
            if(x > 0 && y > 0 && !CanMoveTo(new Point(x, y), Direction.XNZN)) {
                WallInformation[x - 1, y - 1] |= Direction.XPZP;
            }
            if(x < numCells.X - 1 && y > 0 && !CanMoveTo(new Point(x, y), Direction.XPZN)) {
                WallInformation[x + 1, y - 1] |= Direction.XNZP;
            }
            if(x > 0 && y < numCells.Y - 1 && !CanMoveTo(new Point(x, y), Direction.XNZP)) {
                WallInformation[x - 1, y + 1] |= Direction.XPZN;
            }
            if(x < numCells.X - 1 && y < numCells.Y - 1 && !CanMoveTo(new Point(x, y), Direction.XPZP)) {
                WallInformation[x + 1, y + 1] |= Direction.XNZN;
            }
            Array.Copy(walls, Walls[x, y], Walls[x, y].Length);
        }

        public void ClearDynamic() {
            for(int i = 0; i < ActiveGrids.Count; i++)
                EDynamic[ActiveGrids[i].X, ActiveGrids[i].Y] = new List<RTSUnit>();
            ActiveGrids = new List<Point>();
        }

        // Can Move To N From P
        public bool CanMoveFrom(Point p, Point n) {
            return CanMoveTo(p, GetDirection(p, n));
        }
        // Figure Out In Which Direction N Lies From P
        private byte GetDirection(Point p, Point n) {
            if(n.X > p.X && n.Y > p.Y) {
                return Direction.XPZP;
            }
            else if(n.X > p.X && n.Y < p.Y) {
                return Direction.XPZN;
            }
            else if(n.X < p.X && n.Y > p.Y) {
                return Direction.XNZP;
            }
            else if(n.X < p.X && n.Y < p.Y) {
                return Direction.XNZN;
            }
            else if(n.X < p.X && n.Y == p.Y) {
                return Direction.XN;
            }
            else if(n.X > p.X && n.Y == p.Y) {
                return Direction.XP;
            }
            else if(n.X == p.X && n.Y < p.Y) {
                return Direction.ZN;
            }
            else {
                return Direction.ZP;
            }
        }
        public bool CanMoveTo(Point pOrigin, byte direction) {
            return (WallInformation[pOrigin.X, pOrigin.Y] & direction) == 0;
        }

        // Precondition This[x,y] Must Be Active
        public void HandleGridCollision(int x, int y, int dx, int dy) {
            // Check Bounds
            int ox = x + dx;
            if(ox < 0 || ox >= numCells.X) return;
            int oy = y + dy;
            if(oy < 0 || oy >= numCells.Y) return;

            var al1 = EDynamic[x, y];
            var al2 = EDynamic[ox, oy];
            var sl2 = EStatic[ox, oy];
            var wl2 = Walls[ox, oy];

            // Empty Check
            if(al2.Count + (sl2 == null ? 0 : 1) + (wl2 == null ? 0 : 1) < 1) return;

            // Dynamic-Dynamic
            for(int i1 = 0; i1 < al1.Count; i1++)
                for(int i2 = 0; i2 < al2.Count; i2++)
                    // Get Rid Of Doubles
                    if(al1[i1].UUID > al2[i2].UUID)
                        CollisionController.ProcessCollision(al1[i1].CollisionGeometry, al2[i2].CollisionGeometry);
            // Dynamic-Static
            if(sl2 != null)
                for(int i1 = 0; i1 < al1.Count; i1++)
                    CollisionController.ProcessCollision(al1[i1].CollisionGeometry, sl2.CollisionGeometry);
            if(wl2 != null)
                for(int i1 = 0; i1 < al1.Count; i1++)
                    for(int i2 = 0; i2 < wl2.Length; i2++)
                        CollisionController.ProcessCollision(al1[i1].CollisionGeometry, wl2[i2]);
        }
        public void HandleGridCollision(int x, int y) {
            var al = EDynamic[x, y];
            var sl = EStatic[x, y];
            var wl = Walls[x, y];

            // Dynamic-Dynamic
            for(int i1 = 0; i1 < al.Count - 1; i1++)
                for(int i2 = i1 + 1; i2 < al.Count; i2++)
                    CollisionController.ProcessCollision(al[i1].CollisionGeometry, al[i2].CollisionGeometry);
            // Dynamic-Static
            if(sl != null)
                for(int i1 = 0; i1 < al.Count; i1++)
                    CollisionController.ProcessCollision(al[i1].CollisionGeometry, sl.CollisionGeometry);
            if(wl != null)
                for(int i1 = 0; i1 < al.Count; i1++)
                    for(int i2 = 0; i2 < wl.Length; i2++)
                        CollisionController.ProcessCollision(al[i1].CollisionGeometry, wl[i2]);
        }

        public void SetFogOfWar(int x, int y, int p, FogOfWar f) {
            FogOfWar of = GetFogOfWar(x, y, p);
            if(f == of) return;
            FogOfWarHelper.SetFogOfWar(ref Fog[x, y], p, f);
            if(OnFOWChange != null)
                OnFOWChange(x, y, p, f);
        }
        public FogOfWar GetFogOfWar(int x, int y, int p) {
            return FogOfWarHelper.GetFogOfWar(Fog[x, y], p);
        }
        public FogOfWar GetFogOfWar(Vector2 pos, int p) {
            Point c = HashHelper.Hash(pos, numCells, size);
            return GetFogOfWar(c.X, c.Y, p);
        }

        public void SetCollision(int x, int y, bool c) {
            Collision[x, y] = c;
        }
        public bool GetCollision(int x, int y) {
            return Collision[x, y] || EStatic[x, y] != null;
        }
        public bool GetCollision(Vector2 pos) {
            Point c = HashHelper.Hash(pos, numCells, size);
            return GetCollision(c.X, c.Y);
        }

        public void OnBuildingSpawn(RTSBuilding b) {
            Add(b);
        }
        public void OnBuildingDestruction(IEntity o) {
            o.OnDestruction -= OnBuildingDestruction;
            RTSBuilding b = o as RTSBuilding;

            // Add To All The Cells
            Point p = HashHelper.Hash(b.GridStartPos, numCells, size);
            for(int y = 0; y < b.Data.GridSize.Y; y++) {
                for(int x = 0; x < b.Data.GridSize.X; x++) {
                    EStatic[p.X + x, p.Y + y] = null;
                }
            }
        }

        public void SetHeight(int x, int y, HeightTile ht) {
            heights[x, y] = ht;
        }
        // Retrieve Interpolated Height From The Heightmap
        private float Bilerp(float v1, float v2, float v3, float v4, float rx, float rz) {
            return MathHelper.Lerp(
                MathHelper.Lerp(v1, v2, rx),
                MathHelper.Lerp(v3, v4, rx),
                rz
                );
        }
        public float HeightAt(Vector2 pos) {
            Vector2 r;
            Point c = HashHelper.Hash(pos, numCells, size, out r);

            // Bilerp For Value
            return Bilerp(
                heights[c.X, c.Y].XNZN,
                heights[c.X, c.Y].XPZN,
                heights[c.X, c.Y].XNZP,
                heights[c.X, c.Y].XPZP,
                r.X, r.Y
                );
        }
    }

    public class ImpactGrid {

        // Size Of Each Cell In The Impact Grid
        public readonly float cellSize;

        // Number Of Cells In The Impact Grid
        public readonly Point numCells;

        // Size Of The Impact Grid
        public readonly Vector2 size;

        // Stores The Region Each Cell Is Located In
        public ImpactRegion[,] Region { get; set; }

        // Stores The ImpactGenerators Of Each Cell Of The Impact Grid
        public List<RTSBuilding>[,] ImpactGenerators { get; set; }

        // Stores The Impact of Each Cell Of The Impact Grid
        public int[,] CellImpact { get; private set; }

        // Creates An Impact Grid Using The Size And Cell Size Of The Given Collision Grid
        public ImpactGrid(int w, int h) {
            cellSize = 2 * RTSConstants.IGRID_SIZE;
            numCells = new Point(w, h);
            size = new Vector2(numCells.X, numCells.Y) * cellSize;
            Region = new ImpactRegion[numCells.X, numCells.Y];
            ImpactGenerators = new List<RTSBuilding>[numCells.X, numCells.Y];
            CellImpact = new int[numCells.X, numCells.Y];

            for(int x = 0; x < numCells.X; x++) {
                for(int y = 0; y < numCells.Y; y++) {
                    Region[x, y] = null;
                    ImpactGenerators[x, y] = new List<RTSBuilding>();
                    CellImpact[x, y] = 0;
                }
            }
        }

        // Adds An Impact Generator To The Appropriate Cell In The Impact Grid
        public void AddImpactGenerator(RTSBuilding b) {
            Point p = HashHelper.Hash(b.GridPosition, numCells, size);
            ImpactGenerators[p.X, p.Y].Add(b);
        }

        // Adds Impact To The Appropriate Cell And Region 
        public void AddImpact(Vector2 pos, int amount) {
            Point p = HashHelper.Hash(pos, numCells, size);
            if(CellImpact[p.X, p.Y] + amount < 0) {
                Region[p.X, p.Y].AddToRegionImpact(-1 * CellImpact[p.X, p.Y]);
                CellImpact[p.X, p.Y] = 0;
            }
            else {
                CellImpact[p.X, p.Y] += amount;
                Region[p.X, p.Y].AddToRegionImpact(amount);
            }
        }

        public void AddImpact(Point p, int amount) {
            if(CellImpact[p.X, p.Y] + amount < 0) {
                Region[p.X, p.Y].AddToRegionImpact(-1 * CellImpact[p.X, p.Y]);
                CellImpact[p.X, p.Y] = 0;
            }
            else {
                CellImpact[p.X, p.Y] += amount;
                Region[p.X, p.Y].AddToRegionImpact(amount);
            }
        }
    }

    public struct LevelGrid {
        public static void Serialize(BinaryWriter s, GameState state) {
            LevelGrid g = state.LevelGrid;
            for(int y = 0; y < g.L1.numCells.Y; y++) {
                for(int x = 0; x < g.L1.numCells.X; x++) {
                    s.Write(g.L1.Fog[x, y]);
                }
            }
            for(int y = 0; y < g.L2.numCells.Y; y++) {
                for(int x = 0; x < g.L2.numCells.X; x++) {
                    s.Write(g.L2.ImpactGenerators[x, y].Count);
                    for(int i = 0; i < g.L2.ImpactGenerators[x, y].Count; i++) {
                        // TODO: Custom Serialization
                    }
                    s.Write(g.L2.CellImpact[x, y]);
                }
            }
        }
        public static void Deserialize(BinaryReader s, GameState state) {
            LevelGrid g = state.LevelGrid;
            for(int y = 0; y < g.L1.numCells.Y; y++) {
                for(int x = 0; x < g.L1.numCells.X; x++) {
                    g.L1.Fog[x, y] = s.ReadUInt32();
                }
            }
            for(int y = 0; y < g.L2.numCells.Y; y++) {
                for(int x = 0; x < g.L2.numCells.X; x++) {
                    int c = s.ReadInt32();
                    for(int i = 0; i < c; i++) {
                        // TODO: Custom Deserialization
                    }
                    g.L2.CellImpact[x, y] = s.ReadInt32();
                }
            }
        }

        // IO Information
        private string localFile;
        public string InfoFile {
            get { return localFile; }
            set {
                localFile = value;
                File = new FileInfo(localFile);
            }
        }
        public FileInfo File {
            get;
            private set;
        }
        public DirectoryInfo Directory {
            get { return File.Directory; }
        }

        //public Heightmap L0;
        public CollisionGrid L1;
        public ImpactGrid L2;
    }

    public static class HashHelper {
        public static Point Hash(Vector2 pos, ref int gx, ref float sx, out float rx, ref int gy, ref float sy, out float ry) {
            pos.X *= gx / sx;
            pos.Y *= gy / sy;
            Point p = new Point((int)pos.X, (int)pos.Y);
            rx = pos.X - p.X;
            ry = pos.Y - p.Y;
            if(p.X < 0) p.X = 0;
            else if(p.X >= gx) p.X = gx - 1;
            if(p.Y < 0) p.Y = 0;
            else if(p.Y >= gy) p.Y = gy - 1;
            return p;
        }
        public static Point Hash(Vector2 pos, ref int gx, ref float sx, ref int gy, ref float sy) {
            pos.X *= gx / sx;
            pos.Y *= gy / sy;
            Point p = new Point((int)pos.X, (int)pos.Y);
            if(p.X < 0) p.X = 0;
            else if(p.X >= gx) p.X = gx - 1;
            if(p.Y < 0) p.Y = 0;
            else if(p.Y >= gy) p.Y = gy - 1;
            return p;
        }
        public static Point Hash(Vector2 pos, ref Point g, ref Vector2 s) {
            return Hash(pos, ref g.X, ref s.X, ref g.Y, ref s.Y);
        }
        public static Point Hash(Vector2 pos, int gx, float sx, int gy, float sy) {
            pos.X *= gx / sx;
            pos.Y *= gy / sy;
            Point p = new Point((int)pos.X, (int)pos.Y);
            if(p.X < 0) p.X = 0;
            else if(p.X >= gx) p.X = gx - 1;
            if(p.Y < 0) p.Y = 0;
            else if(p.Y >= gy) p.Y = gy - 1;
            return p;
        }
        public static Point Hash(Vector2 pos, Point g, Vector2 s) {
            return Hash(pos, ref g.X, ref s.X, ref g.Y, ref s.Y);
        }
        public static Point Hash(Vector2 pos, Point g, Vector2 s, out Vector2 r) {
            return Hash(pos, ref g.X, ref s.X, out r.X, ref g.Y, ref s.Y, out r.Y);
        }
    }
}