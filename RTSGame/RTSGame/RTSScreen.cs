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
    public class RTSScreen : GameScreen<App> {
        private GameplayController playController;
        private GameState state;
        private RTSRenderer renderer;
        private Camera camera;
        private DevConsoleView dcv;
        private PlayerInputController gameInput;

        Vector3 clickWorldPos;
        int team, unit;
        bool pauseEngine, pauseRender;
        bool addUnit, addBuilding;
        int playing;
        Thread tEngine;
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
            addUnit = false;
            team = 0;
            unit = 0;

            state = game.LoadScreen.LoadedState;
            camera = game.LoadScreen.LoadedCamera;
            renderer = game.LoadScreen.LoadedRenderer;
            playController = new GameplayController();
            gameInput = state.teams[0].Input as PlayerInputController;
            gameInput.Camera = camera;
            gameInput.UI = renderer.RTSUI;
            playController.Init(state);

            sfDebug = renderer.CreateFont("Courier New", 32);
            tEngine = new Thread(EngineThread);
            tEngine.Priority = ThreadPriority.Highest;
            tEngine.TrySetApartmentState(ApartmentState.MTA);
            tEngine.IsBackground = true;
            playing = 1;
            pauseEngine = false;
            pauseRender = false;
            tEngine.Start();
        }
        public override void OnExit(GameTime gameTime) {
            MouseEventDispatcher.OnMousePress -= OnMP;
            KeyboardEventDispatcher.OnKeyPressed -= OnKP;
            KeyboardEventDispatcher.OnKeyReleased -= OnKR;
            dcv.Dispose();
            dcv = null;
            DevConsole.Deactivate();

            camera.Controller.Unhook(game.Window);
            renderer.Dispose();

            Thread.VolatileWrite(ref playing, 0);
            tEngine.Join();
            GameEngine.Dispose(state);
            state = null;
        }

        public override void Update(GameTime gameTime) {
            // This Tells Us We Are GPU-Bound
            //Thread.Sleep(10);
            renderer.UpdateAnimations(state, (float)game.TargetElapsedTime.TotalSeconds);
        }
        public override void Draw(GameTime gameTime) {
            if(!pauseRender) {
                camera.Update(state.Map, RTSConstants.GAME_DELTA_TIME);
                renderer.Update(state);
                renderer.Draw(state, RTSConstants.GAME_DELTA_TIME);

                // TODO: Draw UI
                renderer.DrawUI(SB);
            }

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
                    clickWorldPos = r.Position + r.Direction * rec.T;
                    if(addUnit)
                        gameInput.AddEvent(new SpawnUnitEvent(
                            team, unit, new Vector2(clickWorldPos.X, clickWorldPos.Z)
                            ));
                    else if(addBuilding)
                        gameInput.AddEvent(new SpawnBuildingEvent(
                            team, 0,
                            HashHelper.Hash(new Vector2(clickWorldPos.X, clickWorldPos.Z), state.CGrid.numCells, state.CGrid.size)
                            ));
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
                    addUnit = true;
                    break;
                case Keys.R:
                    addBuilding = true;
                    break;
                case Keys.P:
                    if(!DevConsole.IsActivated)
                        pauseEngine = !pauseEngine;
                    break;
                case Keys.O:
                    if(!DevConsole.IsActivated)
                        pauseRender = !pauseRender;
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
                case Keys.F2:
                    using(var fileStream = File.Create("game.sg")) {
                        BinaryWriter bw = new BinaryWriter(fileStream);
                        StateSerializer.Serialize(bw, state);
                        bw.Flush();
                    }
                    break;
            }
        }
        public void OnKR(object s, KeyEventArgs a) {
            switch(a.KeyCode) {
                case Keys.E:
                    addUnit = false;
                    break;
                case Keys.R:
                    addBuilding = false;
                    break;
            }
        }

        void EngineThread() {
            TimeSpan tCur;
            TimeSpan tPrev = DateTime.Now.TimeOfDay;
            int milliRun = (int)(RTSConstants.GAME_DELTA_TIME * 1000);
            while(playing != 0) {
                if(pauseEngine) {
                    Thread.Sleep(milliRun);
                    continue;
                }
                playController.Update(state, RTSConstants.GAME_DELTA_TIME);

                // Sleep For A While
                tCur = DateTime.Now.TimeOfDay;
                int dt = (int)(tCur.TotalMilliseconds - tPrev.TotalMilliseconds);
                if(dt < milliRun) Thread.Sleep(milliRun - dt);
                tPrev = tCur;
            }
        }
    }
}