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
            }
        }
        private RectWidget rectBounds;

        private RectWidget rectMapBack;
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

            BuildBounds(renderer);
            BuildMinimap(renderer, uic.MinimapBorder, uic.MinimapSize);
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

        private void BuildBounds(RTSRenderer renderer) {
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
            };
            rectBounds.LayerDepth = 1f;
        }
        private void BuildMinimap(RTSRenderer renderer, int buf, int s) {
            rectMapBack = new RectWidget(wrMain, null);
            rectMapBack.Height = s + buf * 2;
            rectMapBack.Width = rectMapBack.Height;
            rectMapBack.AlignX = Alignment.RIGHT;
            rectMapBack.AlignY = Alignment.BOTTOM;
            rectMapBack.OffsetAlignX = Alignment.RIGHT;
            rectMapBack.OffsetAlignY = Alignment.BOTTOM;
            rectMapBack.Parent = rectBounds;
            rectMapBack.Color = UserConfig.MainScheme.WidgetBorder;

            Minimap = new RectWidget(wrMain, renderer.Minimap.Terrain);
            Minimap.Width = s;
            Minimap.Height = s;
            Minimap.Color = Color.White;
            Minimap.AlignX = Alignment.RIGHT;
            Minimap.AlignY = Alignment.BOTTOM;
            Minimap.Offset = new Point(-buf, -buf);
            Minimap.OffsetAlignX = Alignment.RIGHT;
            Minimap.OffsetAlignY = Alignment.BOTTOM;
            Minimap.Parent = rectMapBack;
        }
        private void BuildBBPanel(RTSRenderer renderer) {
            BBPanel = new RTSUIBuildingButtonPanel(wrMain, uic.BBRows, uic.BBColumns, uic.BBIconSize, uic.BBIconBuffer);
            BBPanel.BackPanel.Texture = renderer.LoadTexture2D(uic.BBTexture);
            BBPanel.BackPanel.Parent = rectBounds;
            BBPanel.BackPanel.AlignY = Alignment.BOTTOM;
            BBPanel.BackPanel.OffsetAlignY = Alignment.BOTTOM;
            BBPanel.BackPanel.Offset = new Point(uic.BBIconBuffer, 0);
            BBPanel.IconLibrary = renderer.IconLibrary;
            BBPanel.BackPanel.Color = UserConfig.MainScheme.WidgetBase;
        }
        private void BuildBuildingPanel() {
            BuildingPanel = new RTSUIBuildPanel(wrMain, 180, 26, 5, 12, 24);
            BuildingPanel.Parent = BBPanel.BackPanel;
        }
        private void BuildSelectionPanel(RTSRenderer renderer) {
            SelectionPanel = new RTSUISelectionPanel(wrMain, uic.SelectionRows, uic.SelectionColumns, uic.SelectionIconSize, uic.SelectionIconBuffer);
            SelectionPanel.BackPanel.Texture = renderer.LoadTexture2D(uic.SelectionTexture);
            SelectionPanel.BackPanel.Parent = BBPanel.BackPanel;
            SelectionPanel.BackPanel.AlignX = Alignment.LEFT;
            SelectionPanel.BackPanel.AlignY = Alignment.BOTTOM;
            SelectionPanel.BackPanel.OffsetAlignX = Alignment.RIGHT;
            SelectionPanel.BackPanel.OffsetAlignY = Alignment.BOTTOM;
            SelectionPanel.BackPanel.Offset = new Point(0, 0);
            SelectionPanel.IconLibrary = renderer.IconLibrary;
            SelectionPanel.BackPanel.Color = UserConfig.MainScheme.WidgetBase;
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
            return
                Minimap.Inside(x, y) ||
                SelectionPanel.BackPanel.Inside(x, y) ||
                BBPanel.BackPanel.Inside(x, y) ||
                BuildingPanel.Inside(x, y) ||
                TeamDataPanel.Inside(x, y);
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

        //private void OnWindowResize() {
        //    PanelBottom.Width = rectBounds.Width;
        //}
    }
}