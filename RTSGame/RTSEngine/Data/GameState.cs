﻿using System;
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

        // The Grids For The Level
        private LevelGrid grid;
        public Heightmap Map {
            get { return grid.L0; }
        }
        public CollisionGrid CGrid {
            get { return grid.L1; }
        }
        public ImpactGrid IGrid {
            get { return grid.L2; }
        }
        public FlowGrid FGrid {
            get { return grid.L3; }
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
            teams = new RTSTeam[MAX_PLAYERS];
            activeTeams = new IndexedTeam[0];

            // No Data Yet Available
            UnitControllers = new Dictionary<string, ReflectedUnitController>();
            SquadControllers = new Dictionary<string, ReflectedSquadController>();
            BuildingControllers = new Dictionary<string, ReflectedBuildingController>();
            grid = new LevelGrid();
            grid.L0 = null;
            grid.L1 = null;
            grid.L2 = null;
            grid.L3 = null;

            curFrame = 0;
            timePlayed = 0f;

            lckParticles = new object();
            particles = new List<Particle>();
            tmpParticles = new List<Particle>();
        }

        // Create With Premade Data
        public void SetGrids(LevelGrid lg) {
            grid.L0 = lg.L0;
            grid.L1 = lg.L1;
            grid.L2 = lg.L2;
            grid.L3 = lg.L3;
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