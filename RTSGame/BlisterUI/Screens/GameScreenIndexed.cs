using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BlisterUI
{
    public abstract class GameScreenIndexed : GameScreen
    {
        public override int Next { get; protected set; }
        public override int Previous { get; protected set; }

        public GameScreenIndexed(int previousScreen, int nextScreen)
        {
            Previous = previousScreen;
            Next = nextScreen;
        }
        public GameScreenIndexed(int index)
            : this(index - 1, index + 1)
        {
        }
    }
}
