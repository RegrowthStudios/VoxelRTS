using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlisterUI.Input {
    public sealed class GamePadManager {
        private PlayerIndex index;
        public PlayerIndex Index {
            get { return index; }
        }

        private GamePadState pGS;
        private GamePadState cGS;
        public GamePadState Current {
            get { return cGS; }
        }
        public GamePadState Previous {
            get { return pGS; }
        }

        public GamePadManager(PlayerIndex index) {
            this.index = index;
            refresh();
            pGS = cGS;
        }

        public bool isButtonJustPressed(Buttons button) {
            return cGS.IsButtonDown(button) && pGS.IsButtonUp(button);
        }
        public bool isButtonJustReleased(Buttons button) {
            return cGS.IsButtonUp(button) && pGS.IsButtonDown(button);
        }

        public void refresh() {
            pGS = cGS;
            cGS = Microsoft.Xna.Framework.Input.GamePad.GetState(index);
        }
    }
}
