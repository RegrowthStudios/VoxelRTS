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

    public class RTSUI : IDisposable {
        private WidgetRenderer wrButtonPanel, wrMain;

        public Rectangle WindowSize {
            set {
                rectBounds.Width = value.Width;
                rectBounds.Height = value.Height;
                OnWindowResize();
            }
        }
        private RectWidget rectBounds;
        public RectWidget PanelBottom {
            get;
            private set;
        }

        private Texture2D tTransparent;
        private RectButton[,] btnPanel;
        private Action<GameState>[,] fPanel;
        private LinkedList<Point> pressed;

        public RectWidget Minimap {
            get;
            private set;
        }
        public RTSUISelectionPanel SelectionPanel {
            get;
            private set;
        }
        public RTSUITeamDataPanel TeamDataPanel {
            get;
            private set;
        }

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
            BuildSelectionPanel(renderer);
            BuildTeamDataPanel();
            SelectionPanel.IconLibrary = renderer.IconLibrary;
        }
        public void Dispose() {
            foreach(var b in btnPanel)
                b.Dispose();
            tTransparent.Dispose();
            wrButtonPanel.Dispose();
            wrMain.Dispose();
            TeamDataPanel.Dispose();
            SelectionPanel.Dispose();
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

            PanelBottom = new RectWidget(wrMain);
            PanelBottom.Color = c;
            PanelBottom.Width = rectBounds.Width;
            PanelBottom.Height = ph;
            PanelBottom.Offset = new Point(0, 0);
            PanelBottom.OffsetAlignY = Alignment.BOTTOM;
            PanelBottom.OffsetAlignX = Alignment.MID;
            PanelBottom.AlignX = Alignment.MID;
            PanelBottom.AlignY = Alignment.BOTTOM;
            PanelBottom.Parent = rectBounds;
        }
        private void BuildMinimap(RTSRenderer renderer, int buf) {
            int s = PanelBottom.Height - buf * 2;
            Minimap = new RectWidget(wrMain, renderer.Minimap.Terrain);
            Minimap.Width = s;
            Minimap.Height = s;
            Minimap.Color = Color.White;
            Minimap.AlignX = Alignment.RIGHT;
            Minimap.AlignY = Alignment.BOTTOM;
            Minimap.Offset = new Point(-buf, -buf);
            Minimap.OffsetAlignX = Alignment.RIGHT;
            Minimap.OffsetAlignY = Alignment.BOTTOM;
            Minimap.Parent = PanelBottom;
        }
        private void BuildSelectionPanel(RTSRenderer renderer) {
            SelectionPanel = new RTSUISelectionPanel(wrMain, 2, 4, 64, 4);
            SelectionPanel.BackPanel.Parent = Minimap;
            SelectionPanel.BackPanel.AlignX = Alignment.RIGHT;
            SelectionPanel.BackPanel.Offset = new Point(-4, 0);
            SelectionPanel.LayerDepth = PanelBottom.LayerDepth - 0.01f;
        }
        private void BuildTeamDataPanel() {
            TeamDataPanel = new RTSUITeamDataPanel(wrMain);
            TeamDataPanel.Width = (rectBounds.Width * 5) / 7;
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

        public void Draw(RTSRenderer renderer, SpriteBatch batch) {
            wrMain.Draw(batch);
            Rectangle rMap = new Rectangle(Minimap.X, Minimap.Y, Minimap.Width, Minimap.Height);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            batch.Draw(renderer.Minimap.TeamMap, rMap, Color.White);
            batch.End();
            renderer.Minimap.DrawCamera(renderer, new Rectangle(Minimap.X, Minimap.Y, Minimap.Width, Minimap.Height));
        }

        private void OnWindowResize() {
            PanelBottom.Width = rectBounds.Width;
        }
    }
}