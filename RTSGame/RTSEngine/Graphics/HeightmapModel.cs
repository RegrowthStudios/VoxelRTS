using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public class HeightmapModel : IDisposable {
        // How To Convert When Reading The Heightmap
        private const ParsingFlags MODEL_READ_FLAGS = ParsingFlags.ConversionOpenGL;

        // Graphics Resources
        private VertexBuffer vbPrimary, vbSecondary;
        private IndexBuffer ibPrimary, ibSecondary;
        public Texture2D PrimaryTexture {
            get;
            set;
        }
        public Texture2D SecondaryTexture {
            get;
            set;
        }

        // Nice Information
        public int TrianglesPrimary {
            get { return ibPrimary == null ? 0 : ibPrimary.IndexCount / 3; }
        }
        public int TrianglesSecondary {
            get { return ibSecondary == null ? 0 : ibSecondary.IndexCount / 3; }
        }
        public int TriangleCount {
            get { return TrianglesPrimary + TrianglesSecondary; }
        }

        public HeightmapModel(GraphicsDevice g, Stream sPrimary, Stream sSecondary = null) {
            IsDisposed = false;
            vbPrimary = null;
            ibPrimary = null;
            PrimaryTexture = null;
            vbSecondary = null;
            ibSecondary = null;
            SecondaryTexture = null;

            VertexPositionNormalTexture[] vertsPNT;
            VertexPositionTexture[] verts;
            int[] inds;
            ObjParser.TryParse(sPrimary, out vertsPNT, out inds, MODEL_READ_FLAGS);
            verts = new VertexPositionTexture[vertsPNT.Length];
            for(int i = 0; i < vertsPNT.Length; i++) {
                verts[i].Position = vertsPNT[i].Position;
                verts[i].TextureCoordinate = vertsPNT[i].TextureCoordinate;
            }
            vbPrimary = new VertexBuffer(g, VertexPositionTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vbPrimary.SetData(verts);
            ibPrimary = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
            ibPrimary.SetData(inds);

            if(sSecondary != null) {
                ObjParser.TryParse(sSecondary, out vertsPNT, out inds, MODEL_READ_FLAGS);
                verts = new VertexPositionTexture[vertsPNT.Length];
                for(int i = 0; i < vertsPNT.Length; i++) {
                    verts[i].Position = vertsPNT[i].Position;
                    verts[i].TextureCoordinate = vertsPNT[i].TextureCoordinate;
                }
                vbSecondary = new VertexBuffer(g, VertexPositionTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
                vbSecondary.SetData(verts);
                ibSecondary = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
                ibSecondary.SetData(inds);
            }
        }
        #region Disposal
        ~HeightmapModel() {
            if(!IsDisposed) Dispose();
        }
        public event Action<object> OnDisposal;
        public bool IsDisposed {
            get;
            private set;
        }

        public void Dispose() {
            if(IsDisposed)
                throw new ObjectDisposedException("Heightmap Disposed Already");
            IsDisposed = true;
            if(OnDisposal != null) OnDisposal(this);

            // Dispose Buffers
            if(vbPrimary != null) {
                vbPrimary.Dispose();
                vbPrimary = null;
            }
            if(ibPrimary != null) {
                ibPrimary.Dispose();
                ibPrimary = null;
            }
            if(PrimaryTexture != null) {
                PrimaryTexture.Dispose();
                PrimaryTexture = null;
            }
            if(vbSecondary != null) {
                vbSecondary.Dispose();
                vbSecondary = null;
            }
            if(ibSecondary != null) {
                ibSecondary.Dispose();
                ibSecondary = null;
            }
            if(SecondaryTexture != null) {
                SecondaryTexture.Dispose();
                SecondaryTexture = null;
            }
        }
        #endregion

        // Drawing Functions
        public void SetPrimaryModel(GraphicsDevice g) {
            g.SetVertexBuffer(vbPrimary);
            g.Indices = ibPrimary;
        }
        public void DrawPrimary(GraphicsDevice g) {
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vbPrimary.VertexCount, 0, TrianglesPrimary);
        }
        public void SetSecondaryModel(GraphicsDevice g) {
            g.SetVertexBuffer(vbSecondary);
            g.Indices = ibSecondary;
        }
        public void DrawSecondary(GraphicsDevice g) {
            // Draw Only If Possible
            if(vbSecondary == null || ibSecondary == null) return;
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vbSecondary.VertexCount, 0, TrianglesPrimary);
        }
    }
}