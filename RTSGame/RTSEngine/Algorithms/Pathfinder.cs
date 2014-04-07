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
        public bool IsComplete { get; set; }

        // Flag If The Query Is Old And Shouldn't Be Completed
        public bool IsOld { get; set; }

        // The Start And End For Pathfinding
        public Vector2 Start { get; private set; }
        public Vector2 End { get; private set; }

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

    public class Pathfinder : IDisposable {
        // A*
        private CollisionGrid world;
        protected Point[,] prev;
        protected int[,] gScore, fScore;
        protected Point start;
        protected Point end;
        // Threading
        private bool running;
        private ConcurrentQueue<PathQuery> queries;
        private Thread thread;

        public Pathfinder(CollisionGrid cg) {
            // A*
            world = cg;
            prev = new Point[world.numCells.X, world.numCells.Y];
            fScore = new int[world.numCells.X, world.numCells.Y];
            gScore = new int[world.numCells.X, world.numCells.Y];
            // Threading
            running = true;
            queries = new ConcurrentQueue<PathQuery>();
            thread = new Thread(WorkThread);
            thread.Priority = ThreadPriority.Normal;
            thread.IsBackground = true;
            thread.Start();
        }

        public void Add(PathQuery q) {
            queries.Enqueue(q);
        }

        public void WorkThread() {
            while(running) {
                while(queries.Count > 0) {
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

        // Heuristic Function
        private int Estimate(int x, int y) {
            Point md = new Point(Math.Abs(x - end.X), Math.Abs(y - end.Y));
            if(md.X < md.Y)
                return md.X * 14 + (md.Y - md.X) * 10;
            else
                return md.Y * 14 + (md.X - md.Y) * 10;
        }

        private int Comparison(Point p1, Point p2) {
            return fScore[p1.X, p1.Y].CompareTo(fScore[p2.X, p2.Y]);
        }

        // Get The Search Grid Locations Adjacent To The Input
        private bool InGrid(Point p) {
            return p.X >= 0 && p.X < world.numCells.X && p.Y >= 0 && p.Y < world.numCells.Y;
        }

        private IEnumerable<Point> NeighborhoodDiag(Point p) {
            yield return new Point(p.X + 1, p.Y + 1);
            yield return new Point(p.X + 1, p.Y - 1);
            yield return new Point(p.X - 1, p.Y + 1);
            yield return new Point(p.X - 1, p.Y - 1);
        }

        private IEnumerable<Point> NeighborhoodAlign(Point p) {
            yield return new Point(p.X + 1, p.Y);
            yield return new Point(p.X - 1, p.Y);
            yield return new Point(p.X, p.Y + 1);
            yield return new Point(p.X, p.Y - 1);
        }

        // Return The Two Aligned Locations One Could Cross Instead Of Moving Diagonally From P to N
        private IEnumerable<Point> DiagDecomp(Point p, Point n) {
            if(n.X < p.X && n.Y < p.Y) {
                yield return new Point(p.X - 1, p.Y);
                yield return new Point(p.X, p.Y - 1);
            }
            else if(n.X < p.X) {
                yield return new Point(p.X - 1, p.Y);
                yield return new Point(p.X, p.Y + 1);
            }
            else if(n.Y < p.Y) {
                yield return new Point(p.X + 1, p.Y);
                yield return new Point(p.X, p.Y - 1);
            }
            else {
                yield return new Point(p.X + 1, p.Y);
                yield return new Point(p.X, p.Y + 1);
            }
        }

        private void BuildPath(List<Point> p) {
            Point cur = end;
            while(cur.X != start.X || cur.Y != start.Y) {
                p.Add(cur);
                cur = prev[cur.X, cur.Y];
            }
            p.Add(cur);
        }

        // Run A* Search, Given This Pathfinder's World And A Query
        private void Pathfind(PathQuery q) {
            // Initialization
            start = HashHelper.Hash(q.Start, world.numCells, world.size);
            end = HashHelper.Hash(q.End, world.numCells, world.size); ;
            for(int y = 0; y < world.numCells.Y; y++) {
                for(int x = 0; x < world.numCells.X; x++) {
                    fScore[x, y] = int.MaxValue;
                    gScore[x, y] = int.MaxValue;
                }
            }
            gScore[start.X, start.Y] = 0;
            fScore[start.X, start.Y] = Estimate(start.X, start.Y);
            var openSet = new MinHeap<Point>(Comparison, 30);
            openSet.Insert(start);

            // A* Loop
            // TODO: Add Fog Awareness
            List<Point> path = null;
            while(openSet.Count > 0) {
                Point p = openSet.Pop();
                if(p.X == end.X && p.Y == end.Y) {
                    path = new List<Point>();
                    BuildPath(path);
                    break;
                }
                bool canMove = false;
                foreach(Point n in NeighborhoodAlign(p).Where(InGrid)) {
                    int tgs = gScore[p.X, p.Y] + 10;
                    canMove = InGrid(n) && !world.GetCollision(n.X, n.Y);
                    if(canMove && tgs < gScore[n.X, n.Y]) {
                        prev[n.X, n.Y] = p;
                        gScore[n.X, n.Y] = tgs;
                        fScore[n.X, n.Y] = gScore[n.X, n.Y] + Estimate(n.X, n.Y);
                        if(!openSet.Contains(n)) {
                            openSet.Insert(n);
                        }
                    }
                }
                foreach(Point n in NeighborhoodDiag(p).Where(InGrid)) {
                    int tgs = gScore[p.X, p.Y] + 14;
                    // To Move Diagonally, Destination Must Be Reachable By Horizontal & Vertical Moves As Well
                    canMove = InGrid(n) && !world.GetCollision(n.X, n.Y);
                    foreach(Point d in DiagDecomp(p, n)) {
                        canMove &= InGrid(d) && !world.GetCollision(d.X, d.Y);
                    }
                    if(canMove && tgs < gScore[n.X, n.Y]) {
                        prev[n.X, n.Y] = p;
                        gScore[n.X, n.Y] = tgs;
                        fScore[n.X, n.Y] = gScore[n.X, n.Y] + Estimate(n.X, n.Y);
                        if(!openSet.Contains(n)) {
                            openSet.Insert(n);
                        }
                    }
                }
            }
            // A* Conclusion
            if(path != null) {
                foreach(Point wp in path) {
                    q.waypoints.Add(new Vector2(wp.X * world.cellSize, wp.Y * world.cellSize));
                }
            }
            q.IsComplete = true;
            // TODO: Add Path Smoothing
        }
    }
}