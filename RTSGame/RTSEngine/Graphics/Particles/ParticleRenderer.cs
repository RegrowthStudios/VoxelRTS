using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data.Parsers;

namespace RTSEngine.Graphics {
    public class ParticleOptions {
        public int BulletMaxCount;
        public string BulletModel;
        public string BulletTexture;

        public int FireMaxCount;
        public int FireDetail;
        public string FireShader;
        public string FireNoise;
        public string FireColor;
        public string FireAlpha;
    }

    public class ParticleList<PType, VType>
        where PType : Particle
        where VType : struct, IVertexType {

        public ParticleType Type {
            get;
            private set;
        }

        public List<PType> particles;
        public int ParticleCount {
            get { return particles.Count; }
        }
        public VType[] vertices;
        public int MaxCount {
            get { return vertices.Length; }
        }

        private VertexBuffer vb;
        public VertexBuffer VBuffer {
            get { return vb; }
            set {
                vb = value;
                VBBinds[0] = new VertexBufferBinding(vb);
            }
        }
        public int VertexCount {
            get { return VBuffer.VertexCount; }
        }
        public IndexBuffer IBuffer {
            get;
            set;
        }
        public int IndexCount {
            get { return IBuffer.IndexCount; }
        }
        public int TriCount {
            get { return IndexCount / 3; }
        }
        public DynamicVertexBuffer InstanceBuffer {
            get;
            private set;
        }

        public VertexBufferBinding[] VBBinds {
            get;
            private set;
        }

        public ParticleList(RTSRenderer renderer, int maxCount, ParticleType pt) {
            Type = pt;

            // Make The Lists
            particles = new List<PType>();
            vertices = new VType[maxCount];

            // Create The Instance Buffer
            InstanceBuffer = renderer.CreateDynamicVertexBuffer(vertices[0].VertexDeclaration, MaxCount, BufferUsage.WriteOnly);
            InstanceBuffer.SetData(vertices);

            VBBinds = new VertexBufferBinding[2];
            VBBinds[1] = new VertexBufferBinding(InstanceBuffer, 0, 1);
        }

        public void Update(List<Particle> newParticles, float dt) {
            // Update The Particles
            Action<Particle> fp = (p) => { p.Update(dt); };
            particles.AsParallel().ForAll(fp);

            bool add = particles.RemoveAll(Particle.IsParticleDead) > 0;

            // Add New Particles
            for(int i = 0; i < newParticles.Count; i++) {
                if(newParticles[i].Type == Type) {
                    particles.Add(newParticles[i] as PType);
                    add = true;
                }
            }

            if(add) {
                // Make Sure We Don't Run Over
                if(particles.Count > MaxCount)
                    particles.RemoveRange(0, particles.Count - MaxCount);
                for(int i = 0; i < particles.Count; i++)
                    vertices[i] = (VType)particles[i].Vertex;
                InstanceBuffer.SetData(vertices);
            }
        }
    }

    public class ParticleRenderer {

        // Lists Of Particles
        private ParticleList<BulletParticle, VertexBulletInstance> plBullets;
        private ParticleList<FireParticle, VertexFireInstance> plFires;
        private ParticleList<LightningParticle, VertexLightningInstance> plBolts;

        private Texture2D tBullet;
        private FireShader fxFire;
        private Texture2D tLightning;

        public ParticleRenderer() {
        }

