using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using RTSEngine.Data;
using RTSEngine.Controllers;
using BlisterUI.Input;
using RTSEngine.Data.Parsers;
using System.IO;
using RTSEngine.Data.Team;

namespace RTSEngine.Graphics {
    public class RTSRenderer : IDisposable {
        // Really Should Not Be Holding This Though
        private GameWindow window;
        private GraphicsDeviceManager gManager;
        private GraphicsDevice G {
            get { return gManager.GraphicsDevice; }
        }

        // Selection Box
        private Texture2D tPixel;
        private bool drawBox;
        private Vector2 start, end;

        // The Camera
        public Camera Camera {
            get;
            set;
        }

        // Map To Render
        public HeightmapModel Map {
            get;
            set;
        }

        // All The Unit Models To Render
        public List<RTSUnitModel> UnitModels {
            get;
            private set;
        }

        // Effects
        private BasicEffect fxSelection;
        private RTSFXEntity fxAnim;
        private RTSFXMap fxMap;

        public RTSRenderer(GameEngine ge, GraphicsDeviceManager gdm, string fxAnimFile, string fxMapFile, GameWindow w) {
            window = w;
            gManager = gdm;
            UnitModels = new List<RTSUnitModel>();

            tPixel = ge.CreateTexture2D(1, 1);
            tPixel.SetData(new Color[] { Color.White });

            fxMap = new RTSFXMap(ge.LoadEffect(fxMapFile));

            fxSelection = ge.CreateEffect();
            fxSelection.LightingEnabled = false;
            fxSelection.FogEnabled = false;
            fxSelection.TextureEnabled = false;
            fxSelection.VertexColorEnabled = true;
            fxSelection.World = Matrix.Identity;
            fxSelection.Texture = tPixel;

            fxAnim = new RTSFXEntity(ge.LoadEffect(fxAnimFile));
            fxAnim.World = Matrix.Identity;
            fxAnim.CPrimary = Vector3.UnitX;
            fxAnim.CSecondary = Vector3.UnitY;
            fxAnim.CTertiary = Vector3.UnitZ;

            drawBox = false;
            MouseEventDispatcher.OnMousePress += OnMousePress;
            MouseEventDispatcher.OnMouseRelease += OnMouseRelease;
            MouseEventDispatcher.OnMouseMotion += OnMouseMove;
        }
        public void Dispose() {
            MouseEventDispatcher.OnMousePress -= OnMousePress;
            MouseEventDispatcher.OnMouseRelease -= OnMouseRelease;
            MouseEventDispatcher.OnMouseMotion -= OnMouseMove;
            Camera.Controller.Unhook(window);
        }

        public void HookToGame(GameEngine ge, Camera camera, EngineLoadData geLoad) {
            // Get The Camera
            Camera = camera;

            // Create The Map
            Heightmap map = ge.State.Map;
            Map = HeightmapParser.ParseModel(ge, new Vector3(map.Width, map.ScaleY, map.Depth), ge.State.CGrid.grids.X, ge.State.CGrid.grids.Y, geLoad.MapFile);
            Camera.MoveTo(map.Width * 0.5f, map.Depth * 0.5f);
            fxMap.MapSize = new Vector2(map.Width, map.Depth);

            // Get Unit Models
            for(int ti = 0; ti < geLoad.Teams.Length; ti++) {
                RTSTeamResult res = geLoad.Teams[ti];
                RTSTeam team = ge.State.Teams[ti];
                for(int ui = 0; ui < team.unitData.Count; ui++) {
                    RTSUnitModel uModel = RTSUnitDataParser.ParseModel(ge, team.unitData[ui], res.TeamType.UnitTypes[ui]);
                    uModel.ColorPrimary = team.ColorScheme.Primary;
                    uModel.ColorSecondary = team.ColorScheme.Secondary;
                    uModel.ColorTertiary = team.ColorScheme.Tertiary;
                    team.OnUnitSpawn += uModel.OnUnitSpawn;
                    UnitModels.Add(uModel);
                }
            }
        }

        private void CheckSetFOW(CollisionGrid cg, int x, int y, int pIndex, FogOfWar f, float fN) {
            if(x < 0 || x >= cg.grids.X) return;
            if(y < 0 || y >= cg.grids.Y) return;
            FogOfWar of = cg.GetFogOfWar(x, y, pIndex);
            if(of != f && Map.FogOfWar[y * cg.grids.X + x] < fN) {
                cg.SetFogOfWar(x, y, pIndex, f);
                Map.SetFOW(x, y, fN);
            }
        }
        public void UpdateFOW(GameState s, RTSTeam team, int pIndex) {
            CollisionGrid cg = s.CGrid;
            for(int ui = 0; ui < team.units.Count; ui++) {
                Point p = HashHelper.Hash(team.units[ui].GridPosition, cg.grids, cg.size);
                FogOfWar f = cg.GetFogOfWar(p.X, p.Y, pIndex);
                switch(f) {
                    case FogOfWar.Active:
                        continue;
                    case FogOfWar.Passive:
                        cg.SetFogOfWar(p.X, p.Y, pIndex, f);
                        Map.SetFOW(p.X, p.Y, 1f);
                        CheckSetFOW(cg, p.X + 1, p.Y, pIndex, FogOfWar.Passive, 0.5f);
                        CheckSetFOW(cg, p.X - 1, p.Y, pIndex, FogOfWar.Passive, 0.5f);
                        CheckSetFOW(cg, p.X, p.Y + 1, pIndex, FogOfWar.Passive, 0.5f);
                        CheckSetFOW(cg, p.X, p.Y - 1, pIndex, FogOfWar.Passive, 0.5f);
                        break;
                    case FogOfWar.Nothing:
                        cg.SetFogOfWar(p.X, p.Y, pIndex, f);
                        Map.SetFOW(p.X, p.Y, 1f);
                        CheckSetFOW(cg, p.X + 1, p.Y, pIndex, FogOfWar.Passive, 0.5f);
                        CheckSetFOW(cg, p.X - 1, p.Y, pIndex, FogOfWar.Passive, 0.5f);
                        CheckSetFOW(cg, p.X, p.Y + 1, pIndex, FogOfWar.Passive, 0.5f);
                        CheckSetFOW(cg, p.X, p.Y - 1, pIndex, FogOfWar.Passive, 0.5f);
                        break;
                }
            }
        }

