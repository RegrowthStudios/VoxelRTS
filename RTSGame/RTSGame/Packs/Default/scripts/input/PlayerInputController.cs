using System;
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
using Microsoft.Xna.Framework.Graphics;
using Grey.Vox.Ops;
using VRegion = Grey.Vox.Region;

namespace RTS.Input {
    public class Player : ACInputController, IVisualInputController {
        private const float MIN_RECT_SIZE = 20f;
        private const MouseButton BUTTON_SELECT = MouseButton.Left;
        private const MouseButton BUTTON_ACTION = MouseButton.Right;

        private Vector2 selectionRectStart;
        private bool useSelectRect;

        // Visual Elements That Are Used By The Controller
        public Camera Camera {
            get;
            set;
        }
        public RTSUI UI {
            get;
            set;
        }
        //private BVH bvh;

        public bool HasSelectedEnemy {
            get { return selected.Count == 1 && selected[0].Team != Team; }
        }

        private RTSBuildingData buildingToPlace;
        private bool isShiftPressed;

        public Player()
            : base() {
            Type = RTSInputType.Player;
            //bvh = new BVH();
        }
        public void Build(RTSRenderer renderer) {
            // Create UI
            UI = new RTSUI(renderer, @"Packs\Default\scripts\input\RTS.uic");
            UI.SetTeam(Team);
            OnNewSelection += UI.SelectionPanel.OnNewSelection;
            OnNewSelection += UI.BBPanel.OnNewSelection;

            Team.OnPopulationChange += (t, c) => { UI.TeamDataPanel.Population = Team.Population; };
            Team.OnPopulationCapChange += (t, c) => { UI.TeamDataPanel.PopulationCap = Team.PopulationCap; };
            Team.OnCapitalChange += (t, c) => { UI.TeamDataPanel.Capital = Team.Capital; };

            // Build The BVH
            // bvh = renderer.Map.BVH;
        }
        public override void Begin() {
            useSelectRect = false;
            MouseEventDispatcher.OnMouseRelease += OnMouseRelease;
            MouseEventDispatcher.OnMousePress += OnMousePress;
            KeyboardEventDispatcher.OnKeyPressed += OnKeyPress;
            KeyboardEventDispatcher.OnKeyReleased += OnKeyRelease;
            if(System.Windows.Forms.Form.ActiveForm != null) {
                System.Windows.Forms.Form.ActiveForm.KeyDown += ActiveForm_KeyPress;
                System.Windows.Forms.Form.ActiveForm.KeyUp += ActiveForm_KeyPress;
            }
        }
        public override void Dispose() {
            MouseEventDispatcher.OnMouseRelease -= OnMouseRelease;
            MouseEventDispatcher.OnMousePress -= OnMousePress;
            KeyboardEventDispatcher.OnKeyPressed -= OnKeyPress;
            KeyboardEventDispatcher.OnKeyReleased -= OnKeyRelease;
            if(System.Windows.Forms.Form.ActiveForm != null) {
                System.Windows.Forms.Form.ActiveForm.KeyDown -= ActiveForm_KeyPress;
                System.Windows.Forms.Form.ActiveForm.KeyUp -= ActiveForm_KeyPress;
            }
            if(UI != null) {
                UI.Dispose();
            }
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
                AddEvent(new SelectEvent(TeamIndex, sb, isShiftPressed));
            }
            else if(selectedBuildings.Count > 0) {
                // Choose The Closest Building
                selectedBuildings.Sort(ClosestBuilding);
                sb.Add(selectedBuildings[0]);
                AddEvent(new SelectEvent(TeamIndex, sb, isShiftPressed));
            }
            else if(!isShiftPressed) {
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
                int ti = GameState.activeTeams[i].Index;
                RTSTeam team = GameState.activeTeams[i].Team;

                // Loop Through All The Units
                for(int ui = 0; ui < team.Units.Count; ui++) {
                    RTSUnit unit = team.Units[ui];
                    FogOfWar f = GameState.CGrid.GetFogOfWar(unit.GridPosition, TeamIndex);
                    if(f != FogOfWar.Active) continue;

                    box = unit.BBox;
                    dist = r.Intersects(box);
                    if(dist != null && dist.Value < closest) {
                        closest = dist.Value;
                        target = unit;
                    }
                }

                // Loop Through All The Buildings
                for(int bi = 0; bi < team.Buildings.Count; bi++) {
                    RTSBuilding building = team.Buildings[bi];
                    FogOfWar f = FogOfWar.Nothing;
                    Point p = HashHelper.Hash(building.GridPosition, GameState.CGrid.numCells, GameState.CGrid.size);
                    for(int y = 0; y < building.Data.GridSize.Y; y++) {
                        for(int x = 0; x < building.Data.GridSize.X; x++) {
                            f = GameState.CGrid.GetFogOfWar(p.X + x, p.Y + y, TeamIndex);
                            if(f == FogOfWar.Active) break;
                        }
                        if(f == FogOfWar.Active) break;
                    }
                    if(f != FogOfWar.Active) continue;

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
            // Check If Camera Available
            if(Camera == null) return;
            Camera.Controller.IsActive = true;

            if(b == BUTTON_SELECT && useSelectRect) {
                useSelectRect = false;

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
                        AddEvent(new SelectEvent(TeamIndex, le, isShiftPressed));
                    }
                    else if(!isShiftPressed) {
                        AddEvent(new SelectEvent(TeamIndex, null));
                    }
                }
            }
        }
        public void OnMousePress(Vector2 location, MouseButton b) {
            if(Camera == null) return;
            Camera.Controller.IsActive = false;

            Point pl = new Point((int)location.X, (int)location.Y);
            if(UI != null && UI.Inside(pl.X, pl.Y)) {
                // Check UI Actions
                OnUIPress(pl, b);
            }
            else {
                // Action In The World
                if(b == BUTTON_ACTION) {
                    if(Camera == null) return;
                    IntersectionRecord rec = new IntersectionRecord();
                    Ray ray = Camera.GetViewRay(location);

                    // Check Building Placement
                    if(buildingToPlace != null) {
                        ray.Position *= new Vector3(0.5f, 1f, 0.5f);
                        ray.Direction *= new Vector3(0.5f, 1f, 0.5f);
                        ray.Direction.Normalize();
                        var nvl = VRayHelper.GetOuter(ray, GameState.VoxState);
                        if(nvl.HasValue) {
                            Vector3 rh = new Vector3(
                                nvl.Value.RegionLoc.X * VRegion.WIDTH + nvl.Value.VoxelLoc.X,
                                nvl.Value.VoxelLoc.Y,
                                nvl.Value.RegionLoc.Y * VRegion.DEPTH + nvl.Value.VoxelLoc.Z
                                );
                            rh *= new Vector3(2f, 1f, 2f);
                            // Vector3 rh = ray.Position + ray.Direction * rec.T;
                            Point bp = HashHelper.Hash(new Vector2(rh.X, rh.Z), GameState.CGrid.numCells, GameState.CGrid.size);
                            AddEvent(new SpawnBuildingEvent(TeamIndex, buildingToPlace.Index, bp));
                        }
                        if(!isShiftPressed) buildingToPlace = null;
                        return;
                    }

                    // Get Ray From Mouse Position
                    IEntity se = SelectFromRay(ray);
                    if(se != null) {
                        // Use Entity As A Target
                        AddEvent(new SetTargetEvent(TeamIndex, se));
                    }
                    else if(!HasSelectedEnemy) {
                        // Add A Waypoint Event
                        ray.Position *= new Vector3(0.5f, 1f, 0.5f);
                        ray.Direction *= new Vector3(0.5f, 1f, 0.5f);
                        ray.Direction.Normalize();
                        var nvl = VRayHelper.GetOuter(ray, GameState.VoxState);
                        if(nvl.HasValue) {
                            Vector3 rh = new Vector3(
                                nvl.Value.RegionLoc.X * VRegion.WIDTH + nvl.Value.VoxelLoc.X,
                                nvl.Value.VoxelLoc.Y,
                                nvl.Value.RegionLoc.Y * VRegion.DEPTH + nvl.Value.VoxelLoc.Z
                                );
                            rh *= new Vector3(2f, 1f, 2f);
                            rh.X += 1f; rh.Z += 1f;
                            //Vector3 rh = ray.Position + ray.Direction * rec.T;
                            AddEvent(new SetWayPointEvent(TeamIndex, new Vector2(rh.X, rh.Z)));
                        }
                    }
                }
                else if(b == BUTTON_SELECT) {
                    buildingToPlace = null;
                    useSelectRect = true;
                    selectionRectStart = location;
                }
            }
        }
        public void OnKeyPress(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.Delete:
                    var arr = selected.ToArray();
                    foreach(var e in arr) e.Destroy();
                    break;
                case Keys.LeftShift:
                case Keys.RightShift:
                    isShiftPressed = true;
                    break;
                case Keys.Escape:
                    buildingToPlace = null;
                    break;
            }
        }
        public void OnKeyRelease(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.LeftShift:
                case Keys.RightShift:
                    isShiftPressed = false;
                    break;
            }
        }
        void ActiveForm_KeyPress(object sender, System.Windows.Forms.KeyEventArgs e) {
            isShiftPressed = (System.Windows.Forms.Control.ModifierKeys & System.Windows.Forms.Keys.Shift) == System.Windows.Forms.Keys.Shift;
        }

        public void OnUIPress(Point p, MouseButton b) {
            Vector2 r = Vector2.Zero;
            if(UI.Minimap.Inside(p.X, p.Y, out r)) {
                // Use The Minimap
                Vector2 mapPos = r * GameState.CGrid.size;
                if(b == BUTTON_SELECT) {
                    // Move To The Minimap Spot
                    if(Camera == null) return;
                    Camera.MoveTo(mapPos.X, mapPos.Y);
                }
                else if(b == BUTTON_ACTION) {
                    // Try To Move Selected Units There
                    if(selected.Count > 0)
                        AddEvent(new SetWayPointEvent(TeamIndex, mapPos));
                }
            }
            else if(UI.SelectionPanel.BackPanel.Inside(p.X, p.Y)) {
                var ug = UI.SelectionPanel.GetSelection(p.X, p.Y);
                if(ug != null) {
                    AddEvent(new SelectEvent(TeamIndex, ug));
                }
            }
            else if(UI.BuildingPanel.Inside(p.X, p.Y)) {
                buildingToPlace = UI.BuildingPanel.GetSelection(p.X, p.Y);
            }
            else if(UI.BBPanel.BackPanel.Inside(p.X, p.Y)) {
                var bbs = UI.BBPanel.GetSelection(p.X, p.Y);
                if(bbs != null) {
                    for(int i = 0; i < bbs.Count; i++) {
                        bbs[i].OnQueueFinished(GameState);
                    }
                }
            }
        }

        public void Update(RTSRenderer renderer, GameState s) {
            UI.BuildingPanel.Update();
        }
        public void Draw(RTSRenderer renderer, SpriteBatch batch) {
            UI.Draw(renderer, batch);
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }
}