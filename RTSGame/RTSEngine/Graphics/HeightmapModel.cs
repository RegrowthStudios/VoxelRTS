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

        public HeightmapModel(GraphicsDevice g, Vector3 size, Action<VertexPositionTexture[], int[]> f, Stream sPrimary, Stream sSecondary = null) {
            IsDisposed = false;
            vbPrimary = null;
            ibPrimary = null;
            PrimaryTexture = null;
            vbSecondary = null;
            ibSecondary = null;
            SecondaryTexture = null;

            // Parsing Information
            BoundingBox aabb = new BoundingBox(new Vector3(float.MaxValue), new Vector3(-float.MaxValue));
            Vector3 scaling = size;
            VertexPositionNormalTexture[] vertsPNT;
            VertexPositionTexture[] verts;
            int[] inds;

            // Read The Map Model
            ObjParser.TryParse(sPrimary, out vertsPNT, out inds, MODEL_READ_FLAGS);
            verts = new VertexPositionTexture[vertsPNT.Length];
            for(int i = 0; i < vertsPNT.Length; i++) {
                // Copy Over Information
                verts[i].Position = vertsPNT[i].Position;
                verts[i].TextureCoordinate = vertsPNT[i].TextureCoordinate;

                // Calculate Bounding Box
                if(verts[i].Position.X > aabb.Max.X) aabb.Max.X = verts[i].Position.X;
                if(verts[i].Position.X < aabb.Min.X) aabb.Min.X = verts[i].Position.X;
                if(verts[i].Position.Y > aabb.Max.Y) aabb.Max.Y = verts[i].Position.Y;
                if(verts[i].Position.Y < aabb.Min.Y) aabb.Min.Y = verts[i].Position.Y;
                if(verts[i].Position.Z > aabb.Max.Z) aabb.Max.Z = verts[i].Position.Z;
                if(verts[i].Position.Z < aabb.Min.Z) aabb.Min.Z = verts[i].Position.Z;
            }
            // Find Scaling
            scaling /= aabb.Max - aabb.Min;

            // Reposition Model
            for(int i = 0; i < verts.Length; i++) {
                // Move Model Minimum To Origin
                verts[i].Position -= aabb.Min;
                // Scale Heights To [0,1]
                verts[i].Position *= scaling;
            }

            // Create Primary Geometry
            vbPrimary = new VertexBuffer(g, VertexPositionTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vbPrimary.SetData(verts);
            ibPrimary = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
            ibPrimary.SetData(inds);
            f(verts, inds);

            if(sSecondary != null) {
                // Read The Detail Model
                ObjParser.TryParse(sSecondary, out vertsPNT, out inds, MODEL_READ_FLAGS);
                verts = new VertexPositionTexture[vertsPNT.Length];
                for(int i = 0; i < vertsPNT.Length; i++) {
                    verts[i].Position = vertsPNT[i].Position;
                    verts[i].TextureCoordinate = vertsPNT[i].TextureCoordinate;
                }


                // Reposition Detail Model To Match The Primary Model
                for(int i = 0; i < verts.Length; i++) {
                    // Move Model Minimum To Origin
                    verts[i].Position -= aabb.Min;
                    // Scale Heights To [0,1]
                    verts[i].Position *= scaling;
                }

                // Create Graphics Geometry
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