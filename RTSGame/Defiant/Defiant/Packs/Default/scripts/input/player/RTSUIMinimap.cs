using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data;
using RTSEngine.Graphics;

namespace RTS.Input {
    public class UICMinimap {
        public int MapSize;
        public int ButtonWidth;
        public int Buffer;
        public Color BaseColor;

        public string PhysicalTexture;
        public Color PhysicalInactiveColor;
        public Color PhysicalActiveColor;

        public string ImpactTexture;
        public Color ImpactInactiveColor;
        public Color ImpactActiveColor;
    }

    public class RTSUIMinimap : IDisposable {
        public RectWidget WidgetBase {
            get;
            private set;
        }

        private RectButton btnPhysical, btnImpact;
        public RectWidget MapRect {
            get;
            private set;
        }

        private Texture2D tPhysical;
        private Texture2D tImpact;

        public RTSUIMinimap(RTSRenderer renderer, WidgetRenderer wr, Texture2D tPhys, Texture2D tImp, UICMinimap uic) {
            tPhysical = tPhys;
            tImpact = tImp;
            
            WidgetBase = new RectWidget(wr);
            WidgetBase.Width = uic.ButtonWidth + uic.MapSize + uic.Buffer * 2;
            WidgetBase.Height = uic.MapSize + uic.Buffer * 2;
            WidgetBase.Color = uic.BaseColor;

            int bh = uic.MapSize / 2;
            btnPhysical = new RectButton(wr, uic.ButtonWidth, bh,
                uic.PhysicalInactiveColor,
                uic.PhysicalActiveColor,
                renderer.LoadTexture2D(uic.PhysicalTexture)
                );
            btnImpact = new RectButton(wr, uic.ButtonWidth, bh,
                uic.ImpactInactiveColor,
                uic.ImpactActiveColor,
                renderer.LoadTexture2D(uic.ImpactTexture)
                );

            btnPhysical.Offset = new Point(uic.Buffer, uic.Buffer);
            btnPhysical.Parent = WidgetBase;
            btnImpact.OffsetAlignY = Alignment.BOTTOM;
            btnImpact.Parent = btnPhysical;

            MapRect = new RectWidget(wr, tPhysical);
            MapRect.Width = uic.MapSize;
            MapRect.Height = uic.MapSize;
            MapRect.Color = Color.White;
            MapRect.OffsetAlignX = Alignment.RIGHT;
            MapRect.Parent = btnPhysical;

            btnPhysical.OnButtonPress += (b, p) => {
                MapRect.Texture = tPhysical;
            };
            btnImpact.OnButtonPress += (b, p) => {
                MapRect.Texture = tImpact;
            };
        }
        public void Dispose() {
            btnPhysical.Dispose();
            btnImpact.Dispose();
        }

        public void Hook() {
            btnPhysical.Hook();
            btnImpact.Hook();
        }
        public void Unhook() {
            btnPhysical.Unhook();
            btnImpact.Unhook();
        }
    }
}