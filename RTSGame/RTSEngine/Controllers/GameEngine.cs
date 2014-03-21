using System;
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
        public DirectoryInfo MapDirectory;
    }

    public class GameEngine : IDisposable {
        // Data To Be Managed By The Engine
        public readonly GameState state;
        public readonly RTSRenderer renderer;
        private readonly GameplayController playController;

        public GameEngine(GraphicsDeviceManager gdm, GameWindow w, EngineLoadData d) {
            var g = gdm.GraphicsDevice;

            // Create Simple Information
            renderer = new RTSRenderer(gdm, @"Content\FX\RTS.fx", w);

            playController = new GameplayController();
            DevConsole.OnNewCommand += playController.OnDevCommand;

            // Load Teams
            state = new GameState(LoadTeams(g, d.Teams));
            LoadControllers();
            for(int ti = 0; ti < state.Teams.Length; ti++) {
                switch(d.Teams[ti].InputType) {
                    case InputType.Player:
                        var pic = new PlayerInputController(state, state.Teams[ti]);
                        pic.Camera = renderer.Camera;
                        state.Teams[ti].Input = pic;
                        break;
                    case InputType.AI:
                        // TODO: Make This Class
                        state.Teams[ti].Input = new AIInputController(state, state.Teams[ti], ti);
                        break;
                    case InputType.Environment:

                        break;
                    default:
                        throw new Exception("Type does not exist");
                }
            }

            // Load The Map
            LoadMap(g, d.MapDirectory);

            for(int ti = 0; ti < state.Teams.Length; ti++) {
                switch(d.Teams[ti].InputType) {
                    case InputType.AI:
                        (state.Teams[ti].Input as AIInputController).Start();
                        break;
                }
            }
        }
        #region Disposal
        public void Dispose() {
            for(int ti = 0; ti < state.Teams.Length; ti++) {
                state.Teams[ti].Input.Dispose();
            }
            DevConsole.OnNewCommand -= playController.OnDevCommand;
            renderer.Dispose();
        }
        #endregion

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
                state.UnitControllers.Add(kv.Key, kv.Value);
            foreach(KeyValuePair<string, ReflectedSquadController> kv in res.SquadControllers)
                state.SquadControllers.Add(kv.Key, kv.Value);
        }
        private void LoadMap(GraphicsDevice g, DirectoryInfo dir) {
            // Parse Map Data
            HeightmapResult res = HeightmapParser.Parse(g, dir);
            if(res.Data == null || res.View == null)
                throw new ArgumentNullException("Could Not Load Heightmap");

            // Set Data
            state.Map = res.Data;
            renderer.Map = res.View;
        }
        private RTSTeam[] LoadTeams(GraphicsDevice g, RTSTeamResult[] teamResults) {
            RTSTeam[] t = new RTSTeam[teamResults.Length];
            RTSTeam team;
            int i = 0;
            foreach(var res in teamResults) {
                team = new RTSTeam();
                team.ColorScheme = res.Colors;
                team.DSAC = res.TeamType.DefaultSquadActionController;
                team.DSTC = res.TeamType.DefaultSquadTargettingController;
                foreach(DirectoryInfo unitDataDir in res.TeamType.UnitTypes)
                    LoadUnit(g, team, unitDataDir);
                t[i++] = team;
            }
            return t;
        }
        public void LoadUnit(GraphicsDevice g, RTSTeam t, DirectoryInfo dir) {
            RTSUnitResult res = RTSUnitDataParser.Parse(g, dir);
            t.AddUnitData(res.Data);
            res.View.ColorPrimary = t.ColorScheme.Primary;
            res.View.ColorSecondary = t.ColorScheme.Secondary;
            res.View.ColorTertiary = t.ColorScheme.Tertiary;
            t.OnNewUnitSpawn += res.View.OnUnitSpawn;
            renderer.UnitModels.Add(res.View);
        }

        // Update Logic
        public void Update(float dt) {
            playController.Update(state, dt);
            renderer.Camera.Update(state.Map, dt);
        }

        // Drawing
        public void Draw(GraphicsDevice g, float dt) {
            renderer.Draw(state, dt);

            // TODO: Draw UI
        }
    }
}