using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;
using BlisterUI.Input;

namespace RTSEngine.Controllers {
    public static class DevConsole {
        public const int MAX_LINES = 16;
        public const int MAX_COMMAND_LENGTH = 60;
        public const Keys ACTIVATION_KEY = Keys.OemTilde;
        private const char ACTIVATION_CHAR1 = '`';
        private const char ACTIVATION_CHAR2 = '~';

        private static int lc;
        public static readonly Queue<string> Lines = new Queue<string>((MAX_LINES * 3) / 2);
        public static event Action<string> OnNewCommand;

        private static TextInput tInput;

        public static bool IsActivated {
            get;
            private set;
        }
        public static string TypedText {
            get { return tInput.Text; }
        }
        public static int TextCaret {
            get { return tInput.Caret; }
        }

        static DevConsole() {
            IsActivated = false;
            tInput = new TextInput();
            tInput.Text = "";
            lc = 0;
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
            tInput.Activate();
            lc = 0;
            tInput.OnTextChanged += OnTextChanged;
            tInput.OnTextEntered += OnTextEntered;
            tInput.Text = "";
            KeyboardEventDispatcher.OnKeyPressed += OnKP;
        }
        public static void Deactivate() {
            if(!IsActivated) return;
            IsActivated = false;
            tInput.Deactivate();
            tInput.OnTextChanged -= OnTextChanged;
            tInput.OnTextEntered -= OnTextEntered;
            KeyboardEventDispatcher.OnKeyPressed -= OnKP;
        }

        public static void OnKP(object sender, KeyEventArgs args) {
            string[] lines;
            switch(args.KeyCode) {
                case Keys.Down:
                    lines = Lines.ToArray();
                    if(lines.Length < 1) return;
                    lc = (lc + 1) % lines.Length;
                    tInput.Text = lines[lc];
                    break;
                case Keys.Up:
                    lines = Lines.ToArray();
                    if(lines.Length < 1) return;
                    lc = (lc + lines.Length - 1) % lines.Length;
                    tInput.Text = lines[lc];
                    break;
                default:
                    return;
            }
        }

        private static void OnTextChanged(TextInput ti, string s) {
            if(s.Length > MAX_COMMAND_LENGTH)
                ti.Text = s.Substring(0, MAX_COMMAND_LENGTH);
        }
        private static void OnTextEntered(TextInput ti, string s) {
            ti.Text = "";
            lc = 0;
            AddCommand(s);
        }
    }
}