using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    public class MovementController : IMovementController {
        const float DECIDE_DIST = 1f;
        const float STOP_DIST = 0.5f;

        // The Waypoint To Which This Controller Has Decided To Send Its Entity
        bool doMove;
        protected Vector2 waypoint;


        // The Entity That This MovementController Is Moving
        protected IMovingEntity entity;
        public IEntity Entity {
            get { return entity; }
        }

        // List Of Waypoints To Move Each Target
        protected Vector2[] waypoints;
        public IEnumerable<Vector2> Waypoints {
            get { return waypoints; }
        }

        // Constructs a MovementController to Move the Given Entity to the Waypoints
        public MovementController() {
            entity = null;
            waypoints = null;
            doMove = false;
        }

        // Set Entity Only Once
        public void SetEntity(IEntity e) {
            if(Entity != null && Entity != e)
                throw new InvalidOperationException("Controllers Can Only Have Entities Set Once");
            entity = e as IMovingEntity;
        }

        // Provides Controller With A New Move List
        public void SetWaypoints(Vector2[] p) {
            waypoints = p;
        }

        // Performs The Critical Logic Of This Controller
        public void DecideMove(GameState g, float dt) {
            if(waypoints == null) return;
            waypoint = waypoints[waypoints.Length - 1];
            Vector2 disp = waypoint - Entity.GridPosition;
            doMove = disp.LengthSquared() > (DECIDE_DIST * DECIDE_DIST);
        }

        public void ApplyMove(GameState g, float dt) {
            if(waypoints == null) return;
            if(!doMove) {
                if(Entity.AnimationController.Animation != AnimationType.Rest) {
                    Entity.AnimationController.Animation = AnimationType.Rest;
                }
                return;
            }

            Vector2 change = waypoint - entity.GridPosition;
            if(change != Vector2.Zero) {
                float magnitude = change.Length();
                Vector2 scaledChange = (change / magnitude) * entity.MovementSpeed * dt;
                // This Logic Prevents The Unit From Hovering Around Its Goal
                if(magnitude < STOP_DIST) {
                    if(Entity.AnimationController.Animation != AnimationType.Rest) {
                        Entity.AnimationController.Animation = AnimationType.Rest;
                    }
                    return;
                }
                if(Entity.AnimationController.Animation != AnimationType.Walking) {
                    Entity.AnimationController.Animation = AnimationType.Walking;
                }
                if(scaledChange.LengthSquared() > magnitude * magnitude)
                    entity.Move(change);
                else
                    entity.Move(scaledChange);
            }
            else {
                Entity.AnimationController.Animation = AnimationType.Rest;
            }
        }
    }
}