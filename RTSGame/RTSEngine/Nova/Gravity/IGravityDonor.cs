using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NovaLibrary.Object;

namespace NovaLibrary.Gravity
{
    public interface IGravityDonor : INovaObject
    {
        Vector2 gAcceleration(IGravityRecipient o);
    }
}
