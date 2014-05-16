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
            // Process event queue if there is any
            if(ButtonQueue.Count > 0 && CurrentButton == null) {
                CurrentButton = ButtonQueue.Dequeue();
                QueueTimer = CurrentButton.QueueTime;
            }
        }

        public override void ApplyAction(GameState g, float dt) {
            if (CurrentButton != null) {
                // If The Unit Is Still Being Produced
                if (QueueTimer != float.MaxValue && QueueTimer > 0) {
                    QueueTimer -= dt;
                }
                // If Finished Building The Unit
                if (QueueTimer <= 0) {
                    CurrentButton.OnQueueFinished(g);
                    QueueTimer = float.MaxValue;
                    CurrentButton = null;
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