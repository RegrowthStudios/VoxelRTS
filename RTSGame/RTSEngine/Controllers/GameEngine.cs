using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Graphics;
using RTSEngine.Interfaces;

namespace RTSEngine.Controllers {
    // The Data The Engine Needs To Know About To Properly Create A Game
    public struct EngineLoadData {
        // Premade Teams
        public RTSTeam[] Teams;
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
        }

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
        }

        private void Cleanup(float dt) {
            /* TODO: Clean The Game State
             * Remove All Dead Entities
             * Add New Living Entities
             */
        }

        public void Draw(GraphicsDevice g, float dt) {
            renderer.Draw(state, dt);

            // TODO: Draw UI
        }
    }
}