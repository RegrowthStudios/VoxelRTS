using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI {
    public enum ScreenState {
        Running,
        ExitApplication,
        ChangeNext,
        ChangePrevious
    }

    public interface IGameScreen {
        ScreenState State { get; }
        int Index { get; }
        int Next { get; }
        int Previous { get; }
        MainGame ParentGame { get; }
        GraphicsDevice G { get; }
        ContentManager C { get; }
        SpriteBatch SB { get; }
        Vector2 ViewSize { get; }

        void SetParentGame(MainGame pgame, int index);

        /// <summary>
        /// Called When Screen Is Brought Forth As Main Game Screen
        /// </summary>
        void Build();
        /// <summary>
        /// Called When New Screen Must Be Brought Up
        /// </summary>
        void Destroy(GameTime gameTime);

        void SetRunning();
        void OnEntry(GameTime gameTime);
        void OnExit(GameTime gameTime);

        /// <summary>
        /// Called During Game Update
        /// </summary>
        /// <param name="gameTime">Game Time</param>
        void Update(GameTime gameTime);
        /// <summary>
        /// Called During Game Draw
        /// </summary>
        /// <param name="gameTime"></param>
        void Draw(GameTime gameTime);
    }
    public abstract class GameScreen : IGameScreen {
        public ScreenState State { get; protected set; }
        public int Index { get; private set; }
        public abstract int Next { get; protected set; }
        public abstract int Previous { get; protected set; }
        public MainGame ParentGame {
            get { return game; }
        }
        protected MainGame game;
        public GraphicsDevice G {
            get { return game.GraphicsDevice; }
        }
        public ContentManager C {
            get { return game.Content; }
        }
        public SpriteBatch SB {
            get { return game.SpriteBatch; }
        }
        public Vector2 ViewSize {
            get { return new Vector2(G.Viewport.Width, G.Viewport.Height); }
        }
        public void SetParentGame(MainGame pgame, int index) {
            game = pgame;
            Index = index;
        }

        public abstract void Build();
        public abstract void Destroy(GameTime gameTime);

        public void SetRunning() {
            State = ScreenState.Running;
        }
        public abstract void OnEntry(GameTime gameTime);
        public abstract void OnExit(GameTime gameTime);

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
    }

    #region Type-Clarified Game
    public interface IGameScreen<T> : IGameScreen
        where T : MainGame {
        new T ParentGame { get; }
        void SetParentGame(T pgame, int index);
    }
    public abstract class GameScreen<T> : IGameScreen<T>
        where T : MainGame {
        public ScreenState State { get; protected set; }
        public int Index { get; private set; }
        public abstract int Next { get; protected set; }
        public abstract int Previous { get; protected set; }
        MainGame IGameScreen.ParentGame {
            get { throw new NotImplementedException(); }
        }
        public T ParentGame {
            get { return game; }
        }
        protected T game;
        public GraphicsDevice G {
            get { return game.GraphicsDevice; }
        }
        public ContentManager C {
            get { return game.Content; }
        }
        public SpriteBatch SB {
            get { return game.SpriteBatch; }
        }
        public Vector2 ViewSize {
            get { return new Vector2(G.Viewport.Width, G.Viewport.Height); }
        }

        public void SetParentGame(T pgame, int index) {
            game = pgame;
            Index = index;
        }
        void IGameScreen.SetParentGame(MainGame pgame, int index) {
            game = pgame as T;
            Index = index;
        }

        public abstract void Build();
        public abstract void Destroy(GameTime gameTime);

        public void SetRunning() {
            State = ScreenState.Running;
        }
        public abstract void OnEntry(GameTime gameTime);
        public abstract void OnExit(GameTime gameTime);

        public abstract void Update(GameTime gameTime);
        public abstract void Draw(GameTime gameTime);
    }
    #endregion
}
