using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data.Team;

namespace RTSEngine.Graphics {

    // TODO: Animations Applied By A Controller From A Check On A Frame (Specified In File)

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
        public RTSUnitData Data {
            get;
            private set;
        }
        private DynamicVertexBuffer dvbInstances;
        private bool rebuildDVB;
        private VertexRTSAnimInst[] instVerts;
        private List<RTSUnit> instances;
        public int InstanceCount {
            get { return instances.Count; }
        }

        public RTSUnitModel(GraphicsDevice g, RTSUnitData data, Stream sModel, Texture2D tAnim) {
            // Create With The Animation Texture
            AnimationTexture = tAnim;
            Vector2 texelSize = new Vector2(1f / (AnimationTexture.Width), 1f / (AnimationTexture.Height));
            Data = data;

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
            instVerts = new VertexRTSAnimInst[Data.MaxCount];
            instances = new List<RTSUnit>(Data.MaxCount);
            for(int i = 0; i < instVerts.Length; i++)
                instVerts[i] = new VertexRTSAnimInst(Matrix.Identity, 0);
            dvbInstances = new DynamicVertexBuffer(g, VertexRTSAnimInst.Declaration, instVerts.Length, BufferUsage.WriteOnly);
            dvbInstances.SetData(instVerts);
            dvbInstances.ContentLost += (s, a) => { rebuildDVB = true; };
            rebuildDVB = false;
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

        public void UpdateInstances(GraphicsDevice g) {
            instances.RemoveAll(RTSEngine.Controllers.GameplayController.IsEntityDead);

            for(int i = 0; i < InstanceCount; i++) {
                instVerts[i].World =
                    Matrix.CreateRotationY(
                        (float)Math.Atan2(-instances[i].ViewDirection.Y, instances[i].ViewDirection.X)
                    ) *
                    Matrix.CreateTranslation(instances[i].WorldPosition)
                    ;
                instVerts[i].AnimationFrame = instances[i].AnimationController.AnimationFrame;
            }
            if(rebuildDVB) {
                dvbInstances = new DynamicVertexBuffer(g, VertexRTSAnimInst.Declaration, instVerts.Length, BufferUsage.WriteOnly);
                dvbInstances.ContentLost += (s, a) => { rebuildDVB = true; };
            }
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

        public void OnUnitSpawn(RTSUnit u) {
            if(u.UnitData == Data)
                instances.Add(u);
        }
    }
}