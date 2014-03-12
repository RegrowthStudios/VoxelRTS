using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data.Team;

namespace RTSEngine.Data {
    // Holds All The Data Necessary For A Game
    public class GameState {
        public const int MIN_RTSUNIT_ID = 0;
        public const int MAX_RTSUNIT_ID = 255;
        public const int MIN_RTSBUILDING_ID = 0;
        public const int MAX_RTSBUILDING_ID = 255;

        // Constant List Of Teams
        public RTSTeam[] Teams {
            get;
            private set;
        }

        // List Of Unit Data
        private RTSUnit[] units;
        public IEnumerable<RTSUnit> Units {
            get { return units; }
        }

        // Create With Premade Data
        public GameState(RTSTeam[] t) {
            // Copy Over Teams
            Teams = new RTSTeam[t.Length];
            t.CopyTo(Teams, 0);

            // No Unit Types Available
            units = new RTSUnit[MAX_RTSUNIT_ID + 1];
        }

        // Need These Accessors For Unit Types
        public void AddRTSUnit(int id, RTSUnit u) {
            // Check If Previous Slot Is Filled
            if(units[id] != null)
                throw new ArgumentException("ID Is Already Taken");

            // TODO: Maybe Add Event
            units[id] = u;
        }
        public void AddRTSUnit(RTSUnit u) {
            // Try To Find An Empty Slot
            for(int i = MIN_RTSUNIT_ID; i <= MAX_RTSUNIT_ID; i++) {
                if(units[i] == null) {
                    AddRTSUnit(i, u);
                    return;
                }
            }
        }
        public RTSUnit GetRTSUnit(int id) {
            return units[id];
        }
    }
}