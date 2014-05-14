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

        public RTSUIMinimap Minimap {
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
        public RTSUIAlertQueue AlertQueue {
            get;
            private set;
        }

        public RTSUI(RTSRenderer renderer, string uicFile, bool showBuildPanel) {
            uic = ZXParser.ParseFile(uicFile, typeof(UICRTS)) as UICRTS;

            SpriteFont font = renderer.LoadFont(uic.Font);
            wrButtonPanel = new WidgetRenderer(renderer.G, font);
            wrMain = new WidgetRenderer(renderer.G, font);

            BuildBounds(renderer);
            BuildMinimap(renderer, uic.UICMinimap);
            BuildBBPanel(renderer);
            BuildBuildingPanel(showBuildPanel);
            BuildSelectionPanel(renderer);
            BuildTeamDataPanel();
            AlertQueue = new RTSUIAlertQueue(wrMain, uic.UICAlertQueue);
            AlertQueue.WidgetBase.Parent = Minimap.WidgetBase;
        }
        public void Dispose() {
            wrButtonPanel.Dispose();
            wrMain.Dispose();
            Minimap.Dispose();
            TeamDataPanel.Dispose();
            SelectionPanel.Dispose();
            if(BuildingPanel != null) BuildingPanel.Dispose();
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
        private void BuildMinimap(RTSRenderer renderer, UICMinimap uic) {
            Minimap = new RTSUIMinimap(renderer, wrMain, renderer.Minimap.Terrain, null, uic);
            Minimap.WidgetBase.AlignX = Alignment.RIGHT;
            Minimap.WidgetBase.AlignY = Alignment.BOTTOM;
            Minimap.WidgetBase.OffsetAlignX = Alignment.RIGHT;
            Minimap.WidgetBase.OffsetAlignY = Alignment.BOTTOM;
            Minimap.WidgetBase.Parent = rectBounds;
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
        private void BuildBuildingPanel(bool b) {
            if(!b) return;
            BuildingPanel = new RTSUIBuildPanel(wrMain, 180, 26, 5, 12, 24);
            BuildingPanel.Menu.Widget.AlignY = Alignment.BOTTOM;
            BuildingPanel.Menu.Parent = BBPanel.BackPanel;
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
            if(BuildingPanel != null) {
                BuildingPanel.Build(team);
                BuildingPanel.Hook();
            }
        }

        public bool Inside(int x, int y) {
            return
                Minimap.WidgetBase.Inside(x, y) ||
                SelectionPanel.BackPanel.Inside(x, y) ||
                BBPanel.BackPanel.Inside(x, y) ||
                (BuildingPanel == null ? false : BuildingPanel.Inside(x, y)) ||
                TeamDataPanel.Inside(x, y);
        }

        public void Draw(RTSRenderer renderer, SpriteBatch batch) {
            wrMain.Draw(batch);
            Rectangle rMap = new Rectangle(Minimap.MapRect.X, Minimap.MapRect.Y, Minimap.MapRect.Width, Minimap.MapRect.Height);
            batch.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            batch.Draw(renderer.Minimap.TeamMap, rMap, Color.White);
            batch.End();
            renderer.Minimap.DrawCamera(renderer, rMap);
        }
    }
}