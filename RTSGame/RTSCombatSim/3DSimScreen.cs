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

        ReflectedEntityController aC, mC;

        public override void OnEntry(GameTime gameTime) {
            game.IsMouseVisible = true;
            dcv = new DevConsoleView(G);
            MouseEventDispatcher.OnMousePress += OnMP;
            MouseEventDispatcher.OnMouseRelease += OnMR;
            KeyboardEventDispatcher.OnKeyPressed += OnKP;
            KeyboardEventDispatcher.OnKeyReleased += OnKR;
            doAdd = false;
            team = 0;
            unit = 0;

            engine = game.LoadScreen.LoadedEngine;

            string[] refs = new string[]  {
                "System.dll",
                "System.Data.dll",
                "System.Core.dll",
                "System.Xml.dll",
                "System.Xml.Linq.dll",
                "lib\\Microsoft.Xna.Framework.dll",
                "RTSEngine.dll"
            };
            string err;
            aC = EntityControllerParser.Compile(@"Controllers\ActionController.cs", refs, out err).Controllers["RTSCS.Controllers.ActionController"];
            mC = EntityControllerParser.Compile(@"Controllers\MovementController.cs", refs, out err).Controllers["RTSCS.Controllers.MovementController"];

            Vector2 p = new Vector2(engine.state.Map.Width, engine.state.Map.Depth) * 0.5f;
            foreach(var t in engine.state.Teams) {
                int ut = 0;
                foreach(var ud in t.UnitData) {
                    for(int i = 0; i < ud.MaxCount / 2; i++)
                        t.AddUnit(ut, p);
                    ut++;
                }
            }
        }
        public override void OnExit(GameTime gameTime) {
            game.IsMouseVisible = false;
            MouseEventDispatcher.OnMousePress -= OnMP;
            MouseEventDispatcher.OnMouseRelease -= OnMR;
            KeyboardEventDispatcher.OnKeyPressed -= OnKP;
            KeyboardEventDispatcher.OnKeyReleased -= OnKR;
            dcv.Dispose();
            dcv = null;
            DevConsole.Deactivate();
            engine.Dispose();
        }

        List<RTSUnitInstance> selected;
        public override void Update(GameTime gameTime) {
            engine.Update(1f / 60f);

            if(doSelect) {
                selected = new List<RTSUnitInstance>();
                OBB? obb;
                Frustum? frustum;
                engine.renderer.GetSelectionBox(sStart, sEnd, out obb, out frustum);
                if(frustum.HasValue) {
                    Frustum f = frustum.Value;
                    foreach(var team in engine.state.Teams) {
                        foreach(var unit in team.Units) {
                            BoundingBox bb = unit.BBox;
                            if(SelectionDetection.Intersects(ref f, ref bb)) {
                                selected.Add(unit);
                            }
                        }
                    }
                }
                doSelect = false;
            }

            // TODO: Omit Move All Units To The Mouse
            if(doMove) {
                if(doAdd) {
                    engine.state.Teams[team].AddUnit(unit, new Vector2(move.X, move.Z));
                }
                else if(selected != null && selected.Count > 0) {
                    Vector2 gp = new Vector2(move.X, move.Z);
                    foreach(var unit in selected) {
                        unit.ActionController = aC.CreateInstance() as IActionController;
                        unit.MovementController = mC.CreateInstance() as IMovementController;
                        unit.MovementController.SetWaypoints(new Vector2[] { gp });
                    }
                }
                doMove = false;
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

        Vector2 sStart, sEnd;
        bool doSelect;

        Vector3 move;
        bool doMove;

        int team, unit;
        bool doAdd;
        public void OnMP(Vector2 p, MouseButton b) {
            if(b == MouseButton.Left) {
                sStart = p;
            }
            else if(b == MouseButton.Right) {
                Ray r = engine.renderer.GetViewRay(p);
                IntersectionRecord rec = new IntersectionRecord();
                if(engine.state.Map.BVH.Intersect(ref rec, r)) {
                    move = r.Position + r.Direction * rec.T;
                    doMove = true;
                }
            }
        }
        public void OnMR(Vector2 p, MouseButton b) {
            if(b == MouseButton.Left) {
                sEnd = p;
                doSelect = true;
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