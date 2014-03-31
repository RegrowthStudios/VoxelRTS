using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI.Input;

namespace RTSEngine.Graphics {
    public class MouseRenderer : IDisposable {
        private Texture2D tMouse;
        public Texture2D Texture {
            get { return tMouse; }
            set {
                tMouse = value;
                fx.Texture = tMouse;
            }
        }

        private VertexPositionColorTexture[] verts;
        public Color Color {
            get { return verts[0].Color; }
            set {
                verts[0].Color = value;
                verts[1].Color = value;
                verts[2].Color = value;
                verts[3].Color = value;
            }
        }
        private Vector2 mPosition;
        private float radius;
        public float InnerRadius {
            get { return radius; }
            set {
                radius = value;
                Update();
            }
        }
        private float rotation;
        public float Rotation {
            get { return rotation; }
            set {
                rotation = value;
                Update();
            }
        }

        private BasicEffect fx;

        private GameWindow window;

        public MouseRenderer(GraphicsDevice g, GameWindow w) {
            // Use Color And Texture
            fx = new BasicEffect(g);
            fx.FogEnabled = false;
            fx.LightingEnabled = false;
            fx.VertexColorEnabled = true;
            fx.TextureEnabled = true;

            // Create Verts At The Center
            verts = new VertexPositionColorTexture[4];
            verts[0].TextureCoordinate = Vector2.Zero;
            verts[1].TextureCoordinate = Vector2.UnitX;
            verts[2].TextureCoordinate = Vector2.UnitY;
            verts[3].TextureCoordinate = Vector2.One;
            Color = Color.White;
            InnerRadius = 12;

            window = w;
            window.ClientSizeChanged += OnWindowResize;
            OnWindowResize(window, null);
            MouseEventDispatcher.OnMouseMotion += OnMouseMotion;
        }
        public void Dispose() {
            if(fx != null) {
                fx.Dispose();
                fx = null;
            }
            window.ClientSizeChanged -= OnWindowResize;
            MouseEventDispatcher.OnMouseMotion -= OnMouseMotion;
        }

        private void Update() {
            // Find Positive X-Vector
            Vector2 x = new Vector2((float)Math.Cos(rotation), (float)-Math.Sin(rotation)) * radius;
            // Get Cross Product
            Vector2 y = new Vector2(x.Y, -x.X);

            // Set The Positions
            verts[0].Position = new Vector3(mPosition - x + y, 0);
            verts[1].Position = new Vector3(mPosition + x + y, 0);
            verts[2].Position = new Vector3(mPosition - x - y, 0);
            verts[3].Position = new Vector3(mPosition + x - y, 0);
        }

        public void BeginPass(GraphicsDevice g) {
            g.SamplerStates[0] = SamplerState.LinearClamp;
            g.DepthStencilState = DepthStencilState.None;
            g.RasterizerState = RasterizerState.CullNone;
            fx.CurrentTechnique.Passes[0].Apply();
        }
        public void Draw(GraphicsDevice g) {
            g.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, 2, VertexPositionColorTexture.VertexDeclaration);
        }

        private void OnWindowResize(object sender, EventArgs args) {
            Vector2 s = new Vector2(window.ClientBounds.Width, window.ClientBounds.Height);
            fx.Projection = Matrix.CreateOrthographic(s.X, s.Y, 0, 2);
            Vector3 target = new Vector3(s.X * 0.5f, s.Y * 0.5f, 0);
            fx.View = Matrix.CreateLookAt(target - Vector3.UnitZ, target, -Vector3.UnitY);
        }
        private void OnMouseMotion(Vector2 p, Vector2 d) {
            mPosition = p;
            Update();
        }
    }
}