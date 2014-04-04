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
        private WidgetRenderer wrLeftPanel, wrProfile;
        private Texture2D tTransparent;
        private RectButton[,] btnLeft;
        public int ButtonRows {
            get;
            private set;
        }
        public int ButtonColumns {
            get;
            private set;
        }

        public RTSUI(GraphicsDevice g, SpriteFont font) {
            wrLeftPanel = new WidgetRenderer(g, font);
            wrProfile = new WidgetRenderer(g, font);

            tTransparent = new Texture2D(g, 1, 1);
            tTransparent.SetData(new Color[] { Color.Transparent });
        }
        public void Dispose() {
            tTransparent.Dispose();
            wrLeftPanel.Dispose();
            wrProfile.Dispose();
        }

        private void BuildLeftPanel(int cols, int rows, int bSize, int bSpacing, Color cInactive, Color cHovered) {
            ButtonRows = rows;
            ButtonColumns = cols;

            ButtonHighlightOptions bh1 = new ButtonHighlightOptions(bSize, bSize, cInactive);
            ButtonHighlightOptions bh2 = new ButtonHighlightOptions(bSize, bSize, cHovered);
            btnLeft = new RectButton[ButtonColumns, ButtonRows];
            for(int iy = 0; iy < ButtonRows; iy++) {
                for(int ix = 0; ix < ButtonColumns; ix++) {
                    btnLeft[ix, iy] = new RectButton(wrLeftPanel, bh1, bh2);
                    if(ix > 0) {
                        btnLeft[ix, iy].OffsetAlignX = Alignment.RIGHT;
                        btnLeft[ix, iy].OffsetAlignY = Alignment.TOP;
                        btnLeft[ix, iy].Offset = new Point(bSpacing, 0);
                        btnLeft[ix, iy].Parent = btnLeft[ix - 1, iy];
                    }
                    else if(iy > 0) {
                        btnLeft[ix, iy].OffsetAlignX = Alignment.LEFT;
                        btnLeft[ix, iy].OffsetAlignY = Alignment.BOTTOM;
                        btnLeft[ix, iy].Offset = new Point(0, bSpacing);
                        btnLeft[ix, iy].Parent = btnLeft[ix, iy - 1];
                    }
                }
            }
        }

        public void SetButtons(IEnumerable<RTSUIButton> buttons) {

        }
        public void ClearButtons() {

        }
    }
}