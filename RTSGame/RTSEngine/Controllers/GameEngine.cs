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
using System.IO.Compression;
using Grey.Vox;
using Grey.Graphics;

namespace RTSEngine.Controllers {
    // This Is How A Team Should Be Made
    public struct TeamInitOption {
        public string PlayerName;
        public int InputType;
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
        private static Dictionary<string, ReflectedScript> scripts;

        public static void SearchAllInitInfo(DirectoryInfo dir, Dictionary<string, FileInfo> races, Dictionary<string, RTSColorScheme> dictSchemes) {
            var files = dir.GetFiles();
            foreach(var file in files) {
                if(file.Extension.ToLower().EndsWith("race")) {
                    races.Add(RTSRaceParser.ParseName(file), file);
                }
                else if(file.Extension.ToLower().EndsWith("scheme")) {
                    object scheme = ZXParser.ParseFile(file.FullName, typeof(RTSColorScheme));
                    if(scheme != null) {
                        var v = (RTSColorScheme)scheme;
                        dictSchemes.Add(v.Name, v);
                    }
                }
            }
            var dirs = dir.GetDirectories();
            foreach(var subDir in dirs) {
                SearchAllInitInfo(subDir, races, dictSchemes);
            }
        }

        public static void CompileAllScripts(DirectoryInfo root) {
            string error;
            List<string> files = new List<string>();
            List<string> libs = new List<string>(RTSConstants.ENGINE_LIBRARIES);
            FindAllInitData(root, files, libs);
            scripts = ScriptParser.Compile(files.ToArray(), libs.ToArray(), out error);
            return;
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
            // Add Scripts
            foreach(KeyValuePair<string, ReflectedScript> kv in scripts)
                state.Scripts.Add(kv.Key, kv.Value);
        }
        private static void BuildMap(GameState state, FileInfo infoFile) {
            // Parse Map Data
            var lg = MapParser.ParseData(infoFile, state.Regions);
            if(lg == null)
                throw new ArgumentNullException("Could Not Load Heightmap");
            state.SetGrids(lg.LGrid);

            // Set Voxel Data
            byte[] data;
            using(var s = File.OpenRead(Path.Combine(infoFile.Directory.FullName, lg.VoxWorldFile))) {
                // Read How Much Data To Allocate
                var br = new BinaryReader(s);
                int l = br.ReadInt32();

                // Decompress Data
                data = new byte[l];
                var gs = new GZipStream(s, CompressionMode.Decompress);
                gs.Read(data, 0, data.Length);
            }

            // Convert Data
            int i = 0;
            int x = BitConverter.ToInt32(data, i); i += 4;
            int z = BitConverter.ToInt32(data, i); i += 4;
            var vw = state.VoxState.World;
            Vector3I loc = Vector3I.Zero;
            Grey.Vox.Region rN;
            for(loc.Z = 0; loc.Z < z; loc.Z++) {
                for(loc.X = 0; loc.X < x; loc.X++) {
                    loc.Y = 0;
                    VoxLocation vl = new VoxLocation(loc);
                    var r = vw.regions[vl.RegionIndex];
                    if(r == null) {
                        // Check If The Region Needs To Be Loaded
                        r = vw.TryCreateRegion(vl.RegionLoc.X, vl.RegionLoc.Y);
                        int rx = vl.RegionLoc.X;
                        int rz = vl.RegionLoc.Y;
                        if(r == null) continue;
                        // Look For Neighbors
                        rN = vw.pager.Obtain(rx - 1, rz);
                        if(rN != null) { r.rNX = rN; rN.rPX = r; }
                        rN = vw.pager.Obtain(rx + 1, rz);
                        if(rN != null) { r.rPX = rN; rN.rNX = r; }
                        rN = vw.pager.Obtain(rx, rz - 1);
                        if(rN != null) { r.rNZ = rN; rN.rPZ = r; }
                        rN = vw.pager.Obtain(rx, rz + 1);
                        if(rN != null) { r.rPZ = rN; rN.rNZ = r; }
                        vw.regions[vl.RegionIndex] = r;
                    }
                    int h = BitConverter.ToInt32(data, i); i += 4;
                    for(vl.VoxelLoc.Y = 0; vl.VoxelLoc.Y <= h; vl.VoxelLoc.Y++) {
                        r.SetVoxel(vl.VoxelLoc.X, vl.VoxelLoc.Y, vl.VoxelLoc.Z, 11);
                    }
                    if(h > 0) r.SetVoxel(vl.VoxelLoc.X, h, vl.VoxelLoc.Z, 1);
                    if(h > 1) r.SetVoxel(vl.VoxelLoc.X, h - 1, vl.VoxelLoc.Z, 6);
                    r.NotifyFacesChanged();
                }
            }

            for(int vi = 0; vi < 15; vi++) {
                var vd = vw.Atlas.Create();
                vd.FaceType = new VoxFaceType();
                vd.FaceType.SetAllTypes(0x01u);
                vd.FaceType.SetAllMasks(0xfeu);
            }
        }
        private static void BuildTeams(GameState state, EngineLoadData eld, Dictionary<string, FileInfo> races) {
            RTSTeam team;
            for(int i = 0; i < eld.Teams.Length; i++) {
                TeamInitOption res = eld.Teams[i];
                if(res.InputType == RTSInputType.None)
                    continue;
                team = new RTSTeam(i, res.InputType);
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
            string mapFile = "";
            using(var s = File.OpenRead(fi)) {
                BinaryReader r = new BinaryReader(s, Encoding.ASCII);
                mapFile = r.ReadString();
                BuildMap(state, new FileInfo(mapFile));
                GameState.Deserialize(r, scripts, state);
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
            state.VoxState.VWorkPool.Dispose();
        }
    }
}