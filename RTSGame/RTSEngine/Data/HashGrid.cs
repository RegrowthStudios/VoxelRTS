using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data
{
    public class Grid {
        public List<ICollidable> list;
        
        public Grid() {
            list = new List<ICollidable>();
        }

        public void Add(ICollidable obj) {
            list.Add(obj);
        }

        public bool IsEmpty(){
            return list.Count == 0;
        }
    }

    public class HashGrid
    {
        public float width;
        public float height;
        public Point gridCount;
        public float gridSize;
        private Grid[,] grids;

        public Grid GetGrid(int x, int y) {
            return grids[x, y];
        }

        // Constructor
        public HashGrid(float width, float height, float gridSize) {
            this.width = width;
            this.height = height;

            // Round down the grid size so they all fit into the map
            gridCount = new Point((int)Math.Ceiling(width/gridSize), (int)Math.Ceiling(height/gridSize));
            gridSize = width / gridCount.X;

            grids = new Grid[gridCount.X, gridCount.Y];
            for (int x = 0; x < gridCount.X; x++) {
                for (int y = 0; y < gridCount.Y; y++) {
                    grids[x,y] = new Grid();
                }
            }
        }

        // Add object to one of the grid
        public void AddObject(ICollidable obj) {
            // Canonical position of the object represented in 0~1
            Vector2 pos = new Vector2(obj.Center.X / width, obj.Center.Y / height);
            // Add object to the corresponding grid
            grids[(int)pos.X * gridCount.X, (int)pos.Y * gridCount.Y].Add(obj);
        }

        // Helper method for resolving collision between grid(x,y) and grid(x+dx,y+dy)
        // Precondition: grid(x, y) is not empty
        public void HandleGridCollision(int x, int y, int dx, int dy) {
            if (IsEmpty(x + dx, y + dy)) return;

            Grid grid1 = GetGrid(x, y);
            Grid grid2 = GetGrid(x+dx, y+dy);
            foreach (ICollidable o1 in grid1.list) {
                foreach (ICollidable o2 in grid2.list) {
                    if (o1 != o2)
                        CollisionController.ProcessCollision(o1, o2);
                }
            }
        }

        public bool IsEmpty(int x, int y) {
            return grids[x, y].IsEmpty();
        }
    }
}
