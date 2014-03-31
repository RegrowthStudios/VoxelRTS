using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Algorithms;

namespace RTSEngine.Controllers {
    public enum TravelDirection {
        PX,
        NX,
        PY,
        NY
    }
    public struct FOWPoint {
        public int X, Y;
        public TravelDirection PreviousDirection;
        public int TravelAmount;

        public FOWPoint(int x, int y, TravelDirection t, int ptAmount) {
            X = x;
            Y = y;
            PreviousDirection = t;
            TravelAmount = ptAmount - 1;
        }
    }

    public class FOWTask : ACBudgetedTask {
        private int teamIndex;
        private GameState state;

        private List<Point> active;
        private int[,] heat;

        public FOWTask(GameState s, int tIndex)
            : base(s.Teams[tIndex].units.Count) {
            teamIndex = tIndex;
            state = s;
            active = new List<Point>();
            heat = new int[s.CGrid.numCells.X, s.CGrid.numCells.Y];
        }

        private bool IsGood(int gx, int gy, int[,] val, ref FOWPoint p) {
            if(p.X >= 0 && p.X < gx && p.Y >= 0 && p.Y < gy) {
                return val[p.X, p.Y] < p.TravelAmount;
            }
            else return false;
        }
        private void AddPoints(int gx, int gy, int[,] val, Queue<FOWPoint> q, ref FOWPoint prev) {
            FOWPoint fp;
            switch(prev.PreviousDirection) {
                case TravelDirection.PX:
                    fp = new FOWPoint(prev.X + 1, prev.Y, TravelDirection.PX, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y + 1, TravelDirection.PY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y - 1, TravelDirection.NY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    break;
                case TravelDirection.NX:
                    fp = new FOWPoint(prev.X - 1, prev.Y, TravelDirection.NX, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y + 1, TravelDirection.PY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y - 1, TravelDirection.NY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    break;
                case TravelDirection.PY:
                    fp = new FOWPoint(prev.X + 1, prev.Y, TravelDirection.PX, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X - 1, prev.Y, TravelDirection.NX, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y + 1, TravelDirection.PY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    break;
                case TravelDirection.NY:
                    fp = new FOWPoint(prev.X + 1, prev.Y, TravelDirection.PX, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X - 1, prev.Y, TravelDirection.NX, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y - 1, TravelDirection.NY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    break;
            }
        }
        public override void DoWork(float dt) {
            RTSTeam team = state.Teams[teamIndex];
            CollisionGrid cg = state.CGrid;

            var newActive = new List<Point>(active.Count);
            var queue = new Queue<FOWPoint>();

            // Generate All The Old FOW
            int[,] val = new int[cg.numCells.X, cg.numCells.Y];
            for(int y = 0; y < cg.numCells.Y; y++) {
                for(int x = 0; x < cg.numCells.X; x++) {
                    if(heat[x, y] > 0) val[x, y] = 1;
                    else val[x, y] = 0;
                }
            }

            for(int i = 0; i < team.units.Count; i++) {
                Point p = HashHelper.Hash(team.units[i].GridPosition, cg.numCells, cg.size);
                if(val[p.X, p.Y] < 2) {
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.PX, 2));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.NX, 2));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.PY, 2));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.NY, 2));
                    cg.SetFogOfWar(p.X, p.Y, teamIndex, FogOfWar.Active);
                    val[p.X, p.Y] = 2;
                }
            }

            while(queue.Count > 0) {
                FOWPoint fp = queue.Dequeue();
                val[fp.X, fp.Y] = fp.TravelAmount;
                AddPoints(cg.numCells.X, cg.numCells.Y, val, queue, ref fp);
            }

            for(int y = 0; y < cg.numCells.Y; y++) {
                for(int x = 0; x < cg.numCells.X; x++) {
                    FogOfWar f = cg.GetFogOfWar(x, y, teamIndex);
                    switch(val[x, y]) {
                        case 1:
                            if(f != FogOfWar.Passive)
                                cg.SetFogOfWar(x, y, teamIndex, FogOfWar.Passive);
                            break;
                        default:
                            if(f != FogOfWar.Active)
                                cg.SetFogOfWar(x, y, teamIndex, FogOfWar.Active);
                            break;
                    }
                    if(val[x, y] > 0) val[x, y] = 1;
                    else val[x, y] = 0;
                }
            }
            heat = val;
        }
    }
}