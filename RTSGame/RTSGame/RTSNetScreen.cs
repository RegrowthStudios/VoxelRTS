using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using System.Diagnostics;
using System.Threading;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;
using RTSEngine.Interfaces;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Controllers;
using RTSEngine.Data.Parsers;
using RTSEngine.Graphics;
using RTSEngine.Net;

namespace RTS {
    public class RTSNetScreen : GameScreen<App> {
        private NetStateController stateController;
        private GameState state;
        private RTSRenderer renderer;
        private Camera camera;
        private DevConsoleView dcv;
        private PlayerInputController gameInput;

        Vector3 spawnLoc;
        int team, unit;
        bool doAdd;
        SpriteFont sfDebug;

        public override int Next {
            get { return -1; }
            protected set { }
        }
        public override int Previous {
            get { return game.MenuScreen.Index; }
            protected set { }
        }

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            MouseEventDispatcher.OnMousePress += OnMP;
            KeyboardEventDispatcher.OnKeyPressed += OnKP;
            KeyboardEventDispatcher.OnKeyReleased += OnKR;

            dcv = new DevConsoleView(G);
            doAdd = false;
            team = 0;
            unit = 0;

            state = new GameState();

            state = game.LoadScreen.LoadedState;
            camera = game.LoadScreen.LoadedCamera;
            renderer = game.LoadScreen.LoadedRenderer;
            gameInput = state.teams[0].Input as PlayerInputController;
            gameInput.Camera = camera;
            stateController = new NetStateController();
            stateController.Control(state);

            sfDebug = renderer.CreateFont("Courier New", 32);
        }
        public override void OnExit(GameTime gameTime) {
            MouseEventDispatcher.OnMousePress -= OnMP;
            KeyboardEventDispatcher.OnKeyPressed -= OnKP;
            KeyboardEventDispatcher.OnKeyReleased -= OnKR;
            dcv.Dispose();
            dcv = null;
            DevConsole.Deactivate();

            stateController.Dispose();
            camera.Controller.Unhook(game.Window);
            renderer.Dispose();
        }

        public override void Update(GameTime gameTime) {
            // This Tells Us We Are GPU-Bound
            //Thread.Sleep(10);
            renderer.UpdateAnimations(state, (float)game.TargetElapsedTime.TotalSeconds);
        }
        public override void Draw(GameTime gameTime) {
            camera.Update(state.Map, RTSConstants.GAME_DELTA_TIME);
            renderer.Draw(state, RTSConstants.GAME_DELTA_TIME);
            // TODO: Draw UI

            if(DevConsole.IsActivated) {
                SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
                dcv.Draw(SB, Vector2.Zero);
                SB.End();
            }

            // Show FPS
            double fps = gameTime.ElapsedGameTime.TotalSeconds;
            fps = Math.Round(1000.0 / fps) / 1000.0;
            SB.Begin();
            SB.DrawString(sfDebug, fps.ToString(), Vector2.One * 10, Color.White);
            SB.End();

            game.mRenderer.BeginPass(G);
            game.mRenderer.Draw(G);
        }

        public void OnMP(Vector2 p, MouseButton b) {
            if(b == MouseButton.Right) {
                Ray r = renderer.Camera.GetViewRay(p);
                IntersectionRecord rec = new IntersectionRecord();
                if(state.Map.BVH.Intersect(ref rec, r)) {
                    spawnLoc = r.Position + r.Direction * rec.T;
                    if(doAdd)
                        DevConsole.AddCommand(string.Format("spawn [{0},{1},{2},{3},{4}]", team, unit, 1, spawnLoc.X, spawnLoc.Z));
                }
            }
        }
        public void OnKP(object s, KeyEventArgs a) {
            switch(a.KeyCode) {
                case Keys.D1:
                    team = 0;
                    break;
                case Keys.D2:
                    team = 1;
                    break;
                case Keys.D8:
                    unit = 0;
                    break;
                case Keys.D9:
                    unit = 1;
                    break;
                case Keys.D0:
                    unit = 2;
                    break;
                case Keys.E:
                    doAdd = true;
                    break;
                case DevConsole.ACTIVATION_KEY:
                    if(DevConsole.IsActivated)
                        DevConsole.Deactivate();
                    else
                        DevConsole.Activate();
                    break;
                case Keys.Escape:
                    State = ScreenState.ChangePrevious;
                    break;
            }
        }
        public void OnKR(object s, KeyEventArgs a) {
            switch(a.KeyCode) {
                case Keys.E:
                    doAdd = false;
                    break;
            }
        }
    }
}
