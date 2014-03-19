using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;
using RTSEngine.Data;

namespace RTSCS.Controllers {
    public class MovementController : ACUnitMovementController {
        const float DECIDE_DIST = 1f;
        const float STOP_DIST = 0.5f;

        // The Waypoint To Which This Controller Has Decided To Send Its Entity
        bool doMove;
        protected Vector2 waypoint;

        // Performs The Critical Logic Of This Controller
        public override void DecideMove(GameState g, float dt) {
            if(waypoints.Count < 1) return;
            waypoint = waypoints[waypoints.Count - 1];
            Vector2 disp = waypoint - unit.GridPosition;
            doMove = disp.LengthSquared() > (DECIDE_DIST * DECIDE_DIST);
        }

        public override void ApplyMove(GameState g, float dt) {
            if(!doMove) {
                if(unit.AnimationController.Animation != AnimationType.Rest)
                    unit.AnimationController.SetAnimation(AnimationType.Rest);
                return;
            }

            Vector2 change = waypoint - unit.GridPosition;
            if(change != Vector2.Zero) {
                float magnitude = change.Length();
                Vector2 scaledChange = (change / magnitude) * unit.MovementSpeed * dt;

                // This Logic Prevents The Unit From Hovering Around Its Goal
                if(magnitude < STOP_DIST) {
                    if(unit.AnimationController.Animation != AnimationType.Rest)
                        unit.AnimationController.SetAnimation(AnimationType.Rest);
                    return;
                }
                if(unit.AnimationController.Animation != AnimationType.Walking)
                    unit.AnimationController.SetAnimation(AnimationType.Walking);

                if(scaledChange.LengthSquared() > magnitude * magnitude)
                    unit.Move(change);
                else
                    unit.Move(scaledChange);
            }
            else
                unit.AnimationController.SetAnimation(AnimationType.Rest);
        }
    }
}