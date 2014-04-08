﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;

namespace RTSEngine.Data {
    public enum FogOfWar : uint {
        Nothing = 0x00,
        Passive = 0x01,
        Active = 0x02,
        All = 0x03
    }

    public static class FogOfWarHelper {
        public static FogOfWar GetFogOfWar(uint fog, int p) {
            return (FogOfWar)((fog >> (p << 2)) & 0x03);
        }
        public static void SetFogOfWar(ref uint fog, int p, FogOfWar f) {
            p <<= 2;
            fog &= ~(0x03u << p);
            fog |= ((uint)f << p);
        }
    }

    public class CollisionGrid {
        // Full Size Of The Map
        public readonly Vector2 size;

        // Number And Size Of Cells
        public readonly Point numCells;
        public readonly float cellSize;

        public List<RTSUnit>[,] EDynamic {
            get;
            private set;
        }
        public RTSBuilding[,] EStatic {
            get;
            private set;
        }
        public uint[,] Fog {
            get;
            private set;
        }
        public bool[,] Collision {
            get;
            private set;
        }

        // Locations In The Grid Which Contain Units
        public List<Point> ActiveGrids {
            get;
            private set;
        }

        public event Action<int, int, int, FogOfWar> OnFOWChange;

        public CollisionGrid(int w, int h, float cs) {
            numCells = new Point(w, h);
            cellSize = cs;
            size = new Vector2(w, h) * cellSize;

            EDynamic = new List<RTSUnit>[numCells.X, numCells.Y];
            EStatic = new RTSBuilding[numCells.X, numCells.Y];
            for(int x = 0; x < numCells.X; x++) {
                for(int y = 0; y < numCells.Y; y++) {
                    EDynamic[x, y] = new List<RTSUnit>();
                }
            }
            ActiveGrids = new List<Point>();
            Fog = new uint[numCells.X, numCells.Y];
            Collision = new bool[numCells.X, numCells.Y];
        }

        public void Add(RTSUnit o) {
            Point p = HashHelper.Hash(o.CollisionGeometry.Center, numCells, size);

            // Check If Active
            if(EDynamic[p.X, p.Y].Count < 1)
                ActiveGrids.Add(p);
            EDynamic[p.X, p.Y].Add(o);
        }
        public bool CanAddBuilding(Vector2 pos, Point gs) {
            // Check All The Cells
            Point p = HashHelper.Hash(pos, numCells, size);
            for(int y = 0; y < gs.Y; y++) {
                for(int x = 0; x < gs.X; x++) {
                    if(EStatic[p.X + x, p.Y + y] != null)
                        return false;
                }
            }
            return true;
        }
        public void Add(RTSBuilding b) {
            b.OnDestruction += OnBuildingDestruction;

            // Add To All The Cells
            Point p = HashHelper.Hash(b.CollisionGeometry.Center, numCells, size);
            for(int y = 0; y < b.BuildingData.GridSize.Y; y++) {
                for(int x = 0; x < b.BuildingData.GridSize.X; x++) {
                    EStatic[p.X + x, p.Y + y] = b;
                }
            }
        }

        public void ClearDynamic() {
            for(int i = 0; i < ActiveGrids.Count; i++)
                EDynamic[ActiveGrids[i].X, ActiveGrids[i].Y] = new List<RTSUnit>();
            ActiveGrids = new List<Point>();
        }

        // Precondition This[x,y] Must Be Active
        public void HandleGridCollision(int x, int y, int dx, int dy) {
            // Check Bounds
            int ox = x + dx;
            if(ox < 0 || ox >= numCells.X) return;
            int oy = y + dy;
            if(oy < 0 || oy >= numCells.Y) return;

            var al1 = EDynamic[x, y];
            var al2 = EDynamic[ox, oy];
            var sl2 = EStatic[ox, oy];

            // Empty Check
            if(al2.Count + (sl2 == null ? 0 : 1) < 1) return;

            for(int i1 = 0; i1 < al1.Count; i1++) {
                // Dynamic-Dynamic
                for(int i2 = 0; i2 < al2.Count; i2++)
                    // Get Rid Of Doubles
                    if(al1[i1].UUID > al2[i2].UUID)
                        CollisionController.ProcessCollision(al1[i1].CollisionGeometry, al2[i2].CollisionGeometry);
                // Dynamic-Static
                if(sl2 != null)
                    CollisionController.ProcessCollision(al1[i1].CollisionGeometry, sl2.CollisionGeometry);
            }
        }
        public void HandleGridCollision(int x, int y) {
            var al = EDynamic[x, y];
            var sl = EStatic[x, y];

            // Dynamic-Dynamic
            for(int i1 = 0; i1 < al.Count - 1; i1++)
                for(int i2 = i1 + 1; i2 < al.Count; i2++)
                    CollisionController.ProcessCollision(al[i1].CollisionGeometry, al[i2].CollisionGeometry);
            // Dynamic-Static
            if(sl != null)
                for(int i1 = 0; i1 < al.Count; i1++)
                    CollisionController.ProcessCollision(al[i1].CollisionGeometry, sl.CollisionGeometry);
        }

