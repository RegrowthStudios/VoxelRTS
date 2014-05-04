using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Grey.Graphics {
    public struct VertexVoxel : IVertexType {
        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 3, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 5, VertexElementFormat.Vector4, VertexElementUsage.TextureCoordinate, 1),
            new VertexElement(sizeof(float) * 9, VertexElementFormat.Color, VertexElementUsage.Color, 0)
            );
        public VertexDeclaration VertexDeclaration {
            get { return Declaration; }
        }

        public Vector3 Position;
        public Vector2 UV;
        public Vector4 UVRect;
        public Color Tint;

        public VertexVoxel(Vector3 pos, Vector2 uv, Vector4 uvr, Color c) {
            Position = pos;
            UV = uv;
            UVRect = uvr;
            Tint = c;
        }
    }
}
