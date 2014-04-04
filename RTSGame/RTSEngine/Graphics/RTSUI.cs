using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;
using BlisterUI.Widgets;
using RTSEngine.Data;

namespace RTSEngine.Graphics {
    public struct RTSUIButton {
        public int X, Y;
        public Texture2D Texture;
        public Action<GameState> FOnPress;
    }

    /**
     * RTS UI Menus:
     * When Units Selected: Unit/Type Counts
     * When Unit Selected: Unit/Profile
     * When Building Selected: Building/Profile
     */
    public class RTSUI : IDisposable {
        private WidgetRenderer wrButtonPanel, wrProfile;
        private Texture2D tTransparent;
        private RectButton[,] btnPanel;
        private Action<GameState>[,] fPanel;
        private LinkedList<Point> pressed;

        public int ButtonRows {
            get;
            private set;
        }
        public int ButtonColumns {
            get;
            private set;
        }

        public RTSUI(GraphicsDevice g, SpriteFont font) {
            wrButtonPanel = new WidgetRenderer(g, font);
            pressed = new LinkedList<Point>();
            wrProfile = new WidgetRenderer(g, font);

            tTransparent = new Texture2D(g, 1, 1);
            tTransparent.SetData(new Color[] { Color.Transparent });
        }
        public void Dispose() {
            foreach(var b in btnPanel) b.Dispose();
            tTransparent.Dispose();
            wrButtonPanel.Dispose();
            wrProfile.Dispose();
        }

        private void BuildButtonPanel(int cols, int rows, int bSize, int bSpacing, Color cInactive, Color cHovered) {
            ButtonRows = rows;
            ButtonColumns = cols;

            ButtonHighlightOptions bh1 = new ButtonHighlightOptions(bSize, bSize, cInactive);
            ButtonHighlightOptions bh2 = new ButtonHighlightOptions(bSize, bSize, cHovered);
            btnPanel = new RectButton[ButtonColumns, ButtonRows];
            fPanel = new Action<GameState>[ButtonColumns, ButtonRows];
            for(int iy = 0; iy < ButtonRows; iy++) {
                for(int ix = 0; ix < ButtonColumns; ix++) {
                    btnPanel[ix, iy] = new RectButton(wrButtonPanel, bh1, bh2);
                    if(ix > 0) {
                        btnPanel[ix, iy].OffsetAlignX = Alignment.RIGHT;
                        btnPanel[ix, iy].OffsetAlignY = Alignment.TOP;
                        btnPanel[ix, iy].Offset = new Point(bSpacing, 0);
                        btnPanel[ix, iy].Parent = btnPanel[ix - 1, iy];
                    }
                    else if(iy > 0) {
                        btnPanel[ix, iy].OffsetAlignX = Alignment.LEFT;
                        btnPanel[ix, iy].OffsetAlignY = Alignment.BOTTOM;
                        btnPanel[ix, iy].Offset = new Point(0, bSpacing);
                        btnPanel[ix, iy].Parent = btnPanel[ix, iy - 1];
                    }
                    btnPanel[ix, iy].OnButtonPress += (b) => {
                        pressed.AddLast(new Point(ix, iy));
                    };
                }
            }
            ClearButtons();
        }
        public void SetButtons(IEnumerable<RTSUIButton> buttons) {
            ClearButtons();
            foreach(var b in buttons) {
                btnPanel[b.X, b.Y].Texture = b.Texture;
                fPanel[b.X, b.Y] = b.FOnPress;
                btnPanel[b.X, b.Y].Hook();
            }
        }
        public void ClearButtons() {
            Array.Clear(fPanel, 0, ButtonRows * ButtonColumns);
            for(int iy = 0; iy < ButtonRows; iy++) {
                for(int ix = 0; ix < ButtonColumns; ix++) {
                    btnPanel[ix, iy].Texture = tTransparent;
                    btnPanel[ix, iy].Unhook();
                }
            }
        }

        public void UpdateButtons(GameState s) {
            int c = pressed.Count;
            while(c > 0) {
                Point p = pressed.First.Value;
                pressed.RemoveFirst();
                if(fPanel[p.X, p.Y] != null)
                    fPanel[p.X, p.Y](s);
                c--;
            }
        }
    }
}