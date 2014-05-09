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
using Microsoft.Xna.Framework.Audio;

namespace RTS {
    public class RTSScreen : GameScreen<App> {
        const int NUM_FPS_SAMPLES = 64;
        const string BS_SOUND_DIR = @"Packs\audio\bg";
        static readonly Random rSong = new Random();
        static readonly Regex rgxTeam = RegexHelper.GenerateInteger("setteam");
        static readonly Regex rgxType = RegexHelper.GenerateInteger("settype");
        static readonly Regex rgxSpawn = RegexHelper.GenerateVec2Int("setspawn");

        private GameplayController playController;
        private GameState state;
        private RTSRenderer renderer;
        private Camera camera;
        private ACInputController gameInput;
        private IVisualInputController vInput;

        int team, type;
        bool pauseEngine, pauseRender;
        int playing;
        Thread tEngine;
        SpriteFont sfDebug;
        int eFPS;

        Texture2D tPopup;
        Rectangle rPopup;

        // End Animations
        Texture2D tVictory, tDefeat;
        Vector2 ctV, ctD;
        float tl;
        bool end;
        int gResult;

        Jukebox jukeBox;

        public override int Next {
            get { return -1; }
            protected set { }
        }
        public override int Previous {
            get { return game.MenuScreen.Index; }
            protected set { }
        }

        public override void Build() {
            GameEngine.CompileAllScripts(new DirectoryInfo(@"Packs"));

            using(var s = File.OpenRead(@"Content\Textures\Victory.png")) {
                tVictory = Texture2D.FromStream(G, s);
            }
            ctV = new Vector2(tVictory.Width, tVictory.Height) * 0.5f;
            using(var s = File.OpenRead(@"Content\Textures\Defeat.png")) {
                tDefeat = Texture2D.FromStream(G, s);
            }
            ctD = new Vector2(tDefeat.Width, tDefeat.Height) * 0.5f;
        }
        public override void Destroy(GameTime gameTime) {
            tVictory.Dispose();
            tDefeat.Dispose();
        }

        public override void OnEntry(GameTime gameTime) {
            tPopup = null;
            end = false;
            MouseEventDispatcher.OnMousePress += OnMP;
            KeyboardEventDispatcher.OnKeyPressed += OnKP;
            KeyboardEventDispatcher.OnKeyReleased += OnKR;
            DevConsole.OnNewCommand += DevConsole_OnNewCommand;

            team = 0;
            type = 0;

            state = game.LoadScreen.LoadedState;
            state.OnNewPopup += OnNewPopup;
            camera = game.LoadScreen.LoadedCamera;
            camera.CamOrigin = new Vector3(30, 35, 30);
            renderer = game.LoadScreen.LoadedRenderer;
            renderer.UseFOW = true;
            gameInput = (from t in state.teams
                         where t.Input != null && t.Input.Type == RTSInputType.Player
                         select t.Input).FirstOrDefault();
            vInput = gameInput == null ? null : gameInput as IVisualInputController;
            playController = game.LoadScreen.LoadedGPlay;

            sfDebug = renderer.CreateFont("Courier New", 32);


            // Create Game Engine Thread
            playController.BeginPlaying(state);
            tEngine = new Thread(EngineThread);
            tEngine.Priority = ThreadPriority.Highest;
            tEngine.TrySetApartmentState(ApartmentState.MTA);
            tEngine.IsBackground = true;
            playing = 1;
            pauseEngine = false;
            pauseRender = false;
            tEngine.Start();

            // Create Background Music
            jukeBox = new Jukebox();
            jukeBox.LoadFromDirectory(new DirectoryInfo(BS_SOUND_DIR));
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

            jukeBox.Dispose();
            jukeBox = null;

            if(tPopup != null) tPopup.Dispose();
        }

        public override void Update(GameTime gameTime) {
            // We Are GPU-Bound
            vInput.Update(renderer, state);
            jukeBox.Update();
            renderer.UpdateAnimations(state, (float)game.TargetElapsedTime.TotalSeconds);
        }
        public override void Draw(GameTime gameTime) {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            if(!pauseRender) {
                camera.Update(state.CGrid, RTSConstants.GAME_DELTA_TIME);
                renderer.Update(state);
                renderer.Draw(state, RTSConstants.GAME_DELTA_TIME);
            }
            else {
                G.Clear(Color.Black);
            }
            vInput.Draw(renderer, SB);
            DrawFPS(gameTime);
            game.DrawDevConsole();
            game.DrawMouse();

            if(tPopup != null) {
                SB.Begin(SpriteSortMode.Deferred, BlendState.NonPremultiplied);
                SB.Draw(tPopup, rPopup, Color.White);
                SB.End();
            }

            if(end) {
                tl -= dt;
                if(tl < 0)
                    State = ScreenState.ChangePrevious;

                Vector2 center = new Vector2(G.Viewport.Width, G.Viewport.Height) * 0.5f;
                SB.Begin(SpriteSortMode.Immediate, BlendState.NonPremultiplied, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
                SB.Draw(
                    gResult < 0 ? tDefeat : tVictory,
                    center, null,
                    Color.Lerp(Color.Transparent, Color.White, MathHelper.Clamp(3f - tl, 0, 1)),
                    0f,
                    gResult < 0 ? ctD : ctV,
                    MathHelper.Lerp(0.3f, 1f, MathHelper.Clamp(3f - tl, 0, 1)),
                    SpriteEffects.None,
                    0f
                    );
                SB.End();
            }
            else if(state.gtC.VictoriousTeam.HasValue) {
                end = true;
                tl = 3f;
                gResult = state.gtC.VictoriousTeam.Value;
            }
        }
        [Conditional("DEBUG")]
        void DrawFPS(GameTime gameTime) {
            if(!DevConsole.IsActivated) {
                // Show FPS
                double fps = gameTime.ElapsedGameTime.TotalSeconds;
                fps = Math.Round(1000.0 / fps) / 1000.0;
                SB.Begin();
                SB.DrawString(sfDebug, fps + " / " + eFPS, Vector2.One * 10, Color.White);
                SB.End();
            }
        }

        public void OnMP(Vector2 p, MouseButton b) {
        }
        public void OnKP(object s, KeyEventArgs a) {
            switch(a.KeyCode) {
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
                case Keys.Enter:
                    Texture2D t = System.Threading.Interlocked.Exchange(ref tPopup, null);
                    if(t != null) t.Dispose();
                    pauseEngine = false;
                    break;
            }
        }
        public void OnKR(object s, KeyEventArgs a) {
            switch(a.KeyCode) {
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

        private void DevConsole_OnNewCommand(string obj) {
            Match m;
            if((m = rgxTeam.Match(obj)).Success) {
                team = RegexHelper.ExtractInt(m);
            }
            else if((m = rgxType.Match(obj)).Success) {
                type = RegexHelper.ExtractInt(m);
            }
            else if((m = rgxSpawn.Match(obj)).Success) {
                int[] buf = RegexHelper.ExtractVec2I(m);
                team = buf[0];
                type = buf[1];
            }
        }
        private void OnNewPopup(string texFile, Rectangle destination) {
            Texture2D t = System.Threading.Interlocked.Exchange(ref tPopup, null);
            if(t != null) t.Dispose();
            using(var s = File.OpenRead(texFile))
                t = Texture2D.FromStream(G, s);
            rPopup = destination;
            pauseEngine = true;
            System.Threading.Interlocked.Exchange(ref tPopup, t);
        }
    }
}