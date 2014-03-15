using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;

namespace BlisterUI {
    public abstract class MainGame : Game {
        protected GraphicsDeviceManager graphics;
        public GraphicsDeviceManager Graphics { get { return graphics; } }
        protected SpriteBatch spriteBatch;
        public SpriteBatch SpriteBatch { get { return spriteBatch; } }

        // List Of Screens And The Current Screen
        protected ScreenList screenList;
        protected IGameScreen screen;

        public MainGame()
            : base() {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.PreferredDepthStencilFormat = DepthFormat.Depth24Stencil8;
            graphics.ApplyChanges();
            Content.RootDirectory = "Content";
        }

        protected abstract void BuildScreenList();
        protected abstract void FullInitialize();
        protected abstract void FullLoad();

        protected override void Initialize() {
            base.Initialize();
            FullInitialize();
        }
        protected override void LoadContent() {
            spriteBatch = new SpriteBatch(GraphicsDevice);

            FullLoad();

            BuildScreenList();
            screen = screenList.Current;
        }
        protected override void UnloadContent() {
        }

        protected override void Update(GameTime gameTime) {
            if(screen != null) {
                switch(screen.State) {
                    case ScreenState.Running:
                        screen.Update(gameTime);
                        break;
                    case ScreenState.ChangeNext:
                        screen.OnExit(gameTime);
                        screen = screenList.Next;
                        if(screen != null) {
                            screen.SetRunning();
                            screen.OnEntry(gameTime);
                        }
                        break;
                    case ScreenState.ChangePrevious:
                        screen.OnExit(gameTime);
                        screen = screenList.Previous;
                        if(screen != null) {
                            screen.SetRunning();
                            screen.OnEntry(gameTime);
                        }
                        break;
                    case ScreenState.ExitApplication:
                        FullQuit(gameTime);
                        return;
                }
                base.Update(gameTime);
            }
            else {
                FullQuit(gameTime);
            }
        }
        protected override void Draw(GameTime gameTime) {
            if(screen != null && screen.State == ScreenState.Running) {
                screen.Draw(gameTime);
            }
            base.Draw(gameTime);
        }

        protected virtual void FullQuit(GameTime gameTime) {
            if(screen != null) {
                screen.OnExit(gameTime);
            }
            screenList.destroy(gameTime);
            Exit();
        }
    }
}