        public void UpdateAnimations(GameState s, float dt) {
            RTSTeam team;
            for(int ti = 0; ti < s.Teams.Length; ti++) {
                team = s.Teams[ti];
                for(int i = 0; i < team.units.Count; i++)
                    if(team.units[i].AnimationController != null)
                        team.units[i].AnimationController.Update(s, dt);
            }
        }

        // Rendering Passes
        public void Draw(GameState s, float dt) {
            G.Clear(Color.Black);

            // TODO: Draw Environment Cube
            UpdateFOW(s, s.Teams[0], 0);
            DrawMap();
            DrawAnimated();
            if(drawBox) DrawSelectionBox();
        }
        private void DrawMap() {
            if(Map.Reset) Map.ApplyFOW();

            // Set States
            G.DepthStencilState = DepthStencilState.Default;
            G.RasterizerState = RasterizerState.CullCounterClockwise;
            G.BlendState = BlendState.Opaque;
            G.SamplerStates[0] = SamplerState.LinearClamp;

            // Set Camera
            fxMap.VP = Camera.View * Camera.Projection;

            // Primary Map Model
            if(Map.TrianglesPrimary > 0) {
                fxMap.SetTextures(G, Map.PrimaryTexture, Map.FogOfWarTexture);
                G.SetVertexBuffer(Map.VBPrimary);
                G.Indices = Map.IBPrimary;
                fxMap.ApplyPassPrimary();
                G.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Map.VBPrimary.VertexCount, 0, Map.TrianglesPrimary);
            }
            // Secondary Map Model
            if(Map.TrianglesSecondary > 0) {
                fxMap.SetTextures(G, Map.SecondaryTexture, Map.FogOfWarTexture);
                G.SetVertexBuffer(Map.VBSecondary);
                G.Indices = Map.IBSecondary;
                fxMap.ApplyPassSecondary();
                G.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Map.VBSecondary.VertexCount, 0, Map.TrianglesSecondary);
            }
        }
        private void DrawAnimated() {
            // Set Camera
            fxAnim.VP = Camera.View * Camera.Projection;

            // Loop Through Models
            G.VertexSamplerStates[0] = SamplerState.PointClamp;
            G.SamplerStates[1] = SamplerState.LinearClamp;
            G.SamplerStates[2] = SamplerState.LinearClamp;
            foreach(RTSUnitModel unitModel in UnitModels) {
                fxAnim.SetTextures(G, unitModel.AnimationTexture, unitModel.ModelTexture, unitModel.ColorCodeTexture);
                fxAnim.CPrimary = unitModel.ColorPrimary;
                fxAnim.CSecondary = unitModel.ColorSecondary;
                fxAnim.CTertiary = unitModel.ColorTertiary;
                fxAnim.ApplyPassAnimation();
                unitModel.UpdateInstances(G);
                unitModel.SetInstances(G);
                unitModel.DrawInstances(G);
            }

            // Cause XNA Is Retarded Like That
            G.VertexTextures[0] = null;
            G.VertexSamplerStates[0] = SamplerState.LinearClamp;
        }
        private void DrawSelectionBox() {
            Vector2 ss = new Vector2(G.Viewport.TitleSafeArea.Width, G.Viewport.TitleSafeArea.Height);
            fxSelection.View = Matrix.CreateLookAt(new Vector3(ss / 2, -1), new Vector3(ss / 2, 0), Vector3.Down);
            fxSelection.Projection = Matrix.CreateOrthographic(ss.X, ss.Y, 0, 2);

            G.DepthStencilState = DepthStencilState.None;
            G.BlendState = BlendState.NonPremultiplied;
            G.RasterizerState = RasterizerState.CullNone;

            Vector3 min = new Vector3(Vector2.Min(start, end), 0);
            Vector3 max = new Vector3(Vector2.Max(start, end), 0);
            fxSelection.CurrentTechnique.Passes[0].Apply();
            G.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionColor[] {
                    new VertexPositionColor(min, new Color(0f, 0, 1f, 0.3f)),
                    new VertexPositionColor(new Vector3(max.X, min.Y, 0), new Color(1f, 0, 1f, 0.3f)),
                    new VertexPositionColor(new Vector3(min.X, max.Y, 0), new Color(1f, 0, 1f, 0.3f)),
                    new VertexPositionColor(max, new Color(1f, 0, 0f, 0.3f)),
                }, 0, 2, VertexPositionColor.VertexDeclaration);
        }

        // Selection Box Handling
        private void OnMousePress(Vector2 p, MouseButton b) {
            if(b == MouseButton.Left) {
                drawBox = true;
                start = p;
            }
        }
        private void OnMouseRelease(Vector2 p, MouseButton b) {
            if(b == MouseButton.Left) {
                drawBox = false;
            }
        }
        private void OnMouseMove(Vector2 p, Vector2 d) {
            end = p;
        }
    }
}