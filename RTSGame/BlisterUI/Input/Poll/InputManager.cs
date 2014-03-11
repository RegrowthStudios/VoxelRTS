using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlisterUI.Input {
    public sealed class InputManager {
        private KeyboardManager keyManager;
        public KeyboardManager Keyboard {
            get { return keyManager; }
        }

        private MouseManager mouseManager;
        public MouseManager Mouse {
            get { return mouseManager; }
        }

        private int numPads;
        private GamePadManager[] gamePadManagers;
        public GamePadManager[] GamePads {
            get { return gamePadManagers; }
        }

        public InputManager() {
            keyManager = new KeyboardManager();
            mouseManager = new MouseManager();
            gamePadManagers = new GamePadManager[4];
            numPads = 0;
        }

        public void AddGamePad(PlayerIndex index) {
            if(numPads < 4) {
                for(int i = 0; i < numPads; i++) {
                    if(gamePadManagers[i].Index == index) {
                        return;
                    }
                }
                gamePadManagers[numPads] = new GamePadManager(index);
                numPads++;
            }
        }
        public void RemoveGamePad(PlayerIndex index) {
            for(int i = 0; i < numPads; i++) {
                if(gamePadManagers[i].Index == index) {
                    gamePadManagers[i] = null;
                    numPads--;
                    for(; i < numPads; i++) {
                        gamePadManagers[i] = gamePadManagers[i + 1];
                    }
                    return;
                }
            }
        }

        public void Refresh() {
            keyManager.Refresh();
            mouseManager.Refresh();
            for(int i = 0; i < numPads; i++) {
                gamePadManagers[i].Refresh();
            }
        }
    }
}