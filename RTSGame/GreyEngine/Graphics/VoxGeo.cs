using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grey.Vox;
using Microsoft.Xna.Framework.Graphics;

namespace Grey.Graphics {
    public class VoxGeo : IDisposable {
        public Region Region;
        public VertexBuffer VB;
        public IndexBuffer IB;

        public void Dispose() {
            VB.Dispose();
            IB.Dispose();
        }
    }
}
