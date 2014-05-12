using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public class CameraPointer {
        private BasicEffect fx;
        public Texture2D Texture {
            get;
            private set;
        }

        private VertexBuffer vb;
        private IndexBuffer ib;

        public CameraPointer() {

        }

        public void Build(RTSRenderer renderer, string image, Vector3 radii, Vector3 heights) {
            fx = renderer.CreateEffect();

            fx.FogEnabled = false;
            fx.VertexColorEnabled = false;
            fx.LightingEnabled = false;
            fx.TextureEnabled = true;

            if(image == null || !File.Exists(image)) {
                Texture = renderer.CreateTexture2D(3, 3, SurfaceFormat.Color, false);
                Texture.SetData(new Color[] {
                    Color.White, Color.Transparent, Color.White,
                    Color.Transparent, Color.Transparent, Color.Transparent,
                    Color.White, Color.Transparent, Color.White
                });
            }
            else {
                Texture = renderer.LoadTexture2D(image);
            }
            fx.Texture = Texture;

            VertexPositionTexture[] verts = new VertexPositionTexture[] {
                new VertexPositionTexture(new Vector3(-1, 1f, -1), new Vector2(0, 0)),
                new VertexPositionTexture(new Vector3(1, 1f, -1), new Vector2(0.5f, 0)),
                new VertexPositionTexture(new Vector3(-1, 1f, 1), new Vector2(0, 0.5f)),
                new VertexPositionTexture(new Vector3(1, 1f, 1), new Vector2(0.5f, 0.5f)),

                new VertexPositionTexture(new Vector3(-1, 1f, -1), new Vector2(0.5f, 0)),
                new VertexPositionTexture(new Vector3(1, 1f, -1), new Vector2(1f, 0)),
                new VertexPositionTexture(new Vector3(-1, 1f, 1), new Vector2(0.5f, 0.5f)),
                new VertexPositionTexture(new Vector3(1, 1f, 1), new Vector2(1f, 0.5f)),

                new VertexPositionTexture(new Vector3(-1, 1f, -1), new Vector2(0, 0.5f)),
                new VertexPositionTexture(new Vector3(1, 1f, -1), new Vector2(0.5f, 0.5f)),
                new VertexPositionTexture(new Vector3(-1, 1f, 1), new Vector2(0, 1f)),
                new VertexPositionTexture(new Vector3(1, 1f, 1), new Vector2(0.5f, 1f))
            };
            int[] inds = new int[] {
                0, 1, 2, 2, 1, 3,
                4, 5, 6, 6, 5, 7,
                8, 9, 10, 10, 9, 11
            };
            for(int i = 0; i < 4; i++) {
                verts[i].Position *= new Vector3(radii.X, heights.X, radii.X);
                verts[i + 4].Position *= new Vector3(radii.Y, heights.Y, radii.Y);
                verts[i + 8].Position *= new Vector3(radii.Z, heights.Z, radii.Z);
            }

            vb = renderer.CreateVertexBuffer(VertexPositionTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vb.SetData(verts);
            ib = renderer.CreateIndexBuffer(IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
            ib.SetData(inds);
        }

        public void Draw(GraphicsDevice g, Matrix mView, Matrix mProj, Vector3 position) {
            g.SetVertexBuffer(vb);
            g.Indices = ib;
            g.Textures[0] = Texture;
            g.SamplerStates[0] = SamplerState.LinearClamp;
            fx.View = mView;
            fx.Projection = mProj;
            fx.World = Matrix.CreateTranslation(position);
            fx.CurrentTechnique.Passes[0].Apply();
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vb.VertexCount, 0, ib.IndexCount / 3);
        }
    }
}