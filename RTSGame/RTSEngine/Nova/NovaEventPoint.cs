using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NovaLibrary
{
    public class NovaEventPoint : NovaEvent
    {
        int points;
        public NovaEventPoint(int points) : base(NOVA_EVENT.POINT_CHANGE)
        {
            this.points = points;
        }

        public int getPoints()
        {
            return points;
        }
    }
}
