using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grey.Engine;
using Grey.Vox;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Grey.Graphics {
    public class VoxelRenderer : IDisposable {
        private GraphicsDeviceManager graphics;
        public GraphicsDevice G {
            get { return graphics.GraphicsDevice; }
        }

        private object lckDraw = new object();

        public Effect FX {
            get;
            private set;
        }
        EffectParameter fxpVP, fxpWorld;

        public Texture2D VoxelMap {
            get;
            private set;
        }

        List<Region> activeRegions;
        List<VoxGeo> geos;

        public VoxelRenderer(GraphicsDeviceManager gdm) {
            graphics = gdm;
            activeRegions = new List<Region>();
            geos = new List<VoxGeo>();
        }
        public void Dispose() {
            FX.Dispose();
            foreach(var vGeo in geos) vGeo.Dispose();
            VoxelMap.Dispose();
        }

        public void Hook(VoxState state) {
            state.World.OnRegionAddition += OnRegionAddition;
            state.World.OnRegionDeletion += OnRegionRemoval;
        }
        public void LoadVMap(string file) {
            using(var s = System.IO.File.OpenRead(file))
                VoxelMap = Texture2D.FromStream(G, s);
        }
        public void LoadEffect(string file) {
            FX = XNAEffect.Compile(G, file);
            FX.CurrentTechnique = FX.Techniques[0];
            fxpVP = FX.Parameters["VP"];
            fxpWorld = FX.Parameters["World"];
        }
        public void AddRegionGeo(VoxGeo g) {
            lock(lckDraw) {
                var nl = new List<VoxGeo>(geos.Count + 1);
                for(int i = 0; i < geos.Count; i++) {
                    if(geos[i].Region != g.Region) nl.Add(geos[i]);
                    else geos[i].Dispose();
                }
                geos = nl;
                geos.Add(g);
            }
        }

        public void DrawAll(Matrix mWorld, Matrix mView, Matrix mProj) {
            G.DepthStencilState = DepthStencilState.Default;
            G.BlendState = BlendState.Opaque;
            G.RasterizerState = RasterizerState.CullNone;

            fxpVP.SetValue(mView * mProj);

            G.Textures[0] = VoxelMap;
            G.SamplerStates[0] = SamplerState.PointClamp;
            lock(lckDraw) {
                foreach(var vGeo in geos) {
                    fxpWorld.SetValue(Matrix.CreateTranslation(vGeo.Region.loc.X * Region.WIDTH, 0, vGeo.Region.loc.Y * Region.DEPTH) * mWorld);
                    FX.CurrentTechnique.Passes[0].Apply();
                    G.SetVertexBuffer(vGeo.VB);
                    G.Indices = vGeo.IB;
                    G.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vGeo.VB.VertexCount, 0, vGeo.IB.IndexCount / 3);
                }
            }
        }
        public void DrawAll(Matrix mView, Matrix mProj) {
            DrawAll(Matrix.Identity, mView, mProj);
        }

        void OnRegionAddition(VoxWorld w, Region r) {
            lock(activeRegions) {
                activeRegions.Add(r);
            }
        }
        void OnRegionRemoval(VoxWorld w, Region r) {
            lock(activeRegions) {
                activeRegions.Remove(r);
            }
        }

        public void RetaskVisualChanges() {
            lock(activeRegions) {
                for(int i = 0; i < activeRegions.Count; i++) {
                    var r = activeRegions[i];
                    int cc = System.Threading.Interlocked.Exchange(ref r.changedCount, 0);

                    // This Region Needs To Be Recreated
                    if(cc != 0) r.World.state.VWorkPool.AddWork(new RegionTesselator(r, this));
                }
            }
        }
    }
}