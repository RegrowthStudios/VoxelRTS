using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Microsoft.Xna.Framework.Graphics {
    public static class TextCaretHelper {
        public static Vector3 GetCaretOffsetAndHeight(this SpriteFont font, string text, int i) {
            StringBuilder sb = new StringBuilder();
            Vector3 pos = Vector3.Zero;
            sb.Clear();
            int ci = 0;
            while(ci < i) {
                if(text[ci] == '\n') {
                    pos.Y += font.LineSpacing;
                    sb.Clear();
                }
                else {
                    sb.Append(text[ci]);
                }
                ci++;
            }
            if(sb.Length > 0) {
                Vector2 ts = font.MeasureString(sb.ToString());
                pos.X += ts.X;
                pos.Z = ts.Y;
            }
            else {
                pos.Z = font.LineSpacing;
            }
            return pos;
        }
    }
}