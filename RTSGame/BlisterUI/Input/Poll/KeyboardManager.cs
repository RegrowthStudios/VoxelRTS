using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlisterUI.Input {
    public class KeyboardManager {
        private static readonly Keys[] allKeys;
        static KeyboardManager() {
            allKeys = (Keys[])Enum.GetValues(typeof(Keys));
        }

        protected KeyboardState cKS, pKS;
        public KeyboardState Current {
            get { return cKS; }
        }
        public KeyboardState Previous {
            get { return pKS; }
        }

        public IEnumerable<Keys> AllKeysJustPressed {
            get {
                foreach(Keys k in allKeys) {
                    if(IsKeyJustPressed(k))
                        yield return k;
                }
            }
        }
        public IEnumerable<Keys> AllKeysJustReleased {
            get {
                foreach(Keys k in allKeys) {
                    if(IsKeyJustReleased(k))
                        yield return k;
                }
            }
        }

        public KeyboardManager() {
            Refresh();
            pKS = cKS;
        }

        public bool IsKeyJustPressed(Keys key) {
            return cKS.IsKeyDown(key) && pKS.IsKeyUp(key);
        }
        public bool IsKeyJustReleased(Keys key) {
            return cKS.IsKeyUp(key) && pKS.IsKeyDown(key);
        }

        public void Refresh() {
            pKS = cKS;
            cKS = Keyboard.GetState();
        }
    }
}
