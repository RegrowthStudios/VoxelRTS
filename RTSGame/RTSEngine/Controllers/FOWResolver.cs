using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Algorithms;
using RTSEngine.Interfaces;
using System.IO;

namespace RTSEngine.Controllers {
    public enum TravelDirection {
        PX,
        NX,
        PY,
        NY,
        PXPY,
        PXNY,
        NXPY,
        NXNY
    }
    public struct FOWPoint {
        public int X, Y;
        public TravelDirection PreviousDirection;
        public int TravelAmount;

        public FOWPoint(int px, int py, TravelDirection t, int ptAmount) {
            X = px;
            Y = py;
            PreviousDirection = t;
            bool isDiag = false;
            switch(PreviousDirection) {
                case TravelDirection.PX:
                    X++;
                    break;
                case TravelDirection.NX:
                    X--;
                    break;
                case TravelDirection.PY:
                    Y++;
                    break;
                case TravelDirection.NY:
                    Y--;
                    break;
                case TravelDirection.PXPY:
                    X++; Y++;
                    isDiag = true;
                    break;
                case TravelDirection.PXNY:
                    X++; Y--;
                    isDiag = true;
                    break;
                case TravelDirection.NXPY:
                    X--; Y++;
                    isDiag = true;
                    break;
                case TravelDirection.NXNY:
                    X--; Y--;
                    isDiag = true;
                    break;
            }
            TravelAmount = ptAmount - (isDiag ? 14 : 10);
        }
    }

    public class FOWTask : ACBudgetedTask {
        private const int HEAT_NONE = 0;
        private const int HEAT_PASSIVE = 1;

        private int teamIndex;
        private GameState state;

        private int[,] heat;

        public FOWTask(GameState s, int tIndex)
            : base(1) {
            teamIndex = tIndex;
            state = s;
            heat = new int[s.CGrid.numCells.X, s.CGrid.numCells.Y];
        }

