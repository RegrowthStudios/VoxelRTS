using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlisterUI.Input {
    public sealed class GamePadManager {
        public PlayerIndex Index {
            get;
            private set;
        }

        private GamePadState cGS, pGS;
        public GamePadState Current {
            get { return cGS; }
        }
        public GamePadState Previous {
            get { return pGS; }
        }

        public GamePadManager(PlayerIndex i) {
            Index = i;
            Refresh();
            pGS = cGS;
        }

        public bool IsButtonJustPressed(Buttons button) {
            return cGS.IsButtonDown(button) && pGS.IsButtonUp(button);
        }
        public bool IsButtonJustReleased(Buttons button) {
            return cGS.IsButtonUp(button) && pGS.IsButtonDown(button);
        }

        public void Refresh() {
            pGS = cGS;
            cGS = GamePad.GetState(Index);
        }
    }
}
