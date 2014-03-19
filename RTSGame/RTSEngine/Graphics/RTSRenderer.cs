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
using RTSEngine.Data;

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
        private BasicEffect fxMap;
        private RTSEffect fxRTS;

        public RTSRenderer(GraphicsDeviceManager gdm, string rtsFXFile, GameWindow w) {
            window = w;
            gManager = gdm;
            UnitModels = new List<RTSUnitModel>();

            fxMap = new BasicEffect(G);
            fxMap.LightingEnabled = false;
            fxMap.FogEnabled = false;
            fxMap.VertexColorEnabled = false;
            fxMap.TextureEnabled = true;
            fxMap.World = Matrix.Identity;

            fxRTS = new RTSEffect(XNAEffect.Compile(G, rtsFXFile));
            fxRTS.World = Matrix.Identity;
            fxRTS.CPrimary = Vector3.UnitX;
            fxRTS.CSecondary = Vector3.UnitY;
            fxRTS.CTertiary = Vector3.UnitZ;

            tPixel = new Texture2D(G, 1, 1);
            tPixel.SetData(new Color[] { Color.White });

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

            tPixel.Dispose();

            Map.Dispose();

            fxMap.Dispose();
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

        public void Draw(GameState s, float dt) {
            G.Clear(Color.Black);

            // TODO: Draw Environment Cube

            // Setup Camera Matrices
            fxMap.View = camera.View;
            fxMap.Projection = camera.Projection;
            fxRTS.VP = camera.View * camera.Projection;

            // Draw The Map
            G.DepthStencilState = DepthStencilState.Default;
            G.RasterizerState = RasterizerState.CullCounterClockwise;
            G.BlendState = BlendState.Opaque; Map.SetPrimaryModel(G);
            // Primary Map Model
            fxMap.Texture = Map.PrimaryTexture;
            G.SamplerStates[0] = SamplerState.LinearClamp;
            fxMap.CurrentTechnique.Passes[0].Apply();
            Map.DrawPrimary(G);
            // Secondary Map Model
            fxMap.Texture = Map.SecondaryTexture;
            G.SamplerStates[0] = SamplerState.LinearClamp;
            Map.SetSecondaryModel(G);
            fxMap.CurrentTechnique.Passes[0].Apply();
            Map.DrawSecondary(G);


            // Draw All Animated Entities
            foreach(RTSUnitModel unitModel in UnitModels) {
                fxRTS.TexModelMap = unitModel.AnimationTexture;
                fxRTS.TexOverlay = unitModel.ModelTexture;
                fxRTS.TexColor = unitModel.ColorCodeTexture;
                fxRTS.ApplyPassAnimation();
                unitModel.UpdateInstances(G);
                unitModel.SetInstances(G);
                unitModel.DrawInstances(G);
            }

            if(drawBox) {
                fxMap.VertexColorEnabled = true;
                fxMap.TextureEnabled = false;
                fxMap.Texture = tPixel;
                Vector2 ss = new Vector2(G.Viewport.Width, G.Viewport.Height);
                fxMap.View = Matrix.CreateLookAt(new Vector3(ss / 2, -1), new Vector3(ss / 2, 0), Vector3.Down);
                fxMap.Projection = Matrix.CreateOrthographic(ss.X, ss.Y, 0, 2);
                G.DepthStencilState = DepthStencilState.None;
                G.BlendState = BlendState.NonPremultiplied;
                G.RasterizerState = RasterizerState.CullNone;
                Vector3 min = new Vector3(Vector2.Min(start, end), 0);
                Vector3 max = new Vector3(Vector2.Max(start, end), 0);
                fxMap.CurrentTechnique.Passes[0].Apply();
                G.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionColor[] {
                    new VertexPositionColor(min, new Color(0f, 0, 1f, 0.3f)),
                    new VertexPositionColor(new Vector3(max.X, min.Y, 0), new Color(1f, 0, 1f, 0.3f)),
                    new VertexPositionColor(new Vector3(min.X, max.Y, 0), new Color(1f, 0, 1f, 0.3f)),
                    new VertexPositionColor(max, new Color(1f, 0, 0f, 0.3f)),
                }, 0, 2, VertexPositionColor.VertexDeclaration);

                fxMap.View = camera.View;
                fxMap.Projection = camera.Projection;
                fxMap.VertexColorEnabled = false;
                fxMap.TextureEnabled = true;
            }
        }
    }
}