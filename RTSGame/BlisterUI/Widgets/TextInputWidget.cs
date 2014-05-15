using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Input;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI.Widgets {
    public class TextInputWidget : TextWidget {
        public RectWidget Caret {
            get;
            private set;
        }
        public TextInput TextInput {
            get;
            private set;
        }
        public override string Text {
            get {
                return TextInput.Text;
            }
            set {
                TextInput.Text = value;
                drawText.Text = value;
                Recompute();
            }
        }

        public bool IsActive {
            get { return TextInput.IsActive; }
        }

        public TextInputWidget(WidgetRenderer wr, SpriteFont f = null, Texture2D t = null)
            : base(wr, f) {
            Caret = new RectWidget(wr, t);
            Caret.Parent = this;
            Caret.Width = 1;
            Caret.Height = Height;
            TextInput = new TextInput();
            TextInput.OnTextChanged += tInput_OnTextChanged;
            TextInput.OnCaretMoved += TextInput_OnCaretMoved;
        }

        void TextInput_OnCaretMoved(TextInput arg1, int arg2) {
            Vector3 cPosH = Font.GetCaretOffsetAndHeight(arg1.Text, Math.Max(0, arg2));
            Vector2 cPos = new Vector2(cPosH.X, cPosH.Y) * drawText.TextScale;
            Caret.Offset = new Point((int)cPos.X, (int)cPos.Y);
            Caret.Height = (int)(cPosH.Z * drawText.TextScale);
        }
        void tInput_OnTextChanged(TextInput arg1, string arg2) {
            drawText.Text = arg2;
            Recompute();
            if(Font == null) return;

            Vector3 cPosH = Font.GetCaretOffsetAndHeight(arg2, Math.Max(0, arg1.Caret));
            Vector2 cPos = new Vector2(cPosH.X, cPosH.Y) * drawText.TextScale;
            Caret.Offset = new Point((int)cPos.X, (int)cPos.Y);
            Caret.Height = (int)(cPosH.Z * drawText.TextScale);
        }

        public void ActivateInput() {
            TextInput.Activate();
        }
        public void DeactivateInput() {
            TextInput.Deactivate();
        }

        protected override void DisposeOther() {
            base.DisposeOther();
            TextInput.Dispose();
        }
    }
}
