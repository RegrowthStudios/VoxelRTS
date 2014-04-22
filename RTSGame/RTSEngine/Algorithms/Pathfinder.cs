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

        // Which Fog Of War Is To Be Read
        public readonly int FOWIndex;

        public PathQuery(Vector2 s, Vector2 e, int fowI) {
            Start = s;
            End = e;
            IsComplete = false;
            IsOld = false;
            waypoints = new List<Vector2>();
            FOWIndex = fowI;
        }
    }

    public class Pathfinder : IDisposable {
        // A*
        private static GameState gameState;
        private static CollisionGrid World { get { return gameState.CGrid; } }
        // Whether A World Coordinate Is Collidable, Given The Pathfinding Team's Knowledge Of The World
        private bool[,] isCollidable;
        protected Point[,] prev;
        protected int[,] gScore, fScore;
        protected Point start;
        protected Point end;
        // Threading
        private bool running;
        private ConcurrentQueue<PathQuery> queries;
        private Thread thread;



        public Pathfinder(GameState g) {
            // A*
            gameState = g;
            prev = new Point[World.numCells.X, World.numCells.Y];
            fScore = new int[World.numCells.X, World.numCells.Y];
            gScore = new int[World.numCells.X, World.numCells.Y];
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
        public static bool InGrid(Point p) {
            return p.X >= 0 && p.X < World.numCells.X && p.Y >= 0 && p.Y < World.numCells.Y;
        }

        public static IEnumerable<Point> NeighborhoodDiag(Point p) {
            yield return new Point(p.X + 1, p.Y + 1);
            yield return new Point(p.X + 1, p.Y - 1);
            yield return new Point(p.X - 1, p.Y + 1);
            yield return new Point(p.X - 1, p.Y - 1);
        }

        public static IEnumerable<Point> NeighborhoodAlign(Point p) {
            yield return new Point(p.X + 1, p.Y);
            yield return new Point(p.X - 1, p.Y);
            yield return new Point(p.X, p.Y + 1);
            yield return new Point(p.X, p.Y - 1);
        }

        public static IEnumerable<Point> Neighborhood(Point p) {
            return (NeighborhoodAlign(p).Concat<Point>(NeighborhoodDiag(p))).Where(InGrid);
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

        private void BuildPath(List<Point> p, Point cur) {
            while(cur.X != start.X || cur.Y != start.Y) {
                p.Add(cur);
                cur = prev[cur.X, cur.Y];
            }
            p.Add(cur);
        }

        private bool CanMove(Point n, int fowI) {
            if(!InGrid(n)) return false;

            FogOfWar f = World.GetFogOfWar(n.X, n.Y, fowI);
            switch(f) {
                case FogOfWar.Nothing:
                    return true;
                case FogOfWar.Passive:
                    return !(isCollidable[n.X, n.Y] || World.Collision[n.X, n.Y]); 
                case FogOfWar.Active:
                    return !World.GetCollision(n.X, n.Y);
            }
            return false;
        }

        private Point FindClosestGoal(Point e, int[,] f, bool[,] c, out int s) {
            if(InGrid(e) && c[e.X, e.Y] == false) {
                c[e.X, e.Y] = true;

                if(f[e.X, e.Y] != int.MaxValue) {
                    s = Estimate(e.X, e.Y);
                    return e;
                }

                s = int.MaxValue;
                Point p = e;

                int ns;
                foreach(var n in NeighborhoodAlign(e)) {
                    Point np = FindClosestGoal(n, f, c, out ns);
                    if(ns != int.MaxValue && ns < s) {
                        s = ns;
                        p = np;
                    }
                }
                return p;
            }
            else {
                s = int.MaxValue;
                return new Point(-1, -1);
            }
        }

        // Run A* Search, Given This Pathfinder's World And A Query
        private void Pathfind(PathQuery q) {
            // Initialization
            isCollidable = new bool[World.numCells.X, World.numCells.Y];
            start = HashHelper.Hash(q.Start, World.numCells, World.size);
            end = HashHelper.Hash(q.End, World.numCells, World.size); ;
            for(int y = 0; y < World.numCells.Y; y++) {
                for(int x = 0; x < World.numCells.X; x++) {
                    fScore[x, y] = int.MaxValue;
                    gScore[x, y] = int.MaxValue;
                }
            }
            // Precondition: Any Buildings In World Have Valid Centers
            var viewedBuildings = gameState.teams[q.FOWIndex].ViewedEnemyBuildings;
            foreach(var vb in viewedBuildings) {
                var vbData = gameState.teams[vb.Team].Race.Buildings[vb.Type];
                Point p = vb.CellPoint;
                for(int y = 0; y < vbData.GridSize.Y; y++) {
                    for(int x = 0; x < vbData.GridSize.X; x++) {
                        isCollidable[p.X + x, p.Y + y] = true;
                    }
                }
            }
            gScore[start.X, start.Y] = 0;
            fScore[start.X, start.Y] = Estimate(start.X, start.Y);
            var openSet = new MinHeap<Point>(Comparison, 30);
            openSet.Insert(start);

            // A* Loop
            List<Point> path = null;
            while(openSet.Count > 0) {
                Point p = openSet.Pop();
                if(p.X == end.X && p.Y == end.Y) {
                    path = new List<Point>();
                    BuildPath(path, end);
                    break;
                }
                bool canMove = false;
                foreach(Point n in NeighborhoodAlign(p).Where(InGrid)) {
                    int tgs = gScore[p.X, p.Y] + 10;
                    canMove = CanMove(n, q.FOWIndex);
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
                    canMove = CanMove(n, q.FOWIndex);
                    foreach(Point d in DiagDecomp(p, n)) {
                        canMove &= CanMove(d, q.FOWIndex);
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

            // Check If We Need To Find The Nearest Point
            if(path == null) {
                int s;
                bool[,] ch = new bool[World.numCells.X, World.numCells.Y];
                Array.Clear(ch, 0, ch.Length);
                Point cg = FindClosestGoal(end, fScore, ch, out s);
                if(s == int.MaxValue) {
                    // Impossible
                }
                else {
                    path = new List<Point>();
                    BuildPath(path, cg);
                }
            }

            // A* Conclusion
            if(path != null) {
                foreach(Point wp in path) {
                    q.waypoints.Add(new Vector2(((float)wp.X + 0.5f) * World.cellSize, ((float)wp.Y + 0.5f) * World.cellSize));
                }
            }
            q.IsComplete = true;
        }
    }
}