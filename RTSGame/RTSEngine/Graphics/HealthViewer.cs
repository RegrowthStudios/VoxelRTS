using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public struct VertexHealthInstance : IVertexType {
        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 1),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(sizeof(float) * 7, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            );
        public VertexDeclaration VertexDeclaration {
            get { return Declaration; }
        }

        public Vector3 Position;
        public Vector4 DirRadiusHealth;
        public Color Tint;

        public VertexHealthInstance(Vector3 p, Vector2 d, float r, float h, Color c) {
            Position = p;
            DirRadiusHealth = new Vector4(d, r, h);
            Tint = c;
        }
    }

    public class HealthViewer {
        public const int MAX_COUNT = 1000;

        EffectPass fxPass;

        VertexBuffer vb;
        IndexBuffer ib;
        DynamicVertexBuffer dvb;
        VertexHealthInstance[] instances;
        VertexBufferBinding[] vbBinds;

        public HealthViewer() {
        }

        public void Build(RTSRenderer renderer, RTSFXEntity fx, string technique, string pass, string texture) {
            vb = renderer.CreateVertexBuffer(VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
            vb.SetData(new VertexPositionTexture[] {
               new VertexPositionTexture(new Vector3(-1, 0, -1), Vector2.Zero), 
               new VertexPositionTexture(new Vector3(1, 0, -1), Vector2.UnitX), 
               new VertexPositionTexture(new Vector3(-1, 0, 1), Vector2.UnitY), 
               new VertexPositionTexture(new Vector3(1, 0, 1), Vector2.One)
            });
            ib = renderer.CreateIndexBuffer(IndexElementSize.ThirtyTwoBits, 6, BufferUsage.WriteOnly);
            ib.SetData(new int[] { 0, 1, 2, 2, 1, 3 });
            dvb = renderer.CreateDynamicVertexBuffer(VertexHealthInstance.Declaration, MAX_COUNT, BufferUsage.WriteOnly);
            instances = new VertexHealthInstance[MAX_COUNT];
            vbBinds = new VertexBufferBinding[]{
                new VertexBufferBinding(vb),
                new VertexBufferBinding(dvb, 0, 1)
            };

            fxPass = fx.GetEffectPass(technique, pass);
        }

        public void Draw(GraphicsDevice g, List<VertexHealthInstance> insts) {
            fxPass.Apply();
            g.Indices = ib;
            int i = insts.Count;
            while(i > MAX_COUNT) {
                insts.CopyTo(i - MAX_COUNT, instances, 0, MAX_COUNT);
                dvb.SetData(instances, 0, MAX_COUNT);
                g.SetVertexBuffers(vbBinds);
                g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2, MAX_COUNT);
                i -= MAX_COUNT;
                g.SetVertexBuffers(null);
            }
            insts.CopyTo(0, instances, 0, i);
            dvb.SetData(instances, 0, i);
            g.SetVertexBuffers(vbBinds);
            g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, 4, 0, 2, i);
        }
    }
}