        public void SetFogOfWar(int x, int y, int p, FogOfWar f) {
            FogOfWar of = GetFogOfWar(x, y, p);
            if(f == of) return;
            FogOfWarHelper.SetFogOfWar(ref Fog[x, y], p, f);
            if(OnFOWChange != null)
                OnFOWChange(x, y, p, f);
        }
        public FogOfWar GetFogOfWar(int x, int y, int p) {
            return FogOfWarHelper.GetFogOfWar(Fog[x, y], p);
        }

        public void SetCollision(int x, int y, bool c) {
            Collision[x, y] = c;
        }
        public bool GetCollision(int x, int y) {
            return Collision[x, y] || EStatic[x, y] != null;
        }

        public void OnBuildingSpawn(RTSBuilding b) {
            Add(b);
        }
        public void OnBuildingDestruction(IEntity o) {
            o.OnDestruction -= OnBuildingDestruction;
            RTSBuilding b = o as RTSBuilding;

            // Add To All The Cells
            Point p = HashHelper.Hash(b.CollisionGeometry.Center, numCells, size);
            for(int y = 0; y < b.BuildingData.GridSize.Y; y++) {
                for(int x = 0; x < b.BuildingData.GridSize.X; x++) {
                    EStatic[p.X + x, p.Y + y] = null;
                }
            }
        }
    }

    public class ImpactGrid {

        // Size Of Each Cell In The Impact Grid
        public readonly float cellSize;

        // Number Of Cells In The Impact Grid
        public readonly Point numCells;

        // Size Of The Impact Grid
        public readonly Vector2 size;

        // Stores The Region Each Cell Is Located In
        public Region[,] Region { get; set; }

        // Stores The ImpactGenerators Of Each Cell Of The Impact Grid
        public List<ImpactGenerator>[,] ImpactGenerators { get; set; }

        // Stores The Impact of Each Cell Of The Impact Grid
        public int[,] CellImpact { get; private set; }

        // Creates An Impact Grid Using The Size And Cell Size Of The Given Collision Grid
        public ImpactGrid(CollisionGrid cg) {
            cellSize = 2 * cg.cellSize;
            numCells = new Point((int)Math.Ceiling(cg.size.X / cellSize), (int)Math.Ceiling(cg.size.Y / cellSize));
            cellSize = cg.size.X / numCells.X;
            size = cg.size;
            Region = new Region[numCells.X, numCells.Y];
            ImpactGenerators = new List<ImpactGenerator>[numCells.X, numCells.Y];
            CellImpact = new int[numCells.X, numCells.Y];

            for(int x = 0; x < numCells.X; x++) {
                for(int y = 0; y < numCells.Y; y++) {
                    Region[x, y] = null;
                    ImpactGenerators[x, y] = new List<ImpactGenerator>();
                    CellImpact[x, y] = 0;
                }
            }
        }

        // Adds An Impact Generator To The Appropriate Cell In The Impact Grid
        public void AddImpactGenerator(ImpactGenerator g) {
            Point p = HashHelper.Hash(g.GridPosition, numCells, size);
            ImpactGenerators[p.X, p.Y].Add(g);
            g.GenerateImpact += AddToCellImpact;
        }

        // Listens To GenerateImpact Events And Adds Impact To The Appropriate Cell And Region 
        public void AddToCellImpact(Vector2 pos, int amount) {
            Point p = HashHelper.Hash(pos, numCells, size);
            CellImpact[p.X, p.Y] += amount;
            Region[p.X, p.Y].AddToRegionImpact(amount);
        }
    }

    public class FlowGrid {

        // How Much More Granularity This Grid Has Compared To The Collsion Grid
        public const int granularityMultiplier = 2;

        // The Constants Used In Flow Grid Calculations
        // +: Repulsive
        // -: Attractive
        // Static Entity Force
        public static float sForce = 20f;
        // Dynamic Entity Force
        public static float dForce = 10f;
        // Path Force
        public static float pForce = -200f;

        // Size Of Each Cell In The Flow Grid
        public readonly float cellSize;

        // Number Of Cells In The Flow Grid
        public readonly Point numCells;

        // Size Of The Flow Grid
        public readonly Vector2 size;

        // The Net Flows At Each Grid Location
        private Vector2[,] flowVectors;
        public Vector2[,] FlowVectors {
            get { return flowVectors; }
        }

