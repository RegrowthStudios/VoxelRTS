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
    public class VisibleBuilding {
        public Vector3 Position;
        public Vector2 View;
        public float BuildAmount;
    }

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
        private List<VisibleBuilding> visible;
        public int VisibleInstanceCount {
            get { return visible.Count; }
        }

        private RTSTeam fTeam;
        private int fTeamIndex, eTeamIndex, bType;

        public RTSBuildingModel(RTSRenderer renderer, Stream sModel) {
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
            RTSModelHelper.CreateBuffers(renderer, verts, VertexPositionTexture.VertexDeclaration, inds, out vbModel, out ibModel, BufferUsage.WriteOnly);
        }

        public void Hook(RTSRenderer renderer, GameState s, int ti, int fti, int building) {
            // Filter For Unit Types
            RTSTeam team = s.teams[ti];
            Data = team.Race.Buildings[building];
            fTeam = s.teams[fti];
            eTeamIndex = ti;
            fTeamIndex = fti;
            bType = building;

            // Create Instance Buffer
            visible = new List<VisibleBuilding>();
            instVerts = new VertexRTSAnimInst[Data.MaxCount];

            for(int i = 0; i < instVerts.Length; i++)
                instVerts[i] = new VertexRTSAnimInst(Matrix.Identity, 1);
            dvbInstances = renderer.CreateDynamicVertexBuffer(VertexRTSAnimInst.Declaration, instVerts.Length, BufferUsage.WriteOnly);
            dvbInstances.SetData(instVerts);
            dvbInstances.ContentLost += (sender, args) => { rebuildDVB = true; };
            rebuildDVB = false;
        }

        public void UpdateInstances(GraphicsDevice g, Predicate<BoundingBox> fVisible) {
            visible = new List<VisibleBuilding>();
            if(fTeamIndex == eTeamIndex) {
                // Always Show Friendly Buildings
                for(int i = 0; i < fTeam.Buildings.Count; i++) {
                    if(fTeam.Buildings[i].Data != Data) continue;
                    if(fVisible(fTeam.Buildings[i].BBox)) {
                        VisibleBuilding vb = new VisibleBuilding();
                        vb.Position = fTeam.Buildings[i].WorldPosition;
                        vb.View = fTeam.Buildings[i].ViewDirection;
                        vb.BuildAmount = 1 - (float)fTeam.Buildings[i].BuildAmountLeft / (float)fTeam.Buildings[i].Data.BuildAmount;
                        if(vb.BuildAmount < 0.5) {
                            visible.Add(vb);
                        }
                        else {
                            visible.Add(vb);
                        }
                    }
                }
            }
            else {
                for(int i = 0; i < fTeam.ViewedEnemyBuildings.Count; i++) {
                    ViewedBuilding bv = fTeam.ViewedEnemyBuildings[i];
                    if(bv.Team != eTeamIndex || bv.Type != bType) continue;

                    BoundingBox bb = new BoundingBox(
                        Data.BBox.Min + bv.WorldPosition,
                        Data.BBox.Max + bv.WorldPosition
                        );
                    if(fVisible(bb)) {
                        VisibleBuilding vb = new VisibleBuilding();
                        vb.Position = bv.WorldPosition;
                        vb.View = bv.ViewDirection;
                        vb.BuildAmount = bv.BuildAmount;
                        visible.Add(vb);
                    }
                }
            }


            for(int i = 0; i < VisibleInstanceCount; i++) {
                instVerts[i].World =
                    Matrix.CreateRotationY(
                        (float)Math.Atan2(-visible[i].View.Y, visible[i].View.X)
                    ) *
                    Matrix.CreateTranslation(visible[i].Position)
                    ;
                instVerts[i].AnimationFrame = visible[i].BuildAmount;
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
    }
}