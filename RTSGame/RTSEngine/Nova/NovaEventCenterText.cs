using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NovaLibrary
{
    public class NovaEventCenterText : NovaEvent
    {
        public string Text { get; set; }
        public float ShowTime { get; set; }

        public NovaEventCenterText(string text, float time = 1f)
            : base(NOVA_EVENT.CENTER_TEXT)
        {
            Text = text;
            ShowTime = time;
        }


    }
}
