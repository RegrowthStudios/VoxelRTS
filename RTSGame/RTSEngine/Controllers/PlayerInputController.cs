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
                BoundingBox box = new BoundingBox();   //delete and use actual bounding boxes
                List<IEntity> selected = new List<IEntity>();
                Renderer.GetSelectionBox(Vector2.Min(location, mousePressedPos), Vector2.Max(location, mousePressedPos), out obb, out frustum);

                if(obb != null) {
                    OBB obb2 = (OBB) obb;
                    for(int i = 0; i < GameState.Teams.Length; i++){
                        foreach(RTSUnitInstance unit in GameState.Teams[i].Units) {
                            if(SelectionDetection.Intersects(ref obb2,ref box)){
                                selected.Add(unit);
                            }
                        }
                    }
                }
                else if(frustum != null) {
                    Frustum frustum2 = (Frustum) frustum;
                    for(int i = 0; i < GameState.Teams.Length; i++){
                        foreach(RTSUnitInstance unit in GameState.Teams[i].Units) {
                            if(SelectionDetection.Intersects(ref frustum2, ref box)){
                                selected.Add(unit);
                            }
                        }
                    }
                }   
                AddEvent(new SelectEvent(selected));
            }
        }

        public void OnMousePress(Vector2 location, MouseButton b) {  
            if(b == MouseButton.Right) {  
                BoundingBox box = new BoundingBox(); //delete and use actual bounding boxes
                IDestructibleEntity target = null;
                Ray clickedLocation = Renderer.GetViewRay(location);
                float? dist;
                for(int i = 0; i < GameState.Teams.Length; i++) {
                    foreach(RTSUnitInstance unit in GameState.Teams[i].Units) {
                       dist = clickedLocation.Intersects(box);
                       if(dist != null) {
                            target = unit;
                       }
                    }
                }
                if(target == null) {
                    AddEvent(new SetWayPointEvent(location));
                }
                else {
                    AddEvent(new SetTargetEvent(target));
                }
            }
            else if(b == MouseButton.Left) {
                mousePressedPos = location;
            }    
        }

    }
}
