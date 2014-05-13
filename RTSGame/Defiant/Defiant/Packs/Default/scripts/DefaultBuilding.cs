using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RTS.UIInput.BuildingInput;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Interfaces;

// TODO: Verify
namespace RTS.Default.Building {
    public class Action : ACBuildingActionController {
        public Queue<int> unitQueue = new Queue<int>();
        public Queue<EventType> eventQueue = new Queue<EventType>();
        private EventType currentEvent = EventType.None;
        public float buildTime; // How Long It Takes To Finish Producing The Unit
        private int unit = -1; // Unit To Be Produced

        public override void Init(GameState s, GameplayController c) {

        }

        public override void DecideAction(GameState g, float dt) {
            // Process event queue if there is any
            if(eventQueue.Count > 0 && currentEvent == EventType.None) {
                currentEvent = eventQueue.Dequeue();

                switch(currentEvent) {
                    // Production event
                    case EventType.Production:
                        if(unit < 0 && unitQueue.Count > 0) {
                            unit = unitQueue.Dequeue();
                            buildTime = building.Team.Race.Units[unit].BuildTime;
                        }
                        break;
                    // TODO: Implement
                    case EventType.Research:
                        break;
                    default:
                        break;
                }
            }
        }

        public override void ApplyAction(GameState g, float dt) {
            switch(currentEvent) {
                case EventType.Production:
                    // If The Unit Is Still Being Produced
                    if(unit >= 0) {
                        buildTime -= dt;
                        // If Finished Building The Unit
                        if(buildTime < 0) {
                            building.Team.AddUnit(unit, building.GridPosition);
                            buildTime = 0;
                            unit = -1;
                            currentEvent = EventType.None;
                        }
                    }
                    break;
            }
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }
}