using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
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

namespace RTSCS {
    public class _3DSimScreen : GameScreen<App> {
        private GameEngine engine;
        private DevConsoleView dcv;

        Vector3 spawnLoc;
        int team, unit;
        bool doAdd, mPress;

        public override int Next {
            get {
                return -1;
            }
            protected set {
                throw new NotImplementedException();
            }
        }
        public override int Previous {
            get {
                return game.LoadScreen.Index;
            }
            protected set {
                throw new NotImplementedException();
            }
        }

        public _3DSimScreen()
            : base() {
        }

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            game.IsMouseVisible = true;
            dcv = new DevConsoleView(G);
            MouseEventDispatcher.OnMousePress += OnMP;
            KeyboardEventDispatcher.OnKeyPressed += OnKP;
            KeyboardEventDispatcher.OnKeyReleased += OnKR;
            doAdd = mPress = false;
            team = 0;
            unit = 0;

            engine = game.LoadScreen.LoadedEngine;
        }
        public override void OnExit(GameTime gameTime) {
            game.IsMouseVisible = false;
            MouseEventDispatcher.OnMousePress -= OnMP;
            KeyboardEventDispatcher.OnKeyPressed -= OnKP;
            KeyboardEventDispatcher.OnKeyReleased -= OnKR;
            dcv.Dispose();
            dcv = null;
            DevConsole.Deactivate();
            engine.Dispose();
        }

        public override void Update(GameTime gameTime) {
            engine.Update(1f / 60f);

            // TODO: Omit Move All Units To The Mouse
            if(doAdd && mPress) {
                DevConsole.AddCommand(string.Format("spawn [{0},{1},{2},{3},{4}]", team, unit, 1, spawnLoc.X, spawnLoc.Z));
                mPress = false;
            }
        }
        public override void Draw(GameTime gameTime) {
            engine.Draw(G, 1f / 60f);
            if(DevConsole.IsActivated) {
                SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
                dcv.Draw(SB, Vector2.Zero);
                SB.End();
            }
        }

        public void OnMP(Vector2 p, MouseButton b) {
            if(b == MouseButton.Right) {
                Ray r = engine.renderer.Camera.GetViewRay(p);
                IntersectionRecord rec = new IntersectionRecord();
                if(engine.state.Map.BVH.Intersect(ref rec, r)) {
                    spawnLoc = r.Position + r.Direction * rec.T;
                    mPress = true;
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
                case Keys.Q:
                    doAdd = true;
                    break;
                case DevConsole.ACTIVATION_KEY:
                    if(DevConsole.IsActivated)
                        DevConsole.Deactivate();
                    else
                        DevConsole.Activate();
                    break;
            }
        }
        public void OnKR(object s, KeyEventArgs a) {
            switch(a.KeyCode) {
                case Keys.Q:
                    doAdd = false;
                    break;
            }
        }
    }
}