using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BEU.Data;
using Microsoft.Xna.Framework;

namespace BEU.Controllers {
    public class GameplayController {
        // The Update Function
        public void Update(GameState s, float dt) {
            // Time Keeping
            s.IncrementFrame(dt);

            // Logic Pass
            ApplyLogic(s, dt);

            // Physics Pass
            ResolvePhysics(s, dt);

            // Cleanup The State
            Cleanup(s, dt);
        }

        private void ApplyLogic(GameState s, float dt) {

        }

        // Physics Stage
        private void ResolvePhysics(GameState s, float dt) {
            Team team;

            // Initialize hash grid
            var cGrid = s.CGrid;
            cGrid.ClearDynamic();

            // Move Geometry To The Unit's Location and hash into the grid
            for(int ti = 0; ti < s.Teams.Length; ti++) {
                team = s.Teams[ti];
                foreach(Tank unit in team.Units) {
                    unit.CollisionGeometry.Center = unit.Position;
                    cGrid.Add(unit);
                }
            }

            // Use hash grid to perform collision using 3 by 3 grid around the geometry
            for(int i = 0; i < cGrid.ActiveGrids.Count; i++) {
                Point p = cGrid.ActiveGrids[i];
                cGrid.HandleGridCollision(p.X, p.Y);
                cGrid.HandleGridCollision(p.X, p.Y, -1, -1);
                cGrid.HandleGridCollision(p.X, p.Y, -1, 0);
                cGrid.HandleGridCollision(p.X, p.Y, -1, 1);
                cGrid.HandleGridCollision(p.X, p.Y, 0, -1);
                cGrid.HandleGridCollision(p.X, p.Y, 0, 1);
                cGrid.HandleGridCollision(p.X, p.Y, 1, -1);
                cGrid.HandleGridCollision(p.X, p.Y, 1, 0);
                cGrid.HandleGridCollision(p.X, p.Y, 1, 1);
            }

            // Move Unit's Location To The Geometry After Heightmap Collision
            for(int ti = 0; ti < s.Teams.Length; ti++) {
                team = s.Teams[ti];
                foreach(Tank unit in team.Units) {
                    unit.Position = unit.CollisionGeometry.Center;
                }
            }
        }

        // Cleanup Stage
        private void Cleanup(GameState s, float dt) {
            // Remove All Dead Entities
            for(int ti = 0; ti < s.Teams.Length; ti++) {
                s.Teams[ti].RemoveAll(IsDead);
            }
        }

        private bool IsDead(Tank obj) {
            return obj.IsDestroyed;
        }
    }
}