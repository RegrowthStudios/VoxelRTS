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
using System.Text.RegularExpressions;

namespace RTS {
    public class RTSScreen : GameScreen<App> {
        const int NUM_FPS_SAMPLES = 64;

        private GameplayController playController;
        private GameState state;
        private RTSRenderer renderer;
        private Camera camera;
        private PlayerInputController gameInput;

        Vector3 clickWorldPos;
        int team, type;
        bool pauseEngine, pauseRender;
        bool addUnit, addBuilding;
        int playing;
        Thread tEngine;
        SpriteFont sfDebug;
        int eFPS;

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
            DevConsole.OnNewCommand += DevConsole_OnNewCommand;

            addUnit = false;
            team = 0;
            type = 0;

            state = game.LoadScreen.LoadedState;
            camera = game.LoadScreen.LoadedCamera;
            renderer = game.LoadScreen.LoadedRenderer;
            renderer.UseFOW = true;
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
            DevConsole.OnNewCommand -= DevConsole_OnNewCommand;
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
                renderer.DrawUI(SB);
            }

            game.DrawDevConsole();
            if(!DevConsole.IsActivated) {
                // Show FPS
                double fps = gameTime.ElapsedGameTime.TotalSeconds;
                fps = Math.Round(1000.0 / fps) / 1000.0;
                SB.Begin();
                SB.DrawString(sfDebug, fps + " / " + eFPS, Vector2.One * 10, Color.White);
                SB.End();
            }
            game.DrawMouse();

            //if(state.gtC.VictoriousTeam.HasValue) {
            //    State = ScreenState.ChangePrevious;
            //}
        }

        public void OnMP(Vector2 p, MouseButton b) {
            if(b == MouseButton.Right) {
                Ray r = renderer.Camera.GetViewRay(p);
                IntersectionRecord rec = new IntersectionRecord();
                if(state.Map.BVH.Intersect(ref rec, r)) {
                    clickWorldPos = r.Position + r.Direction * rec.T;
                    if(addUnit)
                        gameInput.AddEvent(new SpawnUnitEvent(
                            team, type, new Vector2(clickWorldPos.X, clickWorldPos.Z)
                            ));
                    else if(addBuilding)
                        gameInput.AddEvent(new SpawnBuildingEvent(
                            team, type,
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
                    type = 0;
                    break;
                case Keys.D9:
                    type = 1;
                    break;
                case Keys.D0:
                    type = 2;
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

        double CalcFPS(double[] fpsSamples, ref int currentSample, double dt) {
            if(dt < 0.001) dt = 0.001;
            fpsSamples[currentSample] = 1.0 / dt;
            double fps = 0;
            for(int i = 0; i < NUM_FPS_SAMPLES; i++)
                fps += fpsSamples[i] / NUM_FPS_SAMPLES;
            currentSample++;
            currentSample %= NUM_FPS_SAMPLES;
            return fps;
        }
        void EngineThread() {
            TimeSpan tCur;
            TimeSpan tPrev = DateTime.Now.TimeOfDay;
            int milliRun = (int)(RTSConstants.GAME_DELTA_TIME * 1000);

            double[] fpsSamples = new double[NUM_FPS_SAMPLES];
            int currentSample = 0;
            for(int i = 0; i < NUM_FPS_SAMPLES; i++) {
                fpsSamples[i] = 60.0;
            }

            while(playing != 0) {
                if(pauseEngine) {
                    Thread.Sleep(milliRun);
                    continue;
                }

                playController.Update(state, RTSConstants.GAME_DELTA_TIME);

                // Sleep For A While
                tCur = DateTime.Now.TimeOfDay;
                double ddt = (tCur.TotalMilliseconds - tPrev.TotalMilliseconds) / 1000.0;
                eFPS = (int)CalcFPS(fpsSamples, ref currentSample, ddt);
                int dt = eFPS == 0 ? milliRun : (eFPS < 0 ? 0 : (int)(1000.0 / eFPS));
                if(dt < milliRun) Thread.Sleep(milliRun - dt);
                tPrev = tCur;
            }
        }

        Regex r1 = RegexHelper.GenerateInteger("setteam");
        Regex r2 = RegexHelper.GenerateInteger("settype");
        private void DevConsole_OnNewCommand(string obj) {
            Match m;
            if((m = r1.Match(obj)).Success) {
                team = RegexHelper.ExtractInt(m);
            }
            else if((m = r2.Match(obj)).Success) {
                type = RegexHelper.ExtractInt(m);
            }
        }
    }
}