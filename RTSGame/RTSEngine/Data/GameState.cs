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

        // List Of Unit Data
        private RTSUnitData[] units;
        public IEnumerable<RTSUnitData> Units {
            get { return units; }
        }

        // Create With Premade Data
        public GameState(RTSTeam[] t) {
            // Copy Over Teams
            Teams = new RTSTeam[t.Length];
            t.CopyTo(Teams, 0);

            // No Data Yet Available
            UnitControllers = new Dictionary<string, ReflectedUnitController>();
            SquadControllers = new Dictionary<string, ReflectedSquadController>();
            units = new RTSUnitData[MAX_RTSUNIT_ID + 1];
            Map = null;
        }

        // Need These Accessors For Unit Types
        public void AddRTSUnit(int id, RTSUnitData u) {
            // Check If Previous Slot Is Filled
            if(units[id] != null)
                throw new ArgumentException("ID Is Already Taken");

            // TODO: Maybe Add Event
            units[id] = u;
        }
        public void AddRTSUnit(RTSUnitData u) {
            // Try To Find An Empty Slot
            for(int i = MIN_RTSUNIT_ID; i <= MAX_RTSUNIT_ID; i++) {
                if(units[i] == null) {
                    AddRTSUnit(i, u);
                    return;
                }
            }
        }
        public RTSUnitData GetRTSUnit(int id) {
            return units[id];
        }

    }
}