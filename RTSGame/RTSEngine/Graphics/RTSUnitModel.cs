using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public class RTSUnitModel : IDisposable {
        public const ParsingFlags MODEL_READ_FLAGS = ParsingFlags.ConversionOpenGL;

        // Visual Information
        public Texture2D ModelTexture {
            get;
            set;
        }
        public Texture2D ColorCodeTexture {
            get;
            set;
        }
        public Vector3 ColorPrimary {
            get;
            set;
        }
        public Vector3 ColorSecondary {
            get;
            set;
        }
        public Vector3 ColorTertiary {
            get;
            set;
        }

        // Geometry Information
        private VertexBuffer vbModel;
        private IndexBuffer ibModel;
        public Texture2D AnimationTexture {
            get;
            set;
        }

        // Instances That Will Be Animated
        private DynamicVertexBuffer dvbInstances;
        private VertexRTSAnimInst[] instVerts;
        public int InstanceCount {
            get;
            private set;
        }

        public RTSUnitModel(GraphicsDevice g, Stream sModel, Texture2D tAnim, int maxInstanceCount) {
            // Create With The Animation Texture
            AnimationTexture = tAnim;
            Vector2 texelSize = new Vector2(1f / (AnimationTexture.Width), 1f / (AnimationTexture.Height));

            // Parse The Model File
            VertexPositionNormalTexture[] pVerts;
            VertexPositionTexture[] verts;
            int[] inds;
            if(!ObjParser.TryParse(sModel, out pVerts, out inds, MODEL_READ_FLAGS))
                throw new ArgumentException("Bad Model File Format");

            // Reformat Vertices
            verts = new VertexPositionTexture[pVerts.Length];
            for(int i = 0; i < verts.Length; i++) {
                verts[i].Position = new Vector3((i + 0.5f) * texelSize.X, 0, 0);
                verts[i].TextureCoordinate = pVerts[i].TextureCoordinate;
            }

            // Create Model Geometry
            vbModel = new VertexBuffer(g, VertexPositionTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vbModel.SetData(verts);
            ibModel = new IndexBuffer(g, IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
            ibModel.SetData(inds);

            // Create Instance Buffer
            instVerts = new VertexRTSAnimInst[maxInstanceCount];
            for(int i = 0; i < instVerts.Length; i++)
                instVerts[i] = new VertexRTSAnimInst(Matrix.Identity, 0);
            dvbInstances = new DynamicVertexBuffer(g, VertexRTSAnimInst.Declaration, instVerts.Length, BufferUsage.WriteOnly);
            dvbInstances.SetData(instVerts);
            InstanceCount = 0;
        }
        public void Dispose() {
            if(vbModel != null) {
                vbModel.Dispose();
                vbModel = null;
            }
            if(ibModel != null) {
                ibModel.Dispose();
                ibModel = null;
            }
            if(dvbInstances != null) {
                dvbInstances.Dispose();
                dvbInstances = null;
            }
            if(ModelTexture != null) {
                ModelTexture.Dispose();
                ModelTexture = null;
            }
            if(ColorCodeTexture != null) {
                ColorCodeTexture.Dispose();
                ColorCodeTexture = null;
            }
            if(AnimationTexture != null) {
                AnimationTexture.Dispose();
                AnimationTexture = null;
            }
        }

        public void UpdateInstances(IEnumerable<VertexRTSAnimInst> instances) {
            InstanceCount = 0;
            foreach(var inst in instances)
                instVerts[InstanceCount++] = inst;
            dvbInstances.SetData(instVerts);
        }
        public void SetInstances(GraphicsDevice g) {
            g.SetVertexBuffers(
                new VertexBufferBinding(vbModel),
                new VertexBufferBinding(dvbInstances, 0, 1)
                );
            g.Indices = ibModel;
        }
        public void DrawInstances(GraphicsDevice g) {
            if(InstanceCount > 0)
                g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, vbModel.VertexCount, 0, ibModel.IndexCount / 3, InstanceCount);
        }
    }
}