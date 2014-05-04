using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Content.Pipeline;
using Microsoft.Xna.Framework.Content.Pipeline.Graphics;
using Microsoft.Xna.Framework.Content.Pipeline.Processors;
using System.IO;
using RTSEngine.Controllers;
using RTSEngine.Graphics;
using Microsoft.Win32;

namespace Microsoft.Xna.Framework.Graphics {
    public static class RTSModelHelper {
        public static void CreateBuffers<T>(RTSRenderer renderer, T[] verts, VertexDeclaration vd, int[] inds, out VertexBuffer vb, out IndexBuffer ib, BufferUsage bu = BufferUsage.WriteOnly) where T : struct, IVertexType {
            vb = renderer.CreateVertexBuffer(vd, verts.Length, bu);
            vb.SetData(verts);
            ib = renderer.CreateIndexBuffer(IndexElementSize.ThirtyTwoBits, inds.Length, bu);
            ib.SetData(inds);
        }
        public static void CreateBuffers<T>(RTSRenderer renderer, T[] verts, VertexDeclaration vd, short[] inds, out VertexBuffer vb, out IndexBuffer ib, BufferUsage bu = BufferUsage.WriteOnly) where T : struct, IVertexType {
            vb = renderer.CreateVertexBuffer(vd, verts.Length, bu);
            vb.SetData(verts);
            ib = renderer.CreateIndexBuffer(IndexElementSize.SixteenBits, inds.Length, bu);
            ib.SetData(inds);
        }
    }
}