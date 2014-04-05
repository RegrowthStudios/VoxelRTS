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
using RTSEngine.Controllers;

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
        private const float LAYER_DELTA = 0.05f;

        private WidgetRenderer wrButtonPanel, wrMain;

        private RectWidget rectBounds, rectBottomPanel;

        private Texture2D tTransparent;
        private RectButton[,] btnPanel;
        private Action<GameState>[,] fPanel;
        private LinkedList<Point> pressed;

        public RectButton ButtonMinimap {
            get;
            private set;
        }
        public event Action<Vector2> OnMinimapClick;

        public int ButtonRows {
            get;
            private set;
        }
        public int ButtonColumns {
            get;
            private set;
        }

        public RTSUI(RTSRenderer renderer, string fontName, int fontSize, int ph) {
            SpriteFont font = renderer.CreateFont(fontName, fontSize);
            wrButtonPanel = new WidgetRenderer(renderer.G, font);
            pressed = new LinkedList<Point>();
            wrMain = new WidgetRenderer(renderer.G, font);

            // Invisible Texture
            tTransparent = renderer.CreateTexture2D(1, 1, SurfaceFormat.Color, false);
            tTransparent.SetData(new Color[] { Color.Transparent });

            BuildBounds(renderer, ph, new Color(20, 20, 20));
            BuildMinimap(renderer, 5);
            ButtonMinimap.Hook();
        }
        public void Dispose() {
            foreach(var b in btnPanel)
                b.Dispose();
            tTransparent.Dispose();
            wrButtonPanel.Dispose();
            wrMain.Dispose();
        }

        private void BuildBounds(RTSRenderer renderer, int ph, Color c) {
            rectBounds = new RectWidget(wrMain, tTransparent);
            rectBounds.Anchor = new Point(0, 0);
            rectBounds.AlignX = Alignment.LEFT;
            rectBounds.AlignY = Alignment.TOP;
            rectBounds.Width = renderer.G.Viewport.Width;
            rectBounds.Height = renderer.G.Viewport.Height;
            renderer.Window.ClientSizeChanged += (sender, args) => {
                rectBounds.Width = renderer.G.Viewport.Width;
                rectBounds.Height = renderer.G.Viewport.Height;
                OnWindowResize();
            };
            rectBounds.LayerDepth = 1f;

            rectBottomPanel = new RectWidget(wrMain);
            rectBottomPanel.Color = c;
            rectBottomPanel.Width = rectBounds.Width;
            rectBottomPanel.Height = ph;
            rectBottomPanel.Offset = new Point(0, 0);
            rectBottomPanel.OffsetAlignY = Alignment.BOTTOM;
            rectBottomPanel.OffsetAlignX = Alignment.MID;
            rectBottomPanel.AlignX = Alignment.MID;
            rectBottomPanel.AlignY = Alignment.BOTTOM;
            rectBottomPanel.Parent = rectBounds;
            rectBottomPanel.LayerDepth = rectBounds.LayerDepth - LAYER_DELTA;
        }
        private void BuildMinimap(RTSRenderer renderer, int buf) {
            int s = rectBottomPanel.Height - buf * 2;
            ButtonHighlightOptions bh1 = new ButtonHighlightOptions(s, s, Color.LightCyan);
            ButtonHighlightOptions bh2 = new ButtonHighlightOptions(s, s, Color.LightGoldenrodYellow);
            ButtonMinimap = new RectButton(wrMain, bh1, bh2, renderer.Minimap.Terrain);
            ButtonMinimap.AlignX = Alignment.RIGHT;
            ButtonMinimap.AlignY = Alignment.BOTTOM;
            ButtonMinimap.Offset = new Point(-s, -s);
            ButtonMinimap.OffsetAlignX = Alignment.RIGHT;
            ButtonMinimap.OffsetAlignY = Alignment.BOTTOM;
            ButtonMinimap.Parent = rectBottomPanel;
            ButtonMinimap.LayerDepth = rectBottomPanel.LayerDepth - LAYER_DELTA;
            ButtonMinimap.OnButtonPress += OnMinimapPress;
        }

        public void BuildButtonPanel(int cols, int rows, int bSize, int bSpacing, Color cInactive, Color cHovered) {
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
                    btnPanel[ix, iy].OnButtonPress += (b, m) => {
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
        public void OnMinimapPress(RectButton obj, Vector2 m) {
            Vector2 r;
            ButtonMinimap.Inside((int)m.X, (int)m.Y, out r);
            r.X = MathHelper.Clamp(r.X, 0, 1);
            r.Y = MathHelper.Clamp(r.Y, 0, 1);
            if(OnMinimapClick != null) {
                OnMinimapClick(r);
            }
        }

        public void Draw(SpriteBatch batch) {
            wrMain.Draw(batch);
        }

        private void OnWindowResize() {
            rectBottomPanel.Width = rectBounds.Width;
        }
    }
}