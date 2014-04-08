﻿using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using BlisterUI.Input;
using RTSEngine.Graphics;
using RTSEngine.Interfaces;
using RTSEngine.Data.Team;
using System.IO;

namespace RTSEngine.Controllers {
    public class PlayerInputController : InputController {
        private const float MIN_RECT_SIZE = 20f;
        private const MouseButton BUTTON_SELECT = MouseButton.Left;
        private const MouseButton BUTTON_ACTION = MouseButton.Right;

        private Vector2 selectionRectStart;

        // Visual Elements That Are Used By The Controller
        public Camera Camera {
            get;
            set;
        }
        public RTSUI UI {
            get;
            set;
        }

        public PlayerInputController(GameState g, int t)
            : base(g, t, InputType.Player) {
            MouseEventDispatcher.OnMouseRelease += OnMouseRelease;
            MouseEventDispatcher.OnMousePress += OnMousePress;
        }
        public override void Dispose() {
            MouseEventDispatcher.OnMouseRelease -= OnMouseRelease;
            MouseEventDispatcher.OnMousePress -= OnMousePress;
        }

        private static bool UseSelectionRect(Vector2 min, Vector2 max) {
            return max.X - min.X >= MIN_RECT_SIZE || max.Y - min.Y >= MIN_RECT_SIZE;
        }
        private int ClosestBuilding(RTSBuilding b1, RTSBuilding b2) {
            BoundingBox bb1 = b1.BBox;
            BoundingBox bb2 = b2.BBox;
            float d1 = (0.5f * (bb1.Max + bb1.Min) - Camera.CamOrigin).LengthSquared();
            float d2 = (0.5f * (bb2.Max + bb2.Min) - Camera.CamOrigin).LengthSquared();
            return d1.CompareTo(d2);
        }
        private void SelectAllWithinFrustum(BoundingFrustum f) {
            List<RTSUnit> selectedUnits = new List<RTSUnit>();
            List<RTSBuilding> selectedBuildings = new List<RTSBuilding>();
            BoundingBox box;

            // Loop Through All The Units
            var units = Team.Units;
            for(int i = 0; i < units.Count; i++) {
                box = units[i].BBox;
                if(SelectionDetection.Intersects(f, ref box))
                    selectedUnits.Add(units[i]);
            }

            // Loop Through All The Buildings
            var buildings = Team.Buildings;
            for(int i = 0; i < buildings.Count; i++) {
                box = buildings[i].BBox;
                if(SelectionDetection.Intersects(f, ref box))
                    selectedBuildings.Add(buildings[i]);
            }

            List<IEntity> sb = new List<IEntity>();
            if(selectedUnits.Count > 0) {
                // Only Select Units
                sb.AddRange(selectedUnits);
                AddEvent(new SelectEvent(TeamIndex, sb));
            }
            else if(selectedBuildings.Count > 0) {
                // Choose The Closest Building
                selectedBuildings.Sort(ClosestBuilding);
                sb.Add(selectedBuildings[0]);
                AddEvent(new SelectEvent(TeamIndex, sb));
            }
            else {
                AddEvent(new SelectEvent(TeamIndex, null));
            }
        }
        private IEntity SelectFromRay(Ray r) {
            BoundingBox box;
            IEntity target = null;
            float? dist;
            float closest = float.MaxValue;

            // Loop Through All The Teams
            for(int i = 0; i < GameState.activeTeams.Length; i++) {
                RTSTeam team = GameState.activeTeams[i].Team;

                // TODO: Check FOW As Well

                // Loop Through All The Units
                foreach(RTSUnit unit in team.Units) {
                    box = unit.BBox;
                    dist = r.Intersects(box);
                    if(dist != null && dist.Value < closest) {
                        closest = dist.Value;
                        target = unit;
                    }
                }

                // Loop Through All The Buildings
                foreach(RTSBuilding building in team.Buildings) {
                    box = building.BBox;
                    dist = r.Intersects(box);
                    if(dist != null && dist.Value < closest) {
                        closest = dist.Value;
                        target = building;
                    }
                }
            }
            return target;
        }

        public void OnMouseRelease(Vector2 location, MouseButton b) {
            if(b == BUTTON_SELECT) {
                // Check If Camera Available
                if(Camera == null) return;

                // Order Mouse Positions
                Vector2 mMin = Vector2.Min(location, selectionRectStart);
                Vector2 mMax = Vector2.Max(location, selectionRectStart);

                // Check If Should Use A Selection Rectangle
                if(UseSelectionRect(mMin, mMax)) {
                    // Get Selection Frustum
                    BoundingFrustum frustum = Camera.GetSelectionBox(mMin, mMax);
                    SelectAllWithinFrustum(frustum);
                }
                else {
                    // Get Ray From Average Mouse Position
                    Vector2 mAv = (mMin + mMax) * 0.5f;
                    Ray ray = Camera.GetViewRay(mAv);
                    IEntity se = SelectFromRay(ray);
                    if(se != null) {
                        List<IEntity> le = new List<IEntity>();
                        le.Add(se);
                        AddEvent(new SelectEvent(TeamIndex, le));
                    }
                    else {
                        AddEvent(new SelectEvent(TeamIndex, null));
                    }
                }
            }
        }
        public void OnMousePress(Vector2 location, MouseButton b) {
            Point pl = new Point((int)location.X, (int)location.Y);
            if(UI.PanelBottom.Inside(pl.X, pl.Y)) {
                // Check UI Actions
                Vector2 r;
                if(UI.Minimap.Inside(pl.X, pl.Y, out r)) {
                    // Use The Minimap
                    Vector2 mapPos = r * GameState.CGrid.size;
                    if(b == BUTTON_SELECT) {
                        // Move To The Minimap Spot
                        Camera.MoveTo(mapPos.X, mapPos.Y);
                    }
                    else if(b == BUTTON_ACTION) {
                        // Try To Move Selected Units There
                        if(selected.Count > 0) {
                            AddEvent(new SetWayPointEvent(TeamIndex, mapPos));
                        }
                    }
                }
            }
            else {
                // Action In The World
                if(b == BUTTON_ACTION) {
                    if(Camera == null) return;

                    // Get Ray From Mouse Position
                    Ray ray = Camera.GetViewRay(location);
                    IEntity se = SelectFromRay(ray);
                    if(se != null) {
                        // Use Entity As A Target
                        AddEvent(new SetTargetEvent(TeamIndex, se));
                    }
                    else {
                        // Add A Waypoint Event
                        IntersectionRecord rec = new IntersectionRecord();
                        if(GameState.Map.BVH.Intersect(ref rec, ray)) {
                            Vector3 rh = ray.Position + ray.Direction * rec.T;
                            AddEvent(new SetWayPointEvent(TeamIndex, new Vector2(rh.X, rh.Z)));
                        }
                    }
                }
                else if(b == BUTTON_SELECT) {
                    selectionRectStart = location;
                }
            }
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement
        }
    }
}