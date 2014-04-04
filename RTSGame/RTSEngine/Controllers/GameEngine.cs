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
    // This Is How A Team Should Be Made
    public struct TeamInitOption {
        public string PlayerName;
        public InputType InputType;
        public string Race;
        public RTSColorScheme Colors;
    }

    // The Data The Engine Needs To Know About To Properly Create A Game
    public struct EngineLoadData {
        // Teams In The Battle
        public TeamInitOption[] Teams;

        public Dictionary<string, RTSRaceData> Races;

        // Where To Load The Map
        public FileInfo MapFile;
    }

    public static class GameEngine {
        public static void SearchAllInitInfo(DirectoryInfo dir, Dictionary<string, RTSRaceData> dictRaces, Dictionary<string, RTSColorScheme> dictSchemes) {
            var files = dir.GetFiles();
            foreach(var file in files) {
                if(file.Extension.ToLower().EndsWith("race")) {
                    RTSRaceData rd = RTSRaceParser.Parse(file);
                    if(rd != null)
                        dictRaces.Add(rd.Name, rd);
                }
                else if(file.Extension.ToLower().EndsWith("scheme")) {
                    RTSColorScheme? scheme = RTSColorSchemeParser.Parse(file);
                    if(scheme.HasValue)
                        dictSchemes.Add(scheme.Value.Name, scheme.Value);
                }
            }
            var dirs = dir.GetDirectories();
            foreach(var subDir in dirs) {
                SearchAllInitInfo(subDir, dictRaces, dictSchemes);
            }
        }

        public static void BuildLocal(GameState state, EngineLoadData eld) {
            BuildControllers(state);
            state.SetTeams(BuildTeams(state, eld));

            for(int ti = 0; ti < state.activeTeams.Length; ti++) {
                switch(eld.Teams[ti].InputType) {
                    case InputType.Player:
                        var pic = new PlayerInputController(state, state.activeTeams[ti].Index);
                        state.activeTeams[ti].Team.Input = pic;
                        break;
                    case InputType.AI:
                        // TODO: Make This Class
                        state.activeTeams[ti].Team.Input = new AIInputController(state, state.activeTeams[ti].Index);
                        break;
                    case InputType.Environment:
                        // TODO: Make This Class
                        EnvironmentInputController envController = new EnvironmentInputController(state, state.activeTeams[ti].Index);
                        //envController.Init();
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
            foreach(KeyValuePair<string, ReflectedBuildingController> kv in res.BuildingControllers)
                state.BuildingControllers.Add(kv.Key, kv.Value);
        }
        private static void BuildMap(GameState state, FileInfo infoFile) {
            // Parse Map Data
            state.Map = HeightmapParser.ParseData(infoFile);
            if(state.Map == null)
                throw new ArgumentNullException("Could Not Load Heightmap");

            // Create Grid
            state.CGrid = new CollisionGrid(state.Map.Width, state.Map.Depth, RTSConstants.CGRID_SIZE);
        }
        private static IndexedTeam[] BuildTeams(GameState state, EngineLoadData eld) {
            var t = new List<IndexedTeam>();
            RTSTeam team;
            for(int i = 0; i < eld.Teams.Length; i++) {
                TeamInitOption res = eld.Teams[i];
                if(res.InputType == InputType.None)
                    continue;
                team = new RTSTeam();
                RTSRaceData rd = eld.Races[res.Race];
                team.ColorScheme = res.Colors;
                team.race.scAction = state.SquadControllers[rd.DefaultSquadActionController];
                team.race.scMovement = state.SquadControllers[rd.DefaultSquadMovementController];
                team.race.scTargetting = state.SquadControllers[rd.DefaultSquadTargettingController];
                int type = 0;
                foreach(FileInfo unitDataFile in rd.UnitTypes) {
                    RTSUnitData data = RTSUnitDataParser.ParseData(state.UnitControllers, unitDataFile);
                    team.race.units[type++] = data;
                }
                team.race.UpdateActiveUnits();
                type = 0;
                foreach(FileInfo buildingDataFile in rd.BuildingTypes) {
                    RTSBuildingData data = RTSBuildingDataParser.ParseData(state.BuildingControllers, buildingDataFile);
                    team.race.buildings[type++] = data;
                }
                team.race.UpdateActiveBuildings();
                t.Add(new IndexedTeam(i, team));
            }
            return t.ToArray();
        }

        public static void Dispose(GameState state) {
            for(int ti = 0; ti < state.teams.Length; ti++) {
                if(state.teams[ti] != null)
                    state.teams[ti].Input.Dispose();
            }
        }
    }
}