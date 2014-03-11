using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Control = System.Windows.Forms.Control;
using WinKeys = System.Windows.Forms.Keys;

namespace BlisterUI.Input {
    public delegate void ReceiveChar(object sender, CharacterEventArgs args);
    public delegate void ReceiveCommand(object sender, CharacterEventArgs args);
    public delegate void OnKeyStateChange(object sender, KeyEventArgs args);

    public static class ControlCharacters {
        public const char CtrlA = (char)0x01;
        public const char CtrlB = (char)0x02;
        public const char CtrlC = (char)0x03;
        public const char CtrlD = (char)0x04;
        public const char CtrlE = (char)0x05;
        public const char CtrlF = (char)0x06;
        public const char CtrlG = (char)0x07;
        public const char CtrlH = (char)0x08;
        public const char CtrlI = (char)0x09;
        public const char CtrlJ = (char)0x0a;
        public const char CtrlK = (char)0x0b;
        public const char CtrlL = (char)0x0c;
        public const char CtrlM = (char)0x0d;
        public const char CtrlN = (char)0x0e;
        public const char CtrlO = (char)0x0f;
        public const char CtrlP = (char)0x10;
        public const char CtrlQ = (char)0x11;
        public const char CtrlR = (char)0x12;
        public const char CtrlS = (char)0x13;
        public const char CtrlT = (char)0x14;
        public const char CtrlU = (char)0x15;
        public const char CtrlV = (char)0x16;
        public const char CtrlW = (char)0x17;
        public const char CtrlX = (char)0x18;
        public const char CtrlY = (char)0x19;
        public const char CtrlZ = (char)0x1a;
        public const char CtrlOpenBrackets = (char)0x1b;
        public const char CtrlForwardSlash_Pipe = (char)0x1c;
        public const char CtrlCloseBrackets = (char)0x1d;

        public const char CtrlBackSpace = (char)0x7f;

        public const char CtrlEnter = CtrlJ;
    }
    public static class VirtualKeys {
        public const int LShift = 0xa0;
        public const int RShift = 0xa1;
        public const int LControl = 0xa2;
        public const int RControl = 0xa3;
        public const int LAlt = 0xa4;
        public const int RAlt = 0xa5;
    }

    public static class KeyboardEventDispatcher {
        private static ModifierList Modifiers;
        public static ModifierList GetCurrentModifiers() {
            return Modifiers;
        }
        public static void RefreshModfiers() {
            Modifiers.ShiftPressed = 0;
            if((GetKeyState(0xa0) & 0x8000) != 0) { Modifiers.ShiftPressed++; }
            if((GetKeyState(0xa1) & 0x8000) != 0) { Modifiers.ShiftPressed++; }

            Modifiers.ControlPressed = 0;
            if((GetKeyState(0xa2) & 0x8000) != 0) { Modifiers.ControlPressed++; }
            if((GetKeyState(0xa3) & 0x8000) != 0) { Modifiers.ControlPressed++; }

            Modifiers.AltPressed = 0;
            if((GetKeyState(0xa4) & 0x8000) != 0) { Modifiers.AltPressed++; }
            if((GetKeyState(0xa5) & 0x8000) != 0) { Modifiers.AltPressed++; }
        }

        [DllImport("user32.dll", CharSet = CharSet.Unicode)]
        static extern short GetKeyState(int keyCode);

        public static event ReceiveChar ReceiveChar;
        public static event ReceiveCommand ReceiveCommand;
        public static event OnKeyStateChange OnKeyPressed;
        public static event OnKeyStateChange OnKeyReleased;

        static string clipboard = "";
        public static string Clipboard {
            get {
                return clipboard;
            }
        }

        static KeyboardEventDispatcher() {
            //Get All The Modifiers In The Beginning
            Modifiers = new ModifierList();
            Modifiers.CapsLockState = Control.IsKeyLocked(WinKeys.CapsLock);
            Modifiers.NumLockState = Control.IsKeyLocked(WinKeys.NumLock);
            Modifiers.ScrollLockState = Control.IsKeyLocked(WinKeys.Scroll);
            RefreshModfiers();
            OnKeyPressed +=
                (sender, args) => {
                    switch(args.KeyCode) {
                        case Keys.CapsLock: Modifiers.CapsLockState = !Modifiers.CapsLockState; return;
                        case Keys.Scroll: Modifiers.ScrollLockState = !Modifiers.ScrollLockState; return;
                        case Keys.NumLock: Modifiers.NumLockState = !Modifiers.NumLockState; return;
                    }
                };
        }

        public static void EventInput_KeyDown(object sender, Keys key) {
            RefreshModfiers();
            if(OnKeyPressed != null) {
                OnKeyPressed(sender, new KeyEventArgs(
                    key
                    ));
            }
        }
        public static void EventInput_KeyUp(object sender, Keys key) {
            RefreshModfiers();
            if(OnKeyReleased != null) {
                OnKeyReleased(sender, new KeyEventArgs(
                    key
                    ));
            }
        }
        public static void EventInput_CharEntered(object sender, char c, int lParam) {
            CharacterEventArgs e = new CharacterEventArgs(c, lParam);
            if(char.IsControl(e.Character)) {
                if(ReceiveCommand != null) {
                    ReceiveCommand(sender, e);
                }
            }
            else {
                if(ReceiveChar != null) {
                    ReceiveChar(sender, e);
                }
            }
        }

        public static void SetToClipboard(string s) {
            clipboard = s;
            Thread thread = new Thread(DoCopyThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
        }
        [STAThread]
        static void DoCopyThread() {
            System.Windows.Forms.Clipboard.SetText(clipboard);
        }

        public static string GetNewClipboard() {
            Thread thread = new Thread(DoPasteThread);
            thread.SetApartmentState(ApartmentState.STA);
            thread.Start();
            thread.Join();
            return clipboard;
        }
        [STAThread]
        static void DoPasteThread() {
            if(System.Windows.Forms.Clipboard.ContainsText()) {
                clipboard = System.Windows.Forms.Clipboard.GetText();
            }
            else {
                clipboard = "";
            }
        }
    }
}