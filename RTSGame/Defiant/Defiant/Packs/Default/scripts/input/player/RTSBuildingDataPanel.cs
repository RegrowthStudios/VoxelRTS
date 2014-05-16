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
    public class UICBuildingData {
        public string PanelImage;
        public Point PanelSize;
        public Color PanelColor;

        public int IconSize;
        public int IconBuffer;

        public int HealthHeight;
        public Color HealthMinColor;
        public Color HealthMaxColor;

        public int QueueIconMainSize;
        public int QueueIconQueuedSize;
        public Color QueueMinColor;
        public Color QueueMaxColor;
        public Color QueueButtonHighlightColor;

        public int TextHeight;

        public int CombatSize;
        public string CombatImage;
    }

    public class RTSBuildingDataPanel : IDisposable {
        public RectWidget WidgetBase {
            get;
            private set;
        }
        RectWidget icon, rectHealthBack, rectHealthFore, rectQueueBack, rectQueueFore;
        TextWidget txtName;
        UICBuildingData uic;

        Dictionary<string, Texture2D> iconLib;
        RTSBuilding prevBuilding;

        RectButton[] queueButtons;

        public RTSBuildingDataPanel(RTSRenderer renderer, WidgetRenderer wr, UICBuildingData _uic) {
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
            rectHealthFore.Height = rectHealthBack.Height;
            rectHealthFore.AlignX = Alignment.MID;
            rectHealthFore.AlignY = Alignment.MID;
            rectHealthFore.OffsetAlignX = Alignment.MID;
            rectHealthFore.OffsetAlignY = Alignment.MID;
            rectHealthFore.Offset = new Point(0, 0);
            rectHealthFore.Color = uic.HealthMaxColor;
            rectHealthFore.Parent = rectHealthBack;

            queueButtons = new RectButton[6];
            queueButtons[0] = new RectButton(wr, uic.QueueIconMainSize, uic.QueueIconMainSize, Color.White, uic.QueueButtonHighlightColor);
            queueButtons[0].AlignY = Alignment.BOTTOM;
            queueButtons[0].OffsetAlignY = Alignment.BOTTOM;
            queueButtons[0].OffsetAlignX = Alignment.RIGHT;
            queueButtons[0].Offset = new Point(uic.IconBuffer, 0);
            queueButtons[0].Parent = rectHealthBack;
            for(int i = 1; i < 6; i++) {
                queueButtons[i] = new RectButton(wr, uic.QueueIconQueuedSize, uic.QueueIconQueuedSize, Color.White, uic.QueueButtonHighlightColor);
                queueButtons[i].AlignY = Alignment.BOTTOM;
                queueButtons[i].OffsetAlignY = Alignment.BOTTOM;
                queueButtons[i].OffsetAlignX = Alignment.RIGHT;
                queueButtons[i].Parent = queueButtons[i - 1];
            }

            rectQueueBack = new RectWidget(wr);
            rectQueueBack.Width = uic.QueueIconQueuedSize * 5;
            rectQueueBack.Height = uic.QueueIconMainSize - uic.QueueIconQueuedSize;
            rectQueueBack.OffsetAlignX = Alignment.RIGHT;
            rectQueueBack.Color = UserConfig.MainScheme.WidgetBorder;
            rectQueueBack.Parent = queueButtons[0];

            rectQueueFore = new RectWidget(wr);
            rectQueueFore.Width = uic.IconSize;
            rectQueueFore.Height = rectQueueBack.Height;
            rectQueueFore.AlignY = Alignment.MID;
            rectQueueFore.OffsetAlignY = Alignment.MID;
            rectQueueFore.Offset = new Point(0, 0);
            rectQueueFore.Color = uic.QueueMaxColor;
            rectQueueFore.Parent = rectQueueBack;

            txtName = new TextWidget(wr);
            txtName.Height = uic.TextHeight;
            txtName.OffsetAlignX = Alignment.RIGHT;
            txtName.Offset = new Point(uic.IconBuffer, 0);
            txtName.Color = UserConfig.MainScheme.Text;
            txtName.Parent = icon;
        }
        public void Dispose() {
            WidgetBase.Dispose();
            icon.Dispose();
            rectHealthBack.Dispose();
            rectHealthFore.Dispose();
            txtName.Dispose();
            rectQueueBack.Dispose();
            rectQueueFore.Dispose();
            foreach(var btn in queueButtons) btn.Dispose();
        }

        public void Hook() {
            foreach(var btn in queueButtons) btn.Hook();
        }
        public void Unhook() {
            foreach(var btn in queueButtons) btn.Unhook();
        }

        public void SetData(RTSBuilding b) {
            if(prevBuilding != null)
                prevBuilding.OnDamage -= u_OnDamage;
            prevBuilding = b;

            b.OnDamage += u_OnDamage;
            rectHealthFore.Width = (int)(b.GetHealthRatio() * uic.IconSize);
            rectHealthFore.Color = Color.Lerp(uic.HealthMinColor, uic.HealthMaxColor, b.GetHealthRatio());

            Texture2D t;
            if(iconLib.TryGetValue(b.IconKey, out t)) {
                icon.Texture = t;
            }

            txtName.Text = b.Data.FriendlyName;
            Update();
        }

        public void Update() {
            var b = prevBuilding;
            for(int i = 0; i < queueButtons.Length; i++) {
                queueButtons[i].Texture = null;
                queueButtons[i].Color = Color.Transparent;
            }

            if(b != null && b.ActionController != null) {
                Texture2D t;
                var btns = b.ActionController.ButtonQueue.ToArray();
                var cbtn = b.ActionController.CurrentButton;
                if(cbtn != null) {
                    float r = b.ActionController.QueueTimer / cbtn.QueueTime;
                    rectQueueFore.Width = (int)(r * rectQueueBack.Width);
                    rectQueueFore.Color = Color.Lerp(uic.QueueMinColor, uic.QueueMaxColor, r);
                    if(iconLib.TryGetValue(cbtn.IconKey, out t)) {
                        queueButtons[0].Texture = t;
                        queueButtons[0].Color = Color.White;
                    }
                }
                else {
                    rectQueueFore.Width = rectQueueBack.Width;
                    rectQueueFore.Color = uic.QueueMaxColor;
                }
                if(btns.Length > 0) {
                    for(int i = 0; i < 5 && i < btns.Length; i++) {
                        if(iconLib.TryGetValue(btns[i].IconKey, out t)) {
                            queueButtons[i + 1].Texture = t;
                            queueButtons[i + 1].Color = Color.White;
                        }
                    }
                }
            }
        }

        void u_OnDamage(IEntity arg1, int arg2) {
            rectHealthFore.Width = (int)(arg1.GetHealthRatio() * uic.IconSize);
            rectHealthFore.Color = Color.Lerp(uic.HealthMinColor, uic.HealthMaxColor, arg1.GetHealthRatio());
        }
    }
}