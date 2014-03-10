using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI {
    public struct UIGlyph {
        public bool Created;
        public Vector2 UVMin, UVMax;
        public readonly float AspectRatio;
        public float InvAspectRatio {
            get { return 1 / AspectRatio; }
        }

        public Vector2 TopLeft {
            get { return UVMin; }
        }
        public Vector2 TopRight {
            get { return new Vector2(UVMax.X, UVMin.Y); }
        }
        public Vector2 BottomLeft {
            get { return new Vector2(UVMin.X, UVMax.Y); }
        }
        public Vector2 BottomRight {
            get { return UVMax; }
        }

        public UIGlyph(UIGlyph g1, UIGlyph g2) {
            Created = true;
            UVMin = Vector2.Min(g1.UVMin, g2.UVMin);
            UVMax = Vector2.Max(g1.UVMax, g2.UVMax);
            AspectRatio = (g1.AspectRatio + g2.AspectRatio) * 0.5f;
        }
        public UIGlyph(Point min, Point max, Point texSize) {
            Created = true;
            AspectRatio = (float)(max.X - min.X) / (float)(max.Y - min.Y);
            UVMin = new Vector2(
                (float)min.X / (float)texSize.X,
                (float)min.Y / (float)texSize.Y
                );
            UVMax = new Vector2(
                (float)max.X / (float)texSize.X,
                (float)max.Y / (float)texSize.Y
                );
        }
    }

    public class UITexture {
        #region Static
        public const int CharStart = ' ';
        public const int CharEnd = '~';

        public const int DataStart = CharStart;

        public const int DataChar = CharStart;
        public const int TotalChars = CharEnd - CharStart + 1;

        public const int DataPixel = CharEnd + 1;

        public const int DataBorder = DataPixel + 1;
        public const int GlyphsPerBorder = 9;
        public const int TotalBorders = 3;

        public const int TotalData = TotalChars + 1 + (TotalBorders * GlyphsPerBorder);
        public const int DataEnd = DataStart + TotalData - 1;

        private static uint upperPower2(uint v) {
            v--;
            v |= v >> 1;
            v |= v >> 2;
            v |= v >> 4;
            v |= v >> 8;
            v |= v >> 16;
            v++;
            return v;
        }
        private static Vector2 toUV(Point c, Point tSize) {
            return new Vector2(
                (float)c.X / (float)tSize.X,
                (float)c.Y / (float)tSize.Y
                );
        }
        public static void createTemplate(GraphicsDevice g, SpriteFont f, SpriteBatch batch, string imageFile, string dataFile, int spacing = 4) {
            Rectangle[] gRects = new Rectangle[TotalChars];
            int tWidth = 0, tHeight = 0;
            for(int i = 0; i < gRects.Length; i++) {
                char c = (char)(i + CharStart);
                Vector2 s = f.MeasureString(new string(c, 1));
                gRects[i] = new Rectangle(0, 0, (int)Math.Ceiling(s.X), (int)Math.Ceiling(s.Y));
                tWidth += gRects[i].Width;
                tHeight += gRects[i].Height;
            }
            tHeight /= TotalChars;

            // Get A Pretty Square Texture
            int charsPerLine = TotalChars;
            while(tWidth / 2 > tHeight * 2) {
                tWidth /= 2;
                tHeight *= 2;
                charsPerLine /= 2;
            }

            // Find The Glyph Locations
            tWidth = 0;
            tHeight = 0;
            for(int ci = 0; ci < TotalChars; ) {
                int x = 0, mh = 0;
                tHeight += spacing;
                for(int li = 0; li < charsPerLine && ci < TotalChars; li++, ci++) {
                    x += spacing;

                    gRects[ci].Y = tHeight;
                    gRects[ci].X = x;

                    x += gRects[ci].Width;
                    if(gRects[ci].Height > mh) mh = gRects[ci].Height;
                }
                tHeight += mh;
                x += spacing;

                if(x > tWidth) tWidth = x;
            }
            tHeight += spacing;

            Point tSize = new Point((int)upperPower2((uint)tWidth), (int)upperPower2((uint)tHeight));

            RenderTarget2D rt = new RenderTarget2D(g, tSize.X, tSize.Y, false, SurfaceFormat.Color, DepthFormat.None);
            g.SetRenderTarget(rt);
            g.Clear(Color.Transparent);
            batch.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied);
            for(int i = 0; i < TotalChars; i++) {
                batch.DrawString(f, new string((char)(CharStart + i), 1), new Vector2(gRects[i].X, gRects[i].Y), Color.White);
            }
            batch.End();
            g.SetRenderTarget(null);

            using(FileStream fImage = File.Create(imageFile)) {
                rt.SaveAsPng(fImage, tSize.X, tSize.Y);
            }
            using(FileStream fData = File.Create(dataFile)) {
                using(StreamWriter s = new StreamWriter(fData)) {
                    s.WriteLine("{0,-8} {1}", tSize.X, tSize.Y);
                    for(int i = 0; i < TotalChars; i++) {
                        s.WriteLine("{0,-8} {1,-8} {2,-8} {3,-8} {4}", gRects[i].X, gRects[i].Y, gRects[i].Width, gRects[i].Height, i + CharStart);
                    }
                }
            }
        } 
        #endregion

        private readonly UIGlyph[] glyphs;
        private Texture2D texture;
        public Texture2D Texture {
            get { return texture; }
        }

        public UITexture(GraphicsDevice g, string imageFile, string dataFile) {
            glyphs = new UIGlyph[TotalData];
            using(FileStream fData = File.OpenRead(dataFile)) {
                using(StreamReader s = new StreamReader(fData)) {
                    // Get Texture Size
                    string[] sArr = s.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                    Point texSize = new Point(int.Parse(sArr[0]), int.Parse(sArr[1]));

                    // Read Glyphs
                    while(!s.EndOfStream) {
                        sArr = s.ReadLine().Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
                        if(sArr.Length < 5) continue;

                        Point min = new Point(int.Parse(sArr[0]), int.Parse(sArr[1]));
                        Point max = new Point(int.Parse(sArr[2]), int.Parse(sArr[3]));
                        max.X += min.X;
                        max.Y += min.Y;
                        int data = int.Parse(sArr[4]);
                        if(data < DataStart || data > DataEnd) continue;
                        glyphs[data - DataStart] = new UIGlyph(min, max, texSize);
                    }
                }
            }
            using(FileStream fImage = File.OpenRead(imageFile)) {
                texture = Texture2D.FromStream(g, fImage);
            }
        }

        public UIGlyph getGlyphChar(char c) {
            return glyphs[c - DataChar];
        }
        public UIGlyph getGlyphPixel() {
            return glyphs[DataPixel - DataStart];
        }
        public UIGlyph getGlyphBorder(int type, int x, int y) {
            return glyphs[(DataBorder + (GlyphsPerBorder * type) + (y * 3 + x)) - DataStart];
        }
        public UIGlyph getGlyphFullBorder(int type) {
            return new UIGlyph(getGlyphBorder(type, 0, 0), getGlyphBorder(type, 2, 2));
        }

        public void writeString(string s, Vector2 start, float height, Vector4 tint, out VertexUI[] verts, out int[] inds) {
            verts = new VertexUI[s.Length * 4];
            inds = new int[s.Length * 6];
            int vi = 0, ii = 0;
            foreach(char c in s) {
                inds[ii++] = vi;
                inds[ii++] = vi + 1;
                inds[ii++] = vi + 2;
                inds[ii++] = vi + 2;
                inds[ii++] = vi + 1;
                inds[ii++] = vi + 3;

                UIGlyph g = getGlyphChar(c);
                Vector2 cs = new Vector2(height * g.AspectRatio, height);

                verts[vi++] = new VertexUI(start, g.TopLeft, tint);
                verts[vi++] = new VertexUI(new Vector2(start.X + cs.X, start.Y), g.TopRight, tint);
                verts[vi++] = new VertexUI(new Vector2(start.X, start.Y + cs.Y), g.BottomLeft, tint);
                verts[vi++] = new VertexUI(start + cs, g.BottomRight, tint);
                start.X += cs.X;
            }
        }
    }
}
