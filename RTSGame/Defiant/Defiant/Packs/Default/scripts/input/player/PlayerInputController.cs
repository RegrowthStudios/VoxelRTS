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
using Grey.Vox;
using Grey.Graphics;

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
        bool showBuild;

        public bool HasSelectedEnemy {
            get { return selected.Count == 1 && selected[0].Team != Team; }
        }

        private RTSBuildingData buildingToPlace;
        private bool isShiftPressed;

        public Player()
            : base() {
            Type = RTSInputType.Player;
        }

        public override void Init(GameState s, int ti, object args) {
            base.Init(s, ti, args);

            if(args == null) showBuild = false;
            else {
                showBuild = (bool)args;
            }

            // Open File
            FileInfo fi = new FileInfo(s.LevelGrid.Directory.FullName + @"\camera.dat");
            if(fi.Exists && Camera != null) {
                BinaryReader r = new BinaryReader(fi.OpenRead());
                int x = r.ReadInt32();
                int y = r.ReadInt32();
                int z = r.ReadInt32();
                Camera.CamOrigin = new Vector3(x, y, z) * new Vector3(2, 1, 2) + Vector3.One;
                r.BaseStream.Dispose();
            }
        }

        public void Build(RTSRenderer renderer) {
            // Create UI
            UI = new RTSUI(renderer, @"Packs\Default\scripts\input\player\RTS.uic", showBuild);
            UI.SetTeam(Team);
            OnNewSelection += UI.SelectionPanel.OnNewSelection;
            OnNewSelection += UI.BBPanel.OnNewSelection;
            OnNewSelection += Player_OnNewSelection;
            GameState.OnAlert += UI.AlertQueue.OnAlert;

            Team.OnPopulationChange += (t, c) => { UI.TeamDataPanel.Population = Team.Population; };
            Team.OnPopulationCapChange += (t, c) => { UI.TeamDataPanel.PopulationCap = Team.PopulationCap; };
            Team.OnCapitalChange += (t, c) => { UI.TeamDataPanel.Capital = Team.Capital; };
        }
        public override void Begin() {
            useSelectRect = false;
            MouseEventDispatcher.OnMouseRelease += OnMouseRelease;
            MouseEventDispatcher.OnMouseMotion += OnMouseMotion;
            MouseEventDispatcher.OnMousePress += OnMousePress;
            KeyboardEventDispatcher.OnKeyPressed += OnKeyPress;
            KeyboardEventDispatcher.OnKeyReleased += OnKeyRelease;
            if(System.Windows.Forms.Form.ActiveForm != null) {
                System.Windows.Forms.Form.ActiveForm.KeyDown += ActiveForm_KeyPress;
                System.Windows.Forms.Form.ActiveForm.KeyUp += ActiveForm_KeyPress;
            }
            UI.Minimap.Hook();
            UI.UnitDataPanel.Hook();
        }
        public override void Dispose() {
            MouseEventDispatcher.OnMouseRelease -= OnMouseRelease;
            MouseEventDispatcher.OnMouseMotion -= OnMouseMotion;
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
        public void OnMouseMotion(Vector2 pos, Vector2 disp) {
            Point m = new Point((int)pos.X, (int)pos.Y);
            UI.UnitDataPanel.Update(m.X, m.Y);
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
                    Ray ray = Camera.GetViewRay(location);

                    // Check Building Placement
                    if(buildingToPlace != null) {
                        ray.Position *= new Vector3(0.5f, 1f, 0.5f);
                        ray.Direction *= new Vector3(0.5f, 1f, 0.5f);
                        ray.Direction.Normalize();
                        var nvl = VRayHelper.GetOuter(ray, GameState.VoxState);
                        if(nvl.HasValue) {
                            Vector3 rh = new Vector3(
                                nvl.Value.RegionLoc.X * Region.WIDTH + nvl.Value.VoxelLoc.X,
                                nvl.Value.VoxelLoc.Y,
                                nvl.Value.RegionLoc.Y * Region.DEPTH + nvl.Value.VoxelLoc.Z
                                );
                            rh *= new Vector3(2f, 1f, 2f);
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
                                nvl.Value.RegionLoc.X * Region.WIDTH + nvl.Value.VoxelLoc.X,
                                nvl.Value.VoxelLoc.Y,
                                nvl.Value.RegionLoc.Y * Region.DEPTH + nvl.Value.VoxelLoc.Z
                                );
                            rh *= new Vector3(2f, 1f, 2f);
                            rh.X += 1f; rh.Z += 1f;
                            GameState.AddParticle(new AlertParticle(
                                rh, 1f, Color.Purple, rh + Vector3.Up * 2f, 0.2f, Color.Green, GameState.TotalGameTime, 4f
                                ));
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
                    foreach(var e in arr)
                        AddEvent(new DamageEvent(TeamIndex, e.UUID, 1000000));
                    break;
                case Keys.LeftShift:
                case Keys.RightShift:
                    isShiftPressed = true;
                    break;
                case Keys.Escape:
                    buildingToPlace = null;
                    break;
                case Keys.K:
                    foreach(var entity in selected.ToArray()) {
                        RTSUnit unit = entity as RTSUnit;
                        if(unit != null) AddEvent (new SetOrdersEvent(TeamIndex, unit.UUID, BehaviorFSM.AttackMove, 3));
                    }
                    break;
                case Keys.M:
                    foreach (var entity in selected.ToArray()) {
                        RTSUnit unit = entity as RTSUnit;
                        if (unit != null) AddEvent(new SetOrdersEvent(TeamIndex, unit.UUID, BehaviorFSM.JustMove, 3));
                    }
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

        void Player_OnNewSelection(ACInputController arg1, List<IEntity> arg2) {
            if(arg2 == null || arg2.Count != 1)
                UI.SelectionToggle = 0;
            else {
                var u = arg2[0] as RTSUnit;
                var b = arg2[0] as RTSBuilding;
                if(u != null) {
                    UI.SelectionToggle = 1;
                    UI.UnitDataPanel.SetData(u);
                }
                else if(b != null) {
                    UI.SelectionToggle = 2;
                    UI.BuildingDataPanel.SetData(b);
                }
            }
        }

        public void OnUIPress(Point p, MouseButton b) {
            Vector2 r = Vector2.Zero;
            if(UI.Minimap.WidgetBase.Inside(p.X, p.Y)) {
                if(!UI.Minimap.MapRect.Inside(p.X, p.Y, out r))
                    return;
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
                    // Shift Clicking
                    int c = isShiftPressed ? 5 : 1;
                    for(int ci = 0; ci < c; ci++) {
                        for(int i = 0; i < bbs.Count; i++) {
                            bbs[i].OnClick();
                        }
                    }
                }
            }
        }

        public void Update(RTSRenderer renderer, GameState s) {
            UI.AlertQueue.Update();
            UI.BuildingDataPanel.Update();
        }
        public void Draw(RTSRenderer renderer, SpriteBatch batch) {
            UI.Draw(renderer, batch);
        }

        #region Level Editor
        List<LEVoxel> voxels;
        ushort camID;
        public override List<LEVoxel> CreateVoxels(VoxAtlas atlas) {
            float duv = 1f / 8f;
            voxels = new List<LEVoxel>(1);

            // Create Camera Voxel
            LEVoxel lev = new LEVoxel("Camera Position", atlas);
            lev.VData.FaceType.SetAllTypes(0x00000001u);
            lev.VData.FaceType.SetAllMasks(0xfffffffeu);
            VGPCube vgp = new VGPCube();
            vgp.Color = Color.White;
            vgp.UVRect = new Vector4(duv * 0, duv * 1, duv, duv);
            lev.VData.GeoProvider = vgp;
            voxels.Add(lev);

            camID = voxels[0].VData.ID;
            return voxels;
        }
        public override void LESave(VoxWorld world, int w, int h, DirectoryInfo dir) {
            // Create File
            FileInfo fi = new FileInfo(dir.FullName + @"\camera.dat");
            BinaryWriter s = new BinaryWriter(fi.Create());

            // Search Through Columns
            Vector3I loc = Vector3I.Zero;
            for(loc.Z = 0; loc.Z < h; loc.Z++) {
                for(loc.X = 0; loc.X < w; loc.X++) {
                    loc.Y = 0;
                    VoxLocation vl = new VoxLocation(loc);
                    Region r = world.regions[vl.RegionIndex];

                    // Search Through The Region
                    for(; vl.VoxelLoc.Y < Region.HEIGHT; vl.VoxelLoc.Y++) {
                        ushort id = r.voxels[vl.VoxelIndex].ID;
                        if(id == camID) {
                            // Write Camera Position
                            s.Write(loc.X);
                            s.Write(vl.VoxelLoc.Y);
                            s.Write(loc.Z);
                            s.Flush();
                            s.BaseStream.Dispose();
                            return;
                        }
                    }
                }
            }

            // Flush And Close (No Data)
            s.Write(w / 2);
            s.Write((Grey.Vox.Region.HEIGHT * 3) / 4);
            s.Write(h / 2);
            s.Flush();
            s.BaseStream.Dispose();
        }
        #endregion

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }
}