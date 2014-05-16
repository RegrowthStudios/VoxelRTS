using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Interfaces;

namespace RTS.Default.Building {
    public class Action : ACBuildingActionController {
        
        public override void Init(GameState s, GameplayController c, object args) {
            ButtonQueue = new Queue<ACBuildingButtonController>();
            QueueTimer = float.MaxValue;
            CurrentButton = null;
            QueueCap = 5;
        }

        public override void DecideAction(GameState g, float dt) {
        }
        public override void ApplyAction(GameState g, float dt) {
            if (CurrentButton != null) {
                // If The Unit Is Still Being Produced
                if (QueueTimer != float.MaxValue && QueueTimer > 0) {
                    QueueTimer -= dt;
                }
                // If Finished Building The Unit
                if(QueueTimer <= 0 && CurrentButton.CanFinish(g)) {
                    CurrentButton.OnQueueFinished(g);
                    QueueTimer = float.MaxValue;
                    CurrentButton = null;
                }
            }

            // Add New Buttons To The Queue
            for(int i = 0; i < building.ButtonControllers.Count; i++) {
                int ec = building.ButtonControllers[i].GetEnqueueCount();
                while(ec > 0) {
                    ButtonQueue.Enqueue(building.ButtonControllers[i]);
                    ec--;
                }
            }

            // Get New Button
            if(CurrentButton == null && ButtonQueue.Count > 0) {
                CurrentButton = ButtonQueue.Dequeue();
                QueueTimer = CurrentButton.QueueTime;
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