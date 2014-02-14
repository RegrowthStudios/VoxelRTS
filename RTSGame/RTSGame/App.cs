using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace RTS {
    public class App : Game {
        protected GraphicsDeviceManager graphics;
        protected SpriteBatch spriteBatch;
        SpriteFont font;
        public App()
            : base() {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.ApplyChanges();
        }

        protected override void Initialize() {
            base.Initialize();
        }
        protected override void LoadContent() {
            Content = new ContentManager(Services, @"Content");
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = Content.Load<SpriteFont>("Font");
            base.LoadContent();
        }
        protected override void UnloadContent() {

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime) {

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "Hello", Vector2.One * 10, Color.White);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
