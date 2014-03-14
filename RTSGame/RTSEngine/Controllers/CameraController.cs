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
        public const Keys ORBIT_KEY = Keys.Q;
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

        public bool IsHooked {
            get;
            private set;
        }

        // Output Values
        public int ScrollX {
            get;
            private set;
        }
        public int ScrollY {
            get;
            private set;
        }
        public int Yaw {
            get;
            private set;
        }
        public int Pitch {
            get;
            private set;
        }

        // Input Context
        private bool useOrbit;

        public CameraController(int wWindow, int hWindow) {
            WindowWidth = wWindow;
            WindowHeight = hWindow;
            IsHooked = false;
            ScrollX = 0;
            ScrollY = 0;
            Yaw = 0;
            Pitch = 0;
            useOrbit = false;
        }

        public void Hook() {
            if(IsHooked) return;
            IsHooked = true;
            MouseEventDispatcher.OnMouseMotion += OnMouseMovement;
            KeyboardEventDispatcher.OnKeyPressed += OnKeyPress;
            KeyboardEventDispatcher.OnKeyReleased += OnKeyRelease;
        }
        public void Unhook() {
            if(!IsHooked) return;
            IsHooked = false;
            ScrollX = 0;
            ScrollY = 0;
            Pitch = 0;
            Yaw = 0;
            useOrbit = false;
            MouseEventDispatcher.OnMouseMotion -= OnMouseMovement;
            KeyboardEventDispatcher.OnKeyPressed -= OnKeyPress;
            KeyboardEventDispatcher.OnKeyReleased -= OnKeyRelease;
        }

        // Event Hooks
        public void OnMouseMovement(Vector2 pos, Vector2 disp) {
            if(useOrbit) {
                if(pos.X < MinX) Yaw = -1;
                else if(pos.X > MaxX) Yaw = 1;
                else Yaw = 0;

                if(pos.Y < MinY) Pitch = 1;
                else if(pos.Y > MaxY) Pitch = -1;
                else Pitch = 0;
            }
            else {
                if(pos.X < MinX) ScrollX = -1;
                else if(pos.X > MaxX) ScrollX = 1;
                else ScrollX = 0;

                if(pos.Y < MinY) ScrollY = 1;
                else if(pos.Y > MaxY) ScrollY = -1;
                else ScrollY = 0;
            }
        }
        public void OnKeyPress(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case ORBIT_KEY:
                    useOrbit = true;
                    ScrollX = 0;
                    ScrollY = 0;
                    return;
            }
        }
        public void OnKeyRelease(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case ORBIT_KEY:
                    useOrbit = false;
                    Yaw = 0;
                    Pitch = 0;
                    return;
            }
        }
        public void OnWindowResize(object sender, EventArgs args) {
            GameWindow window = sender as GameWindow;
            if(window == null) return;
            WindowWidth = window.ClientBounds.Width;
            WindowHeight = window.ClientBounds.Height;
            ScrollX = 0;
            ScrollY = 0;
            Yaw = 0;
            Pitch = 0;
        }
    }
}