using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using Microsoft.Xna.Framework;
using RTSEngine.Data;

namespace RTS {
    public class UICLEMenu {
        public int TextHeight;
        public Point TextInputBufferSize;

        public int WidgetSpacing;

        public Color CaretColor;

        public string DefaultMapLocText;
        public int DefaultWidth;
        public int DefaultHeight;

        public UICLEMenu() {
            TextHeight = 16;
            TextInputBufferSize = new Point(180, 24);
            WidgetSpacing = 10;
            CaretColor = Color.Red;

            DefaultMapLocText = @"Default\maps\[Name Here]";
            DefaultWidth = 100;
            DefaultHeight = 100;
        }
    }

    public class LEMenu {
        public RectWidget WidgetBase {
            get;
            private set;
        }

        private RectButton[] btns;
        private TextWidget[] txts;
        private TextInputWidget[] tInputs;

        public RectButton SaveButton {
            get { return btns[3]; }
        }
        public RectButton LoadButton {
            get { return btns[4]; }
        }

        public string MapLocation {
            get { return tInputs[0].Text; }
            set { tInputs[0].Text = value; }
        }
        public int MapWidth {
            get {
                int v;
                if(int.TryParse(tInputs[1].Text, out v)) return v;
                return uic.DefaultWidth;
            }
            set {
                tInputs[1].Text = value.ToString();
            }
        }
        public int MapHeight {
            get {
                int v;
                if(int.TryParse(tInputs[2].Text, out v)) return v;
                return uic.DefaultHeight;
            }
            set {
                tInputs[2].Text = value.ToString();
            }
        }

        public bool IsActive {
            get;
            private set;
        }

        private UICLEMenu uic;

        public LEMenu(WidgetRenderer wr, UICLEMenu _uic) {
            uic = _uic;
            WidgetBase = new RectWidget(wr);
            WidgetBase.Color = UserConfig.MainScheme.WidgetBase;
            WidgetBase.Width = uic.TextInputBufferSize.X + uic.WidgetSpacing * 2;

            btns = new RectButton[5];
            for(int i = 0; i < btns.Length; i++) {
                btns[i] = new RectButton(wr,
                    i == 0 ? uic.TextInputBufferSize.X : (uic.TextInputBufferSize.X - uic.WidgetSpacing) / 2,
                    uic.TextInputBufferSize.Y,
                    UserConfig.MainScheme.WidgetInactive,
                    UserConfig.MainScheme.WidgetActive
                    );
            }
            txts = new TextWidget[3];
            for(int i = 0; i < txts.Length; i++) {
                txts[i] = new TextWidget(wr);
                txts[i].Height = uic.TextHeight;
                txts[i].Color = UserConfig.MainScheme.Text;
                txts[i].OffsetAlignX = Alignment.MID;
                txts[i].AlignX = Alignment.MID;
                if(i != 0) {
                    txts[i].OffsetAlignY = Alignment.MID;
                    txts[i].AlignY = Alignment.MID;
                }
            }
            txts[0].Text = "Map Name";
            txts[1].Text = "Save";
            txts[2].Text = "Load";

            tInputs = new TextInputWidget[3];
            for(int i = 0; i < tInputs.Length; i++) {
                tInputs[i] = new TextInputWidget(wr);
                tInputs[i].Height = uic.TextHeight;
                tInputs[i].Color = UserConfig.MainScheme.Text;
                tInputs[i].Caret.Width = 1;
                tInputs[i].Caret.Color = uic.CaretColor;
                tInputs[i].OffsetAlignX = Alignment.MID;
                tInputs[i].OffsetAlignY = Alignment.MID;
                tInputs[i].AlignX = Alignment.MID;
                tInputs[i].AlignY = Alignment.MID;
            }
            MapLocation = uic.DefaultMapLocText;
            MapWidth = uic.DefaultWidth;
            MapHeight = uic.DefaultHeight;


            txts[0].Parent = WidgetBase;
            txts[0].Offset = new Point(0, uic.WidgetSpacing);

            btns[0].OffsetAlignX = Alignment.MID;
            btns[0].OffsetAlignY = Alignment.BOTTOM;
            btns[0].Offset = new Point(0, uic.WidgetSpacing);
            btns[0].AlignX = Alignment.MID;
            btns[0].Parent = txts[0];
            btns[1].OffsetAlignX = Alignment.LEFT;
            btns[1].OffsetAlignY = Alignment.BOTTOM;
            btns[1].Offset = new Point(0, uic.WidgetSpacing);
            btns[1].AlignX = Alignment.LEFT;
            btns[1].Parent = btns[0];
            btns[2].OffsetAlignX = Alignment.RIGHT;
            btns[2].OffsetAlignY = Alignment.BOTTOM;
            btns[2].Offset = new Point(0, uic.WidgetSpacing);
            btns[2].AlignX = Alignment.RIGHT;
            btns[2].Parent = btns[0];
            for(int i = 0; i < 3; i++)
                tInputs[i].Parent = btns[i];
            for(int i = 0; i < 2; i++) {
                btns[i + 3].OffsetAlignY = Alignment.BOTTOM;
                btns[i + 3].Offset = new Point(0, uic.WidgetSpacing);
                btns[i + 3].Parent = btns[i + 1];
                txts[i + 1].Parent = btns[i + 3];
            }
            WidgetBase.Height = btns[4].Y + btns[4].Height + uic.WidgetSpacing;

            for(int i = 0; i < 3; i++) {
                btns[i].OnButtonPress += GetInputActivator(i);
            }
        }
        public void Dispose() {
            foreach(var w in btns) w.Dispose();
            foreach(var w in txts) w.Dispose();
            foreach(var w in tInputs) w.Dispose();
            WidgetBase.Dispose();
        }

        public void Activate() {
            IsActive = true;
            foreach(var w in btns) w.Hook();
        }
        public void Deactivate() {
            IsActive = false;
            foreach(var w in btns) {
                w.Unhook();
                w.InactiveColor = UserConfig.MainScheme.WidgetInactive;
            }
            foreach(var w in tInputs) w.DeactivateInput();
        }

        public Action<RectButton, Vector2> GetInputActivator(int i) {
            return (b, pos) => {
                for(int ti = 0; ti < tInputs.Length; ti++) {
                    if(ti != i) {
                        btns[ti].InactiveColor = UserConfig.MainScheme.WidgetInactive;
                        tInputs[ti].DeactivateInput();
                    }
                    else {
                        btns[ti].InactiveColor = UserConfig.MainScheme.WidgetActive;
                        tInputs[i].ActivateInput();
                    }
                }
            };
        }
    }
}