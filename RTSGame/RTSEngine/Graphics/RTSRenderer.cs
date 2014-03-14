using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data;

namespace RTSEngine.Graphics {
    public class RTSRenderer : IDisposable {
        // Camera Matrices
        Matrix mView, mProj;
        private Matrix View {
            get { return mView; }
            set {
                mView = value;
                fxMap.View = mView;
            }
        }
        private Matrix Projection {
            get { return mProj; }
            set {
                mProj = value;
                fxMap.Projection = mProj;
            }
        }

        // Map To Render
        public HeightmapModel Map {
            get;
            set;
        }

        // Effects
        private BasicEffect fxMap;

        public RTSRenderer(GraphicsDevice g) {
            fxMap = new BasicEffect(g);
            fxMap.LightingEnabled = false;
            fxMap.FogEnabled = false;
            fxMap.VertexColorEnabled = false;
            fxMap.TextureEnabled = true;
            fxMap.World = Matrix.Identity;

            View = Matrix.CreateLookAt(Vector3.One * 1000f, Vector3.Zero, Vector3.Up);
            Projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, g.Viewport.AspectRatio, 0.01f, 2000f);
        }
        public void Dispose() {
            Map.Dispose();

            fxMap.Dispose();
        }

        public void Draw(GraphicsDevice g, GameState s, float dt) {
            g.Clear(Color.Black);

            // Draw The Map
            fxMap.Texture = Map.PrimaryTexture;
            Map.SetPrimaryModel(g);
            fxMap.CurrentTechnique.Passes[0].Apply();
            Map.DrawPrimary(g);

            fxMap.Texture = Map.SecondaryTexture;
            Map.SetSecondaryModel(g);
            fxMap.CurrentTechnique.Passes[0].Apply();
            Map.DrawSecondary(g);
        }
    }
}