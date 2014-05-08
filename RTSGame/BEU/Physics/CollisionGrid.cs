using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEU.Data;
using Microsoft.Xna.Framework;

namespace BEU.Physics {
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

        public List<Tank>[,] EDynamic {
            get;
            private set;
        }
        public Obstacle[,] EStatic {
            get;
            private set;
        }

        // Locations In The Grid Which Contain Units
        public List<Point> ActiveGrids {
            get;
            private set;
        }

        public CollisionGrid(int w, int h) {
            cellSize = Constants.CGRID_SIZE;
            numCells = new Point(w, h);
            size = new Vector2(w, h) * cellSize;

            EDynamic = new List<Tank>[numCells.X, numCells.Y];
            EStatic = new Obstacle[numCells.X, numCells.Y];
            for(int x = 0; x < numCells.X; x++) {
                for(int y = 0; y < numCells.Y; y++) {
                    EDynamic[x, y] = new List<Tank>();
                }
            }
            ActiveGrids = new List<Point>();
        }

        public void Add(Tank o) {
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
        public void Add(Obstacle b) {
            b.OnDestruction += OnObstacleDestruction;

            // Add To All The Cells
            Point p = HashHelper.Hash(b.StartPos, numCells, size);
            for(int y = 0; y < b.Data.GridSize.Y; y++) {
                for(int x = 0; x < b.Data.GridSize.X; x++) {
                    EStatic[p.X + x, p.Y + y] = b;
                }
            }
        }

        public void ClearDynamic() {
            for(int i = 0; i < ActiveGrids.Count; i++)
                EDynamic[ActiveGrids[i].X, ActiveGrids[i].Y] = new List<Tank>();
            ActiveGrids = new List<Point>();
        }

        public bool CanMoveTo(Point pOrigin, byte direction) {

            return false;
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

            // Empty Check
            if(al2.Count + (sl2 == null ? 0 : 1) < 1) return;

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
        }
        public void HandleGridCollision(int x, int y) {
            var al = EDynamic[x, y];
            var sl = EStatic[x, y];

            // Dynamic-Dynamic
            for(int i1 = 0; i1 < al.Count - 1; i1++)
                for(int i2 = i1 + 1; i2 < al.Count; i2++)
                    CollisionController.ProcessCollision(al[i1].CollisionGeometry, al[i2].CollisionGeometry);
            // Dynamic-Static
            if(sl != null)
                for(int i1 = 0; i1 < al.Count; i1++)
                    CollisionController.ProcessCollision(al[i1].CollisionGeometry, sl.CollisionGeometry);
        }

        public bool GetCollision(int x, int y) {
            return EStatic[x, y] != null;
        }
        public bool GetCollision(Vector2 pos) {
            Point c = HashHelper.Hash(pos, numCells, size);
            return GetCollision(c.X, c.Y);
        }

        public void OnObstacleSpawn(Obstacle b) {
            Add(b);
        }
        public void OnObstacleDestruction(ACEntity o) {
            o.OnDestruction -= OnObstacleDestruction;
            Obstacle b = o as Obstacle;

            // Add To All The Cells
            Point p = HashHelper.Hash(b.StartPos, numCells, size);
            for(int y = 0; y < b.Data.GridSize.Y; y++) {
                for(int x = 0; x < b.Data.GridSize.X; x++) {
                    EStatic[p.X + x, p.Y + y] = null;
                }
            }
        }
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
