using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data.Team;
using RTSEngine.Controllers;
using RTSEngine.Data;

namespace RTSEngine.Graphics {
    // TODO: Animations Applied By A Controller From A Check On A Frame (Specified In File)
    public class RTSUnitModel {
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
        private List<RTSUnit> visible;
        public int VisibleInstanceCount {
            get { return visible.Count; }
        }

        public RTSUnitModel(RTSRenderer renderer, Stream sModel, Texture2D tAnim) {
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
                verts[i].Position = new Vector3((i + 0.5f) / AnimationTexture.Width, 0, 0);
                verts[i].TextureCoordinate = pVerts[i].TextureCoordinate;
            }

            // Create Model Geometry
            ModelHelper.CreateBuffers(renderer, verts, VertexPositionTexture.VertexDeclaration, inds, out vbModel, out ibModel, BufferUsage.WriteOnly);
        }

        public void Hook(RTSRenderer renderer, GameState s, int team, int unit) {
            // Filter For Unit Types
            Data = s.teams[team].race.Units[unit];

            // Always Add A Unit To List When Spawned
            s.teams[team].OnUnitSpawn += OnUnitSpawn;

            // Create Instance Buffer
            visible = new List<RTSUnit>();
            instVerts = new VertexRTSAnimInst[Data.MaxCount];
            instances = new List<RTSUnit>(Data.MaxCount);
            for(int i = 0; i < instVerts.Length; i++)
                instVerts[i] = new VertexRTSAnimInst(Matrix.Identity, 0);
            dvbInstances = renderer.CreateDynamicVertexBuffer(VertexRTSAnimInst.Declaration, instVerts.Length, BufferUsage.WriteOnly);
            dvbInstances.SetData(instVerts);
            dvbInstances.ContentLost += (sender, args) => { rebuildDVB = true; };
            rebuildDVB = false;
        }

        public void UpdateInstances(GraphicsDevice g, Predicate<RTSUnit> fRemoval, Predicate<RTSUnit> fVisible) {
            instances.RemoveAll(fRemoval);
            visible = new List<RTSUnit>();
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
                instVerts[i].AnimationFrame = visible[i].AnimationController == null ? 0 : visible[i].AnimationController.AnimationFrame;
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

        private void OnUnitSpawn(RTSUnit u) {
            if(u.UnitData == Data)
                instances.Add(u);
        }
    }
}