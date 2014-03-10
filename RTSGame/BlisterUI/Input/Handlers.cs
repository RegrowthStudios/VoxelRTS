using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlisterUI.Input {
    public class CharacterEventArgs : EventArgs {
        private readonly char character;
        public char Character {
            get { return character; }
        }

        private readonly int lParam;
        public int Param {
            get { return lParam; }
        }
        public int RepeatCount {
            get { return lParam & 0xffff; }
        }
        public bool ExtendedKey {
            get { return (lParam & (1 << 24)) > 0; }
        }
        public bool AltPressed {
            get { return (lParam & (1 << 29)) > 0; }
        }
        public bool PreviousState {
            get { return (lParam & (1 << 30)) > 0; }
        }
        public bool TransitionState {
            get { return (lParam & (1 << 31)) > 0; }
        }

        public CharacterEventArgs(char character, int lParam) {
            this.character = character;
            this.lParam = lParam;
        }
    }
    public delegate void CharEnteredHandler(object sender, CharacterEventArgs e);

    //This Must Only Be Maintained By The Keyboard Event Dispatcher
    public struct ModifierList {
        public int ControlPressed;
        public bool IsControlPressed {
            get {
                return ControlPressed > 0;
            }
        }
        public int AltPressed;
        public bool IsAltPressed {
            get {
                return AltPressed > 0;
            }
        }
        public int ShiftPressed;
        public bool IsShiftPressed {
            get {
                return ShiftPressed > 0;
            }
        }
        public bool CapsLockState;
        public bool ShiftEffect {
            get {
                return CapsLockState ^ (IsShiftPressed);
            }
        }
        public bool NumLockState;
        public bool ScrollLockState;
    }

    public class KeyEventArgs : EventArgs {
        private Keys keyCode;
        public Keys KeyCode {
            get { return keyCode; }
        }

        public KeyEventArgs(Keys keyCode) {
            this.keyCode = keyCode;
        }
    }
    public delegate void KeyEventHandler(object sender, KeyEventArgs e);

    public class MouseMotionEventArgs : EventArgs {
        private int x;
        public int X { get { return x; } }
        private int y;
        public int Y { get { return y; } }

        public MouseMotionEventArgs(long lParam) {
            x = (int)lParam & 0x0000ffff;
            if(x > 10000) {
                x = 0;
            }
            y = (int)(lParam >> 16) & 0x0000ffff;
            if(y > 10000) {
                y = 0;
            }
        }
    }
    public delegate void MouseMotionHandler(object sender, MouseMotionEventArgs e);

    public class MouseButtonEventArgs : MouseMotionEventArgs {
        private MOUSE_BUTTON button;
        public MOUSE_BUTTON Button { get { return button; } }
        private ButtonState state;
        public ButtonState State { get { return state; } }

        public MouseButtonEventArgs(long lParam, MOUSE_BUTTON button, ButtonState state)
            : base(lParam) {
            this.button = button;
            this.state = state;
        }
    }
    public delegate void MouseButtonHandler(object sender, MouseButtonEventArgs e);

    public class MouseWheelEventArgs : EventArgs {
        private int deltas;
        public int Deltas { get { return deltas; } }
        public bool ScrollUp { get { return deltas > 0; } }
        public bool ScrollDown { get { return deltas < 0; } }

        public MouseWheelEventArgs(int wParam) {
            unchecked {
                deltas = (wParam & (int)0xffff0000) >> 16;
            }
        }
    }
    public delegate void MouseWheelHandler(object sender, MouseWheelEventArgs e);
}
