using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data.Team;

namespace RTSEngine.Data {
    public class GameState {
        public readonly RTSTeam[] teams;

        public GameState(int numTeams) {
            teams = new RTSTeam[numTeams];
        }
    }
}
