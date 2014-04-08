using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTS.Worker.Squad {
    
    public class Action : ACSquadActionController {

        public override void DecideAction(GameState g, float dt) {
            if (squad.TargetingController != null)
                squad.TargetingController.DecideTarget(g, dt);
        }

        public override void ApplyAction(GameState g, float dt) {
            if (squad.TargetingController != null)
                squad.TargetingController.ApplyTarget(g, dt);
        }
    }

    public class Movement : ACSquadMovementController {

        public override void DecideMoves(GameState g, float dt) {
            throw new NotImplementedException();
        }

        public override void ApplyMoves(GameState g, float dt) {
            throw new NotImplementedException();
        }
    }

    public class Target : ACSquadTargetingController {

        // Resource that worker squad is harvesting 
        private RTSBuilding targetResource;
        // The Region that the resource is located in
        private Region targetRegion;

        public void setTargetUnit(RTSUnit u) {
            // Worker squad is given unit to attack
            Target = u;
            targetResource = null;
            targetRegion = null;
        }

        public void setTargetResource (GameState g, RTSBuilding b){
            // Worker squad is given resource to harvest
            targetResource = b;
            Point p = HashHelper.Hash(b.GridPosition, g.IGrid.numCells, g.IGrid.size);
            targetRegion = g.IGrid.Region[p.X, p.Y];
            Target = null;
        }
        public override void DecideTarget(GameState g, float dt) {
            // If currently attacking units, updates targetUnit if there are still units in targetSquad and the current targetUnit is dead
            if (targetSquad != null && targetUnit == null) {
                float minDist = float.MaxValue;
                for (int i = 0; i < targetSquad.Units.Count; i++) {
                    float d = (targetSquad.Units[i].GridPosition - squad.GridPosition).LengthSquared();
                    if (d < minDist) {
                        targetUnit = targetSquad.Units[i];
                        minDist = d;
                    }
                }
            }
            // If currently harvesting, updates targetResource if there are still resources in targetRegion and the current targetResource is dead
            else if (targetRegion != null && targetResource == null) { 
                float minDist = float.MaxValue;
                foreach (var c in targetRegion.Cells) {
                    foreach (var re in g.IGrid.ImpactGenerators[c.X, c.Y]) {
                        RTSBuilding resource = re as RTSBuilding;
                        String name = re.BuildingData.FriendlyName;
                        float d = (re.GridPosition - squad.GridPosition).LengthSquared();
                        if (d < minDist && name.Equals(targetResource.BuildingData.FriendlyName)) {
                            minDist = d;
                            targetResource = re as RTSBuilding;
                        }
                    }
                }
           }
        }

        public override void ApplyTarget(GameState g, float dt) {
            foreach (var unit in squad.Units) {
                if (targetUnit != null)
                    unit.Target = targetUnit;
                else if (targetResource != null)
                    unit.Target = targetResource;
            }
        }

    }

}

namespace RTS.Worker {

    public class Action : ACUnitActionController {

        public override void DecideAction(GameState g, float dt) {
            throw new NotImplementedException();
        }

        public override void ApplyAction(GameState g, float dt) {
            throw new NotImplementedException();
        }
    }

    public class Combat : ACUnitCombatController {

        public override void Attack(GameState g, float dt) {
            throw new NotImplementedException();
        }
    }


    public class Movement : ACUnitMovementController {

    }

    public class Animation : ACUnitAnimationController {

        public override void Update(GameState s, float dt) {
            throw new NotImplementedException();
        }
    }

}
