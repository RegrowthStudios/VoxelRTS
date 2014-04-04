using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data.Team;
using RTSEngine.Controllers;

namespace RTSEngine.Graphics {
    public class RTSBuildingModel {
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
        public RTSColorScheme ColorScheme {
            get;
            set;
        }

        // Geometry Information
        private VertexBuffer vbModel;
        private IndexBuffer ibModel;

        // Instances That Will Be Animated
        public RTSBuildingData Data {
            get;
            private set;
        }
        private DynamicVertexBuffer dvbInstances;
        private bool rebuildDVB;
        private VertexRTSAnimInst[] instVerts;
        private List<RTSBuilding> instances;
        private List<RTSBuilding> visible;
        public int VisibleInstanceCount {
            get { return visible.Count; }
        }

        public RTSBuildingModel(RTSRenderer renderer, RTSTeam team, int buildingType, Stream sModel) {
            // Create With The Animation Texture
            Data = team.race.buildings[buildingType];
            team.OnBuildingSpawn += OnBuildingSpawn;

            // Parse The Model File
            VertexPositionNormalTexture[] pVerts;
            VertexPositionTexture[] verts;
            int[] inds;
            if(!ObjParser.TryParse(sModel, out pVerts, out inds, MODEL_READ_FLAGS))
                throw new ArgumentException("Bad Model File Format");
            verts = new VertexPositionTexture[pVerts.Length];
            for(int i = 0; i < verts.Length; i++) {
                verts[i].Position = pVerts[i].Position;
                verts[i].TextureCoordinate = pVerts[i].TextureCoordinate;
            }

            // Create Model Geometry
            ModelHelper.CreateBuffers(renderer, verts, VertexPositionTexture.VertexDeclaration, inds, out vbModel, out ibModel, BufferUsage.WriteOnly);

            // Create Instance Buffer
            visible = new List<RTSBuilding>();
            instVerts = new VertexRTSAnimInst[Data.MaxCount];
            instances = new List<RTSBuilding>(Data.MaxCount);
            for(int i = 0; i < instVerts.Length; i++)
                instVerts[i] = new VertexRTSAnimInst(Matrix.Identity, 0);
            dvbInstances = renderer.CreateDynamicVertexBuffer(VertexRTSAnimInst.Declaration, instVerts.Length, BufferUsage.WriteOnly);
            dvbInstances.SetData(instVerts);
            dvbInstances.ContentLost += (s, a) => { rebuildDVB = true; };
            rebuildDVB = false;
        }

        public void UpdateInstances(GraphicsDevice g, Predicate<RTSBuilding> fRemoval, Predicate<RTSBuilding> fVisible) {
            instances.RemoveAll(fRemoval);
            visible = new List<RTSBuilding>();
            for(int i = 0; i < instances.Count; i++) {
                if(fVisible(instances[i]))
                    visible.Add(instances[i]);
            }
            for(int i = 0; i < VisibleInstanceCount; i++) {
                instVerts[i].World =
                    Matrix.CreateRotationY(
                        (float)Math.Atan2(-visible[i].ViewDirection.Y, visible[i].ViewDirection.X)
                    ) *
                    Matrix.CreateTranslation(visible[i].WorldPosition)
                    ;
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
            if(VisibleInstanceCount > 0)
                g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, vbModel.VertexCount, 0, ibModel.IndexCount / 3, VisibleInstanceCount);
        }

        private void OnBuildingSpawn(RTSBuilding b) {
            if(b.BuildingData == Data)
                instances.Add(b);
        }
    }
}
