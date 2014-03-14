﻿using System;
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
        // Premade Teams
        public RTSTeam[] Teams;

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

            state = new GameState(d.Teams);
            renderer = new RTSRenderer(gdm, @"Content\FX\RTS.fx", w);
            playController = new GameplayController();

            // Load The Map
            LoadMap(g, d.MapDirectory);
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
        public void LoadUnit(GraphicsDevice g, int team, DirectoryInfo dir) {
            RTSUnitResult res = RTSUnitParser.Parse(g, dir);
            state.Teams[team].AddUnitType(res.Data);
            state.Teams[team].OnNewUnitSpawn += res.View.OnUnitSpawn;
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