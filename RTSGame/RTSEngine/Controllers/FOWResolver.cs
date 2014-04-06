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
        private const int HEAT_NONE = 0;
        private const int HEAT_PASSIVE = 1;

        private int teamIndex;
        private GameState state;

        private int[,] heat;

        public FOWTask(GameState s, int tIndex)
            : base(s.teams[tIndex].units.Count + s.teams[tIndex].buildings.Count) {
            teamIndex = tIndex;
            state = s;
            heat = new int[s.CGrid.numCells.X, s.CGrid.numCells.Y];
        }

        private bool InBounds(int gx, int gy, ref FOWPoint p) {
            return p.X >= 0 && p.X < gx && p.Y >= 0 && p.Y < gy;
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
            RTSTeam team = state.teams[teamIndex];
            CollisionGrid cg = state.CGrid;

            // Generate All The Old FOW
            int[,] val = new int[cg.numCells.X, cg.numCells.Y];
            for(int y = 0; y < cg.numCells.Y; y++) {
                for(int x = 0; x < cg.numCells.X; x++) {
                    // Set To Passive If There Was Some Visibility
                    if(heat[x, y] > HEAT_NONE)
                        val[x, y] = HEAT_PASSIVE;
                    else
                        val[x, y] = 0;
                }
            }

            // Add Starting Points To The Queue
            var queue = new Queue<FOWPoint>(); // TODO: Use MinHeap Maybe
            for(int i = 0; i < team.units.Count; i++) {
                Point p = HashHelper.Hash(team.units[i].GridPosition, cg.numCells, cg.size);
                int vRadius = (int)(team.units[i].UnitData.BaseCombatData.MaxRange / cg.cellSize);
                if(val[p.X, p.Y] < vRadius) {
                    queue.Enqueue(new FOWPoint(p.X + 1, p.Y, TravelDirection.PX, vRadius));
                    queue.Enqueue(new FOWPoint(p.X - 1, p.Y, TravelDirection.NX, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y + 1, TravelDirection.PY, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y - 1, TravelDirection.NY, vRadius));
                    cg.SetFogOfWar(p.X, p.Y, teamIndex, FogOfWar.Active);
                    val[p.X, p.Y] = vRadius;
                }
            }
            for(int i = 0; i < team.buildings.Count; i++) {
                Point p = HashHelper.Hash(team.buildings[i].GridPosition, cg.numCells, cg.size);
                int vRadius = (int)(team.buildings[i].BuildingData.SightRadius / cg.cellSize);
                if(val[p.X, p.Y] < vRadius) {
                    queue.Enqueue(new FOWPoint(p.X + 1, p.Y, TravelDirection.PX, vRadius));
                    queue.Enqueue(new FOWPoint(p.X - 1, p.Y, TravelDirection.NX, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y + 1, TravelDirection.PY, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y - 1, TravelDirection.NY, vRadius));
                    cg.SetFogOfWar(p.X, p.Y, teamIndex, FogOfWar.Active);
                    val[p.X, p.Y] = vRadius;
                }
            }

            // Fan Out Heat
            while(queue.Count > 0) {
                FOWPoint fp = queue.Dequeue();
                if(InBounds(cg.numCells.X, cg.numCells.Y, ref fp) && val[fp.X, fp.Y] < fp.TravelAmount) {
                    val[fp.X, fp.Y] = fp.TravelAmount;
                    AddPoints(cg.numCells.X, cg.numCells.Y, val, queue, ref fp);
                }
            }

            // Set The Fog Of War To The Grid
            for(int y = 0; y < cg.numCells.Y; y++) {
                for(int x = 0; x < cg.numCells.X; x++) {
                    FogOfWar f = cg.GetFogOfWar(x, y, teamIndex);
                    switch(val[x, y]) {
                        case HEAT_PASSIVE:
                            if(f != FogOfWar.Passive)
                                cg.SetFogOfWar(x, y, teamIndex, FogOfWar.Passive);
                            break;
                        case 0:
                            // Black, But Don't Do Anything
                            break;
                        default:
                            // Active
                            if(f != FogOfWar.Active)
                                cg.SetFogOfWar(x, y, teamIndex, FogOfWar.Active);
                            break;
                    }
                }
            }
            heat = val;
        }
    }
}