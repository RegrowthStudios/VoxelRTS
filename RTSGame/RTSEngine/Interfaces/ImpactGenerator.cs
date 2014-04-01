using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Interfaces {
    public interface ImpactGenerator {
        Vector2 Position { get; set; }
        int ImpactAmount { get; set; }
        event Action<Vector2, int> GenerateImpact;
    }
}
