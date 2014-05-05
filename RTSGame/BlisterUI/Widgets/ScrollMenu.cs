using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BlisterUI.Widgets {
    public class ScrollMenu : IDisposable {
        const float TEXT_H_RATIO = 0.95f;
        const int TEXT_X_OFF = 5;

        public RectWidget Widget;
        private RectButton[] buttons;
        private TextWidget[] buttonsText;
        private ScrollBar scrollBar;

        string[] vText;
        int si;

        public bool HasButtons {
            get { return buttons != null && buttons.Length > 0; }
        }
        public bool HasButtonsText {
            get { return buttonsText != null && buttonsText.Length > 0; }
        }
        public int ButtonCount {
            get { return HasButtons ? buttons.Length : 0; }
        }
        public int DataCount {
            get { return vText.Length; }
        }
        public int FullWidth {
            get { return Widget.Width + scrollBar.Width; }
        }

        public BaseWidget Parent {
            get { return Widget.Parent; }
            set { Widget.Parent = value; }
        }
        public Color BaseColor {
            get { return Widget.Color; }
            set {
                Widget.Color = value;
                if(HasButtons) {
                    foreach(var b in buttons)
                        b.InactiveColor = value;
                }
                scrollBar.Color = value;
            }
        }
        public Color HighlightColor {
            get { return HasButtons ? buttons[0].Color : Color.Transparent; }
            set {
                if(HasButtons) {
                    foreach(var b in buttons)
                        b.ActiveColor = value;
                }
                scrollBar.ScrollButton.ActiveColor = value;
            }
        }
        public Color TextColor {
            get { return HasButtonsText ? buttonsText[0].Color : Color.Transparent; }
            set {
                if(HasButtonsText) {
                    foreach(var t in buttonsText)
                        t.Color = value;
                }
            }
        }
        public Color ScrollBarBaseColor {
            get { return scrollBar.ScrollButton.InactiveColor; }
            set { scrollBar.ScrollButton.InactiveColor = value; }
        }

        public ScrollMenu(WidgetRenderer wr, int w, int h, int bCount, int sbw, int sbh) {
            Widget = new RectWidget(wr);
            Widget.Width = w;
            Widget.Height = h * bCount;

            scrollBar = new ScrollBar(wr);
            scrollBar.IsVertical = true;
            scrollBar.Width = sbw;
            scrollBar.ScrollButton.InactiveWidth = scrollBar.Width;
            scrollBar.ScrollButton.InactiveHeight = sbh;
            scrollBar.ScrollButton.ActiveWidth = scrollBar.Width;
            scrollBar.ScrollButton.ActiveHeight = sbh;
            scrollBar.Height = Widget.Height;
            scrollBar.OffsetAlignX = Alignment.RIGHT;
            scrollBar.Parent = Widget;
            scrollBar.ScrollRatio = 0;


            buttons = new RectButton[bCount];
            buttonsText = new TextWidget[buttons.Length];
            for(int i = 0; i < buttons.Length; i++) {
                buttons[i] = new RectButton(wr, Widget.Width, h, Color.Black, Color.White);
                if(i > 0) {
                    buttons[i].Parent = buttons[i - 1];
                    buttons[i].OffsetAlignY = Alignment.BOTTOM;
                    buttons[i].LayerOffset = 0f;
                }
                else {
                    buttons[i].Parent = Widget;
                }
                buttonsText[i] = new TextWidget(wr);
                buttonsText[i].Height = (int)(TEXT_H_RATIO * buttons[i].Height);
                buttonsText[i].Text = "";
                buttonsText[i].Offset = new Point(TEXT_X_OFF, 0);
                buttonsText[i].AlignX = Alignment.LEFT;
                buttonsText[i].AlignY = Alignment.MID;
                buttonsText[i].OffsetAlignX = Alignment.LEFT;
                buttonsText[i].OffsetAlignY = Alignment.MID;
                buttonsText[i].Parent = buttons[i];
            }

            BaseColor = Color.Black;
            HighlightColor = Color.DarkSlateGray;
            TextColor = Color.White;
            ScrollBarBaseColor = Color.Red;
        }
        public void Dispose() {
            Widget.Dispose();
            if(HasButtons) foreach(var b in buttons) b.Dispose();
            if(HasButtonsText) foreach(var b in buttonsText) b.Dispose();
            scrollBar.Dispose();
        }

        public void Hook() {
            if(HasButtons) {
                foreach(var b in buttons) {
                    b.Hook();
                }
            }
            scrollBar.Hook();
            scrollBar.OnScrollValueChanged += ScrollChange;
        }
        public void Unhook() {
            scrollBar.OnScrollValueChanged -= ScrollChange;
            scrollBar.Unhook();
            if(HasButtons) {
                foreach(var b in buttons) {
                    b.Unhook();
                }
            }
        }

        public void Build(string[] options) {
            vText = new string[options.Length];
            options.CopyTo(vText, 0);
            si = -1;
            RefreshVisible();
            Widget.Offset = new Point(-FullWidth, 0);
        }

        void ScrollChange(ScrollBar sb, float r) {
            RefreshVisible();
        }
        public void RefreshVisible() {
            int lo = DataCount - ButtonCount;
            if(si < 0) {
                int mc = Math.Min(DataCount, ButtonCount);
                for(int i = 0; i < mc; i++) {
                    buttonsText[i].Text = vText[i];
                }
                si = 0;
                return;
            }
            if(lo > 0) {
                int nsi = (int)((lo + 1) * scrollBar.ScrollRatio);
                nsi = Math.Max(0, Math.Min(lo, nsi));
                if(si != nsi) {
                    si = nsi;
                    for(int i = 0; i < ButtonCount; i++) {
                        buttonsText[i].Text = vText[i + si];
                    }
                }
            }
        }

        public bool Inside(int x, int y) {
            return Widget.Inside(x, y) || scrollBar.Inside(x, y);
        }
        public string GetSelection(int x, int y) {
            if(!Widget.Inside(x, y)) return null;

            for(int i = 0; i < ButtonCount; i++) {
                if(buttons[i].Inside(x, y))
                    return buttonsText[i].Text;
            }
            return null;
        }
    }
}