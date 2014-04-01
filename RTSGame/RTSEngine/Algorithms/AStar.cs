using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Algorithms;
using RTSEngine.Data;

namespace RTSEngine.Algorithms {
    public class AStar {
        protected CollisionGrid world;
        protected Point[,] prev;
        protected int[,] gScore, fScore;
        protected Point start;
        protected Point end;

        private int Estimate(int x, int y) {
            Point d = new Point(Math.Abs(x - end.X), Math.Abs(x - end.Y));
            if(d.X > d.Y)
                return (d.X - d.Y) * 14 + d.Y * 10;
            else
                return (d.Y - d.X) * 14 + d.X * 10;
        }
        private int Comparison(Point p1, Point p2) {
            return fScore[p1.X, p1.Y].CompareTo(fScore[p2.X, p2.Y]);
        }

        // Get The Search Grid Locations Adjacent To The Input
        private bool PointPossible(Point p) {
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

        public AStar(CollisionGrid w, Point s, Point e) {
            world = w;
            start = s;
            end = e;
        }

        public List<Point> Evaluate() {
            prev = new Point[world.numCells.X, world.numCells.Y];
            fScore = new int[world.numCells.X, world.numCells.Y];
            gScore = new int[world.numCells.X, world.numCells.Y];
            for(int y = 0; y < world.numCells.Y; y++) {
                for(int x = 0; x < world.numCells.X; x++) {
                    fScore[x, y] = int.MaxValue;
                    gScore[x, y] = int.MaxValue;
                }
            }

            gScore[start.X, start.Y] = 0;
            fScore[start.X, start.Y] = gScore[start.X, start.Y] + Estimate(start.X, start.Y);

            var openSet = new MinHeap<Point>(Comparison, 30);
            var closedSet = new List<Point>();
            openSet.Insert(start);
            while(openSet.Count > 0) {
                Point p = openSet.Pop();
                if(p.X == end.X && p.Y == end.Y) {
                    List<Point> path = new List<Point>();
                    BuildPath(path);
                    return path;
                }
                foreach(Point n in NeighborhoodAlign(p).Where(PointPossible)) {
                    int tgs = gScore[p.X, p.Y] + 10;
                    if(tgs < gScore[n.X, n.Y]) {
                        prev[n.X, n.Y] = p;
                        gScore[n.X, n.Y] = tgs;
                        fScore[n.X, n.Y] = gScore[n.X, n.Y] + Estimate(n.X, n.Y);
                        if(!openSet.Contains(n)) {
                            openSet.Insert(n);
                        }
                    }
                }
                foreach(Point n in NeighborhoodDiag(p).Where(PointPossible)) {
                    int tgs = gScore[p.X, p.Y] + 14;
                    if(tgs < gScore[n.X, n.Y]) {
                        prev[n.X, n.Y] = p;
                        gScore[n.X, n.Y] = tgs;
                        fScore[n.X, n.Y] = gScore[n.X, n.Y] + Estimate(n.X, n.Y);
                        if(!openSet.Contains(n)) {
                            openSet.Insert(n);
                        }
                    }
                }
            }
            return null;
        }

        private void BuildPath(List<Point> p) {
            Point cur = end;
            while(cur.X != start.X || cur.Y != start.Y) {
                p.Add(cur);
                cur = prev[cur.X, cur.Y];
            }
            p.Add(cur);
        }
    }
}