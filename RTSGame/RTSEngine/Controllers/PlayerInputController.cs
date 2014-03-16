using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using Microsoft.Xna.Framework;
using BlisterUI.Input;

namespace RTSEngine.Controllers {
    public class PlayerInputController : InputController {

        private Vector2 MousePressedPos;
        private MouseButton MouseButtonPressed;
        
        public PlayerInputController(GameState g, RTSTeam t)
            : base(g, t) {
            SetToHook();
        }

        public void SetToHook() {
            MouseEventDispatcher.OnMouseRelease += new OnMouseRelease(OnMouseRelease);
            MouseEventDispatcher.OnMousePress += new OnMousePress(OnMousePress);
        }

        public void OnMouseRelease(Vector2 location, MouseButton b) {
            if(b == MouseButtonPressed){
                if(b == MouseButton.Left) {

                }
                else if(b == MouseButton.Right) {
                    
                }     
            }
        }

        public void OnMousePress(Vector2 location, MouseButton b) {
            MousePressedPos = location;
            MouseButtonPressed = b; 
        }

    }
}
