using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using BlisterUI.Widgets;
using RTSEngine.Graphics;
using RTSEngine.Data;

namespace RTS.Input {
    public class UICCombatStats {
        public int TextHeight;
        public int TextBuffer;
        public int TextSeparation;
        public int PanelWidth;
        public Color TextColor;
        public Color BaseColor;
    }

    public class RTSUICombatStats : IDisposable {
        public RectWidget WidgetBase {
            get;
            private set;
        }
        public TextWidget[] texts;
        UICCombatStats uic;

        public RTSUICombatStats(WidgetRenderer wr, UICCombatStats _uic) {
            uic = _uic;

            WidgetBase = new RectWidget(wr);
            WidgetBase.Color = uic.BaseColor;
            WidgetBase.Width = uic.PanelWidth;
            WidgetBase.Height = (uic.TextHeight + uic.TextBuffer) * 5 + uic.TextBuffer;
            WidgetBase.LayerDepth = 0.1f;

            texts = new TextWidget[2 * 5];
            string[] s = {
                "Damage",
                "Range",
                "Attack Speed",
                "Armor",
                "Crit. Chance"
            };
            for(int i = 0; i < texts.Length; i++) {
                texts[i] = new TextWidget(wr);
                texts[i].OffsetAlignX = Alignment.MID;
                texts[i].AlignY = Alignment.BOTTOM;
                texts[i].Height = uic.TextHeight;
                texts[i].Color = uic.TextColor;
                if(i % 2 == 0) {
                    texts[i].AlignX = Alignment.RIGHT;
                    texts[i].Offset = new Point(-uic.TextSeparation / 2, (i / 2 + 1) * (uic.TextHeight + uic.TextBuffer));
                    texts[i].Text = s[i / 2];
                }
                else {
                    texts[i].AlignX = Alignment.LEFT;
                    texts[i].Offset = new Point(uic.TextSeparation / 2, (i / 2 + 1) * (uic.TextHeight + uic.TextBuffer));
                }
                texts[i].Parent = WidgetBase;
            }
        }
        public void Dispose() {
            WidgetBase.Dispose();
            foreach(var t in texts) t.Dispose();
        }

        public void SetStats(BaseCombatData cd) {
            texts[1].Text = cd.AttackDamage + "/" + cd.CriticalDamage;
            texts[3].Text = cd.MinRange + "-" + cd.MaxRange;
            texts[5].Text = cd.AttackTimer.ToString();
            texts[7].Text = cd.Armor.ToString();
            texts[9].Text = (cd.CriticalChance * 100f) + "%";
        }
    }
}
