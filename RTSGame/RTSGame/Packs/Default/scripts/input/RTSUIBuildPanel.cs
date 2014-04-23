using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;

namespace RTSEngine.Graphics {
    public class RTSUIBuildPanel : IDisposable {
        const float TEXT_H_RATIO = 0.95f;
        const int TEXT_X_OFF = 5;
        const int MOVE_SPEED = 15;

        private RectWidget rectBase;
        private RectButton[] buttons;
        private TextWidget[] buttonsText;
        private ScrollBar scrollBar;

        Dictionary<string, RTSBuildingData> buildings;
        string[] vText;
        int si, mi;

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
            get { return buildings.Count; }
        }
        public int FullWidth {
            get { return rectBase.Width + scrollBar.Width; }
        }

        public BaseWidget Parent {
            get { return rectBase.Parent; }
            set { rectBase.Parent = value; }
        }
        public Color BaseColor {
            get { return rectBase.Color; }
            set {
                rectBase.Color = value;
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

        public RTSUIBuildPanel(WidgetRenderer wr, int w, int h, int bCount, int sbw, int sbh) {
            rectBase = new RectWidget(wr);
            rectBase.Width = w;
            rectBase.Height = h * bCount;
            rectBase.AlignY = Alignment.BOTTOM;

            scrollBar = new ScrollBar(wr);
            scrollBar.IsVertical = true;
            scrollBar.Width = sbw;
            scrollBar.ScrollButton.InactiveWidth = scrollBar.Width;
            scrollBar.ScrollButton.InactiveHeight = sbh;
            scrollBar.ScrollButton.ActiveWidth = scrollBar.Width;
            scrollBar.ScrollButton.ActiveHeight = sbh;
            scrollBar.Height = rectBase.Height;
            scrollBar.OffsetAlignX = Alignment.RIGHT;
            scrollBar.Parent = rectBase;
            scrollBar.ScrollRatio = 0;


            buttons = new RectButton[bCount];
            buttonsText = new TextWidget[buttons.Length];
            for(int i = 0; i < buttons.Length; i++) {
                buttons[i] = new RectButton(wr, rectBase.Width, h, Color.Black, Color.White);
                if(i > 0) {
                    buttons[i].Parent = buttons[i - 1];
                    buttons[i].OffsetAlignY = Alignment.BOTTOM;
                    buttons[i].LayerOffset = 0f;
                }
                else {
                    buttons[i].Parent = rectBase;
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
            rectBase.Dispose();
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

        public void Build(RTSTeam team) {
            vText = new string[team.Race.ActiveBuildings.Length];
            buildings = new Dictionary<string, RTSBuildingData>();
            for(int i = 0; i < vText.Length; i++) {
                var bd = team.Race.ActiveBuildings[i];
                buildings.Add(bd.FriendlyName, bd);
                vText[i] = bd.FriendlyName;
            }
            si = -1;
            Array.Sort(vText);
            RefreshVisible();

            team.Input.OnNewSelection += Input_OnNewSelection;
            rectBase.Offset = new Point(-FullWidth, 0);
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
            return rectBase.Inside(x, y) || scrollBar.Inside(x, y);
        }
        public RTSBuildingData GetSelection(int x, int y) {
            if(!rectBase.Inside(x, y)) return null;

            for(int i = 0; i < ButtonCount; i++) {
                if(buttons[i].Inside(x, y)) {
                    if(buildings.ContainsKey(buttonsText[i].Text)) {
                        return buildings[buttonsText[i].Text];
                    }
                    return null;
                }
            }
            return null;
        }

        void Input_OnNewSelection(ACInputController arg1, List<IEntity> arg2) {
            for(int i = 0; i < arg2.Count; i++) {
                RTSUnit u = arg2[i] as RTSUnit;
                if(u != null && u.Team.Index == arg1.TeamIndex && u.Data.IsWorker && u.IsAlive) {
                    mi = MOVE_SPEED;
                    return;
                }
            }
            if(rectBase.Offset.X > -FullWidth) mi = -MOVE_SPEED;
            else mi = 0;
        }

        public void Update() {
            if(mi != 0) {
                int x = Math.Max(-FullWidth, Math.Min(0, rectBase.Offset.X + mi));
                if(x == -FullWidth || x == 0) mi = 0;
                rectBase.Offset = new Point(x, rectBase.Offset.Y);
            }
        }
    }
}