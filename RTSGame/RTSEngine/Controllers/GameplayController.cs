using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Interfaces;
using RTSEngine.Data.Team;

namespace RTSEngine.Controllers {
    public class GameplayController {
        public float TimePlayed {
            get;
            private set;
        }

        public GameplayController() {
            TimePlayed = 0f;
        }

        // The Update Function
        public void Update(GameState s, float dt) {
            TimePlayed += dt;

            // Input Pass
            ResolveInput(s, dt);
            ApplyInput(s, dt);

            // Logic Pass
            ApplyLogic(s, dt);

            // Physics Pass
            ResolvePhysics(s, dt);

            // Cleanup The State
            Cleanup(s, dt);
        }

        // Input Stage
        private void ResolveInput(GameState s, float dt) {
            // TODO: Use InputControllers From The Teams
        }
        private void ApplyInput(GameState s, float dt) {

        }
        
        // Logic Stage
        private void ApplyLogic(GameState s, float dt) {

        }
        
        // Physics Stage
        private void ResolvePhysics(GameState s, float dt) {
            /* TODO: Collision
             * Hash All Units To A Grid
             * Apply Collisions To Entity Collision Geometries
             * Clamp Geometry To Heightmap
             * Update Entity To Geometry
             */

            // Initialize hash grid
            float gridSize = 1;
            HashGrid hashGrid = new HashGrid(s.Map.Width, s.Map.Depth, gridSize);

            // Move Geometry To The Unit's Location and hash into the grid
            foreach(var team in s.Teams) {
                foreach(var unit in team.Units) {
                    unit.CollisionGeometry.Center = unit.GridPosition;
                    hashGrid.AddObject(unit.CollisionGeometry);
                }
            }

            // Use hash grid to perform collision using 3 by 3 grid around the geometry
            for (int x = 0; x < hashGrid.gridCount.X; x++) {
                for (int y = 0; y < hashGrid.gridCount.Y; y++) {
                    // Handle corner cases
                    if (x == 0 && y != 0) {
                        
                    }
                    else if (x != 0 && y == 0) {

                    }
                    else if (x == 0 && y == 0) {

                    }
                    // Regular collision handling
                    else {

                    }
                }
            }

            // Move Unit's Location To The Geometry After Heightmap Collision
            foreach (var team in s.Teams) {
                foreach (var unit in team.Units) {
                    CollisionController.CollideHeightmap(unit.CollisionGeometry, s.Map);
                    unit.CollisionGeometry.Center = unit.GridPosition;
                }
            }
        }

        // Cleanup Stage
        private void Cleanup(GameState s, float dt) {
            // Remove All Dead Entities
            foreach(var team in s.Teams) {
                team.RemoveAll(IsEntityDead);
                // TODO: Remove Empty Squads
                // team.RemoveAll(IsSquadEmpty);
            }

            // Add Newly Created Instances
            AddInstantiatedData(s);
        }
        private static bool IsEntityDead(IEntity e) {
            return !e.IsAlive;
        }
        private static bool IsSquadEmpty(ISquad s) {
            return s.EntityCount < 1;
        }
        public void AddInstantiatedData(GameState s) {

        }
    }
}
