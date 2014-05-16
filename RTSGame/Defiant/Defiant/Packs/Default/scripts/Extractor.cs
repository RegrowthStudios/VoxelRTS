using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Interfaces;
using RTSEngine.Data;
using RTSEngine.Controllers;
using RTSEngine.Data.Team;
using Microsoft.Xna.Framework;

namespace RTS.Default.Building {
    public class Extractor : ACBuildingActionController {

        List<RTSBuilding> resources;
        int harvestradius;
        Point hStart, hEnd;

        public override void Init(GameState s, GameplayController c, object initArgs) {
            resources = new List<RTSBuilding>();
            Point p = HashHelper.Hash(building.GridStartPos, s.CGrid.numCells, s.CGrid.size);
            harvestradius = 5;
            hStart = new Point(
                Math.Max(0, p.X - harvestradius),
                Math.Max(0, p.Y - harvestradius)
                );
            hEnd = new Point(
                Math.Min(s.CGrid.numCells.X - 1, p.X + harvestradius),
                Math.Min(s.CGrid.numCells.Y - 1, p.Y + harvestradius)
                );
        }

        public override void DecideAction(GameState g, float dt) {
            resources = new List<RTSBuilding>();
            if(!building.IsBuilt) return;

            for(int x = hStart.X; x <= hEnd.X; x++) {
                for(int y = hStart.Y; y <= hEnd.Y; y++) {
                    var b = g.CGrid.EStatic[x, y];
                    if(b != null && b.IsResource && !resources.Contains(b)) {
                        resources.Add(b);
                    }
                }
            }
        }
        public override void ApplyAction(GameState g, float dt) {
            if(g.CurrentFrame % 100 == 0) {
                foreach(var b in resources) {
                    building.Team.Input.AddEvent(new CapitalEvent(
                        building.Team.Index,
                        b.Data.Index == 0 ? 5 : 20
                        ));
                    building.Team.Input.AddEvent(new DamageEvent(
                        building.Team.Index,
                        b.UUID,
                        10
                        ));
                }
            }
        }

        public override void Serialize(System.IO.BinaryWriter s) {
        }
        public override void Deserialize(System.IO.BinaryReader s) {
        }
    }
}
