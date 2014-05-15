using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BlisterUI.Widgets;
using Microsoft.Xna.Framework;
using RTSEngine.Data;

namespace RTS.Input {
    public class UICAlertQueue {
        public int AlertDuration;
        public int Width;
        public int Height;
        public int TextHeight;
        public int Buffer;
    }

    public class RTSUIAlertQueue {
        class AlertTuple {
            public int FramesLeft;
            public RectWidget Base;
            public RectWidget Icon;
            public TextWidget Text;

            public AlertTuple(int frames, WidgetRenderer wr, int buf, int w, int h, int th) {
                FramesLeft = frames;

                Base = new RectWidget(wr);
                Base.Color = UserConfig.MainScheme.WidgetBase;
                Base.Width = w;
                Base.Height = h;


                Icon = new RectWidget(wr);
                Icon.Offset = new Point(buf, buf);
                Icon.Color = Color.White;
                Icon.Height = h - buf * 2;
                Icon.Width = Icon.Height;
                Icon.Parent = Base;

                Text = new TextWidget(wr);
                Text.AlignY = Alignment.MID;
                Text.OffsetAlignX = Alignment.RIGHT;
                Text.OffsetAlignY = Alignment.MID;
                Text.Color = UserConfig.MainScheme.Text;
                Text.Offset = new Point(buf, 0);
                Text.Parent = Icon;
                Text.Height = th;
            }
        }

        UICAlertQueue uic;
        WidgetRenderer wr;

        public RectWidget WidgetBase {
            get;
            private set;
        }
        private LinkedList<AlertTuple> lRects;

        public RTSUIAlertQueue(WidgetRenderer _wr, UICAlertQueue _uic) {
            uic = _uic;
            wr = _wr;

            WidgetBase = new RectWidget(wr);
            WidgetBase.Color = Color.Transparent;
            WidgetBase.LayerOffset = 0.01f;
            WidgetBase.OffsetAlignX = Alignment.RIGHT;
            WidgetBase.Offset = new Point(-uic.Width, 0);

            lRects = new LinkedList<AlertTuple>();
        }

        public void OnAlert(string m, AlertLevel l) {
            var at = new AlertTuple(uic.AlertDuration, wr, uic.Buffer, uic.Width, uic.Height, uic.TextHeight);
            if(lRects.Last != null) {
                lRects.Last.Value.Base.Parent = at.Base;
            }
            lRects.AddLast(at);
            at.Base.Parent = WidgetBase;

            at.Text.Text = m;
        }

        public void Update() {
            var node = lRects.First;
            if(node != null) {
                foreach(var d in lRects.ToArray()) {
                    d.FramesLeft--;
                    if(d.Base.Offset.Y > -d.Base.Height)
                        d.Base.Offset = new Point(0, d.Base.Offset.Y - 1);
                }
                if(node.Value.FramesLeft < 0) {
                    lRects.RemoveFirst();
                    node.Value.Base.Parent = null;
                    node.Value.Base.Dispose();
                    node.Value.Icon.Dispose();
                    node.Value.Text.Dispose();
                }
            }
        }
    }
}