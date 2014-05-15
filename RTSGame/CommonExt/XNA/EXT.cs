using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;

namespace Microsoft.Xna.Framework.Graphics {
    public static class ModelHelper {
        public static void CreateBuffers<T>(GraphicsDevice g, T[] verts, VertexDeclaration vd, int[] inds, out VertexBuffer vb, out IndexBuffer ib, BufferUsage bu = BufferUsage.WriteOnly) where T : struct, IVertexType {
            vb = new VertexBuffer(g, vd, verts.Length, bu);
            vb.SetData(verts);
            ib = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, inds.Length, bu);
            ib.SetData(inds);
        }
        public static void CreateBuffers<T>(GraphicsDevice g, T[] verts, VertexDeclaration vd, short[] inds, out VertexBuffer vb, out IndexBuffer ib, BufferUsage bu = BufferUsage.WriteOnly) where T : struct, IVertexType {
            vb = new VertexBuffer(g, vd, verts.Length, bu);
            vb.SetData(verts);
            ib = new IndexBuffer(g, IndexElementSize.SixteenBits, inds.Length, bu);
            ib.SetData(inds);
        }
    }
}

namespace Microsoft.Xna.Framework {
    public static class SerializationHelper {
        public static void Write(this BinaryWriter s, Vector2 v) {
            s.Write(v.X);
            s.Write(v.Y);
        }
        public static void Write(this BinaryWriter s, Point p) {
            s.Write(p.X);
            s.Write(p.Y);
        }
        public static void Write(this BinaryWriter s, Vector3 v) {
            s.Write(v.X);
            s.Write(v.Y);
            s.Write(v.Z);
        }
        public static void Write(this BinaryWriter s, Vector4 v) {
            s.Write(v.X);
            s.Write(v.Y);
            s.Write(v.Z);
            s.Write(v.W);
        }
        public static void Write(this BinaryWriter s, Rectangle r) {
            s.Write(r.X);
            s.Write(r.Y);
            s.Write(r.Width);
            s.Write(r.Height);
        }

        public static Vector2 ReadVector2(this BinaryReader s) {
            float x = s.ReadSingle();
            float y = s.ReadSingle();
            return new Vector2(x, y);
        }
        public static Point ReadPoint(this BinaryReader s) {
            int x = s.ReadInt32();
            int y = s.ReadInt32();
            return new Point(x, y);
        }
        public static Vector3 ReadVector3(this BinaryReader s) {
            float x = s.ReadSingle();
            float y = s.ReadSingle();
            float z = s.ReadSingle();
            return new Vector3(x, y, z);
        }
        public static Vector4 ReadVector4(this BinaryReader s) {
            float x = s.ReadSingle();
            float y = s.ReadSingle();
            float z = s.ReadSingle();
            float w = s.ReadSingle();
            return new Vector4(x, y, z, w);
        }
        public static Rectangle ReadRectangle(this BinaryReader s) {
            int x = s.ReadInt32();
            int y = s.ReadInt32();
            int z = s.ReadInt32();
            int w = s.ReadInt32();
            return new Rectangle(x, y, z, w);
        }
    }
}
