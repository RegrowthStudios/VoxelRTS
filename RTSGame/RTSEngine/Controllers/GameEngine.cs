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
        // Premade Teams
        public RTSTeam[] Teams;

        // Where To Load The Map
        public DirectoryInfo MapDirectory;
    }

    public class GameEngine {
        // Data To Be Managed By The Engine
        public readonly GameState state;
        public readonly RTSRenderer renderer;
        private readonly GameplayController playController;

        public GameEngine(GraphicsDevice g, EngineLoadData d) {
            state = new GameState(d.Teams);
            renderer = new RTSRenderer(g);
            playController = new GameplayController();

            // Load The Map
            LoadMap(g, d.MapDirectory);
        }

        // Data Parsing And Loading
        public void LoadMap(GraphicsDevice g, DirectoryInfo dir) {
            // Parse Map Data
            HeightMapResult res = HeightmapParser.Parse(g, dir);
            if(res.HeightData == null || res.Model == null)
                throw new ArgumentNullException("Could Not Load Heightmap");

            // Set Data
            state.Map = res.HeightData;
            renderer.Map = res.Model;
        }

        // Update Logic
        public void Update(float dt) {
            ResolveInput(dt);
            playController.Update(state, dt);
            ResolveInput(dt);
        }
        private void ResolveInput(float dt) {
            // TODO: Use InputControllers From The Teams
        }
        private void ResolvePhysics(float dt) {
            /* TODO: Collision
             * Hash All Units To A Grid
             * Apply Collisions To Entity Collision Geometries
             * Clamp Geometry To Heightmap
             * Update Entity To Geometry
             */

            // TODO: Hash The Units To The Grid


            // Move Geometry To The Unit's Location
            foreach(var team in state.Teams) {
                foreach(var unit in team.Units) {
                    unit.CollisionGeometry.Center = unit.GridPosition;
                }
            }

            // TODO: Use Hash Grid For Better Collision Resolution



            // Move Unit's Location To The Geometry After Heightmap Collision
            foreach(var team in state.Teams) {
                foreach(var unit in team.Units) {
                    CollisionController.CollideHeightmap(unit.CollisionGeometry, state.Map);
                    unit.CollisionGeometry.Center = unit.GridPosition;
                }
            }
        }

        // Cleanup Stage
        private void Cleanup(float dt) {
            // Remove All Dead Entities
            foreach(var team in state.Teams) {
                team.RemoveAll(IsEntityDead);
                // TODO: Remove Empty Squads
                // team.RemoveAll(IsSquadEmpty);
            }

            // Add Newly Created Instances
            playController.AddInstantiatedData(state);
        }
        private static bool IsEntityDead(IEntity e) {
            return !e.IsAlive;
        }
        private static bool IsSquadEmpty(ISquad s) {
            return s.EntityCount < 1;
        }

        // Drawing
        public void Draw(GraphicsDevice g, float dt) {
            renderer.Draw(state, dt);

            // TODO: Draw UI
        }
    }
}