using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NovaLibrary.Object;

namespace NovaLibrary
{
    public class NovaEventPowerUp : NovaEvent
    {
        public POWER_UP PowerUp { get; set; }

        public NovaEventPowerUp(POWER_UP powerUp)
            : base(NOVA_EVENT.POWER_UP)
        {
            PowerUp = powerUp;
        }
    }
}
