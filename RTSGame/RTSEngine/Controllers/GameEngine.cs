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

        public static void BuildLocal(GameState state, EngineLoadData eld, Dictionary<string, RTSRaceData> races) {
            BuildControllers(state);

            // Load The Map
            FileInfo fiEnvSpawn;
            BuildMap(state, eld.MapFile, out fiEnvSpawn);

            state.SetTeams(BuildTeams(state, eld, races));

            for(int ti = 0; ti < state.teams.Length; ti++) {
                switch(eld.Teams[ti].InputType) {
                    case InputType.Player:
                        var pic = new PlayerInputController(state, ti);
                        state.teams[ti].Input = pic;
                        break;
                    case InputType.AI:
                        // TODO: Make This Class
                        state.teams[ti].Input = new AIInputController(state, ti);
                        break;
                    case InputType.Environment:
                        // TODO: Make This Class
                        EnvironmentInputController envController = new EnvironmentInputController(state, ti);
                        envController.Init(races[eld.Teams[ti].Race].InfoFile, fiEnvSpawn);
                        state.teams[ti].Input = envController;
                        break;
                    default:
                        break;
                }
            }

            // Hook Building Spawn Events To Collision Grid
            foreach(var team in (from t in state.activeTeams select t.Team)) {
                team.OnBuildingSpawn += state.CGrid.OnBuildingSpawn;
            }
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
        private static IndexedTeam[] BuildTeams(GameState state, EngineLoadData eld, Dictionary<string, RTSRaceData> races) {
            var t = new List<IndexedTeam>();
            RTSTeam team;
            for(int i = 0; i < eld.Teams.Length; i++) {
                TeamInitOption res = eld.Teams[i];
                if(res.InputType == InputType.None)
                    continue;
                team = new RTSTeam();
                RTSRaceData rd = races[res.Race];
                team.ColorScheme = res.Colors;
                team.race.FriendlyName = rd.Name;
                team.race.SCAction = state.SquadControllers[rd.DefaultSquadActionController];
                team.race.SCMovement = state.SquadControllers[rd.DefaultSquadMovementController];
                team.race.SCTargeting = state.SquadControllers[rd.DefaultSquadTargetingController];
                int type = 0;
                foreach(FileInfo unitDataFile in rd.UnitTypes) {
                    RTSUnitData data = RTSUnitDataParser.ParseData(state.UnitControllers, unitDataFile);
                    team.race.Units[type++] = data;
                }
                team.race.UpdateActiveUnits();
                type = 0;
                foreach(FileInfo buildingDataFile in rd.BuildingTypes) {
                    RTSBuildingData data = RTSBuildingDataParser.ParseData(state.BuildingControllers, buildingDataFile);
                    team.race.Buildings[type++] = data;
                }
                team.race.UpdateActiveBuildings();
                t.Add(new IndexedTeam(i, team));
            }
            return t.ToArray();
        }
        private static void BuildMap(GameState state, FileInfo infoFile, out FileInfo fiEnvSpawn) {
            // Parse Map Data
            var lg = MapParser.ParseData(infoFile, out fiEnvSpawn);
            if(!lg.HasValue)
                throw new ArgumentNullException("Could Not Load Heightmap");
            state.SetGrids(lg.Value);
        }

        public static void Dispose(GameState state) {
            for(int ti = 0; ti < state.teams.Length; ti++) {
                if(state.teams[ti] != null)
                    state.teams[ti].Input.Dispose();
            }
        }
    }
}