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
    // The Data The Engine Needs To Know About To Properly Create A Game
    public struct EngineLoadData {

        // Types Of Teams
        public enum InputType {
            Player,
            AI,
            Environment
        }

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

        Action<string, float> fLoad;

        public GameEngine(GraphicsDeviceManager gdm, GameWindow w, EngineLoadData d, Action<string, float> loadCallback, EngineLoadData.InputType[] t) {
            var g = gdm.GraphicsDevice;
            fLoad = loadCallback;

            // Create Simple Information
            fLoad("Creating Renderer", 0.0f);
            renderer = new RTSRenderer(gdm, @"Content\FX\RTS.fx", w);
            fLoad("Creating Renderer", 0.07f);

            fLoad("Making Gameplay Controller", 0.1f);
            playController = new GameplayController();

            // Load Teams
            fLoad("Loading Teams", 0.2f);
            state = new GameState(LoadTeams(g, d.Teams, t));
            fLoad("Teams Complete", 0.4f);

            // Load The Map
            fLoad("Loading Map", 0.5f);
            LoadMap(g, d.MapDirectory);
            fLoad("Map Complete", 1f);

        }
        #region Disposal
        public void Dispose() {
            renderer.Dispose();
        }
        #endregion

        // Data Parsing And Loading
        private void LoadMap(GraphicsDevice g, DirectoryInfo dir) {
            // Parse Map Data
            HeightmapResult res = HeightmapParser.Parse(g, dir);
            if(res.Data == null || res.View == null)
                throw new ArgumentNullException("Could Not Load Heightmap");

            // Set Data
            state.Map = res.Data;
            renderer.Map = res.View;
        }
        private RTSTeam[] LoadTeams(GraphicsDevice g, RTSTeamResult[] teamResults, EngineLoadData.InputType[] types) {
            RTSTeam[] t = new RTSTeam[teamResults.Length];
            RTSTeam team;
            int i = 0;
            foreach(var res in teamResults) {
                team = new RTSTeam();
                team.ColorSheme = res.Colors;
                foreach(DirectoryInfo unitDataDir in res.UnitTypes)
                    LoadUnit(g, team, unitDataDir);
                t[i++] = team;

                switch(types[i]) {
                    case EngineLoadData.InputType.Player:
                        team.Input = new PlayerInputController(state,team);
                        break;
                    case EngineLoadData.InputType.AI:
                        //TODO: Make This Class
                        //team.Input = new AIInputController(state, team);
                        break;
                    case EngineLoadData.InputType.Environment:

                        break;
                    default:
                        throw new Exception("Type does not exist");
                        break;
                }
            }
            return t;
        }
        public void LoadUnit(GraphicsDevice g, RTSTeam t, DirectoryInfo dir) {
            RTSUnitResult res = RTSUnitParser.Parse(g, dir);
            t.AddUnitType(res.Data);
            t.OnNewUnitSpawn += res.View.OnUnitSpawn;
            renderer.UnitModels.Add(res.View);
        }

        // Update Logic
        public void Update(float dt) {
            playController.Update(state, dt);
            renderer.UpdateCamera(state.Map, dt);
        }

        // Drawing
        public void Draw(GraphicsDevice g, float dt) {
            renderer.Draw(state, dt);

            // TODO: Draw UI
        }
    }
}