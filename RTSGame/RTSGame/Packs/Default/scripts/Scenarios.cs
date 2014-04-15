using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Team;
using RTSEngine.Algorithms;

namespace RTS.Default {
    namespace Scenarios {
        public class Tutorial : ACGameTypeController {

            public override void Load(GameState s, FileInfo infoFile) {
            }

            public override int? GetVictoriousTeam(GameState s) {
                int tAlive = -1, cAlive = 0;
                foreach(var at in s.activeTeams) {
                    if(at.Team.Units.Count > 0) {
                        tAlive = at.Index;
                        cAlive++;
                    }
                }
                if(cAlive == 1) return tAlive;
                else return null;
            }

            public override void Tick(GameState s) {
            }

            public override void Serialize(BinaryWriter s) {
            }

            public override void Deserialize(BinaryReader s, GameState state) {
            }
        }
    }
}