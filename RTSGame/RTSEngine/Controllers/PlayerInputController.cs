using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using Microsoft.Xna.Framework;
using BlisterUI.Input;
using RTSEngine.Graphics;
using RTSEngine.Interfaces;

namespace RTSEngine.Controllers {
    public class PlayerInputController : InputController {

        private Vector2 mousePressedPos;
        public RTSRenderer Renderer { get; set; }
        
        public PlayerInputController(GameState g, RTSTeam t)
            : base(g, t) {
            SetToHook();
        }

        public void SetToHook() {
            MouseEventDispatcher.OnMouseRelease += new OnMouseRelease(OnMouseRelease);
            MouseEventDispatcher.OnMousePress += new OnMousePress(OnMousePress);
        }

        public void OnMouseRelease(Vector2 location, MouseButton b) {

            if(b == MouseButton.Left) {
                OBB? obb;
                Frustum? frustum;
                BoundingBox box = new BoundingBox();
                List<IEntity> selected = new List<IEntity>();
                Renderer.GetSelectionBox(location, mousePressedPos, out obb, out frustum);
                Frustum f = new Frustum();


                if(obb != null) {
                    for(int i = 0; i < GameState.Teams.Length; i++){
                        foreach(RTSUnitInstance unit in GameState.Teams[i].Units) {
                            //if(SelectionDetection.Intersects(f,box)){
                            //    selected.Add(unit);
                            //}
                        }
                    }

                }
                else {
                    for(int i = 0; i < GameState.Teams.Length; i++){
                        foreach(RTSUnitInstance unit in GameState.Teams[i].Units) {
                            //if(SelectionDetection.Intersects(frustum,box)){
                            //    selected.Add(unit);
                            //}
                        }
                    }
                }

                //AddEvent.(new SelectEvent(selected));

            }
   
            
        }

        public void OnMousePress(Vector2 location, MouseButton b) {
        
            if(b == MouseButton.Right) {  
                BoundingBox box;
                IDestructibleEntity target;
                for(int i = 0; i < GameState.Teams.Length; i++) {
                    foreach(RTSUnitInstance unit in GameState.Teams[i].Units) {
                       // if(Ray.Intersects(box) != null) {
                         //   target = unit;
                       // }
                    }
                }

                //if(target == null) {
                //    AddEvent(new SetWayPoint(location));
                //}
                //else {
                //    AddEvent(new SetTargetEvent(target);
                //}
            }
            else if(b == MouseButton.Left) {
                mousePressedPos = location;
            }
            
        }

    }
}
