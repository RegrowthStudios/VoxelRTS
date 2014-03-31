using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data {
    public enum FogOfWar : uint {
        Nothing = 0x00,
        Passive = 0x01,
        Active = 0x02,
        All = 0x03
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

    public class CollisionGrid {
        public readonly Vector2 size;
        public readonly Point grids;
        public readonly float cellSize;

        public List<IEntity>[,] EDynamic {
            get;
            private set;
        }
        public List<IEntity>[,] EStatic {
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

        public List<Point> ActiveGrids {
            get;
            private set;
        }

        public CollisionGrid(float w, float h, float gridSize) {
            // Round down the grid size so they all fit into the map
            size = new Vector2(w, h);
            grids = new Point((int)Math.Ceiling(size.X / gridSize), (int)Math.Ceiling(size.Y / gridSize));
            cellSize = size.X / grids.X;

            EDynamic = new List<IEntity>[grids.X, grids.Y];
            EStatic = new List<IEntity>[grids.X, grids.Y];
            for(int x = 0; x < grids.X; x++) {
                for(int y = 0; y < grids.Y; y++) {
                    EDynamic[x, y] = new List<IEntity>();
                    EStatic[x, y] = new List<IEntity>();
                }
            }
            ActiveGrids = new List<Point>();
            Fog = new uint[grids.X, grids.Y];
            Collision = new bool[grids.X, grids.Y];
        }

        public void Add(IEntity o) {
            // Canonical position of the object represented in 0~1
            Point p = HashHelper.Hash(o.CollisionGeometry.Center, grids, size);

            // Move To Correct List
            if(!o.CollisionGeometry.IsStatic) {
                // Check If Active
                if(EDynamic[p.X, p.Y].Count < 1)
                    ActiveGrids.Add(p);
                EDynamic[p.X, p.Y].Add(o);
            }
            else EStatic[p.X, p.Y].Add(o);
        }

        public void ClearDynamic() {
            for(int i = 0; i < ActiveGrids.Count; i++)
                EDynamic[ActiveGrids[i].X, ActiveGrids[i].Y] = new List<IEntity>();
            ActiveGrids = new List<Point>();
        }

        // Precondition This[x,y] Must Be Active
        public void HandleGridCollision(int x, int y, int dx, int dy) {
            // Check Bounds
            int ox = x + dx;
            if(ox < 0 || ox >= grids.X) return;
            int oy = y + dy;
            if(oy < 0 || oy >= grids.Y) return;

            var al1 = EDynamic[x, y];
            var al2 = EDynamic[ox, oy];
            var sl2 = EStatic[ox, oy];

            // Empty Check
            if(al2.Count + sl2.Count < 1) return;

            for(int i1 = 0; i1 < al1.Count; i1++) {
                // Dynamic-Dynamic
                for(int i2 = 0; i2 < al2.Count; i2++)
                    // Get Rid Of Doubles
                    if(al1[i1].UUID > al2[i2].UUID)
                        CollisionController.ProcessCollision(al1[i1].CollisionGeometry, al2[i2].CollisionGeometry);
                // Dynamic-Static
                for(int i2 = 0; i2 < sl2.Count; i2++)
                    CollisionController.ProcessCollision(al1[i1].CollisionGeometry, sl2[i2].CollisionGeometry);
            }
        }
        public void HandleGridCollision(int x, int y) {
            var al = EDynamic[x, y];
            var sl = EStatic[x, y];

            // Dynamic-Dynamic
            for(int i1 = 0; i1 < al.Count - 1; i1++)
                for(int i2 = i1 + 1; i2 < al.Count; i2++)
                    CollisionController.ProcessCollision(al[i1].CollisionGeometry, al[i2].CollisionGeometry);
            // Dynamic-Static
            for(int i1 = 0; i1 < al.Count; i1++)
                for(int i2 = 0; i2 < sl.Count; i2++)
                    CollisionController.ProcessCollision(al[i1].CollisionGeometry, sl[i2].CollisionGeometry);
        }

        public void SetFogOfWar(int x, int y, int p, FogOfWar f) {
            FogOfWarHelper.SetFogOfWar(ref Fog[x, y], p, f);
        }
        public FogOfWar GetFogOfWar(int x, int y, int p) {
            return FogOfWarHelper.GetFogOfWar(Fog[x, y], p);
        }

        public void SetCollision(int x, int y, bool c) {
            Collision[x, y] = c;
        }
        public bool GetCollision(int x, int y) {
            return Collision[x, y];
        }
    }

    public class ImpactGrid {

        private float cellSize;
        private Point numCells;
        private Vector2 gridSize;
        private ImpactCell[,] grid;

        public ImpactGrid(CollisionGrid cg) {
            cellSize = 2 * cg.cellSize;
            numCells = new Point((int)Math.Ceiling(cg.size.X / cellSize), (int)Math.Ceiling(cg.size.Y / cellSize));
            cellSize = cg.size.X / numCells.X;
            grid = new ImpactCell[numCells.X, numCells.Y];
            gridSize = cg.size;

            for (int x = 0; x < gridSize.X; x++) {
                for (int y = 0; y < gridSize.Y; y++) {
                    grid[x, y] = new ImpactCell();
                }
            }
        }

        public void AddImpactGenerator(ImpactGenerator g) {
            Point p = HashHelper.Hash(g.Position, numCells, gridSize);
            grid[p.X, p.Y].AddImpactGenerator(g);
        }
    }

    public class ImpactCell {
        
        public Region Region { get; set; }
        public List<ImpactGenerator> ImpactGenerators { get; set; }
        public int CellImpact { get; private set; } 
        public event Action<int> IncreaseImpact; 

        public ImpactCell() {
            Region = null;
            ImpactGenerators = new List<ImpactGenerator>();
            CellImpact = 0;
        }

        public void AddImpactGenerator(ImpactGenerator g) {
            ImpactGenerators.Add(g);
            g.GenerateImpact += AddToImpact;
        }

        public void AddToImpact(int amount){
            CellImpact += amount;
            IncreaseImpact(amount);
        }
    }

    public static class HashHelper {
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
    }
}
