using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI.Input;

namespace RTSEngine.Controllers {
    public class CameraController {
        public const Keys KEY_ORBIT = Keys.Q;
        public const Keys KEY_MOVE_LEFT = Keys.A;
        public const Keys KEY_MOVE_RIGHT = Keys.D;
        public const Keys KEY_MOVE_UP = Keys.W;
        public const Keys KEY_MOVE_DOWN = Keys.S;
        public const Keys KEY_MOVE_LEFT_ALT = Keys.Left;
        public const Keys KEY_MOVE_RIGHT_ALT = Keys.Right;
        public const Keys KEY_MOVE_UP_ALT = Keys.Up;
        public const Keys KEY_MOVE_DOWN_ALT = Keys.Down;
        public const Keys KEY_RESET_DEFAULT = Keys.OemQuotes;
        public const int SCROLL_PANE_WIDTH = 12;

        // How To Calculate The Window Input Locations
        public int WindowWidth {
            get;
            private set;
        }
        public int WindowHeight {
            get;
            private set;
        }
        private int MinX {
            get { return SCROLL_PANE_WIDTH; }
        }
        private int MaxX {
            get { return WindowWidth - SCROLL_PANE_WIDTH; }
        }
        private int MinY {
            get { return SCROLL_PANE_WIDTH; }
        }
        private int MaxY {
            get { return WindowHeight - SCROLL_PANE_WIDTH; }
        }

        // For Checking If OS Events Are Tied In
        public bool IsHooked {
            get;
            private set;
        }
        private bool isActive;
        public bool IsActive {
            get { return isActive; }
            set {
                isActive = value;
                if(!IsActive) {
                    mx = 0;
                    my = 0;
                    zoom = 0;
                }
            }
        }

        // Output Values
        private int mx, my;
        public int KX {
            get {
                int kp = moveKeys[0] || moveKeys[1] ? 1 : 0;
                int kn = moveKeys[2] || moveKeys[3] ? 1 : 0;
                return kp - kn;
            }
        }
        public int KY {
            get {
                int kp = moveKeys[4] || moveKeys[5] ? 1 : 0;
                int kn = moveKeys[6] || moveKeys[7] ? 1 : 0;
                return kp - kn;
            }
        }
        public int ScrollX {
            get {
                if(useOrbit) return 0;
                return mx == 0 ? KX : mx;
            }
        }
        public int ScrollY {
            get {
                if(useOrbit) return 0;
                return my == 0 ? KY : my;
            }
        }
        public int Yaw {
            get {
                if(!useOrbit) return 0;
                return mx == 0 ? KX : mx;
            }
        }
        public int Pitch {
            get {
                if(!useOrbit) return 0;
                return my == 0 ? KY : my;
            }
        }
        private int zoom;
        private bool resetDefault;

        // Input Context
        private bool useOrbit;
        private readonly bool[] moveKeys;

        public CameraController(int wWindow, int hWindow) {
            WindowWidth = wWindow;
            WindowHeight = hWindow;
            IsHooked = false;
            zoom = 0;
            useOrbit = false;
            moveKeys = new bool[8];
            Array.Clear(moveKeys, 0, moveKeys.Length);
            IsActive = true;
        }

        public void Hook(GameWindow w) {
            if(IsHooked) return;
            IsHooked = true;
            MouseEventDispatcher.OnMouseMotion += OnMouseMovement;
            MouseEventDispatcher.OnMouseScroll += OnMouseScroll;
            KeyboardEventDispatcher.OnKeyPressed += OnKeyPress;
            KeyboardEventDispatcher.OnKeyReleased += OnKeyRelease;
            w.ClientSizeChanged += OnWindowResize;
        }
        public void Unhook(GameWindow w) {
            if(!IsHooked) return;
            IsHooked = false;
            zoom = 0;
            useOrbit = false;
            Array.Clear(moveKeys, 0, moveKeys.Length);
            MouseEventDispatcher.OnMouseMotion -= OnMouseMovement;
            KeyboardEventDispatcher.OnKeyPressed -= OnKeyPress;
            KeyboardEventDispatcher.OnKeyReleased -= OnKeyRelease;
            w.ClientSizeChanged -= OnWindowResize;
        }

        public void GetZoom(out int z) {
            z = zoom;
            zoom = 0;
            return;
        }
        public void GetResetDefault(out bool b) {
            b = resetDefault;
            resetDefault = false;
            return;
        }

        // Event Hooks
        public void OnMouseMovement(Vector2 pos, Vector2 disp) {
            if(!IsActive) return;

            if(pos.X < MinX) mx = -1;
            else if(pos.X > MaxX) mx = 1;
            else mx = 0;

            if(pos.Y < MinY) my = 1;
            else if(pos.Y > MaxY) my = -1;
            else my = 0;
        }
        public void OnMouseScroll(int v, int d) {
            if(!IsActive) return;

            zoom = d > 0 ? -1 : 1;
        }
        public void OnKeyPress(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case KEY_ORBIT: useOrbit = true; return;
                case KEY_MOVE_RIGHT: moveKeys[0] = true; return;
                case KEY_MOVE_RIGHT_ALT: moveKeys[1] = true; return;
                case KEY_MOVE_LEFT: moveKeys[2] = true; return;
                case KEY_MOVE_LEFT_ALT: moveKeys[3] = true; return;
                case KEY_MOVE_UP: moveKeys[4] = true; return;
                case KEY_MOVE_UP_ALT: moveKeys[5] = true; return;
                case KEY_MOVE_DOWN: moveKeys[6] = true; return;
                case KEY_MOVE_DOWN_ALT: moveKeys[7] = true; return;
                case KEY_RESET_DEFAULT: resetDefault = true; return;
            }
        }
        public void OnKeyRelease(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case KEY_ORBIT: useOrbit = false; return;
                case KEY_MOVE_RIGHT: moveKeys[0] = false; return;
                case KEY_MOVE_RIGHT_ALT: moveKeys[1] = false; return;
                case KEY_MOVE_LEFT: moveKeys[2] = false; return;
                case KEY_MOVE_LEFT_ALT: moveKeys[3] = false; return;
                case KEY_MOVE_UP: moveKeys[4] = false; return;
                case KEY_MOVE_UP_ALT: moveKeys[5] = false; return;
                case KEY_MOVE_DOWN: moveKeys[6] = false; return;
                case KEY_MOVE_DOWN_ALT: moveKeys[7] = false; return;
            }
        }
        public void OnWindowResize(object sender, EventArgs args) {
            GameWindow window = sender as GameWindow;
            if(window == null) return;
            WindowWidth = window.ClientBounds.Width;
            WindowHeight = window.ClientBounds.Height;

            Array.Clear(moveKeys, 0, moveKeys.Length);
            mx = 0; my = 0;
        }
    }
}