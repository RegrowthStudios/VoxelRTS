using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using Microsoft.Xna.Framework;

namespace RTSEngine.Graphics {
    public class RTSUITeamDataPanel : IDisposable {
        RectWidget rectBase, rectPopPic, rectCapital;
        TextWidget textPopCur, textPopCap, textCapital, textVictory;

        public BaseWidget Parent {
            get { return rectBase.Parent; }
            set { rectBase.Parent = value; }
        }
        public int Width {
            get { return rectBase.Width; }
            set {
                rectBase.Width = value;
                rectCapital.Width = (rectBase.Width * 2) / 3;
            }
        }

        public Color ColorBase {
            get { return rectBase.Color; }
            set { rectBase.Color = value; }
        }
        public Color ColorPop {
            get { return rectPopPic.Color; }
            set { rectPopPic.Color = value; }
        }
        public Color ColorText {
            get { return textPopCur.Color; }
            set {
                textPopCur.Color = value;
                textPopCap.Color = value;
                textCapital.Color = value;
                textVictory.Color = value;
            }
        }

        public string VictoryText {
            get { return textVictory.Text; }
            set { textVictory.Text = value; }
        }
        public int Population {
            set { textPopCur.Text = "CUR: " + value; }
        }
        public int PopulationCap {
            set { textPopCap.Text = "CAP: " + value; }
        }
        public int Capital {
            set { textCapital.Text = "$ " + value; }
        }

        public RTSUITeamDataPanel(WidgetRenderer wr) {
            rectBase = new RectWidget(wr);
            rectBase.Height = 60;

            rectPopPic = new RectWidget(wr);
            rectPopPic.Height = rectBase.Height - 4;
            rectPopPic.Width = rectPopPic.Height;
            rectPopPic.Offset = new Point(2, 2);
            rectPopPic.Parent = rectBase;

            rectCapital = new RectWidget(wr);
            rectCapital.Height = rectBase.Height / 2;
            rectCapital.OffsetAlignX = Alignment.RIGHT;
            rectCapital.AlignX = Alignment.RIGHT;
            rectCapital.Parent = rectBase;
            rectCapital.Color = Color.Transparent;

            textPopCur = new TextWidget(wr);
            textPopCur.OffsetAlignX = Alignment.RIGHT;
            textPopCur.AlignY = Alignment.BOTTOM;
            textPopCur.OffsetAlignY = Alignment.MID;
            textPopCur.Height = rectBase.Height / 2;
            textPopCur.Parent = rectPopPic;

            textPopCap = new TextWidget(wr);
            textPopCap.OffsetAlignX = Alignment.RIGHT;
            textPopCap.AlignY = Alignment.TOP;
            textPopCap.OffsetAlignY = Alignment.MID;
            textPopCap.Height = rectBase.Height / 2;
            textPopCap.Parent = rectPopPic;

            textCapital = new TextWidget(wr);
            textCapital.AlignX = Alignment.MID;
            textCapital.AlignY = Alignment.MID;
            textCapital.OffsetAlignX = Alignment.MID;
            textCapital.OffsetAlignY = Alignment.MID;
            textCapital.Height = rectCapital.Height;
            textCapital.Parent = rectCapital;

            textVictory = new TextWidget(wr);
            textVictory.AlignX = Alignment.MID;
            textVictory.AlignY = Alignment.TOP;
            textVictory.OffsetAlignX = Alignment.MID;
            textVictory.OffsetAlignY = Alignment.BOTTOM;
            textVictory.Height = rectCapital.Height;
            textVictory.Parent = rectCapital;

            Width = 400;

            ColorBase = new Color(8, 8, 8);
            ColorPop = Color.White;
            ColorText = Color.Red;

            VictoryText = "Victory Condition";
            Capital = 0;
            Population = 0;
            PopulationCap = 0;
        }
        public void Dispose() {
            if(rectBase != null) {
                rectBase.Dispose();
                rectBase = null;
            }
            if(rectPopPic != null) {
                rectPopPic.Dispose();
                rectPopPic = null;
            }
            if(rectCapital != null) {
                rectCapital.Dispose();
                rectCapital = null;
            }
            if(textPopCur != null) {
                textPopCur.Dispose();
                textPopCur = null;
            }
            if(textPopCap != null) {
                textPopCap.Dispose();
                textPopCap = null;
            }
            if(textCapital != null) {
                textCapital.Dispose();
                textCapital = null;
            }
            if(textVictory != null) {
                textVictory.Dispose();
                textVictory = null;
            }
        }
    }
}