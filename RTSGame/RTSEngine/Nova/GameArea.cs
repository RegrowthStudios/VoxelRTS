using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NovaLibrary {
    public static class GameArea {
        public static Rectangle bounds = new Rectangle(0, 0, 800, 480);
        public static Rectangle GameBounds {
            get {
                return new Rectangle(0, 0, bounds.Width, bounds.Height);
            }
        }
    }
}