        public void Load(RTSRenderer renderer, ParticleOptions o) {
            // Create Bullet System
            plBullets = new ParticleList<BulletParticle, VertexBulletInstance>(renderer, o.BulletMaxCount, ParticleType.Bullet);
            using(var fs = File.OpenRead(o.BulletModel)) {
                LoadBulletModel(renderer, fs, ParsingFlags.ConversionOpenGL);
            }
            LoadBulletTexture(renderer, o.BulletTexture);

            // Create Fire System
            plFires = new ParticleList<FireParticle, VertexFireInstance>(renderer, o.FireMaxCount, ParticleType.Fire);
            BuildFireModel(renderer, o.FireDetail);
            LoadFireShader(renderer, o.FireShader, o.FireNoise, o.FireColor, o.FireAlpha);
        }
        private void LoadBulletModel(RTSRenderer renderer, Stream s, ParsingFlags pf = ParsingFlags.ConversionOpenGL) {
            VertexPositionNormalTexture[] v;
            int[] inds;
            ObjParser.TryParse(s, out v, out inds, pf);
            VertexPositionTexture[] verts = new VertexPositionTexture[v.Length];
            for(int i = 0; i < verts.Length; i++) {
                verts[i].Position = v[i].Position;
                verts[i].TextureCoordinate = v[i].TextureCoordinate;
            }
            plBullets.VBuffer = renderer.CreateVertexBuffer(VertexPositionTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            plBullets.VBuffer.SetData(verts);
            plBullets.IBuffer = renderer.CreateIndexBuffer(IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
            plBullets.IBuffer.SetData(inds);
        }
        private void LoadBulletTexture(RTSRenderer renderer, string f) {
            tBullet = renderer.LoadTexture2D(f);
        }
        private void BuildFireModel(RTSRenderer renderer, int div) {
            VertexPositionTexture[] verts = new VertexPositionTexture[4 * div];
            short[] inds = new short[6 * div];
            double dTheta = Math.PI / div;
            for(int i = 0, ii = 0, vi = 0; i < div; i++) {
                inds[ii++] = (short)(vi + 0);
                inds[ii++] = (short)(vi + 1);
                inds[ii++] = (short)(vi + 2);
                inds[ii++] = (short)(vi + 2);
                inds[ii++] = (short)(vi + 1);
                inds[ii++] = (short)(vi + 3);
                float x = (float)Math.Cos(i * dTheta);
                float z = -(float)Math.Sin(i * dTheta);
                verts[vi++] = new VertexPositionTexture(new Vector3(-x, 1, -z), Vector2.Zero);
                verts[vi++] = new VertexPositionTexture(new Vector3(x, 1, z), Vector2.UnitX);
                verts[vi++] = new VertexPositionTexture(new Vector3(-x, 0, -z), Vector2.UnitY);
                verts[vi++] = new VertexPositionTexture(new Vector3(x, 0, z), Vector2.One);
            }

            plFires.VBuffer = renderer.CreateVertexBuffer(VertexPositionTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            plFires.VBuffer.SetData(verts);
            plFires.IBuffer = renderer.CreateIndexBuffer(IndexElementSize.SixteenBits, inds.Length, BufferUsage.WriteOnly);
            plFires.IBuffer.SetData(inds);
        }
        private void LoadFireShader(RTSRenderer renderer, string fxFile, string fNoise, string fColor, string fAlpha) {
            fxFire = new FireShader();
            fxFire.Build(
                renderer.LoadEffect(fxFile),
                renderer.LoadTexture2D(fNoise),
                renderer.LoadTexture2D(fColor),
                renderer.LoadTexture2D(fAlpha)
                );
        }
        private void BuildLightningModel(RTSRenderer renderer) {
            VertexPositionTexture[] verts = new VertexPositionTexture[4];
            verts[0] = new VertexPositionTexture(new Vector3(-1, 1, 0), Vector2.Zero);
            verts[1] = new VertexPositionTexture(new Vector3(1, 1, 0), Vector2.UnitX);
            verts[2] = new VertexPositionTexture(new Vector3(-1, 0, 0), Vector2.UnitY);
            verts[3] = new VertexPositionTexture(new Vector3(1, 0, 0), Vector2.One);
            short[] inds = { 0, 1, 2, 2, 1, 3 };

            plBolts.VBuffer = renderer.CreateVertexBuffer(VertexPositionTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            plBolts.VBuffer.SetData(verts);
            plBolts.IBuffer = renderer.CreateIndexBuffer(IndexElementSize.SixteenBits, inds.Length, BufferUsage.WriteOnly);
            plBolts.IBuffer.SetData(inds);
        }

        public void Update(List<Particle> newParticles, float dt) {
            plBullets.Update(newParticles, dt);
            plFires.Update(newParticles, dt);
        }

        public void SetBullets(GraphicsDevice g) {
            g.Textures[0] = tBullet;
            g.SamplerStates[0] = SamplerState.LinearClamp;
            g.SetVertexBuffers(plBullets.VBBinds);
            g.Indices = plBullets.IBuffer;
        }
        public void DrawBullets(GraphicsDevice g) {
            if(plBullets.ParticleCount > 0)
                g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, plBullets.VertexCount, 0, plBullets.TriCount, plBullets.ParticleCount);
        }

        public void SetFire(GraphicsDevice g, Matrix mWVP, float t) {
            fxFire.Apply(g, mWVP, t);
            g.SetVertexBuffers(plFires.VBBinds);
            g.Indices = plFires.IBuffer;
        }
        public void DrawFire(GraphicsDevice g) {
            if(plFires.ParticleCount > 0)
                g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, plFires.VertexCount, 0, plFires.TriCount, plFires.ParticleCount);
        }
    }
}