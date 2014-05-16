using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;

namespace RTS.Default.Building.Extraction {
    public class Action : ACBuildingActionController {
        public bool Enabled;
        public float ActivityInterval;
        public int ExtractAmount;
        public float currTime;

        public override void SetBuilding(RTSBuilding b) {
            base.SetBuilding(b);
        }

        public override void Init(GameState s, GameplayController c) {
            ActivityInterval = 10;
            ExtractAmount = 10;
            currTime = 0;
            Enabled = true;
        }

        public override void DecideAction(GameState g, float dt) {
        }

        public override void ApplyAction(GameState g, float dt) {
            if (Enabled) {
                currTime += dt;
                if (currTime >= ActivityInterval) {
                    currTime = 0;
                    building.Team.Input.AddEvent(new CapitalEvent(building.Team.Index, ExtractAmount));
                    building.Team.Input.AddEvent(new ImpactEvent(building.Team.Index, building.GridPosition, ExtractAmount));
                }
            }
        }

        public override void Serialize(BinaryWriter s) {
            
        }

        public override void Deserialize(BinaryReader s) {
            
        }
    }

    /*public class Animation : ACBuildingAnimationController {
        public override void SetBuilding(RTSBuilding b) {
            base.SetBuilding(b);
        }

        public override void Init(GameState s, GameplayController c) {
            throw new NotImplementedException();
        }

        public override void Serialize(BinaryWriter s) {
            throw new NotImplementedException();
        }

        public override void Deserialize(BinaryReader s) {
            throw new NotImplementedException();
        }
    }*/
}
