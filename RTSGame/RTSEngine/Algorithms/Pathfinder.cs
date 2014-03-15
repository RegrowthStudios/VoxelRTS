using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;

namespace RTSEngine.Algorithms {
    public class PathQuery {
        // Flag If The Path Query Is Resolved
        public bool IsComplete {
            get;
            set;
        }

        // Flag If The Query Is Old And Shouldn't Be Completed
        public bool IsOld {
            get;
            set;
        }

        // The Start And End For Pathfinding
        public Vector2 Start {
            get;
            private set;
        }
        public Vector2 End {
            get;
            private set;
        }

        // Where To Store The Data
        public readonly List<Vector2> waypoints;

        public PathQuery(Vector2 s, Vector2 e) {
            Start = s;
            End = e;
            IsComplete = false;
            IsOld = false;
            waypoints = new List<Vector2>();
        }
    }

    // TODO: Implement Pathfinder
    public class Pathfinder {

        public Pathfinder(Heightmap map) {

        }

        public void Add(PathQuery q) {
            // TODO: Concurrent Addition Of A Pathfinding Query
        }

        public void WorkThread() {

        }
    }
}