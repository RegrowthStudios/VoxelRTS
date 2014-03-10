using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
namespace BlisterUI {
    /// <summary>
    /// The Vertex Type Used By The UI Shader
    /// </summary>
    public struct VertexUI : IVertexType {
        #region Vertex Declaration
        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector2, VertexElementUsage.Position, 0),
            new VertexElement(sizeof(float) * 2, VertexElementFormat.Vector2, VertexElementUsage.TextureCoordinate, 0),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.Color, 0)
            );
        public VertexDeclaration VertexDeclaration {
            get { return Declaration; }
        }
        #endregion

        public Vector2 Position;
        public Vector2 UV;
        public Vector4 Tint;

        public VertexUI(Vector2 pos, Vector2 uv, Vector4 tint) {
            Position = pos;
            UV = uv;
            Tint = tint;
        }
        public VertexUI(Vector2 pos, Vector2 uv)
            : this(pos, uv, Vector4.One) {
        }
        public VertexUI(Vector2 pos, Vector4 tint)
            : this(pos, Vector2.Zero, tint) {
        }
    }

    /// <summary>
    /// Represents A Layer On Which A Widget Is Drawn
    /// </summary>
    public sealed class DrawLayer : IComparable<DrawLayer> {
        public float height;
        public readonly List<VertexUI> vertices;
        public readonly List<int> indices;

        public DrawLayer(float h = 0f) {
            height = h;
            vertices = new List<VertexUI>();
            indices = new List<int>();
        }

        public DrawLayer addVerts(params VertexUI[] v) {
            vertices.AddRange(v);
            return this;
        }
        public DrawLayer addInds(params int[] i) {
            indices.AddRange(i);
            return this;
        }

        /// <summary>
        /// Compares The Heights Draw Layers
        /// </summary>
        /// <param name="other">Other Draw Layer</param>
        /// <returns>Float Comparison</returns>
        public int CompareTo(DrawLayer other) {
            return height.CompareTo(other.height);
        }
    }


    public sealed class DrawBatch : IDisposable {
        public const IndexElementSize IndexType = IndexElementSize.ThirtyTwoBits;
        public const int DefaultCapacity = 4 * 500;
        public const int MinCapacity = 4 * 10;
        public const int MaxCapacity = 4 * 100000;

        private List<DrawLayer> layers;
        private VertexBuffer vBuffer;
        private IndexBuffer iBuffer;
        private int vCount, iCount;
        public int Triangles {
            get { return iCount * 3; }
        }
        public bool CanDraw {
            get { return iCount > 0; }
        }

        public bool IsDisposed { get; private set; }

        public DrawBatch(GraphicsDevice g, int initialCapacity = DefaultCapacity) {
            // Make Sure Initial Capactiy Is Square Even And Greater Than The Minimum
            if(initialCapacity < MinCapacity) initialCapacity = MinCapacity * 10;
            initialCapacity -= initialCapacity % 4;

            vBuffer = new VertexBuffer(g, VertexUI.Declaration, initialCapacity, BufferUsage.WriteOnly);
            iBuffer = new IndexBuffer(g, IndexType, (initialCapacity / 2) * 3, BufferUsage.WriteOnly);

            iCount = 0;
            vCount = 0;
        }
        ~DrawBatch() {
            Dispose();
        }
        public void Dispose() {
            if(IsDisposed) return;
            IsDisposed = true;
            vBuffer.Dispose();
            iBuffer.Dispose();
        }

        public void begin() {
            layers = new List<DrawLayer>();
            vCount = 0;
            iCount = 0;
        }
        public void addLayer(DrawLayer l) {
            if(l == null) return;
            layers.Add(l);
            vCount += l.vertices.Count;
            iCount += l.indices.Count;
        }
        public void end(GraphicsDevice g) {
            // Sort By Height
            layers.Sort((l1, l2) => { return l1.CompareTo(l2); });

            // Check Buffer Size
            if((vCount <= vBuffer.VertexCount / 4) || (vCount > vBuffer.VertexCount)) {
                vBuffer.Dispose();
                vBuffer = new VertexBuffer(g, VertexUI.Declaration, vCount * 2, BufferUsage.WriteOnly);
            }
            if((iCount <= iBuffer.IndexCount / 4) || (iCount > iBuffer.IndexCount)) {
                iBuffer.Dispose();
                iBuffer = new IndexBuffer(g, IndexType, iCount * 2, BufferUsage.WriteOnly);
            }

            // Get Data
            VertexUI[] verts = new VertexUI[vCount];
            int[] inds = new int[iCount];
            int vi = 0, ii = 0;
            foreach(DrawLayer dl in layers) {
                foreach(int i in dl.indices) inds[ii++] = vi + i;
                foreach(VertexUI v in dl.vertices) verts[vi++] = v;
            }
            vBuffer.SetData(verts, 0, verts.Length);
            iBuffer.SetData(inds, 0, inds.Length);

            layers.Clear();
            layers = null;
        }
        public void draw(GraphicsDevice g) {
            g.SetVertexBuffer(vBuffer);
            g.Indices = iBuffer;
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vCount, 0, Triangles);
        }
    }
}
