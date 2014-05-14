using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Interfaces;

// TODO: Verify
namespace RTS.Default.Building {
    public class Action : ACBuildingActionController {
        private GameInputEvent currentEvent;
        public float buildTime; // How Long It Takes To Finish Producing The Unit

        public override void Init(GameState s, GameplayController c, object args) {
            EventQueue = new Queue<GameInputEvent>();
            buildTime = 0;
            currentEvent = null;
        }

        public override void DecideAction(GameState g, float dt) {
            // Process event queue if there is any
            if(EventQueue.Count > 0 && currentEvent == null) {
                currentEvent = EventQueue.Dequeue();
                switch(currentEvent.Action) {
                    // Production event
                    case GameEventType.SpawnUnit:
                        SpawnUnitEvent spawnUnit = currentEvent as SpawnUnitEvent;
                        buildTime = building.Team.Race.Units[spawnUnit.Type].BuildTime;
                        break;
                }
            }
        }

        public override void ApplyAction(GameState g, float dt) {
            if (currentEvent != null) {
                switch (currentEvent.Action) {
                    case GameEventType.SpawnUnit:
                        // If The Unit Is Still Being Produced
                        if (buildTime > 0) {
                            buildTime -= dt;
                            // If Finished Building The Unit
                            if (buildTime < 0) {
                                building.Team.Input.AddEvent(currentEvent);
                                buildTime = 0;
                                currentEvent = null;
                            }
                        }
                        break;
                }
            }
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }

    /*public class Animation : ACBuildingAnimationController {

        public override void SetBuilding(RTSEngine.Data.Team.RTSBuilding b) {
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