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

        // The Map For The Level
        public Heightmap Map {
            get;
            set;
        }

        // The Grids For The Level
        public CollisionGrid CGrid {
            get;
            set;
        }
        public ImpactGrid IGrid {
            get;
            set;
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

        public GameState() {
            teams = new RTSTeam[MAX_PLAYERS];
            activeTeams = new IndexedTeam[0];

            // No Data Yet Available
            UnitControllers = new Dictionary<string, ReflectedUnitController>();
            SquadControllers = new Dictionary<string, ReflectedSquadController>();
            Map = null;

            curFrame = 0;
            timePlayed = 0f;
        }

        // Create With Premade Data
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
        }
    }
}