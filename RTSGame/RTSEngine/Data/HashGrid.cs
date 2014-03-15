using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data
{

    public struct Grid {
        public List<Object> list;

    }

    public class HashGrid
    {
        public int width;
        public int height;
        public Grid[][] grids;

        // Constructor
        public HashGrid() {

        }

        public void addObject(Vector2 center) {

        }
    }
}
