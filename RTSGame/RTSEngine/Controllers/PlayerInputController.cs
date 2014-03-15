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
        private Vector2 MouseReleasedPos;
        private MouseButton MouseButtonPressed;
        private MouseButton MouseButtonReleased;
        
        public PlayerInputController(GameState g, RTSTeam t)
            : base(g, t) {
            SetToHook();
        }

        public void SetToHook() {
            MouseEventDispatcher.OnMouseRelease += new OnMouseRelease(OnMouseRelease);
            MouseEventDispatcher.OnMousePress += new OnMousePress(OnMousePress);
            MouseEventDispatcher.OnMouseScroll += new OnMouseScroll(OnMouseScroll);
            MouseEventDispatcher.OnMouseMotion += new OnMouseMotion(OnMouseMotion);
        }

        public void OnMouseRelease(Vector2 location, MouseButton b) {
            MouseReleasedPos = location;
            MouseButtonReleased = b;
            if(MouseButtonPressed == MouseButtonReleased){
                

                
            }
        }

        public void OnMousePress(Vector2 location, MouseButton b) {
            MousePressedPos = location;
            MouseButtonPressed = b; 
        }

        public void OnMouseScroll(int value, int displacement) {

        }

        public void OnMouseMotion(Vector2 location, Vector2 movement) {

        }


    }
}
