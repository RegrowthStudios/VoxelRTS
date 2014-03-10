using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace BlisterUI.Input {
    public class KeyboardManager {
        private static Keys[] allKeys;
        static KeyboardManager() {
            allKeys = (Keys[])Enum.GetValues(typeof(Keys));
        }

        protected KeyboardState pKS;
        protected KeyboardState cKS;
        public KeyboardState Current {
            get { return cKS; }
        }
        public KeyboardState Previous {
            get { return pKS; }
        }

        public KeyboardManager() {
            refresh();
            pKS = cKS;
        }

        public bool isKeyJustPressed(Keys key) {
            return cKS.IsKeyDown(key) && pKS.IsKeyUp(key);
        }
        public List<Keys> allKeysJustPressed() {
            List<Keys> l = new List<Keys>(5);
            foreach(Keys k in allKeys) {
                if(isKeyJustPressed(k)) {
                    l.Add(k);
                }
            }
            return l;
        }
        public bool isKeyJustReleased(Keys key) {
            return cKS.IsKeyUp(key) && pKS.IsKeyDown(key);
        }
        public List<Keys> allKeysJustReleased() {
            List<Keys> l = new List<Keys>(5);
            foreach(Keys k in allKeys) {
                if(isKeyJustReleased(k)) {
                    l.Add(k);
                }
            }
            return l;
        }

        public void refresh() {
            pKS = cKS;
            cKS = Microsoft.Xna.Framework.Input.Keyboard.GetState();
        }
    }
}
