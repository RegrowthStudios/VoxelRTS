using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data
{
    public class Grid {
        private List<IEntity> list;
        
        public Grid() {
            list = new List<IEntity>();
        }

        public void add(IEntity obj) {
            list.Add(obj);
        }
    }

    public class HashGrid
    {
        public float width;
        public float height;
        public Point gridCount;
        public float gridSize;
        private Grid[,] grids;

        public Grid getGrid(int x, int y) {
            return grids[x, y];
        }

        // Constructor
        public HashGrid(float width, float height, float gridSize) {
            this.width = width;
            this.height = height;

            // Round down the grid size so they all fit into the map
            gridCount = new Point((int)Math.Ceiling(width/gridSize), (int)Math.Ceiling(height/gridSize));
            gridSize = width / gridCount.X;

            for (int x = 0; x < gridCount.X; x++) {
                for (int y = 0; y < gridCount.Y; y++) {
                    grids[x,y] = new Grid();
                }
            }
        }

        // Add object to one of the grid
        public void addObject(IEntity obj) {
            // Canonical position of the object represented in 0~1
            Vector2 pos = new Vector2(obj.GridPosition.X / width, obj.GridPosition.Y / height);
            // Add object to the corresponding grid
            grids[(int)pos.X * gridCount.X, (int)pos.Y * gridCount.Y].add(obj);
        }

        // Detect collision for each object using 3 by 3 grids around each object
        public void detectCollision() {
            for (int x = 0; x < gridCount.X; x++) {
                for (int y = 0; y < gridCount.Y; y++) {
                    // Handle corner cases where 3 by 3 grid is out of range
                    if (x == 0 && y != 0) {

                    }
                    else if (x != 0 && y == 0) {
                    }
                    else if (x == 0 && y == 0) {

                    }
                    // Regular handling
                    else {

                    }
                }
            }
        }

        private void performCollision() {
            return;
        }
    }
}
