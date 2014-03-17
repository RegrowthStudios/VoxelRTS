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
        public RTSRenderer Renderer { get; set; }
        
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
                OBB? obb;
                Frustum? frustum;
                BoundingBox box;
                List<IEntity> selected = new List<IEntity>();
                Renderer.GetSelectionBox(Vector2.Min(location, mousePressedPos), Vector2.Max(location, mousePressedPos), out obb, out frustum);

                if(frustum.HasValue) {
                    Frustum f = frustum.Value;
                    for(int i = 0; i < GameState.Teams.Length; i++){
                        foreach(RTSUnitInstance unit in GameState.Teams[i].Units) {
                            box = unit.BBox;
                            if(SelectionDetection.Intersects(ref f,ref box)){
                                selected.Add(unit);
                            }
                        }
                    }
                }
                else if(obb.HasValue) {
                    OBB o = obb.Value;
                    for(int i = 0; i < GameState.Teams.Length; i++){
                        foreach(RTSUnitInstance unit in GameState.Teams[i].Units) {
                            box = unit.BBox;
                            if(SelectionDetection.Intersects(ref o, ref box)){
                                selected.Add(unit);
                            }
                        }
                    }
                }   
                AddEvent(new SelectEvent(selected, Team));
            }
        }

        public void OnMousePress(Vector2 location, MouseButton b) {  
            if(b == MouseButton.Right) {
                BoundingBox box;
                IDestructibleEntity target = null;
                Ray clickedLocation = Renderer.GetViewRay(location);
                float? dist;
                for(int i = 0; i < GameState.Teams.Length; i++) {
                    foreach(RTSUnitInstance unit in GameState.Teams[i].Units) {
                       box = unit.BBox;
                       dist = clickedLocation.Intersects(box);
                       if(dist != null) {
                            target = unit;
                       }
                    }
                }
                if(target == null) {
                    AddEvent(new SetWayPointEvent(location, Team));
                }
                else {
                    AddEvent(new SetTargetEvent(target, Team));
                }
            }
            else if(b == MouseButton.Left) {
                mousePressedPos = location;
            }    
        }

    }
}
