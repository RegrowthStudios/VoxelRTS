using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using Microsoft.Xna.Framework;
using BlisterUI.Input;
using RTSEngine.Graphics;
using RTSEngine.Interfaces;
using RTSEngine.Data.Team;

namespace RTSEngine.Controllers {
    public class PlayerInputController : InputController {

        private Vector2 mousePressedPos;
        public Camera Camera { get; set; }

        public PlayerInputController(GameState g, int t)
            : base(g, t) {
            MouseEventDispatcher.OnMouseRelease += OnMouseRelease;
            MouseEventDispatcher.OnMousePress += OnMousePress;
        }
        public override void Dispose() {
            MouseEventDispatcher.OnMouseRelease -= OnMouseRelease;
            MouseEventDispatcher.OnMousePress -= OnMousePress;
        }

        public void OnMouseRelease(Vector2 location, MouseButton b) {
            if(b == MouseButton.Left) {
                if(Camera == null) return;

                // Get Selection Frustum
                BoundingFrustum frustum = Camera.GetSelectionBox(Vector2.Min(location, mousePressedPos), Vector2.Max(location, mousePressedPos));

                // Check For All E
                BoundingBox box;
                List<IEntity> selected = new List<IEntity>();
                for(int i = 0; i < Team.units.Count; i++) {
                    box = Team.units[i].BBox;
                    if(SelectionDetection.Intersects(frustum, ref box))
                        selected.Add(Team.units[i]);
                }
                AddEvent(new SelectEvent(TeamIndex, selected));
            }
        }
        public void OnMousePress(Vector2 location, MouseButton b) {
            if(b == MouseButton.Right) {
                if(Camera == null) return;

                BoundingBox box;
                IEntity target = null;
                Ray viewRay = Camera.GetViewRay(location);
                float? dist;
                for(int i = 0; i < GameState.activeTeams.Length; i++) {
                    RTSTeam team = GameState.activeTeams[i].Team;
                    foreach(RTSUnit unit in team.units) {
                        box = unit.BBox;
                        dist = viewRay.Intersects(box);
                        if(dist != null) {
                            target = unit;
                        }
                    }
                }
                if(target == null) {
                    IntersectionRecord rec = new IntersectionRecord();
                    if(GameState.Map.BVH.Intersect(ref rec, viewRay)) {
                        Vector3 rh = viewRay.Position + viewRay.Direction * rec.T;
                        AddEvent(new SetWayPointEvent(TeamIndex, new Vector2(rh.X, rh.Z)));
                    }
                }
                else {
                    AddEvent(new SetTargetEvent(TeamIndex, target));
                }
            }
            else if(b == MouseButton.Left) {
                mousePressedPos = location;
            }
        }
    }
}