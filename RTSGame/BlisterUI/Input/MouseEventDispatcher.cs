using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlisterUI.Input {
    public delegate void OnMouseRelease(Vector2 location, MOUSE_BUTTON b);
    public delegate void OnMousePress(Vector2 location, MOUSE_BUTTON b);
    public delegate void OnMouseScroll(int value, int displacement);
    public delegate void OnMouseMotion(Vector2 location, Vector2 movement);

    public static class MouseEventDispatcher {
        public static void setToHook() {
            WMHookInput.MouseMotion += new MouseMotionHandler(EventInput_MouseMotion);
            WMHookInput.MouseButton += new MouseButtonHandler(EventInput_MouseButton);
            WMHookInput.MouseWheel += new MouseWheelHandler(EventInput_MouseWheel);
        }

        static Vector2 pMouse;
        static Vector2 cMouse;
        static void EventInput_MouseMotion(object sender, MouseMotionEventArgs e) {
            cMouse.X = e.X;
            cMouse.Y = e.Y;
            if(OnMouseMotion != null) {
                OnMouseMotion(cMouse, cMouse - pMouse);
            }
            pMouse = cMouse;
        }
        static void EventInput_MouseButton(object sender, MouseButtonEventArgs e) {
            if(e.State == ButtonState.Pressed) {
                if(OnMousePress != null) {
                    OnMousePress(new Vector2(e.X, e.Y), e.Button);
                }
            }
            else {
                if(OnMouseRelease != null) {
                    OnMouseRelease(new Vector2(e.X, e.Y), e.Button);
                }
            }
        }
        static int totalDeltas = 0;
        static void EventInput_MouseWheel(object sender, MouseWheelEventArgs e) {
            totalDeltas += e.Deltas;
            if(OnMouseScroll != null) {
                OnMouseScroll(totalDeltas, e.Deltas);
            }
        }

        public static event OnMouseRelease OnMouseRelease;
        public static event OnMousePress OnMousePress;
        public static event OnMouseScroll OnMouseScroll;
        public static event OnMouseMotion OnMouseMotion;
    }
}
