using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;

namespace RTSEngine.Controllers {
    public class GameplayController {
        public float TimePlayed {
            get;
            private set;
        }

        public GameplayController() {
            TimePlayed = 0f;
        }

        public void Update(GameState s, float dt) {
            TimePlayed += dt;
            ApplyInput(s, dt);
            ApplyLogic(s, dt);
        }

        private void ApplyInput(GameState s, float dt) {

        }

        private void ApplyLogic(GameState s, float dt) {

        }

        public void AddInstantiatedData(GameState s) {

        }
    }
}
