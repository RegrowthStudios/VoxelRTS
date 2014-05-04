using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Controllers;

namespace RTSEngine.Graphics {
    public class HeightmapModel {
        // Graphics Resources
        public VertexBuffer VBPrimary, VBSecondary;
        public IndexBuffer IBPrimary, IBSecondary;
        public Texture2D PrimaryTexture;
        public Texture2D SecondaryTexture;
        public Texture2D FogOfWarTexture;
        public float[] FogOfWar;
        private int FW, FH;
        public bool Reset {
            get;
            private set;
        }

        public BVH BVH {
            get;
            private set;
        }

        public HeightmapModel(RTSRenderer renderer, int fw, int fh) {
            FW = fw;
            FH = fh;
            FogOfWar = new float[FW * FH];
            Array.Clear(FogOfWar, 0, FogOfWar.Length);
            BVH = new Graphics.BVH();
            Reset = false;
            FogOfWarTexture = renderer.CreateTexture2D(fw, fh, SurfaceFormat.Single, false);
            FogOfWarTexture.SetData(FogOfWar);
        }

        public void SetFOW(int x, int y, float f) {
            FogOfWar[y * FW + x] = f;
            Reset = true;
        }
        public void ApplyFOW() {
            Reset = false;
            FogOfWarTexture.SetData(FogOfWar);
        }

        // Nice Information
        public int TrianglesPrimary {
            get { return IBPrimary == null ? 0 : IBPrimary.IndexCount / 3; }
        }
        public int TrianglesSecondary {
            get { return IBSecondary == null ? 0 : IBSecondary.IndexCount / 3; }
        }
        public int TriangleCount {
            get { return TrianglesPrimary + TrianglesSecondary; }
        }
    }
}