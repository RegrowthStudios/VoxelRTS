using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Controllers;

namespace RTSEngine.Graphics {
    public class DevConsoleView : IDisposable {
        private static readonly Color DEFAULT_BACK_COLOR = new Color(0f, 0f, 0f, 0.7f);
        private static readonly Color DEFAULT_TEXT_COLOR = new Color(0f, 1f, 0f, 1f);
        public static Vector2 TEXT_OFFSET = new Vector2(5f);

        public Color BackColor {
            get;
            set;
        }
        public Color TextColor {
            get;
            set;
        }
        private SpriteFont font;
        private IDisposable fontDisp;
        private Texture2D tPixel;

        public DevConsoleView(GraphicsDevice g) {
            font = XNASpriteFont.Compile(g, "Courier New", 16, out fontDisp);
            tPixel = new Texture2D(g, 1, 1);
            tPixel.SetData(new Color[] { Color.White });
            BackColor = DEFAULT_BACK_COLOR;
            TextColor = DEFAULT_TEXT_COLOR;
        }
        public void Dispose() {
            if(font != null) {
                font = null;
                fontDisp.Dispose();
                fontDisp = null;
            }
            if(tPixel != null) {
                tPixel.Dispose();
                tPixel = null;
            }
        }

        public void Draw(SpriteBatch sb, Vector2 pos) {
            string commands = "";

            var coms = DevConsole.Lines.ToArray();
            foreach(var command in coms) {
                commands += command + "\n";
            }
             
            commands += "\n > " + DevConsole.TypedText;
            Vector2 size = font.MeasureString(commands);

            // Draw Commands
            sb.Draw(tPixel, pos, null, BackColor, 0f, Vector2.Zero, size + TEXT_OFFSET * 2f, SpriteEffects.None, 0f);
            sb.DrawString(font, commands, pos + TEXT_OFFSET, TextColor);
        }
    }
}