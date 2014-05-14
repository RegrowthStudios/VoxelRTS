using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Controllers;
using RTSEngine.Interfaces;
using Microsoft.Xna.Framework;

namespace RTS.Default.Buttons.Spawn {
    public abstract class ACUnit : ACBuildingButtonController {
        public abstract int UnitType {
            get;
        }
        public override int QueueTime {
            get { return building.Team.Race.Units[UnitType].BuildTime; }
        }

        public override void Init(GameState s, GameplayController c, object args) {
        }

        public override void OnQueueFinished(GameState s) {
            building.Team.Input.AddEvent(new SpawnUnitEvent(
                building.Team.Index,
                UnitType,
                building.GridPosition
                ));
        }
        public override void OnClick() {
            if(building.Team.Capital >= building.Team.Race.Units[UnitType].CapitalCost)
                Enqueue();
        }

        public override void DecideAction(GameState s, float dt) {
        }
        public override void ApplyAction(GameState s, float dt) {
        }

        public override void Serialize(BinaryWriter s) {
        }
        public override void Deserialize(BinaryReader s) {
        }
    }

    public class Worker : ACUnit {
        public override int UnitType {
            get { return 0; }
        }
    }
    public class Soldier : ACUnit {
        public override int UnitType {
            get { return 1; }
        }
    }
    public class Heavy : ACUnit {
        public override int UnitType {
            get { return 2; }
        }
    }
    public class Armored : ACUnit {
        public override int UnitType {
            get { return 3; }
        }
    }
}