        // Calculate The Unit Force Between Two Locations
        public static Vector2 UnitForce(Vector2 a, Vector2 b) {
            Vector2 diff = a - b;
            float denom = diff.LengthSquared();
            return diff.X != 0 && diff.Y != 0 ? 1 / denom * Vector2.Normalize(diff) : Vector2.Zero;
        }

        // Creates A Flow Grid Using The Size And Cell Size Of The Given Collision Grid
        public FlowGrid(CollisionGrid cg, bool fillInStatics) {
            cellSize = 1.0f / ((float)granularityMultiplier) * cg.cellSize;
            numCells = new Point((int)Math.Ceiling(cg.size.X / cellSize), (int)Math.Ceiling(cg.size.Y / cellSize));
            cellSize = cg.size.X / numCells.X;
            size = cg.size;
            flowVectors = new Vector2[numCells.X, numCells.Y];

            if(fillInStatics) {
                for(int cgX = 0; cgX < cg.numCells.X; cgX++) {
                    for(int cgY = 0; cgY < cg.numCells.Y; cgY++) {
                        // Establish Flows Due To Static Entities
                        if(cg.GetCollision(cgX, cgY)) {
                            PlaceStaticEntity(cgX, cgY, cg.cellSize);
                        }
                    }
                }
            }
        }

        public Vector2 GetFlow(Point gridPoint) {
            return flowVectors[gridPoint.X, gridPoint.Y];
        }

        public void OnBuildingSpawn(RTSBuilding b) {
            b.OnDestruction += OnBuildingDestruction;
            PlaceStaticEntity(b.GridPosition);
        }

        public void OnBuildingDestruction(IEntity o) {
            o.OnDestruction -= OnBuildingDestruction;
            RTSBuilding b = o as RTSBuilding;
            RemoveStaticEntity(b.GridPosition);
        }

        public void PlaceStaticEntity(int cgX, int cgY, float cgSize) {
            for(int fgX = 0; fgX < numCells.X; fgX++) {
                for(int fgY = 0; fgY < numCells.Y; fgY++) {
                    FlowVectors[fgX, fgY] += sForce * UnitForce(new Vector2(Center(cgX, cgSize), Center(cgY, cgSize)), MakeContinuous(fgX, fgY));
                }
            }
        }

        public void PlaceStaticEntity(Vector2 location) {
            for(int fgX = 0; fgX < numCells.X; fgX++) {
                for(int fgY = 0; fgY < numCells.Y; fgY++) {
                    FlowVectors[fgX, fgY] += sForce * UnitForce(location, MakeContinuous(fgX, fgY));
                }
            }
        }

        public void RemoveStaticEntity(Vector2 location) {
            for(int fgX = 0; fgX < numCells.X; fgX++) {
                for(int fgY = 0; fgY < numCells.Y; fgY++) {
                    FlowVectors[fgX, fgY] -= sForce * UnitForce(location, MakeContinuous(fgX, fgY));
                }
            }
        }

        public Vector2 MakeContinuous(int x, int y) {
            return new Vector2(Center(x, cellSize), Center(y, cellSize));
        }

        private float Center(int i, float cellSize) {
            return ((float)i + 0.5f) * cellSize;
        }
    }

    public struct LevelGrid {
        public string InfoFile;
        public Heightmap L0;
        public CollisionGrid L1;
        public ImpactGrid L2;
        public FlowGrid L3;
    }

    public static class HashHelper {
        public static Point Hash(Vector2 pos, ref int gx, ref float sx, ref int gy, ref float sy) {
            pos.X *= gx / sx;
            pos.Y *= gy / sy;
            Point p = new Point((int)pos.X, (int)pos.Y);
            if(p.X < 0) p.X = 0;
            else if(p.X >= gx) p.X = gx - 1;
            if(p.Y < 0) p.Y = 0;
            else if(p.Y >= gy) p.Y = gy - 1;
            return p;
        }
        public static Point Hash(Vector2 pos, ref Point g, ref Vector2 s) {
            return Hash(pos, ref g.X, ref s.X, ref g.Y, ref s.Y);
        }
        public static Point Hash(Vector2 pos, int gx, float sx, int gy, float sy) {
            pos.X *= gx / sx;
            pos.Y *= gy / sy;
            Point p = new Point((int)pos.X, (int)pos.Y);
            if(p.X < 0) p.X = 0;
            else if(p.X >= gx) p.X = gx - 1;
            if(p.Y < 0) p.Y = 0;
            else if(p.Y >= gy) p.Y = gy - 1;
            return p;
        }
        public static Point Hash(Vector2 pos, Point g, Vector2 s) {
            return Hash(pos, ref g.X, ref s.X, ref g.Y, ref s.Y);
        }
    }
}