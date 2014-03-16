using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data {
    public struct Grid {
        public List<ICollidable> NonStatic;
        public List<ICollidable> Static;
        public bool IsActive {
            get {
                return NonStatic.Count > 0;
            }
        }

        public void Add(ICollidable o) {
            if(o.IsStatic) Static.Add(o);
            else NonStatic.Add(o);
        }
    }

    public class HashGrid {
        private float width;
        private float height;
        private Point gridCount;

        private List<Point> activeGrids;
        public IEnumerable<Point> Active {
            get { return activeGrids; }
        }

        private Grid[,] grids;
        public Grid this[int x, int y] {
            get { return grids[x, y]; }
        }

        // Constructor
        public HashGrid(float w, float h, float gridSize) {
            width = w;
            height = h;

            // Round down the grid size so they all fit into the map
            gridCount = new Point((int)Math.Ceiling(width / gridSize), (int)Math.Ceiling(height / gridSize));
            gridSize = width / gridCount.X;

            grids = new Grid[gridCount.X, gridCount.Y];
            for(int x = 0; x < gridCount.X; x++) {
                for(int y = 0; y < gridCount.Y; y++) {
                    grids[x, y].Static = new List<ICollidable>();
                    grids[x, y].NonStatic = new List<ICollidable>();
                }
            }
            activeGrids = new List<Point>();
        }

        // Add Object To One Of The Grids
        public void AddObject(ICollidable obj) {
            // Canonical position of the object represented in 0~1
            Vector2 pos = obj.Center;
            pos.X *= gridCount.X / width;
            pos.Y *= gridCount.Y / height;
            int px = (int)pos.X;
            int py = (int)pos.Y;
            if(px < 0) px = 0;
            else if(px >= gridCount.X) px = gridCount.X - 1;
            if(py < 0) py = 0;
            else if(py >= gridCount.Y) py = gridCount.Y - 1;

            // Check If Active
            if(!grids[px, py].IsActive && !obj.IsStatic)
                activeGrids.Add(new Point(px, py));

            // Add To Grid
            grids[px, py].Add(obj);
        }

        // Precondition This[x,y] Must Be Active
        public void HandleGridCollision(int x, int y, int dx, int dy) {
            // Check Bounds
            int ox = x + dx;
            if(ox < 0 || ox >= gridCount.X) return;
            int oy = y + dy;
            if(oy < 0 || oy >= gridCount.Y) return;

            var al1 = grids[x, y].NonStatic;
            var al2 = grids[ox, oy].NonStatic;
            var sl2 = grids[ox, oy].Static;

            // Empty Check
            if(al2.Count + sl2.Count < 1) return;

            // Active To All In Second Grid
            foreach(ICollidable o1 in al1) {
                foreach(ICollidable o2 in al2) CollisionController.ProcessCollision(o1, o2);
                foreach(ICollidable o2 in sl2) CollisionController.ProcessCollision(o1, o2);
            }
        }
        public void HandleGridCollision(int x, int y) {
            var al = grids[x, y].NonStatic;
            var sl = grids[x, y].Static;

            // Active To All In Grid
            foreach(ICollidable o1 in al) {
                foreach(ICollidable o2 in al) {
                    // Non-self Check
                    if(o1 != o2)
                        CollisionController.ProcessCollision(o1, o2);
                }
                foreach(ICollidable o2 in sl) CollisionController.ProcessCollision(o1, o2);
            }
        }
    }
}