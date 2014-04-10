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
        private static Dictionary<string, ReflectedScript> CompileAllScripts(DirectoryInfo root) {
            string error;
            List<string> files = new List<string>();
            List<string> libs = new List<string>(RTSConstants.ENGINE_LIBRARIES);
            FindAllInitData(root, files, libs);
            return ScriptParser.Compile(files.ToArray(), libs.ToArray(), out error);
        }
        private static void FindAllInitData(DirectoryInfo dir, List<string> files, List<string> libs) {
            var f = dir.GetFiles();
            if(f != null && f.Length > 0)
                foreach(var fi in f) {
                    if(fi.Extension.EndsWith("cs"))
                        files.Add(fi.FullName);
                    else if(fi.Extension.EndsWith("dll"))
                        libs.Add(fi.FullName);
                }
            foreach(var d in dir.GetDirectories())
                FindAllInitData(d, files, libs);
        }

        public static void BuildLocal(GameState state, EngineLoadData eld, DirectoryInfo root, Dictionary<string, RTSRaceData> races) {
            BuildScripts(state, root);

            // Load The Map
            FileInfo fiEnvSpawn;
            BuildMap(state, eld.MapFile, out fiEnvSpawn);

            state.SetTeams(BuildTeams(state, eld, races));

            for(int ti = 0; ti < state.teams.Length; ti++) {
                switch(eld.Teams[ti].InputType) {
                    case InputType.Player:
                        state.teams[ti].Input = new PlayerInputController(state, ti);
                        break;
                    case InputType.AI:
                        state.teams[ti].Input = new AIInputController(state, ti);
                        break;
                    case InputType.Environment:
                        state.teams[ti].Input = new EnvironmentInputController(state, ti, races[eld.Teams[ti].Race].InfoFile, fiEnvSpawn);
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
        private static void BuildScripts(GameState state, DirectoryInfo root) {
            var res = CompileAllScripts(root);
            if(res == null)
                throw new Exception("Bro, You Fucked Up The Scripts.\nDon't Mod It If You Don't Know It");

            // Add Scripts
            foreach(KeyValuePair<string, ReflectedScript> kv in res)
                state.Scripts.Add(kv.Key, kv.Value);
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
                team.Race.FriendlyName = rd.Name;
                team.Race.SCAction = state.Scripts[rd.DefaultSquadActionController];
                team.Race.SCMovement = state.Scripts[rd.DefaultSquadMovementController];
                team.Race.SCTargeting = state.Scripts[rd.DefaultSquadTargetingController];
                int type = 0;
                foreach(FileInfo unitDataFile in rd.UnitTypes) {
                    RTSUnitData data = RTSUnitDataParser.ParseData(state.Scripts, unitDataFile);
                    team.Race.Units[type++] = data;
                }
                team.Race.UpdateActiveUnits();
                type = 0;
                foreach(FileInfo buildingDataFile in rd.BuildingTypes) {
                    RTSBuildingData data = RTSBuildingDataParser.ParseData(state.Scripts, buildingDataFile);
                    team.Race.Buildings[type++] = data;
                }
                team.Race.UpdateActiveBuildings();
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

        public static void Save(GameState state, string fi) {
            using(var s = File.Create(fi)) {
                BinaryWriter w = new BinaryWriter(s, Encoding.ASCII);
                w.Write(state.LevelGrid.InfoFile);
                GameState.Serialize(w, state);
                w.Flush();
            }
        }
        public static void Load(GameState state, DirectoryInfo root, string fi) {
            var res = CompileAllScripts(root);
            if(res == null)
                throw new Exception("Bro, You Fucked Up The Scripts.\nDon't Mod It If You Don't Know It");

            string mapFile = "";
            using(var s = File.OpenRead(fi)) {
                BinaryReader r = new BinaryReader(s, Encoding.ASCII);
                mapFile = r.ReadString();
                FileInfo fiEnvSpawn;
                BuildMap(state, new FileInfo(mapFile), out fiEnvSpawn);
                GameState.Deserialize(r, res, state);
            }

            // Hook Building Spawn Events To Collision Grid
            foreach(var team in (from t in state.activeTeams select t.Team)) {
                team.OnBuildingSpawn += state.CGrid.OnBuildingSpawn;
            }
        }

        public static void Dispose(GameState state) {
            for(int ti = 0; ti < state.teams.Length; ti++) {
                if(state.teams[ti] != null)
                    state.teams[ti].Input.Dispose();
            }
            state.gtC.Dispose();
        }
    }
}