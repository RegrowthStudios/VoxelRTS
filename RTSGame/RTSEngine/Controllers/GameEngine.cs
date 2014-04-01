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

    public class GameEngine : IDisposable {
        // Data To Be Managed By The Engine
        public GameState State {
            get;
            private set;
        }
        public GameplayController PlayController {
            get;
            private set;
        }

        public GameEngine() {
        }
        public void Dispose() {
            if(State != null) {
                PlayController.Dispose();
                for(int ti = 0; ti < State.Teams.Length; ti++) {
                    State.Teams[ti].Input.Dispose();
                }
            }
        }

        public void Load(EngineLoadData d) {
            // Create Game State
            State = new GameState();

            // Make Gameplay Controller
            PlayController = new GameplayController();

            LoadControllers();
            State.SetTeams(LoadTeams(d.Teams));

            for(int ti = 0; ti < State.Teams.Length; ti++) {
                switch(d.Teams[ti].InputType) {
                    case InputType.Player:
                        var pic = new PlayerInputController(State, State.Teams[ti]);
                        State.Teams[ti].Input = pic;
                        break;
                    case InputType.AI:
                        // TODO: Make This Class
                        State.Teams[ti].Input = new AIInputController(State, State.Teams[ti], ti);
                        break;
                    case InputType.Environment:
                        // TODO: Make This Class
                        break;
                    default:
                        throw new Exception("Type does not exist");
                }
            }

            // Load The Map
            LoadMap(d.MapFile);

            for(int ti = 0; ti < State.Teams.Length; ti++) {
                switch(d.Teams[ti].InputType) {
                    case InputType.AI:
                        (State.Teams[ti].Input as AIInputController).Start();
                        break;
                }
            }

            PlayController.Init(State);
        }

        // Data Parsing And Loading
        private void LoadControllers() {
            // Add Controllers
            string error;
            string[] references = {
               "System.dll",
               "System.Core.dll",
               "System.Data.dll",
               "System.Xml.dll",
               "System.Xml.Linq.dll",
               @"lib\Microsoft.Xna.Framework.dll",
               "RTSEngine.dll"
           };
            DirectoryInfo dir = new DirectoryInfo(@"Packs\Default\scripts");
            var files = dir.GetFiles();
            string[] toCompile = (from fi in files where fi.Extension.EndsWith("cs") select fi.FullName).ToArray();
            DynCompiledResults res = DynControllerParser.Compile(toCompile, references, out error);
            foreach(KeyValuePair<string, ReflectedUnitController> kv in res.UnitControllers)
                State.UnitControllers.Add(kv.Key, kv.Value);
            foreach(KeyValuePair<string, ReflectedSquadController> kv in res.SquadControllers)
                State.SquadControllers.Add(kv.Key, kv.Value);
        }
        private void LoadMap(FileInfo infoFile) {
            // Parse Map Data
            State.Map = HeightmapParser.ParseData(infoFile);
            if(State.Map == null)
                throw new ArgumentNullException("Could Not Load Heightmap");

            // Create Grid
            State.CGrid = new CollisionGrid(State.Map.Width, State.Map.Depth, RTSConstants.CGRID_SIZE);
        }
        private RTSTeam[] LoadTeams(RTSTeamResult[] teamResults) {
            RTSTeam[] t = new RTSTeam[teamResults.Length];
            RTSTeam team;
            int i = 0;
            foreach(var res in teamResults) {
                team = new RTSTeam();
                team.ColorScheme = res.Colors;
                team.scDefaultAction = State.SquadControllers[res.TeamType.DefaultSquadActionController];
                team.scDefaultMovement = State.SquadControllers[res.TeamType.DefaultSquadMovementController];
                team.scDefaultTargetting = State.SquadControllers[res.TeamType.DefaultSquadTargettingController];
                foreach(FileInfo unitDataFile in res.TeamType.UnitTypes) {
                    RTSUnitData data = RTSUnitDataParser.ParseData(State.UnitControllers, unitDataFile);
                    team.AddUnitData(data);
                }
                t[i++] = team;
            }
            return t;
        }

        // Update Logic
        public void Update() {
            PlayController.Update(State, RTSConstants.GAME_DELTA_TIME);
        }
    }
}