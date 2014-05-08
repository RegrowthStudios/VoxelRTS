using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using BlisterUI.Input;

namespace RTSEngine.Controllers {
    public static class DevConsole {
        public const int MAX_LINES = 8;
        public const int MAX_COMMAND_LENGTH = 40;
        public const Keys ACTIVATION_KEY = Keys.OemTilde;
        private const char ACTIVATION_CHAR1 = '`';
        private const char ACTIVATION_CHAR2 = '~';

        public static readonly Queue<string> Lines = new Queue<string>((MAX_LINES * 3) / 2);
        public static event Action<string> OnNewCommand;

        public static bool IsActivated {
            get;
            private set;
        }
        private static readonly StringBuilder typedText = new StringBuilder((MAX_COMMAND_LENGTH * 3) / 2);
        public static string TypedText {
            get {
                if(typedText.Length < 1)
                    return "";
                else
                    return typedText.ToString();
            }
        }

        static DevConsole() {
            IsActivated = false;
        }

        public static void AddCommand(string c) {
            // Check If It Is A Command
            if(string.IsNullOrWhiteSpace(c))
                return;

            // Check For Length
            if(c.Length > MAX_COMMAND_LENGTH)
                c = c.Substring(0, MAX_COMMAND_LENGTH);

            // Move Command In Console
            Lines.Enqueue(c);
            if(Lines.Count > MAX_LINES)
                Lines.Dequeue();

            // Send Event
            if(OnNewCommand != null)
                OnNewCommand(c);
        }

        public static void Toggle(Action<string> f) {
            if(IsActivated) {
                OnNewCommand -= f;
                Deactivate();
            }
            else {
                Activate();
                OnNewCommand += f;
            }
        }
        public static void Activate() {
            if(IsActivated) return;
            IsActivated = true;
            KeyboardEventDispatcher.OnKeyPressed += OnKeyPress;
            KeyboardEventDispatcher.ReceiveChar += OnChar;
            KeyboardEventDispatcher.ReceiveCommand += OnControl;
        }
        public static void Deactivate() {
            if(!IsActivated) return;
            IsActivated = false;
            KeyboardEventDispatcher.OnKeyPressed -= OnKeyPress;
            KeyboardEventDispatcher.ReceiveChar -= OnChar;
            KeyboardEventDispatcher.ReceiveCommand -= OnControl;
        }

        public static void OnKeyPress(object s, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.Enter:
                    if(typedText.Length < 1)
                        return;
                    AddCommand(typedText.ToString());
                    typedText.Clear();
                    return;
                case Keys.Back:
                    if(typedText.Length < 1)
                        return;
                    typedText.Remove(typedText.Length - 1, 1);
                    return;
            }
        }
        public static void OnChar(object s, CharacterEventArgs args) {
            if(typedText.Length == MAX_COMMAND_LENGTH)
                return;
            if(args.Character == ACTIVATION_CHAR1 || args.Character == ACTIVATION_CHAR2)
                return;
            typedText.Append(args.Character);
        }
        public static void OnControl(object s, CharacterEventArgs args) {
            switch(args.Character) {
                case ControlCharacters.CtrlV:
                    if(typedText.Length == MAX_COMMAND_LENGTH)
                        return;

                    string c = KeyboardEventDispatcher.GetNewClipboard();
                    if(typedText.Length + c.Length > MAX_COMMAND_LENGTH)
                        typedText.Append(c.Substring(0, MAX_COMMAND_LENGTH - typedText.Length));
                    else
                        typedText.Append(c);
                    return;
                case ControlCharacters.CtrlC:
                    if(typedText.Length > 0)
                        KeyboardEventDispatcher.SetToClipboard(typedText.ToString());
                    return;
            }
        }
    }
}