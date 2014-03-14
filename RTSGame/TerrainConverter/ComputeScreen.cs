using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;

namespace TerrainConverter {
    public class ComputeScreen : GameScreenIndexed {
        public ComputeScreen(int i) : base(i) { }
        public ComputeScreen(int p, int n) : base(p, n) { }

        private string fIn, fOut;
        private bool ShouldRebuild {
            get { return fIn != null && fOut != null; }
            set {
                if(!value) {
                    fIn = null;
                    fOut = null;
                }
            }
        }
        private BasicEffect fx;
        private RenderTarget2D rtHeight;

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;
            ShouldRebuild = false;

            rtHeight = null;
            fx = new BasicEffect(G);
            fx.DiffuseColor = Vector3.UnitZ;
            fx.TextureEnabled = false;
            fx.LightingEnabled = false;
            fx.VertexColorEnabled = false;
            fx.FogEnabled = true;
            fx.FogColor = Vector3.UnitX;
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;

            fx.Dispose();
            fx = null;
            if(rtHeight != null) {
                rtHeight.Dispose();
                rtHeight = null;
            }
        }

        public override void Update(GameTime gameTime) {
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);
            if(ShouldRebuild)
                Rebuild(fIn, fOut);

            if(rtHeight != null) {
                SB.Begin();
                SB.Draw(rtHeight, G.Viewport.TitleSafeArea, Color.White);
                SB.End();
            }
        }

        private BoundingBox ComputeAABB(VertexPositionNormalTexture[] v) {
            BoundingBox aabb = new BoundingBox(v[0].Position, v[0].Position);

            foreach(var vert in v) {
                if(vert.Position.X > aabb.Max.X) aabb.Max.X = vert.Position.X;
                if(vert.Position.X < aabb.Min.X) aabb.Min.X = vert.Position.X;
                if(vert.Position.Y > aabb.Max.Y) aabb.Max.Y = vert.Position.Y;
                if(vert.Position.Y < aabb.Min.Y) aabb.Min.Y = vert.Position.Y;
                if(vert.Position.Z > aabb.Max.Z) aabb.Max.Z = vert.Position.Z;
                if(vert.Position.Z < aabb.Min.Z) aabb.Min.Z = vert.Position.Z;
            }

            return aabb;
        }
        private void Rebuild(string fIn, string fOut) {
            // Check Args
            FileInfo fiIn = new FileInfo(fIn);
            if(!fiIn.Exists) {
                Console.WriteLine("File Does Not Exist");
                return;
            }
            FileInfo fiOut = new FileInfo(fOut);
            if(!fiOut.Directory.Exists) {
                Console.WriteLine("Output Directory Does Not Exist");
                return;
            }

            // Read Model
            Stream s = File.OpenRead(fiIn.FullName);
            VertexPositionNormalTexture[] verts;
            int[] inds;
            if(!ObjParser.TryParse(s, out verts, out inds, ParsingFlags.ConversionOpenGL)) {
                s.Dispose();
                Console.WriteLine("Could Not Read Model");
                return;
            }
            s.Dispose();

            // Compute The AABB Of The Terrain
            BoundingBox aabb = ComputeAABB(verts);
            Vector3 mid = aabb.Max + aabb.Min;
            Vector3 dif = aabb.Max - aabb.Min;
            Vector3 top = new Vector3(mid.X, aabb.Max.Y, mid.Z);
            mid *= 0.5f;
            fx.FogStart = 1f;
            fx.FogEnd = aabb.Max.Y - aabb.Min.Y + 1f;
            fx.World = Matrix.Identity;
            fx.View = Matrix.CreateLookAt(top + Vector3.UnitY, mid, -Vector3.UnitZ);
            fx.Projection = Matrix.CreateOrthographic(dif.X, dif.Z, 0, dif.Y + 2f);

            // Append A Plane At The Bottom
            int vc = verts.Length, ic = inds.Length;
            Array.Resize(ref verts, verts.Length + 4);
            Array.Resize(ref inds, inds.Length + 6);
            inds[ic++] = vc + 0;
            inds[ic++] = vc + 1;
            inds[ic++] = vc + 2;
            inds[ic++] = vc + 2;
            inds[ic++] = vc + 1;
            inds[ic++] = vc + 3;
            verts[vc++] = new VertexPositionNormalTexture(
                new Vector3(aabb.Min.X, aabb.Min.Y, aabb.Min.Z),
                Vector3.UnitY, Vector2.Zero
                );
            verts[vc++] = new VertexPositionNormalTexture(
                new Vector3(aabb.Max.X, aabb.Min.Y, aabb.Min.Z),
                Vector3.UnitY, Vector2.UnitX
                );
            verts[vc++] = new VertexPositionNormalTexture(
                new Vector3(aabb.Min.X, aabb.Min.Y, aabb.Max.Z),
                Vector3.UnitY, Vector2.UnitY
                );
            verts[vc++] = new VertexPositionNormalTexture(
                new Vector3(aabb.Max.X, aabb.Min.Y, aabb.Max.Z),
                Vector3.UnitY, Vector2.One
                );

            // Create Model
            VertexBuffer vb = new VertexBuffer(G, VertexPositionNormalTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vb.SetData(verts);
            IndexBuffer ib = new IndexBuffer(G, IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
            ib.SetData(inds);

            // Render The Height
            if(rtHeight != null)
                rtHeight.Dispose();
            rtHeight = new RenderTarget2D(G, 4096, 4096, false, SurfaceFormat.Color, DepthFormat.Depth24);
            G.SetRenderTarget(rtHeight);
            G.SetVertexBuffer(vb);
            G.Indices = ib;
            fx.CurrentTechnique.Passes[0].Apply();
            G.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, vb.VertexCount, 0, ib.IndexCount / 3);

            // Dispose Of Buffers
            G.SetRenderTarget(null);
            G.Clear(Color.Black);
            G.SetVertexBuffer(null);
            G.Indices = null;
            vb.Dispose();
            ib.Dispose();

            // Save The Image
            using(Stream os = File.Create(fiOut.FullName)) {
                rtHeight.SaveAsPng(os, rtHeight.Width, rtHeight.Height);
            }

            ShouldRebuild = false;
        }

        void KeyboardEventDispatcher_OnKeyPressed(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.F2:
                    using(var f = new System.Windows.Forms.OpenFileDialog()) {
                        f.CheckFileExists = true;
                        f.CheckPathExists = true;
                        f.ShowDialog();
                        if(!string.IsNullOrWhiteSpace(f.FileName))
                            fIn = f.FileName;
                    }
                    using(var f = new System.Windows.Forms.SaveFileDialog()) {
                        f.CheckPathExists = true;
                        f.ShowDialog();
                        if(!string.IsNullOrWhiteSpace(f.FileName))
                            fOut = f.FileName;
                    }
                    break;
            }
        }
    }
}