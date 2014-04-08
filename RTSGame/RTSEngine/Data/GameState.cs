using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data.Parsers;
using RTSEngine.Data.Team;
using RTSEngine.Controllers;
using RTSEngine.Graphics;
using RTSEngine.Algorithms;

namespace RTSEngine.Data {
    public struct IndexedTeam {
        public readonly int Index;
        public readonly RTSTeam Team;

        public IndexedTeam(int i, RTSTeam t) {
            Index = i;
            Team = t;
        }
    }

    // Holds All The Data Necessary For A Game
    public class GameState {
        public const int MAX_NONENV_PLAYERS = 8;
        public const int MAX_PLAYERS = MAX_NONENV_PLAYERS + 1;
        public const int BUILDING_MEMORIZATION_LATENCY = MAX_PLAYERS * 2;

        public static void Serialize(BinaryWriter s, GameState state) {
            s.Write(state.CurrentFrame);
            s.Write(state.TotalGameTime);
            s.Write(UUIDGenerator.GetUUID());
            s.Write(state.UnitControllers.Count);
            foreach(var key in state.UnitControllers.Keys) {
                s.Write(key);
            }
            s.Write(state.BuildingControllers.Count);
            foreach(var key in state.BuildingControllers.Keys) {
                s.Write(key);
            }
            s.Write(state.SquadControllers.Count);
            foreach(var key in state.SquadControllers.Keys) {
                s.Write(key);
            }
            s.Write(state.activeTeams.Length);
            foreach(var at in state.activeTeams) {
                s.Write(at.Index);
                RTSTeam.Serialize(s, at.Team);
            }
            s.Write(state.tbMemBuildings.TotalTasks);
            foreach(var task in state.tbMemBuildings.Tasks) {
                var ebu = task as EnemyBuildingUpdater;
                EnemyBuildingUpdater.Serialize(s, ebu);
            }
            LevelGrid.Serialize(s, state);
        }
        public static void Deserialize(BinaryReader s, DynCompiledResults res, GameState state) {
            state.curFrame = s.ReadInt32();
            state.timePlayed = s.ReadSingle();
            UUIDGenerator.SetUUID(s.ReadInt32());
            int c = s.ReadInt32();
            string key;
            ReflectedUnitController ruc;
            for(int i = 0; i < c; i++) {
                key = s.ReadString();
                if(res.UnitControllers.TryGetValue(key, out ruc))
                    state.UnitControllers.Add(ruc.TypeName, ruc);
                else
                    throw new Exception("Missing Unit Controller");
            }
            c = s.ReadInt32();
            ReflectedBuildingController rbc;
            for(int i = 0; i < c; i++) {
                key = s.ReadString();
                if(res.BuildingControllers.TryGetValue(key, out rbc))
                    state.BuildingControllers.Add(rbc.TypeName, rbc);
                else
                    throw new Exception("Missing Building Controller");
            }
            c = s.ReadInt32();
            ReflectedSquadController rsc;
            for(int i = 0; i < c; i++) {
                key = s.ReadString();
                if(res.SquadControllers.TryGetValue(key, out rsc))
                    state.SquadControllers.Add(rsc.TypeName, rsc);
                else
                    throw new Exception("Missing Squad Controller");
            }
            c = s.ReadInt32();
            for(int i = 0; i < c; i++) {
                int ti = s.ReadInt32();
                state.teams[ti] = RTSTeam.Deserialize(s, ti, state);
            }
            state.UpdateActiveTeams();
            c = s.ReadInt32();
            for(int i = 0; i < c; i++) {
                var ebu = EnemyBuildingUpdater.Deserialize(s, state);
                state.tbMemBuildings.AddTask(ebu);
            }
            LevelGrid.Deserialize(s, state);
        }

        // The Grids For The Level
        private LevelGrid grid;
        public LevelGrid LevelGrid {
            get { return grid; }
        }
        public Heightmap Map {
            get { return grid.L0; }
        }
        public CollisionGrid CGrid {
            get { return grid.L1; }
        }
        public ImpactGrid IGrid {
            get { return grid.L2; }
        }

        // Controller Dictionary
        public Dictionary<string, ReflectedUnitController> UnitControllers {
            get;
            private set;
        }
        public Dictionary<string, ReflectedSquadController> SquadControllers {
            get;
            private set;
        }
        public Dictionary<string, ReflectedBuildingController> BuildingControllers {
            get;
            private set;
        }

        // Constant List Of Teams
        public readonly RTSTeam[] teams;
        public IndexedTeam[] activeTeams;

        // List of Regions In The Environment
        public List<Region> Regions {
            get;
            private set;
        }

        // Memorized Buildings Information
        public readonly TimeBudget tbMemBuildings;

        // Keeping Track Of Time
        private int curFrame;
        public int CurrentFrame {
            get { return curFrame; }
        }
        private float timePlayed;
        public float TotalGameTime {
            get { return timePlayed; }
        }

        // Particle Events
        private object lckParticles;
        private List<Particle> particles;
        private List<Particle> tmpParticles;

        public GameState() {
            UUIDGenerator.SetUUID(0);
            teams = new RTSTeam[MAX_PLAYERS];
            activeTeams = new IndexedTeam[0];
            Regions = new List<Region>();

            // No Data Yet Available
            UnitControllers = new Dictionary<string, ReflectedUnitController>();
            SquadControllers = new Dictionary<string, ReflectedSquadController>();
            BuildingControllers = new Dictionary<string, ReflectedBuildingController>();
            grid = new LevelGrid();
            grid.L0 = null;
            grid.L1 = null;
            grid.L2 = null;

            curFrame = 0;
            timePlayed = 0f;

            tbMemBuildings = new TimeBudget(BUILDING_MEMORIZATION_LATENCY);

            lckParticles = new object();
            particles = new List<Particle>();
            tmpParticles = new List<Particle>();
        }

        // Create With Premade Data
        public void SetGrids(LevelGrid lg) {
            grid.InfoFile = lg.InfoFile;
            grid.L0 = lg.L0;
            grid.L1 = lg.L1;
            grid.L2 = lg.L2;
        }
        public void SetTeams(IndexedTeam[] t) {
            int c = 0;
            foreach(IndexedTeam it in t) {
                if(teams[it.Index] == null) c++;
                teams[it.Index] = it.Team;
            }
            activeTeams = new IndexedTeam[c];
            c = 0;
            for(int i = 0; i < MAX_PLAYERS; i++) {
                if(teams[i] != null) activeTeams[c++] = new IndexedTeam(i, teams[i]);
            }
        }
        public void UpdateActiveTeams() {
            int c = 0;
            foreach(var team in teams) {
                if(team != null) c++;
            }
            activeTeams = new IndexedTeam[c];
            c = 0;
            for(int i = 0; i < MAX_PLAYERS; i++) {
                if(teams[i] != null) activeTeams[c++] = new IndexedTeam(i, teams[i]);
            }
        }

        // Glorified Way For The Gameplay Controller To Keep Track Of Time
        public void IncrementFrame(float dt) {
            curFrame++;
            timePlayed += dt;
            if(tmpParticles.Count > 0) {
                lock(lckParticles) {
                    particles.AddRange(tmpParticles);
                }
                tmpParticles = new List<Particle>();
            }
        }

        public List<Particle> GetParticles() {
            if(particles.Count > 0) {
                List<Particle> p;
                lock(lckParticles) {
                    p = particles;
                    particles = new List<Particle>();
                }
                return p;
            }
            return null;
        }
        public void AddParticle(Particle p) {
            tmpParticles.Add(p);
        }
    }
}