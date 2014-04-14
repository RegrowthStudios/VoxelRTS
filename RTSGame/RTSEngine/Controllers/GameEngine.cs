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
        public static void SearchAllInitInfo(DirectoryInfo dir, Dictionary<string, FileInfo> races, Dictionary<string, RTSColorScheme> dictSchemes) {
            var files = dir.GetFiles();
            foreach(var file in files) {
                if(file.Extension.ToLower().EndsWith("race")) {
                    races.Add(RTSRaceParser.ParseName(file), file);
                }
                else if(file.Extension.ToLower().EndsWith("scheme")) {
                    RTSColorScheme? scheme = RTSColorSchemeParser.Parse(file);
                    if(scheme.HasValue)
                        dictSchemes.Add(scheme.Value.Name, scheme.Value);
                }
            }
            var dirs = dir.GetDirectories();
            foreach(var subDir in dirs) {
                SearchAllInitInfo(subDir, races, dictSchemes);
            }
        }

        private static Dictionary<string, ReflectedScript> CompileAllScripts(DirectoryInfo root) {
            string error;
            List<string> files = new List<string>();
            List<string> libs = new List<string>(RTSConstants.ENGINE_LIBRARIES);
            FindAllInitData(root, files, libs);
            var s = ScriptParser.Compile(files.ToArray(), libs.ToArray(), out error);
            return s;
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

        public static void BuildLocal(GameState state, EngineLoadData eld, DirectoryInfo root, Dictionary<string, FileInfo> races) {
            BuildScripts(state, root);

            // Load The Map
            BuildMap(state, eld.MapFile);

            BuildTeams(state, eld, races);
            state.UpdateActiveTeams();

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
        private static void BuildMap(GameState state, FileInfo infoFile) {
            // Parse Map Data
            var lg = MapParser.ParseData(infoFile, state.Regions);
            if(!lg.HasValue)
                throw new ArgumentNullException("Could Not Load Heightmap");
            state.SetGrids(lg.Value);
        }
        private static void BuildTeams(GameState state, EngineLoadData eld, Dictionary<string, FileInfo> races) {
            RTSTeam team;
            for(int i = 0; i < eld.Teams.Length; i++) {
                TeamInitOption res = eld.Teams[i];
                if(res.InputType == RTSInputType.None)
                    continue;
                team = new RTSTeam();
                team.ColorScheme = res.Colors;
                team.Race = RTSRaceParser.Parse(races[res.Race], state.Scripts);
                state.teams[i] = team;
            }
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
                BuildMap(state, new FileInfo(mapFile));
                GameState.Deserialize(r, res, state);
            }

            // Hook Building Spawn Events To Collision Grid
            foreach(var team in (from t in state.activeTeams select t.Team)) {
                team.OnBuildingSpawn += state.CGrid.OnBuildingSpawn;
            }
        }

        public static void SetInput(GameState state, int team, ACInputController ic) {
            state.teams[team].Input = ic;
            ic.Init(state, team);
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