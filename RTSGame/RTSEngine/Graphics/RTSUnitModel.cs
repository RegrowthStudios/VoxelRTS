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
    public class RTSUnitModel {
        public const ParsingFlags MODEL_READ_FLAGS = ParsingFlags.ConversionOpenGL;
        public const int MAX_DEAD = 100;

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
        private List<RTSUnit> dead;
        public int VisibleInstanceCount {
            get { return Math.Min(visible.Count + dead.Count, instVerts.Length); }
        }

        public RTSUnitModel(RTSRenderer renderer, Stream sModel, Texture2D tAnim) {
            // Create With The Animation Texture
            AnimationTexture = tAnim;

            // Parse The Model File
            VertexPositionNormalTexture[] pVerts;
            VertexPositionTexture[] verts;
            int[] inds;
            if(!ObjParser.TryParse(sModel, out pVerts, out inds, MODEL_READ_FLAGS))
                throw new ArgumentException("Bad Model File Format");

            // Reformat Vertices
            verts = new VertexPositionTexture[pVerts.Length];
            for(int i = 0; i < verts.Length; i++) {
                verts[i].Position = new Vector3((float)((i + 0.1) / AnimationTexture.Width), 0, 0);
                verts[i].TextureCoordinate = pVerts[i].TextureCoordinate;
            }

            // Create Model Geometry
            RTSModelHelper.CreateBuffers(renderer, verts, VertexPositionTexture.VertexDeclaration, inds, out vbModel, out ibModel, BufferUsage.WriteOnly);
        }

        public void Hook(RTSRenderer renderer, GameState s, int ti, int unit) {
            // Filter For Unit Types
            RTSTeam team = s.teams[ti];
            Data = team.Race.Units[unit];

            // Always Add A Unit To List When Spawned
            team.OnUnitSpawn += OnUnitSpawn;

            // Create Instance Buffer
            visible = new List<RTSUnit>();
            instVerts = new VertexRTSAnimInst[Data.MaxCount];
            instances = new List<RTSUnit>(Data.MaxCount);
            dead = new List<RTSUnit>();
            for(int i = 0; i < team.Units.Count; i++) {
                OnUnitSpawn(team.Units[i]);
            }

            for(int i = 0; i < instVerts.Length; i++)
                instVerts[i] = new VertexRTSAnimInst(Matrix.Identity, 0);
            dvbInstances = renderer.CreateDynamicVertexBuffer(VertexRTSAnimInst.Declaration, instVerts.Length, BufferUsage.WriteOnly);
            dvbInstances.SetData(instVerts);
            dvbInstances.ContentLost += (sender, args) => { rebuildDVB = true; };
            rebuildDVB = false;
        }

        public void UpdateInstances(GraphicsDevice g, Predicate<RTSUnit> fRemoval, Predicate<RTSUnit> fVisible) {
            var oldInst = instances;
            instances = new List<RTSUnit>(instances.Capacity);
            for(int i = 0; i < oldInst.Count; i++) {
                if(fRemoval(oldInst[i]))
                    dead.Add(oldInst[i]);
                else
                    instances.Add(oldInst[i]);
            }
            if(dead.Count > MAX_DEAD)
                dead.RemoveRange(0, dead.Count - MAX_DEAD);

            visible = new List<RTSUnit>();
            for(int i = 0; i < instances.Count; i++) {
                if(fVisible(instances[i]))
                    visible.Add(instances[i]);
            }
            int vi;
            for(vi = 0; vi < visible.Count; vi++) {
                instVerts[vi].World =
                    Matrix.CreateRotationY(
                        (float)Math.Atan2(-visible[vi].ViewDirection.Y, visible[vi].ViewDirection.X)
                    ) *
                    Matrix.CreateTranslation(visible[vi].WorldPosition)
                    ;
                instVerts[vi].AnimationFrame = visible[vi].AnimationController == null ? 0 : visible[vi].AnimationController.AnimationFrame;
            }
            for(int i = 0; i < dead.Count && vi < instVerts.Length; vi++, i++) {
                instVerts[vi].World =
                    Matrix.CreateRotationY(
                        (float)Math.Atan2(-dead[i].ViewDirection.Y, dead[i].ViewDirection.X)
                    ) *
                    Matrix.CreateTranslation(dead[i].WorldPosition)
                    ;
                instVerts[vi].AnimationFrame = dead[i].AnimationController == null ? 0 : dead[i].AnimationController.AnimationFrame;
            }
            if(rebuildDVB) {
                dvbInstances = new DynamicVertexBuffer(g, VertexRTSAnimInst.Declaration, instVerts.Length, BufferUsage.WriteOnly);
                dvbInstances.ContentLost += (s, a) => { rebuildDVB = true; };
            }
            dvbInstances.SetData(instVerts);
        }

        public void Animate(GameState s, float dt, List<Particle> lp) {
            for(int i = 0; i < instances.Count; i++) {
                if(instances[i].AnimationController != null) {
                    instances[i].AnimationController.Update(s, dt);
                    if(instances[i].AnimationController.HasParticles)
                        instances[i].AnimationController.GetParticles(lp);
                }
            }
            for(int i = 0; i < dead.Count; i++) {
                if(dead[i].AnimationController != null) {
                    dead[i].AnimationController.Update(s, dt);
                    if(dead[i].AnimationController.HasParticles)
                        dead[i].AnimationController.GetParticles(lp);
                }
            }
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

        public void OnUnitSpawn(RTSUnit u) {
            if(u.Data == Data)
                instances.Add(u);
        }
    }
}