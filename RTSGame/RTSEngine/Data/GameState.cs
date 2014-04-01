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
    // Holds All The Data Necessary For A Game
    public class GameState {
        public const int MAX_PLAYERS = 8;
        public const int MIN_RTSUNIT_ID = 0;
        public const int MAX_RTSUNIT_ID = 255;
        public const int MIN_RTSBUILDING_ID = 0;
        public const int MAX_RTSBUILDING_ID = 255;

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
        public RTSTeam[] Teams {
            get;
            private set;
        }

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
        public float TotalGameTime {
            get { return curFrame * RTSConstants.GAME_DELTA_TIME; }
        }

        public GameState() {
            // No Data Yet Available
            UnitControllers = new Dictionary<string, ReflectedUnitController>();
            SquadControllers = new Dictionary<string, ReflectedSquadController>();
            //units = new RTSUnitData[MAX_RTSUNIT_ID + 1];
            Map = null;
        }

        // Create With Premade Data
        public void SetTeams(RTSTeam[] t) {
            // Copy Over Teams
            Teams = new RTSTeam[t.Length];
            t.CopyTo(Teams, 0);
        }

        // Glorified Way For The Gameplay Controller To Keep Track Of Time
        public void IncrementFrame() {
            curFrame++;
        }
    }
}