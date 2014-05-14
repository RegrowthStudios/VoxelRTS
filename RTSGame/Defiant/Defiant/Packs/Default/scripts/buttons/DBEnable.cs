using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Interfaces;
using RTS.Default.Building.Extraction;

namespace RTS.Packs.Default.scripts.buttons {
    public class DBEnable : ACBuildingButtonController {
        public override int QueueTime {
            get { return 0; }
        }

        public override void Init(GameState s, GameplayController c, object initArgs) {
            
        }

        public override void OnClick() {
            RTS.Default.Building.Extraction.Action ac = building.ActionController as RTS.Default.Building.Extraction.Action;
            if (ac != null) {
                ac.Enabled = !ac.Enabled;
            }
        }

        public override void DecideAction(GameState s, float dt) { }

        public override void ApplyAction(GameState s, float dt) { }

        public override void OnQueueFinished(GameState s) { }

        public override void Serialize(BinaryWriter s) { }

        public override void Deserialize(BinaryReader s) { }
    }
}
