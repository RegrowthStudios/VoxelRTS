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
using Microsoft.Xna.Framework.Input;
using BlisterUI.Input;

namespace RTSCS {
    public class _3DSimScreen : GameScreen<App> {
        private GameEngine engine;

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

        ReflectedEntityController aC, mC, cC, tC;

        public override void OnEntry(GameTime gameTime) {
            game.IsMouseVisible = true;
            MouseEventDispatcher.OnMousePress += OnMP;
            MouseEventDispatcher.OnMouseRelease += OnMR;
            engine = game.LoadScreen.LoadedEngine;
            foreach(var team in engine.state.Teams) {
                int t = 0;
                foreach(var unitType in team.UnitData) {
                    for(int i = 0; i < unitType.MaxCount; i++) {
                        team.AddUnit(t, new Vector2(200, 200));
                    }
                    t++;
                }
            }

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
            //cC = EntityControllerParser.Compile(@"Controllers\CombatController.cs", refs, out err).Controllers["RTSCS.Controllers.CombatController"];
            //tC = EntityControllerParser.Compile(@"Controllers\TargettingController.cs", refs, out err).Controllers["RTSCS.Controllers.TargettingController"];
        }
        public override void OnExit(GameTime gameTime) {
            game.IsMouseVisible = false;
            MouseEventDispatcher.OnMousePress -= OnMP;
            MouseEventDispatcher.OnMouseRelease -= OnMR;

            engine.Dispose();
        }

        List<RTSUnitInstance> selected;
        public override void Update(GameTime gameTime) {
            engine.Update(1f / 60f);

            if(doSelect) {
                OBB? obb;
                Frustum? frustum;
                engine.renderer.GetSelectionBox(sStart, sEnd, out obb, out frustum);
                if(frustum.HasValue) {
                    Frustum f = frustum.Value;
                    selected = new List<RTSUnitInstance>();
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
            if(doMove && selected != null && selected.Count > 0) {
                Vector2 gp = new Vector2(move.X, move.Z);
                foreach(var unit in selected) {
                    unit.ActionController = aC.CreateInstance() as IActionController;
                    unit.MovementController = mC.CreateInstance() as IMovementController;
                    unit.MovementController.SetWaypoints(new Vector2[] { gp });
                }
                doMove = false;
            }
        }
        public override void Draw(GameTime gameTime) {
            engine.Draw(G, 1f / 60f);
        }

        Vector2 sStart, sEnd;
        bool doSelect;

        Vector3 move;
        bool doMove;
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
    }
}