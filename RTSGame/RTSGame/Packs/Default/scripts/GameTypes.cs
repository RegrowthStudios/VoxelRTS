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
using System.Text.RegularExpressions;
using RTSEngine.Data.Parsers;

namespace RTS.Default {
    namespace GameTypes {
        public class SPSurvival : ACGameTypeController {
            public const int STARTING_CAPITAL = 1000;
            private static readonly Regex rgxTeamSpawn = RegexHelper.GenerateVec2("SURVIVALSPAWN");

            RTSTeam pTeam, eTeam;

            public override void Load(GameState s, FileInfo mapFile) {
                // Give The Player Team Starting Capital
                pTeam = null;
                eTeam = null;
                for(int i = 0; i < s.activeTeams.Length; i++) {
                    var at = s.activeTeams[i];
                    if(pTeam == null && at.Team.Type == RTSInputType.Player) {
                        pTeam = at.Team;
                        pTeam.Input.AddEvent(new CapitalEvent(pTeam.Index, STARTING_CAPITAL));
                    }
                    else if(eTeam == null && at.Team.Type == RTSInputType.Environment) {
                        eTeam = at.Team;
                    }
                }

                Match[] m = RegexHelper.FindMatches(RegexHelper.ReadFile(mapFile),
                    rgxTeamSpawn
                    );
                if(m != null && m[0].Success) {
                    // Give Starting Location
                    Vector2 ts = RegexHelper.ExtractVec2(m[0]);
                    Point hqPos = HashHelper.Hash(ts, s.CGrid.numCells, s.CGrid.size);
                    pTeam.Input.AddEvent(new SpawnBuildingEvent(pTeam.Index, 0, hqPos, true));
                }
            }

            public override int? GetVictoriousTeam(GameState s) {
                // Mercy Time
                if(s.CurrentFrame < 100) return null;
             
                // If All The Buildings And Units Are Destroyed
                if(pTeam.Buildings.Count < 1 && pTeam.Units.Count < 1)
                    return -1;
                return null;
            }

            public override void Tick(GameState s) {
            }

            public override void Serialize(BinaryWriter s) {
            }
            public override void Deserialize(BinaryReader s, GameState state) {
            }
        }

        public class SPEscapeThePlanet : ACGameTypeController {
            public const int STARTING_CAPITAL = 1000;
            private static readonly Regex rgxTeamSpawn = RegexHelper.GenerateVec2("SURVIVALSPAWN");

            RTSTeam pTeam, eTeam;

            public override void Load(GameState s, FileInfo mapFile) {
                // Give The Player Team Starting Capital
                pTeam = null;
                eTeam = null;
                for(int i = 0; i < s.activeTeams.Length; i++) {
                    var at = s.activeTeams[i];
                    if(pTeam == null && at.Team.Type == RTSInputType.Player) {
                        pTeam = at.Team;
                        pTeam.Input.AddEvent(new CapitalEvent(pTeam.Index, STARTING_CAPITAL));
                    }
                    else if(eTeam == null && at.Team.Type == RTSInputType.Environment) {
                        eTeam = at.Team;
                    }
                }

                Match[] m = RegexHelper.FindMatches(RegexHelper.ReadFile(mapFile),
                    rgxTeamSpawn
                    );
                if(m != null && m[0].Success) {
                    // Give Starting Location
                    Vector2 ts = RegexHelper.ExtractVec2(m[0]);
                    Point hqPos = HashHelper.Hash(ts, s.CGrid.numCells, s.CGrid.size);
                    pTeam.Input.AddEvent(new SpawnBuildingEvent(pTeam.Index, 0, hqPos, true));
                }
            }

            public override int? GetVictoriousTeam(GameState s) {
                // Mercy Time
                if(s.CurrentFrame < 100) return null;

                // If All The Buildings And Units Are Destroyed
                if(pTeam.Buildings.Count < 1 && pTeam.Units.Count < 1)
                    return -1;

                if(pTeam.Capital > 1500)
                    return pTeam.Index;
                return null;
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