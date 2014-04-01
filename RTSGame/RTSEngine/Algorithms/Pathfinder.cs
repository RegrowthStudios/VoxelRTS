using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using Microsoft.Xna.Framework;
using RTSEngine.Controllers;
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
        public int GScore;
        public int FScore;
        public static int Compare(SearchLocation x, SearchLocation y) {
            return x.FScore.CompareTo(y.FScore);
        }
        public SearchLocation(Point p) {
            Loc = p;
            GScore = 0;
            FScore = 0;
        }
    }

    public class Pathfinder : IDisposable {
        private bool running; 
        private Thread thread;
        private CollisionGrid world;
        private SearchLocation[,] searchGrid;

        private ConcurrentQueue<PathQuery> queries; 

        public Pathfinder(CollisionGrid cg) {
            world = cg;
            searchGrid = new SearchLocation[cg.numCells.X, cg.numCells.Y];
            for(int i = 0; i < cg.numCells.X; i++) {
                for(int j = 0; j < cg.numCells.Y; j++) {
                    searchGrid[i, j].Loc.X = i;
                    searchGrid[i, j].Loc.Y = j;
                }
            }
            queries = new ConcurrentQueue<PathQuery>();
            running = true;
            thread = new Thread(WorkThread);
            thread.Priority = ThreadPriority.Normal;
            thread.Start();
        }
     
        public void Add(PathQuery q) {
            queries.Enqueue(q);
        }
        
        public void WorkThread() {
            while(running) {
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
            running = false;
            thread.Join();
        }
        
        private bool HasQuery() {
            return queries.Count > 0;
        }
        
        // Get The Search Grid Locations Adjacent To The Input
        private bool PointPossible(Point p) {
            return p.X >= 0 && p.X < world.numCells.X && p.Y >= 0 && p.Y < world.numCells.Y;
        }
        
        private IEnumerable<SearchLocation> NeighborhoodOrdinal(SearchLocation loc) {
            foreach(Point p in NeighborhoodDiag(loc.Loc).Where(PointPossible)) {
                yield return searchGrid[p.X, p.Y];
            }
        }
        
        private IEnumerable<SearchLocation> NeighborhoodCardinal(SearchLocation loc) {
            foreach(Point p in NeighborhoodAlign(loc.Loc).Where(PointPossible)) {
                yield return searchGrid[p.X, p.Y];
            }
        }
        
        // Return An Array Of The Ordinal Points Adjacent To P
        private IEnumerable<Point> NeighborhoodDiag(Point p) {
            yield return new Point(p.X + 1, p.Y + 1);
            yield return new Point(p.X + 1, p.Y - 1);
            yield return new Point(p.X - 1, p.Y + 1);
            yield return new Point(p.X - 1, p.Y - 1);
        }

        // Return An Array Of The Cardinal Points Adjacent To P
        private IEnumerable<Point> NeighborhoodAlign(Point p) {
            yield return new Point(p.X + 1, p.Y);
            yield return new Point(p.X - 1, p.Y);
            yield return new Point(p.X, p.Y + 1);
            yield return new Point(p.X, p.Y - 1);
        }

        // Manhattan Distance
        private int ManhatDist(Point start, Point end) {
            int xDist = Math.Abs(end.X - start.X);
            int yDist = Math.Abs(end.Y - start.Y);
            if(xDist < yDist)
                return xDist * 14 + (yDist - xDist) * 10;
            else
                return yDist * 14 + (xDist - yDist) * 10;
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
            DevConsole.AddCommand("Pathfinding...");
            Point cGridPoint = HashHelper.Hash(q.Start, world.numCells, world.size);
            Point gGridPoint = HashHelper.Hash(q.End, world.numCells, world.size); ;
            var closedSet = new HashSet<Point>();
            var openSet = new MinHeap<SearchLocation>(SearchLocation.Compare);
            var cameFrom = new Dictionary<Point, Point?>();
            SearchLocation current = new SearchLocation(cGridPoint);
            current.FScore = ManhatDist(cGridPoint,gGridPoint);
            openSet.Insert(current);
            // TODO: Verify
            bool success = false;
            while(openSet.Count > 0) {
                DevConsole.AddCommand("Entered while loop");
                current = openSet.Pop();
                if(current.Loc == gGridPoint) {
                    DevConsole.AddCommand("Found goal");
                    success = true;
                    break;
                }
                closedSet.Add(current.Loc);
                foreach(SearchLocation neighbor in NeighborhoodOrdinal(current)) {
                    DevConsole.AddCommand("Searching Ordinal Neighbors...");
                    if(closedSet.Contains(neighbor.Loc) || world.GetCollision(neighbor.Loc.X, neighbor.Loc.Y)) continue;
                    int gScoreNew = current.GScore + 14;
                    if(!openSet.Contains(neighbor) || gScoreNew < neighbor.GScore) {
                        DevConsole.AddCommand("Exploring Ordinal...");
                        cameFrom[neighbor.Loc] = current.Loc;
                        SearchLocation _neighbor = neighbor; // Can't Change Iteration Var
                        _neighbor.GScore = gScoreNew;
                        _neighbor.FScore = gScoreNew + ManhatDist(_neighbor.Loc, gGridPoint);
                        if(!openSet.Contains(_neighbor)) openSet.Insert(_neighbor);
                    }
                }
                foreach(SearchLocation neighbor in NeighborhoodCardinal(current)) {
                    DevConsole.AddCommand("Searching Cardinal Neighbors...");
                    if(closedSet.Contains(neighbor.Loc) || world.GetCollision(neighbor.Loc.X, neighbor.Loc.Y)) continue;
                    int gScoreNew = current.GScore + 10;
                    if(!openSet.Contains(neighbor) || gScoreNew < neighbor.GScore) {
                        DevConsole.AddCommand("Exploring Cardinal...");
                        cameFrom[neighbor.Loc] = current.Loc;
                        SearchLocation _neighbor = neighbor; // Can't Change Iteration Var
                        _neighbor.GScore = gScoreNew;
                        _neighbor.FScore = gScoreNew + ManhatDist(_neighbor.Loc, gGridPoint);
                        if(!openSet.Contains(_neighbor)) openSet.Insert(_neighbor);
                    }
                }
            }
            // Finished
            if(success) {
                foreach(Point wp in ReconstructPath(cameFrom, gGridPoint)) {
                    q.waypoints.Add(new Vector2(wp.X * world.cellSize, wp.Y * world.cellSize));
                }
                DevConsole.AddCommand("Path found with size " + q.waypoints.Count);
            }
            else {
                DevConsole.AddCommand("Pathfinder failed :(");
            }
            q.IsComplete = true;
        }
    }
}