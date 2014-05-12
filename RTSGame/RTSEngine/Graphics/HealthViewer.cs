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
            new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector3, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector3, VertexElementUsage.Color, 0)
            );
        public VertexDeclaration VertexDeclaration {
            get { return VertexDeclaration; }
        }

        public Vector3 Position;
        public Vector4 DirRadiusHealth;
        public Color Tint;
    }

    public class HealthViewer {
        EffectPass fxPass;

        VertexBuffer vb;
        IndexBuffer ib;
        DynamicVertexBuffer dvb;

        public HealthViewer() {

        }

        public void Build(RTSRenderer renderer, Effect fx, string technique, string pass, string texture) {
            fxPass = fx.Techniques[technique].Passes[pass];
        }

        public void Draw() {

        }
    }
}
