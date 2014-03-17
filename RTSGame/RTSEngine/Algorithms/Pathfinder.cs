using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
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

    public struct SearchLocation {
        public Point Loc;
        public float GScore;
        public float FScore;
        public static int Compare(SearchLocation x, SearchLocation y) {
            return x.FScore.CompareTo(y.FScore);
        }

        public SearchLocation(Point p) {
            Loc = p;
            GScore = 0;
            FScore = 0;
        }
    }

    // TODO: Implement Pathfinder
    public class Pathfinder : IDisposable {

        private ConcurrentQueue<PathQuery> queries; 
        private Thread thread;
        private Heightmap world;
        private SearchLocation[,] searchGrid;

        public Pathfinder(Heightmap map) {
            world = map;
            searchGrid = new SearchLocation[world.HValueWidth, world.HValueDepth];
            queries = new ConcurrentQueue<PathQuery>();
            thread = new Thread(WorkThread);
            thread.Priority = ThreadPriority.Normal;
            thread.Start();
        }

        public void Add(PathQuery q) {
            queries.Enqueue(q);
        }

        public void WorkThread() {
            while(true) {
                while(HasQuery()) {
                    PathQuery q;
                    if(queries.TryDequeue(out q)) {
                        if(q.IsOld) continue;
                        Pathfind(q);
                    }
                }
                Thread.Sleep(100);
            }
        }

        public void Dispose() {
            thread.Abort();
        }

        private bool HasQuery() {
            return queries.Count > 0;
        }

        // Get The Search Grid Locations Adjacent To The Input
        private SearchLocation[] Neighborhood(SearchLocation loc) {
            Point[] candidates = Neighborhood(loc.Loc);
            candidates = candidates.Where(p => p.X >= 0 && p.X < searchGrid.GetLength(0)
                                            && p.Y >= 0 && p.Y < searchGrid.GetLength(1)).ToArray();
            SearchLocation[] locs = new SearchLocation[candidates.Length];
            for(int i = 0; i < candidates.Length; i++) {
                locs[i] = searchGrid[candidates[i].X, candidates[i].Y];
            }
            return locs;
        }

        // Return An Array Of The 8 Cardinal & Ordinal Points Adacent To P
        private Point[] Neighborhood(Point p) {
            Point[] points = {  new Point(p.X-1,p.Y),
                                new Point(p.X-1,p.Y-1),
                                new Point(p.X,p.Y-1),
                                new Point(p.X+1,p.Y-1),
                                new Point(p.X+1,p.Y),
                                new Point(p.X+1,p.Y+1),
                                new Point(p.X,p.Y+1),
                                new Point(p.X-1,p.Y+1)  };
            return points;
        }

        // Euclidean Distance
        private float EucDist(Point start, Point end) {
            float xDist = (float)(end.X - start.X);
            float yDist = (float)(end.Y - start.Y);
            return (float)Math.Sqrt(xDist * xDist + yDist * yDist);
        }

        // Manhattan Distance
        private float ManhatDist(Point start, Point end) {
            float xDist = Math.Abs(end.X - start.X);
            float yDist = Math.Abs(end.Y - start.Y);
            return xDist + yDist;
        }

        private LinkedList<Point> ReconstructPath(Dictionary<Point, Point?> cameFrom, Point current) {
            var path = new LinkedList<Point>();
            path.AddFirst(current);
            Point prev = current;
            while(cameFrom[prev] != null) {
                prev = cameFrom[prev].Value;
                path.AddFirst(prev);
            }
            return path;
        }

        // Run A* Search, Given This Pathfinder's World And A Query
        private void Pathfind(PathQuery q) {
            Point cGridPoint = world.Map(q.Start.X, q.Start.Y);
            Point gGridPoint = world.Map(q.End.X, q.End.Y);
            var closedSet = new HashSet<Point>();
            var openSet = new MinHeap<SearchLocation>(SearchLocation.Compare);
            var cameFrom = new Dictionary<Point, Point?>();
            SearchLocation current = new SearchLocation(cGridPoint);
            current.FScore = EucDist(cGridPoint,gGridPoint);
            openSet.Insert(current);
            // TODO: Verify
            bool success = false;
            while(openSet.Count > 0) {
                current = openSet.Pop();
                if(current.Loc == gGridPoint) {
                    success = true;
                    break;
                }

                openSet.Remove(current);
                closedSet.Add(current.Loc);
                foreach(SearchLocation neighbor in Neighborhood(current)) {
                    if(closedSet.Contains(neighbor.Loc) || world.IsCollidable(neighbor.Loc.X,neighbor.Loc.Y)) continue;
                    float gScoreNew = current.GScore + 1;
                    if(!openSet.Contains(neighbor) || gScoreNew < neighbor.GScore) {
                        cameFrom[neighbor.Loc] = current.Loc;
                        SearchLocation _neighbor = neighbor; // Can't Change Iteration Var
                        _neighbor.GScore = gScoreNew;
                        _neighbor.FScore = gScoreNew + EucDist(_neighbor.Loc, gGridPoint);
                        if(!openSet.Contains(_neighbor)) openSet.Insert(_neighbor);
                    }
                }
            }

            // Finished
            if(success) {
                foreach(Point wp in ReconstructPath(cameFrom, gGridPoint)) {
                    q.waypoints.Add(world.UnMap(wp.X, wp.Y));
                }
            }
            q.IsComplete = true;
        }
    }
}