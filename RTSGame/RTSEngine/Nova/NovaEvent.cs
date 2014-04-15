using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NovaLibrary
{
    public enum NOVA_EVENT : byte
    {
        NONE,
        GAME_OVER,
        POWER_UP,
        CENTER_TEXT,

        POINT_CHANGE,
        SINK_HOLE_ADDED
    }

    public class NovaEvent
    {
        public NOVA_EVENT type;
        public NovaEvent(NOVA_EVENT type)
        {
            this.type = type;
        }
    }
}
