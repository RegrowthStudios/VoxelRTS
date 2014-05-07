using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Grey.Engine;
using Grey.Graphics;
using Grey.Vox;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public struct VoxMapConfig {
        public VoxState VoxState;
        public string RootPath;
        public string FXFile;
        public string TexVoxMap;
    }

    public class VoxMap : IDisposable {
        private VoxelRenderer vRenderer;

        public Texture2D FogOfWarTexture;
        public float[] FogOfWar;
        private int FW, FH;
        public bool Reset {
            get;
            private set;
        }

        public VoxMap(RTSRenderer renderer, int fw, int fh) {
            FW = fw;
            FH = fh;
            FogOfWar = new float[FW * FH];
            Array.Clear(FogOfWar, 0, FogOfWar.Length);
            Reset = false;
            FogOfWarTexture = renderer.CreateTexture2D(fw, fh, SurfaceFormat.Single, false);
            FogOfWarTexture.SetData(FogOfWar);
        }
        public void Dispose() {
            vRenderer.Dispose();
        }

        public void Build(GraphicsDeviceManager gdm, VoxMapConfig vc) {
            vRenderer = new VoxelRenderer(gdm);
            vRenderer.LoadEffect(vc.FXFile);
            vRenderer.LoadVMap(Path.Combine(vc.RootPath, vc.TexVoxMap));
            vRenderer.Hook(vc.VoxState);
            foreach(var r in vc.VoxState.World.regions) {
                if(r != null) {
                    vc.VoxState.VWorkPool.AddWork(new RegionTesselator(r, vRenderer));
                }
            }
        }

        public void SetFOW(int x, int y, float f) {
            FogOfWar[y * FW + x] = f;
            Reset = true;
        }
        public void ApplyFOW() {
            Reset = false;
            FogOfWarTexture.SetData(FogOfWar);
        }

        public void Update() {
            vRenderer.RetaskVisualChanges();
        }

        public void Draw(GraphicsDevice g, Matrix mView, Matrix mProj) {
            g.Textures[1] = FogOfWarTexture;
            g.SamplerStates[1] = SamplerState.PointClamp;
            vRenderer.DrawAll(mView, mProj);
        }
    }
}