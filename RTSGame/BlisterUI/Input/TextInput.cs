using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Input;

namespace BlisterUI.Input {
    public class TextInput : IDisposable {
        public bool IsActive {
            get;
            private set;
        }

        private StringBuilder text;
        public string Text {
            get { return text.ToString(); }
            set {
                text.Clear();
                text.Append(value);
                Caret = Math.Min(Caret, Length);
                if(OnTextChanged != null)
                    OnTextChanged(this, Text);
            }
        }
        public int Caret {
            get;
            private set;
        }
        public int Length {
            get { return text.Length; }
        }

        public event Action<TextInput, string> OnTextEntered;
        public event Action<TextInput, string> OnTextChanged;

        public TextInput() {
            text = new StringBuilder();
            Caret = 0;
            IsActive = false;
        }
        public void Dispose() {
            Deactivate();
            text.Clear();
            text = null;
        }

        public void Activate() {
            if(IsActive) return;
            IsActive = true;

            KeyboardEventDispatcher.OnKeyPressed += OnKeyPress;
            KeyboardEventDispatcher.ReceiveChar += OnChar;
            KeyboardEventDispatcher.ReceiveCommand += OnControl;
        }
        public void Deactivate() {
            if(!IsActive) return;
            IsActive = false;

            KeyboardEventDispatcher.OnKeyPressed -= OnKeyPress;
            KeyboardEventDispatcher.ReceiveChar -= OnChar;
            KeyboardEventDispatcher.ReceiveCommand -= OnControl;
        }

        public void Insert(char c) {
            text.Insert(Caret, c);
            Caret++;
            if(OnTextChanged != null)
                OnTextChanged(this, Text);
        }
        public void Insert(string s) {
            if(string.IsNullOrEmpty(s))
                return;
            text.Insert(Caret, s);
            Caret += s.Length;
            if(OnTextChanged != null)
                OnTextChanged(this, Text);
        }
        public void Delete() {
            if(Caret == Length)
                return;
            text.Remove(Caret, 1);
            if(OnTextChanged != null)
                OnTextChanged(this, Text);
        }
        public void BackSpace() {
            if(Caret == 0)
                return;
            Caret--;
            Delete();
        }

        public void OnKeyPress(object s, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.Enter:
                    if(text.Length < 1)
                        return;
                    if(OnTextEntered != null)
                        OnTextEntered(this, Text);
                    return;
                case Keys.Back:
                    BackSpace();
                    return;
                case Keys.Delete:
                    Delete();
                    return;
                case Keys.Left:
                    if(Caret > 0) Caret--;
                    return;
                case Keys.Right:
                    if(Caret < Length) Caret++;
                    return;
            }
        }
        public void OnChar(object s, CharacterEventArgs args) {
            Insert(args.Character);
        }
        public void OnControl(object s, CharacterEventArgs args) {
            switch(args.Character) {
                case ControlCharacters.CtrlV:
                    string c = KeyboardEventDispatcher.GetNewClipboard();
                    Insert(c);
                    return;
                case ControlCharacters.CtrlC:
                    if(text.Length > 0)
                        KeyboardEventDispatcher.SetToClipboard(Text);
                    return;
            }
        }
    }
}