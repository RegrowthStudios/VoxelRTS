using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BlisterUI.Widgets;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using RTSEngine.Graphics;
using RTSEngine.Data;
using Microsoft.Xna.Framework.Graphics;

namespace RTS.Input {
    public class UICUnitData {
        public string PanelImage;
        public Point PanelSize;
        public Color PanelColor;

        public int IconSize;
        public int IconBuffer;

        public int HealthHeight;
        public Color HealthMinColor;
        public Color HealthMaxColor;

        public int TextHeight;

        public int CombatSize;
        public string CombatImage;
    }

    public class RTSUnitDataPanel : IDisposable {
        public RectWidget WidgetBase {
            get;
            private set;
        }
        RectWidget icon, rectHealthBack, rectHealthFore;
        TextWidget txtName;
        RectButton combatData;
        RTSUICombatStats uiCStats;
        RTSUIHoverPanel uiCSHover;
        UICUnitData uic;

        Dictionary<string, Texture2D> iconLib;
        RTSUnit prevUnit;

        public RTSUnitDataPanel(RTSRenderer renderer, WidgetRenderer wr, UICUnitData _uic, UICCombatStats _uicCS) {
            uic = _uic;
            iconLib = renderer.IconLibrary;

            WidgetBase = new RectWidget(wr, renderer.LoadTexture2D(uic.PanelImage));
            WidgetBase.Width = uic.PanelSize.X;
            WidgetBase.Height = uic.PanelSize.Y;
            WidgetBase.Color = uic.PanelColor;

            icon = new RectWidget(wr);
            icon.Width = uic.IconSize;
            icon.Height = uic.IconSize;
            icon.Color = Color.White;
            icon.Offset = new Point(uic.IconBuffer, uic.IconBuffer);
            icon.Parent = WidgetBase;

            rectHealthBack = new RectWidget(wr);
            rectHealthBack.Width = uic.IconSize;
            rectHealthBack.Height = uic.HealthHeight;
            rectHealthBack.AlignX = Alignment.MID;
            rectHealthBack.OffsetAlignX = Alignment.MID;
            rectHealthBack.OffsetAlignY = Alignment.BOTTOM;
            rectHealthBack.Offset = new Point(0, uic.IconBuffer);
            rectHealthBack.Color = UserConfig.MainScheme.WidgetBorder;
            rectHealthBack.Parent = icon;

            rectHealthFore = new RectWidget(wr);
            rectHealthFore.Width = uic.IconSize;
            rectHealthFore.Height = uic.HealthHeight;
            rectHealthFore.AlignX = Alignment.MID;
            rectHealthFore.AlignY = Alignment.MID;
            rectHealthFore.OffsetAlignX = Alignment.MID;
            rectHealthFore.OffsetAlignY = Alignment.MID;
            rectHealthFore.Offset = new Point(0, 0);
            rectHealthFore.Color = uic.HealthMaxColor;
            rectHealthFore.Parent = rectHealthBack;

            txtName = new TextWidget(wr);
            txtName.Height = uic.TextHeight;
            txtName.OffsetAlignX = Alignment.RIGHT;
            txtName.Offset = new Point(uic.IconBuffer, 0);
            txtName.Color = UserConfig.MainScheme.Text;
            txtName.Parent = icon;

            combatData = new RectButton(wr, uic.CombatSize, uic.CombatSize, Color.Gray, Color.White, renderer.LoadTexture2D(uic.CombatImage));
            combatData.AlignX = Alignment.RIGHT;
            combatData.AlignY = Alignment.BOTTOM;
            combatData.OffsetAlignX = Alignment.RIGHT;
            combatData.OffsetAlignY = Alignment.BOTTOM;
            combatData.Parent = WidgetBase;

            uiCStats = new RTSUICombatStats(wr, _uicCS);
            uiCStats.WidgetBase.AlignY = Alignment.BOTTOM;
            uiCStats.WidgetBase.Anchor = new Point(1000000, 1000000);

            uiCSHover = new RTSUIHoverPanel(combatData, uiCStats.WidgetBase);
        }
        public void Dispose() {
            WidgetBase.Dispose();
            icon.Dispose();
            rectHealthBack.Dispose();
            rectHealthFore.Dispose();
            txtName.Dispose();
            combatData.Dispose();
            uiCStats.Dispose();
        }

        public void Hook() {
            combatData.Hook();
        }
        public void Unhook() {
            combatData.Unhook();
        }

        public void SetData(RTSUnit u) {
            if(prevUnit != null)
                prevUnit.OnDamage -= u_OnDamage;
            prevUnit = u;

            u.OnDamage += u_OnDamage;
            rectHealthFore.Width = (int)(u.GetHealthRatio() * uic.IconSize);
            rectHealthFore.Color = Color.Lerp(uic.HealthMinColor, uic.HealthMaxColor, u.GetHealthRatio());        

            string iconKey = string.Join(".", u.Team.Race.FriendlyName, u.Data.FriendlyName);
            Texture2D t;
            if(iconLib.TryGetValue(iconKey, out t)) {
                icon.Texture = t;
            }

            txtName.Text = u.Data.FriendlyName;
            uiCStats.SetStats(u.Data.BaseCombatData);
        }

        public void Update(int mx, int my) {
            uiCSHover.Update(mx, my);
        }
        
        void u_OnDamage(IEntity arg1, int arg2) {
            rectHealthFore.Width = (int)(arg1.GetHealthRatio() * uic.IconSize);
            rectHealthFore.Color = Color.Lerp(uic.HealthMinColor, uic.HealthMaxColor, arg1.GetHealthRatio());
        }
    }
}
