using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BlisterUI
{
    public class FalseFirstScreen : GameScreen
    {
        protected int nS;
        protected bool doNext;
        public override int Next
        {
            get
            {
                if (doNext) { return Index; }
                doNext = true;
                return nS;
            }
            protected set { nS = value; }
        }
        public override int Previous { get; protected set; }

        public FalseFirstScreen(int nextScreen)
        {
            doNext = false;
            Next = nextScreen;
            Previous = ScreenList.NoScreen;
        }

        public override void Build()
        {
        }
        public override void Destroy(GameTime gameTime)
        {
        }

        public override void OnEntry(GameTime gameTime)
        {
        }
        public override void OnExit(GameTime gameTime)
        {
        }

        public override void Update(GameTime gameTime)
        {
            State = ScreenState.ChangeNext;
            return;
        }
        public override void Draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(Color.Black);
        }
    }
}