        public void SetAllFOW(FogOfWar f, CollisionGrid cg) {
            int i = f == FogOfWar.Nothing ? HEAT_NONE : (f == FogOfWar.Passive ? HEAT_PASSIVE : HEAT_PASSIVE + 1);
            for(int y = 0; y < cg.numCells.Y; y++) {
                for(int x = 0; x < cg.numCells.X; x++) {
                    heat[x, y] = i;
                    cg.SetFogOfWar(x, y, teamIndex, f);
                }
            }
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
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.PXPY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.PXNY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.PX, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    break;
                case TravelDirection.NX:
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.NXPY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.NXNY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.NX, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    break;
                case TravelDirection.PY:
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.PXPY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.NXPY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.PY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    break;
                case TravelDirection.NY:
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.PXNY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.NXNY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.NY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    break;
                case TravelDirection.PXPY:
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.PXPY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.PX, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.PY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    break;
                case TravelDirection.PXNY:
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.PXNY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.PX, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.NY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    break;
                case TravelDirection.NXNY:
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.NXNY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.NY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.NX, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    break;
                case TravelDirection.NXPY:
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.NXPY, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.NX, prev.TravelAmount);
                    if(IsGood(gx, gy, val, ref fp)) q.Enqueue(fp);
                    fp = new FOWPoint(prev.X, prev.Y, TravelDirection.PY, prev.TravelAmount);
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
                        val[x, y] = HEAT_PASSIVE * 10;
                    else
                        val[x, y] = 0;
                }
            }

            // Add Starting Points To The Queue
            var queue = new Queue<FOWPoint>();
            for(int i = 0; i < team.Units.Count; i++) {
                Point p = HashHelper.Hash(team.Units[i].GridPosition, cg.numCells, cg.size);
                int vRadius = (int)(team.Units[i].UnitData.BaseCombatData.MaxRange / cg.cellSize);
                vRadius *= 10;
                if(val[p.X, p.Y] < vRadius) {
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.PXPY, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.NXNY, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.NXPY, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.PXNY, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.PX, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.NX, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.PY, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.NY, vRadius));
                    cg.SetFogOfWar(p.X, p.Y, teamIndex, FogOfWar.Active);
                    val[p.X, p.Y] = vRadius;
                }
            }
            for(int i = 0; i < team.Buildings.Count; i++) {
                Point p = HashHelper.Hash(team.Buildings[i].GridPosition, cg.numCells, cg.size);
                int vRadius = (int)(team.Buildings[i].BuildingData.SightRadius / cg.cellSize);
                vRadius *= 10;
                if(val[p.X, p.Y] < vRadius) {
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.PXPY, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.NXNY, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.NXPY, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.PXNY, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.PX, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.NX, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.PY, vRadius));
                    queue.Enqueue(new FOWPoint(p.X, p.Y, TravelDirection.NY, vRadius));
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
                    val[x, y] /= 10;
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

    public class EnemyBuildingUpdater : ACBudgetedTask {
        public static void Serialize(BinaryWriter s, EnemyBuildingUpdater t) {
            s.Write(t.teamIndex);
            s.Write(t.vb.Team);
            s.Write(t.vb.Type);
            s.Write(t.vb.CellPoint);
            s.Write(t.vb.WorldPosition);
            s.Write(t.vb.ViewDirection);
            s.Write(t.EnemyUUID);

            s.Write(t.Added);
            s.Write(t.isDead);
        }
        public static EnemyBuildingUpdater Deserialize(BinaryReader s, GameState state) {
            int ti = s.ReadInt32();
            ViewedBuilding vb = new ViewedBuilding();
            vb.Team = s.ReadInt32();
            vb.Type = s.ReadInt32();
            vb.CellPoint = s.ReadPoint();
            vb.WorldPosition = s.ReadVector3();
            vb.ViewDirection = s.ReadVector2();
            int uuid = s.ReadInt32();
            RTSBuilding b = null;
            foreach(var building in state.teams[vb.Team].Buildings) {
                if(building.UUID == uuid) {
                    b = building;
                    break;
                }
            }
            EnemyBuildingUpdater ebu = new EnemyBuildingUpdater(state, ti, vb, b);
            ebu.Added = s.ReadBoolean();
            ebu.isDead = s.ReadBoolean();
            if(ebu.Added) {
                state.teams[ebu.teamIndex].ViewedEnemyBuildings.Add(vb);
            }
            return ebu;
        }

        private int teamIndex;
        private GameState state;
        private ViewedBuilding vb;
        public int EnemyUUID {
            get;
            private set;
        }
        private Point[] grids;
        public bool Added {
            get;
            private set;
        }
        private bool isDead;
        private RTSBuildingData Data {
            get { return state.teams[vb.Team].Race.Buildings[vb.Type]; }
        }

        public EnemyBuildingUpdater(GameState s, int tIndex, ViewedBuilding _vb, RTSBuilding b)
            : base(1) {
            state = s;
            teamIndex = tIndex;
            Added = false;
            isDead = false;
            vb = _vb;

            if(b != null) b.OnDestruction += OnBuildingDeath;
            else isDead = true;
            
            RTSBuildingData data = state.teams[vb.Team].Race.Buildings[vb.Type];
            grids = new Point[data.GridSize.X * data.GridSize.Y];
            Point p = vb.CellPoint;
            int pi = 0;
            for(int y = 0; y < data.GridSize.Y; y++) {
                for(int x = 0; x < data.GridSize.X; x++) {
                    grids[pi++] = new Point(p.X + x, p.Y + y);
                }
            }
        }

        private void OnBuildingDeath(IEntity o) {
            isDead = true;
            o.OnDestruction -= OnBuildingDeath;
        }

        public override void DoWork(float dt) {
            if(IsFinished) return;

            if(!Added) {
                if(isDead) {
                    // Early Work Escape
                    Finish();
                    return;
                }
                // Check If We Can Memorize
                foreach(var p in grids) {
                    FogOfWar f = state.CGrid.GetFogOfWar(p.X, p.Y, teamIndex);
                    switch(f) {
                        case FogOfWar.Active:
                            state.teams[teamIndex].ViewedEnemyBuildings.Add(vb);
                            Added = true;
                            break;
                    }
                }
            }
            else {
                int fActive = 0, fNothing = 0;
                foreach(var p in grids) {
                    FogOfWar f = state.CGrid.GetFogOfWar(p.X, p.Y, teamIndex);
                    switch(f) {
                        case FogOfWar.Active:
                            fActive++;
                            break;
                        case FogOfWar.Nothing:
                            fNothing++;
                            break;
                    }
                }
                if(fActive > 0 && isDead) {
                    // Know Building Is Dead Now
                    state.teams[teamIndex].ViewedEnemyBuildings.Remove(vb);
                    Added = false;
                    Finish();
                    return;
                }
                else if(fNothing == Data.GridSize.X * Data.GridSize.Y) {
                    // Not To Be Seen Anymore
                    state.teams[teamIndex].ViewedEnemyBuildings.Remove(vb);
                    Added = false;
                    if(isDead) {
                        // Early Exit
                        Finish();
                        return;
                    }
                }
            }
        }
    }
}