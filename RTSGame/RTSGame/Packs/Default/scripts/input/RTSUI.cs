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
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using RTSEngine.Graphics;
using RTSEngine.Data.Parsers;

namespace RTS.Input {
    public class RTSUI : IDisposable {
        private UICRTS uic;

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

        public RectWidget Minimap {
            get;
            private set;
        }
        public RTSUISelectionPanel SelectionPanel {
            get;
            private set;
        }
        public RTSUIBuildingButtonPanel BBPanel {
            get;
            private set;
        }
        public RTSUITeamDataPanel TeamDataPanel {
            get;
            private set;
        }
        public RTSUIBuildPanel BuildingPanel {
            get;
            private set;
        }

        public RTSUI(RTSRenderer renderer, string uicFile) {
            uic = ZXParser.ParseFile(uicFile, typeof(UICRTS)) as UICRTS;

            SpriteFont font = renderer.CreateFont(uic.Font, uic.FontSize);
            wrButtonPanel = new WidgetRenderer(renderer.G, font);
            wrMain = new WidgetRenderer(renderer.G, font);

            BuildBounds(renderer, uic.BottomPanelHeight, uic.BottomPanelColor);
            BuildMinimap(renderer, uic.MinimapBorder);
            BuildBBPanel(renderer);
            BuildBuildingPanel();
            BuildSelectionPanel(renderer);
            BuildTeamDataPanel();
        }
        public void Dispose() {
            wrButtonPanel.Dispose();
            wrMain.Dispose();
            TeamDataPanel.Dispose();
            SelectionPanel.Dispose();
            BuildingPanel.Dispose();
        }

        private void BuildBounds(RTSRenderer renderer, int ph, Color c) {
            rectBounds = new RectWidget(wrMain);
            rectBounds.Color = Color.Transparent;
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
            Minimap.Parent = rectBounds;
        }
        private void BuildBBPanel(RTSRenderer renderer) {
            BBPanel = new RTSUIBuildingButtonPanel(wrMain, 3, 4, 40, 4);
            BBPanel.BackPanel.Parent = rectBounds;
            BBPanel.BackPanel.AlignY = Alignment.BOTTOM;
            BBPanel.BackPanel.OffsetAlignY = Alignment.BOTTOM;
            BBPanel.BackPanel.Offset = new Point(4, 0);
            BBPanel.IconLibrary = renderer.IconLibrary;
        }
        private void BuildBuildingPanel() {
            BuildingPanel = new RTSUIBuildPanel(wrMain, 180, 26, 5, 12, 24);
            BuildingPanel.Parent = BBPanel.BackPanel;
        }
        private void BuildSelectionPanel(RTSRenderer renderer) {
            SelectionPanel = new RTSUISelectionPanel(wrMain, uic.SelectionRows, uic.SelectionColumns, uic.SelectionIconSize, uic.SelectionIconBuffer);
            SelectionPanel.BackPanel.Parent = rectBounds;
            SelectionPanel.BackPanel.AlignX = Alignment.LEFT;
            SelectionPanel.BackPanel.AlignY = Alignment.BOTTOM;
            SelectionPanel.BackPanel.OffsetAlignX = Alignment.LEFT;
            SelectionPanel.BackPanel.OffsetAlignY = Alignment.BOTTOM;
            SelectionPanel.BackPanel.Offset = new Point(BBPanel.BackPanel.Width + uic.SelectionIconBuffer, 0);
            SelectionPanel.IconLibrary = renderer.IconLibrary;
        }
        private void BuildTeamDataPanel() {
            TeamDataPanel = new RTSUITeamDataPanel(wrMain);
            TeamDataPanel.Width = (rectBounds.Width * 5) / 7;
        }

        public void SetTeam(RTSTeam team) {
            BuildingPanel.Build(team);
            BuildingPanel.Hook();
        }

        public bool Inside(int x, int y) {
            return PanelBottom.Inside(x, y) || BuildingPanel.Inside(x, y) || TeamDataPanel.Inside(x, y);
        }

        public void Draw(RTSRenderer renderer, SpriteBatch batch) {
            var f = new System.Windows.Forms.Form();
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