using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Interfaces;
using RTSEngine.Data.Team;
using Microsoft.Xna.Framework;

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
            foreach(var team in s.Teams) {
                IInputController ic = team.Input;

            }
        }
        private void ApplyInput(GameState s, float dt) {

        }

        // Logic Stage
        private void ApplyLogic(GameState s, float dt) {
            // Find Decisions
            foreach(RTSTeam team in s.Teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    if(unit.ActionController == null) continue;
                    unit.ActionController.DecideAction(s, dt);
                }
            }

            // Apply Controllers
            foreach(RTSTeam team in s.Teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    if(unit.ActionController == null) continue;
                    unit.ActionController.ApplyAction(s, dt);
                }
            }
        }

        // Physics Stage
        private void ResolvePhysics(GameState s, float dt) {

            // Initialize hash grid
            HashGrid hashGrid = new HashGrid(s.Map.Width, s.Map.Depth, 2);

            // Move Geometry To The Unit's Location and hash into the grid
            foreach(RTSTeam team in s.Teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    unit.CollisionGeometry.Center = unit.GridPosition;
                    hashGrid.AddObject(unit);
                }
            }

            // Use hash grid to perform collision using 3 by 3 grid around the geometry
            foreach(Point p in hashGrid.Active) {
                hashGrid.HandleGridCollision(p.X, p.Y);
                hashGrid.HandleGridCollision(p.X, p.Y, -1, -1);
                hashGrid.HandleGridCollision(p.X, p.Y, -1, 0);
                hashGrid.HandleGridCollision(p.X, p.Y, -1, 1);
                hashGrid.HandleGridCollision(p.X, p.Y, 0, -1);
                hashGrid.HandleGridCollision(p.X, p.Y, 0, 1);
                hashGrid.HandleGridCollision(p.X, p.Y, 1, -1);
                hashGrid.HandleGridCollision(p.X, p.Y, 1, 0);
                hashGrid.HandleGridCollision(p.X, p.Y, 1, 1);
            }

            // Move Unit's Location To The Geometry After Heightmap Collision
            foreach(RTSTeam team in s.Teams) {
                foreach(RTSUnitInstance unit in team.Units) {
                    CollisionController.CollideHeightmap(unit.CollisionGeometry, s.Map);
                    unit.GridPosition = unit.CollisionGeometry.Center;
                    unit.Height = unit.CollisionGeometry.Height;
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