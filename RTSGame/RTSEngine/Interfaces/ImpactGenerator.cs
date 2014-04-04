using Microsoft.Xna.Framework;
using RTSEngine.Data.Team;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Interfaces {

    public interface ImpactGenerator {
        Vector2 GridPosition { get; set; }
        int Health { get; set; }
        event Action<Vector2, int> GenerateImpact;
        RTSBuildingData Data { get; set; }
    }
}
