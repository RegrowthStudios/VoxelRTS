using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NovaLibrary.Object
{
    public interface IBoundable
    {
        void CheckBound(Rectangle bounds);
    }
}
