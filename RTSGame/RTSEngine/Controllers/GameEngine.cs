using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data;
using RTSEngine.Data.Parsers;
using RTSEngine.Data.Team;
using RTSEngine.Graphics;
using RTSEngine.Interfaces;

namespace RTSEngine.Controllers {
    // A Playable Team
    public struct RTSTeamResult {
        public RTSRaceData TeamType;
        public RTSColorScheme Colors;
        public InputType InputType;
    }

    // The Data The Engine Needs To Know About To Properly Create A Game
    public struct EngineLoadData {
        // Teams In The Battle
        public RTSTeamResult[] Teams;

        // Where To Load The Map
        public FileInfo MapFile;
    }

    public static class GameEngine {
        public static void BuildLocal(GameState state, EngineLoadData eld) {
            BuildControllers(state);
            state.SetTeams(BuildTeams(state, eld.Teams));

            for(int ti = 0; ti < state.activeTeams.Length; ti++) {
                switch(eld.Teams[ti].InputType) {
                    case InputType.Player:
                        var pic = new PlayerInputController(state, state.activeTeams[ti].Team);
                        state.activeTeams[ti].Team.Input = pic;
                        break;
                    case InputType.AI:
                        // TODO: Make This Class
                        state.activeTeams[ti].Team.Input = new AIInputController(state, state.activeTeams[ti].Team, state.activeTeams[ti].Index);
                        break;
                    case InputType.Environment:
                        // TODO: Make This Class
                        EnvironmentInputController envController = new EnvironmentInputController(state, state.activeTeams[ti].Team);
                        envController.Init();
                        break;
                    default:
                        throw new Exception("Type does not exist");
                }
            }

            // Load The Map
            BuildMap(state, eld.MapFile);
        }
        private static void BuildControllers(GameState state) {
            // Add Controllers
            string error;
            DirectoryInfo dir = new DirectoryInfo(@"Packs\Default\scripts");
            var files = dir.GetFiles();
            string[] toCompile = (from fi in files where fi.Extension.EndsWith("cs") select fi.FullName).ToArray();
            DynCompiledResults res = DynControllerParser.Compile(toCompile, RTSConstants.REFERENCES, out error);
            foreach(KeyValuePair<string, ReflectedUnitController> kv in res.UnitControllers)
                state.UnitControllers.Add(kv.Key, kv.Value);
            foreach(KeyValuePair<string, ReflectedSquadController> kv in res.SquadControllers)
                state.SquadControllers.Add(kv.Key, kv.Value);
        }
        private static void BuildMap(GameState state, FileInfo infoFile) {
            // Parse Map Data
            state.Map = HeightmapParser.ParseData(infoFile);
            if(state.Map == null)
                throw new ArgumentNullException("Could Not Load Heightmap");

            // Create Grid
            state.CGrid = new CollisionGrid(state.Map.Width, state.Map.Depth, RTSConstants.CGRID_SIZE);
        }
        private static IndexedTeam[] BuildTeams(GameState state, RTSTeamResult[] teamResults) {
            IndexedTeam[] t = new IndexedTeam[teamResults.Length];
            RTSTeam team;
            int i = 0;
            foreach(var res in teamResults) {
                team = new RTSTeam();
                team.ColorScheme = res.Colors;
                team.race.scAction = state.SquadControllers[res.TeamType.DefaultSquadActionController];
                team.race.scMovement = state.SquadControllers[res.TeamType.DefaultSquadMovementController];
                team.race.scTargetting = state.SquadControllers[res.TeamType.DefaultSquadTargettingController];
                int ui = 0;
                foreach(FileInfo unitDataFile in res.TeamType.UnitTypes) {
                    RTSUnitData data = RTSUnitDataParser.ParseData(state.UnitControllers, unitDataFile);
                    team.race.units[ui++] = data;
                }
                team.race.UpdateActiveUnits();
                t[i] = new IndexedTeam(i, team);
                i++;
            }
            return t;
        }

        public static void Dispose(GameState state) {
            for(int ti = 0; ti < state.teams.Length; ti++) {
                if(state.teams[ti] != null)
                    state.teams[ti].Input.Dispose();
            }
        }
    }
}