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
    public static class RTSConstants {
        public static readonly string[] REFERENCES = {
            "System.dll",
            "System.Core.dll",
            "System.Data.dll",
            "System.Xml.dll",
            "System.Xml.Linq.dll",
            @"lib\Microsoft.Xna.Framework.dll",
            "RTSEngine.dll"
        };

        public const float GAME_DELTA_TIME = 1f / 60f;
        public const float CGRID_SIZE = 2f;

        public const string MC_ADDR = "228.8.8.8";
        public const int MC_LOBBY_PORT = 22880;
        public const int MC_GAME_PORT_MIN = 23000;
        public const int MC_CLIENT_PORT_MIN = 23100;
    }

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

            for(int ti = 0; ti < state.Teams.Length; ti++) {
                switch(eld.Teams[ti].InputType) {
                    case InputType.Player:
                        var pic = new PlayerInputController(state, state.Teams[ti]);
                        state.Teams[ti].Input = pic;
                        break;
                    case InputType.AI:
                        // TODO: Make This Class
                        state.Teams[ti].Input = new AIInputController(state, state.Teams[ti], ti);
                        break;
                    case InputType.Environment:
                        // TODO: Make This Class
                        EnvironmentInputController envController = new EnvironmentInputController(state, state.Teams[ti]);
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
        private static RTSTeam[] BuildTeams(GameState state, RTSTeamResult[] teamResults) {
            RTSTeam[] t = new RTSTeam[teamResults.Length];
            RTSTeam team;
            int i = 0;
            foreach(var res in teamResults) {
                team = new RTSTeam();
                team.ColorScheme = res.Colors;
                team.scDefaultAction = state.SquadControllers[res.TeamType.DefaultSquadActionController];
                team.scDefaultMovement = state.SquadControllers[res.TeamType.DefaultSquadMovementController];
                team.scDefaultTargetting = state.SquadControllers[res.TeamType.DefaultSquadTargettingController];
                foreach(FileInfo unitDataFile in res.TeamType.UnitTypes) {
                    RTSUnitData data = RTSUnitDataParser.ParseData(state.UnitControllers, unitDataFile);
                    team.AddUnitData(data);
                }
                t[i++] = team;
            }
            return t;
        }

        public static void Dispose(GameState state) {
            for(int ti = 0; ti < state.Teams.Length; ti++) {
                state.Teams[ti].Input.Dispose();
            }
        }
    }
}