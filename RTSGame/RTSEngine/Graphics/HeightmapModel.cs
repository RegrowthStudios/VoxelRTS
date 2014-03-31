using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public class HeightmapModel {
        // Graphics Resources
        public VertexBuffer VBPrimary, VBSecondary;
        public IndexBuffer IBPrimary, IBSecondary;
        public Texture2D PrimaryTexture;
        public Texture2D SecondaryTexture;

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