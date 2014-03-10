using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlisterUI.Input {
    public enum MOUSE_BUTTON {
        LEFT_BUTTON,
        RIGHT_BUTTON,
        MIDDLE_BUTTON,
        X_BUTTON_1,
        X_BUTTON_2
    };

    public sealed class MouseManager {
        private static MOUSE_BUTTON[] allButtons;
        static MouseManager() {
            allButtons = new MOUSE_BUTTON[]
            {
                MOUSE_BUTTON.LEFT_BUTTON,
                MOUSE_BUTTON.RIGHT_BUTTON,
                MOUSE_BUTTON.MIDDLE_BUTTON,
                MOUSE_BUTTON.X_BUTTON_1,
                MOUSE_BUTTON.X_BUTTON_2
            };
        }

        private bool mouseBound;
        public bool IsBound {
            get {
                return mouseBound;
            }
        }
        private int[] mouseBinding = new int[] { 400, 300 };

        private MouseState pMS;
        private MouseState cMS;
        public MouseState Current {
            get { return cMS; }
        }
        public MouseState Previous {
            get { return pMS; }
        }

        public MouseManager() {
            refresh();
            pMS = cMS;
            mouseBound = false;
        }

        public int getXDisplacement() {
            return cMS.X - pMS.X;
        }
        public int getYDisplacement() {
            return cMS.Y - pMS.Y;
        }
        public Vector2 getDisplacement() {
            return new Vector2(
                getXDisplacement(),
                getYDisplacement()
                );
        }

        public bool isButtonJustPressed(MOUSE_BUTTON button) {
            switch(button) {
                case MOUSE_BUTTON.LEFT_BUTTON:
                    return cMS.LeftButton == ButtonState.Pressed && pMS.LeftButton == ButtonState.Released;
                case MOUSE_BUTTON.MIDDLE_BUTTON:
                    return cMS.MiddleButton == ButtonState.Pressed && pMS.MiddleButton == ButtonState.Released;
                case MOUSE_BUTTON.RIGHT_BUTTON:
                    return cMS.RightButton == ButtonState.Pressed && pMS.RightButton == ButtonState.Released;
                case MOUSE_BUTTON.X_BUTTON_1:
                    return cMS.XButton1 == ButtonState.Pressed && pMS.XButton1 == ButtonState.Released;
                case MOUSE_BUTTON.X_BUTTON_2:
                    return cMS.XButton2 == ButtonState.Pressed && pMS.XButton2 == ButtonState.Released;
                default:
                    return false;
            }
        }
        public List<MOUSE_BUTTON> allButtonsJustPressed() {
            List<MOUSE_BUTTON> l = new List<MOUSE_BUTTON>();
            foreach(MOUSE_BUTTON b in allButtons) {
                if(isButtonJustPressed(b)) {
                    l.Add(b);
                }
            }
            return l;
        }
        public bool isButtonJustReleased(MOUSE_BUTTON button) {
            switch(button) {
                case MOUSE_BUTTON.LEFT_BUTTON:
                    return cMS.LeftButton == ButtonState.Released && pMS.LeftButton == ButtonState.Pressed;
                case MOUSE_BUTTON.MIDDLE_BUTTON:
                    return cMS.MiddleButton == ButtonState.Released && pMS.MiddleButton == ButtonState.Pressed;
                case MOUSE_BUTTON.RIGHT_BUTTON:
                    return cMS.RightButton == ButtonState.Released && pMS.RightButton == ButtonState.Pressed;
                case MOUSE_BUTTON.X_BUTTON_1:
                    return cMS.XButton1 == ButtonState.Released && pMS.XButton1 == ButtonState.Pressed;
                case MOUSE_BUTTON.X_BUTTON_2:
                    return cMS.XButton2 == ButtonState.Released && pMS.XButton2 == ButtonState.Pressed;
                default:
                    return false;
            }
        }
        public List<MOUSE_BUTTON> allButtonsJustReleased() {
            List<MOUSE_BUTTON> l = new List<MOUSE_BUTTON>();
            foreach(MOUSE_BUTTON b in allButtons) {
                if(isButtonJustReleased(b)) {
                    l.Add(b);
                }
            }
            return l;
        }

        public int getScrollDisplacement() {
            return cMS.ScrollWheelValue - pMS.ScrollWheelValue;
        }

        public void bind(Vector2 binding) {
            bind((int)binding.X, (int)binding.Y);
        }
        public void bind(params int[] binding) {
            mouseBinding = binding;
            mouseBound = true;
            Microsoft.Xna.Framework.Input.Mouse.SetPosition(mouseBinding[0], mouseBinding[1]);
            pMS = Microsoft.Xna.Framework.Input.Mouse.GetState();
        }
        public void unbind() {
            mouseBound = false;
        }

        public void refresh() {
            if(mouseBound) {
                cMS = Microsoft.Xna.Framework.Input.Mouse.GetState();
                Microsoft.Xna.Framework.Input.Mouse.SetPosition(mouseBinding[0], mouseBinding[1]);
            }
            else {
                pMS = cMS;
                cMS = Microsoft.Xna.Framework.Input.Mouse.GetState();
            }
        }
        public void refreshPosition() {
            MouseState nMS = Microsoft.Xna.Framework.Input.Mouse.GetState();
            pMS = new MouseState(
                nMS.X, nMS.Y,
                pMS.ScrollWheelValue,
                pMS.LeftButton,
                pMS.MiddleButton,
                pMS.RightButton,
                pMS.XButton1,
                pMS.XButton2
                );
            cMS = new MouseState(
                nMS.X, nMS.Y,
                nMS.ScrollWheelValue,
                cMS.LeftButton | nMS.LeftButton,
                cMS.MiddleButton | nMS.MiddleButton,
                cMS.RightButton | nMS.RightButton,
                cMS.XButton1 | nMS.XButton1,
                cMS.XButton2 | nMS.XButton2
                );
        }
    }
}
