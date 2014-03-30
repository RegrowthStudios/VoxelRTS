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
        private Camera camera;
        public Camera Camera {
            get { return camera; }
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
        private BasicEffect fxMap, fxSelection;
        private RTSEffect fxRTS;

        public RTSRenderer(GameEngine ge, GraphicsDeviceManager gdm, string rtsFXFile, GameWindow w) {
            window = w;
            gManager = gdm;
            UnitModels = new List<RTSUnitModel>();

            tPixel = ge.CreateTexture2D(1, 1);
            tPixel.SetData(new Color[] { Color.White });

            fxMap = ge.CreateEffect();
            fxMap.LightingEnabled = false;
            fxMap.FogEnabled = false;
            fxMap.VertexColorEnabled = false;
            fxMap.TextureEnabled = true;
            fxMap.World = Matrix.Identity;

            fxSelection = ge.CreateEffect();
            fxSelection.LightingEnabled = false;
            fxSelection.FogEnabled = false;
            fxSelection.TextureEnabled = false;
            fxSelection.VertexColorEnabled = true;
            fxSelection.World = Matrix.Identity;
            fxSelection.Texture = tPixel;

            fxRTS = new RTSEffect(ge.LoadEffect(rtsFXFile));
            fxRTS.World = Matrix.Identity;
            fxRTS.CPrimary = Vector3.UnitX;
            fxRTS.CSecondary = Vector3.UnitY;
            fxRTS.CTertiary = Vector3.UnitZ;

            drawBox = false;
            MouseEventDispatcher.OnMousePress += OnMousePress;
            MouseEventDispatcher.OnMouseRelease += OnMouseRelease;
            MouseEventDispatcher.OnMouseMotion += OnMouseMove;

            camera = new Camera(G.Viewport);
            camera.Controller.Hook(window);
        }
        public void Dispose() {
            MouseEventDispatcher.OnMousePress -= OnMousePress;
            MouseEventDispatcher.OnMouseRelease -= OnMouseRelease;
            MouseEventDispatcher.OnMouseMotion -= OnMouseMove;
            camera.Controller.Unhook(window);
        }

        // Rendering Passes
        public void Draw(GameState s, float dt) {
            G.Clear(Color.Black);

            // TODO: Draw Environment Cube

            DrawMap();
            DrawAnimated();
            if(drawBox) DrawSelectionBox();
        }
        private void DrawMap() {
            // Set States
            G.DepthStencilState = DepthStencilState.Default;
            G.RasterizerState = RasterizerState.CullCounterClockwise;
            G.BlendState = BlendState.Opaque;
            G.SamplerStates[0] = SamplerState.LinearClamp;

            // Set Camera
            fxMap.View = camera.View;
            fxMap.Projection = camera.Projection;

            // Primary Map Model
            if(Map.TrianglesPrimary > 0) {
                G.SetVertexBuffer(Map.VBPrimary);
                G.Indices = Map.IBPrimary;
                fxMap.Texture = Map.PrimaryTexture;
                fxMap.CurrentTechnique.Passes[0].Apply();
                G.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Map.VBPrimary.VertexCount, 0, Map.TrianglesPrimary);
            }
            // Secondary Map Model
            if(Map.TrianglesSecondary > 0) {
                G.SetVertexBuffer(Map.VBSecondary);
                G.Indices = Map.IBSecondary;
                fxMap.Texture = Map.SecondaryTexture;
                fxMap.CurrentTechnique.Passes[0].Apply();
                G.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, Map.VBSecondary.VertexCount, 0, Map.TrianglesSecondary);
            }
        }
        private void DrawAnimated() {
            // Set Camera
            fxRTS.VP = camera.View * camera.Projection;

            // Loop Through Models
            G.VertexSamplerStates[0] = SamplerState.PointClamp;
            G.SamplerStates[1] = SamplerState.LinearClamp;
            G.SamplerStates[2] = SamplerState.LinearClamp;
            foreach(RTSUnitModel unitModel in UnitModels) {
                fxRTS.SetTextures(G, unitModel.AnimationTexture, unitModel.ModelTexture, unitModel.ColorCodeTexture);
                fxRTS.CPrimary = unitModel.ColorPrimary;
                fxRTS.CSecondary = unitModel.ColorSecondary;
                fxRTS.CTertiary = unitModel.ColorTertiary;
                fxRTS.ApplyPassAnimation();
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