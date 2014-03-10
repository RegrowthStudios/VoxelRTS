using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI {
    public sealed class BlisterFont {
        public Texture2D Texture { get; private set; }
        public StringFormatFlags Format { get; private set; }
        public float Height { get; private set; }

        public char CharStart { get; private set; }
        public char CharEnd { get; private set; }
        public int CharRange {
            get { return CharEnd - CharStart + 1; }
        }
        public IEnumerable<char> Chars {
            get {
                for(char c = CharStart; c <= CharEnd; c++) {
                    yield return c;
                }
            }

        }

        private GlyphData[] glyphs;

        public BlisterFont(string fontName, StringFormatFlags f, float h, char s, char e) {
            Format = f;
            Height = h;
            CharStart = s;
            CharEnd = e;
            glyphs = new GlyphData[CharRange];
            Font font = new Font(fontName, h, FontStyle.Regular, GraphicsUnit.Pixel);

            Graphics g;
            using(Bitmap bmp = new Bitmap((int)(h * 4), (int)(h) + 1)) {
                g = Graphics.FromImage(bmp);
                StringFormat sf = new StringFormat(Format);
                int i = 0;
                foreach(char c in Chars) {
                    glyphs[i].Character = c;
                    glyphs[i].Width = g.MeasureString(new string(c, 1), font).Width;
                }
            }
        }
    }
    public struct GlyphData {
        public char Character;
        public float Width;
        public GlyphRect Rect;
    }
    public struct GlyphRect {
        public Vector2 Location, End;
        public Vector2 Size {
            get { return End - Location; }
        }
    }
}
