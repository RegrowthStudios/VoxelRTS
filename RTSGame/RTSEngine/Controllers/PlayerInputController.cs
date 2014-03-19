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

        public PlayerInputController(GameState g, RTSTeam t)
            : base(g, t) {
            RegisterWithEvents();
        }

        public void RegisterWithEvents() {
            MouseEventDispatcher.OnMouseRelease += new OnMouseRelease(OnMouseRelease);
            MouseEventDispatcher.OnMousePress += new OnMousePress(OnMousePress);
        }

        public void OnMouseRelease(Vector2 location, MouseButton b) {
            if(b == MouseButton.Left) {
                // Get Selection Frustum
                BoundingFrustum frustum = Camera.GetSelectionBox(Vector2.Min(location, mousePressedPos), Vector2.Max(location, mousePressedPos));

                // Check For All E
                BoundingBox box;
                List<IEntity> selected = new List<IEntity>();
                for(int i = 0; i < Team.Units.Count; i++) {
                    box = Team.Units[i].BBox;
                    if(SelectionDetection.Intersects(frustum, ref box))
                        selected.Add(Team.Units[i]);
                }
                AddEvent(new SelectEvent(selected, Team));
            }
        }

        public void OnMousePress(Vector2 location, MouseButton b) {
            if(b == MouseButton.Right) {
                BoundingBox box;
                IEntity target = null;
                Ray viewRay = Camera.GetViewRay(location);
                float? dist;
                for(int i = 0; i < GameState.Teams.Length; i++) {
                    foreach(RTSUnit unit in GameState.Teams[i].Units) {
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
                        AddEvent(new SetWayPointEvent(new Vector2(rh.X, rh.Z), Team));
                    }
                }
            }
            else if(b == MouseButton.Left) {
                mousePressedPos = location;
            }
        }
    }
}