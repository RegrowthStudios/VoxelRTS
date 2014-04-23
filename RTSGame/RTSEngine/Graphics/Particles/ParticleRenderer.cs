using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public class ParticleRenderer {
        public const string FILE_BULLET_MODEL = @"Content\FX\Particles\Bullet.obj";
        public const string FILE_BULLET_TEXTURE = @"Content\FX\Particles\Bullet.png";
        public const string FILE_FIRE_SHADER = @"Content\FX\ParticleFire.fx";
        public const string FILE_FIRE_NOISE = @"Content\FX\Particles\FireNoise.png";
        public const string FILE_FIRE_COLOR = @"Content\FX\Particles\FireColor.png";
        public const string FILE_FIRE_ALPHA = @"Content\FX\Particles\FireAlpha.png";
        public const int MAX_BULLETS = 1000;
        public const int MAX_FIRES = 1000;

        // Lists Of Particles
        private List<BulletParticle> pBullets;
        private readonly VertexBulletInstance[] bullets;
        private List<FireParticle> pFires;
        private readonly VertexFireInstance[] fires;

        // Bullet Models
        private VertexBuffer vbBullet;
        private IndexBuffer ibBullet;
        private Texture2D tBullet;
        private DynamicVertexBuffer dvbBullet;
        private VertexBufferBinding[] vbbBullets;

        // Fire Models
        private VertexBuffer vbFire;
        private IndexBuffer ibFire;
        private FireShader fxFire;
        private DynamicVertexBuffer dvbFire;
        private VertexBufferBinding[] vbbFires;

        public ParticleRenderer() {
            pBullets = new List<BulletParticle>();
            bullets = new VertexBulletInstance[MAX_BULLETS];
            pFires = new List<FireParticle>();
            fires = new VertexFireInstance[MAX_FIRES];
        }

        public void LoadBulletModel(RTSRenderer renderer, Stream s, ParsingFlags pf = ParsingFlags.ConversionOpenGL) {
            VertexPositionNormalTexture[] v;
            int[] inds;
            ObjParser.TryParse(s, out v, out inds, pf);
            VertexPositionTexture[] verts = new VertexPositionTexture[v.Length];
            for(int i = 0; i < verts.Length; i++) {
                verts[i].Position = v[i].Position;
                verts[i].TextureCoordinate = v[i].TextureCoordinate;
            }
            vbBullet = renderer.CreateVertexBuffer(VertexPositionTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vbBullet.SetData(verts);
            ibBullet = renderer.CreateIndexBuffer(IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
            ibBullet.SetData(inds);
            dvbBullet = renderer.CreateDynamicVertexBuffer(VertexBulletInstance.Declaration, MAX_BULLETS, BufferUsage.WriteOnly);
            dvbBullet.SetData(bullets);

            vbbBullets = new VertexBufferBinding[2];
            vbbBullets[0] = new VertexBufferBinding(vbBullet);
            vbbBullets[1] = new VertexBufferBinding(dvbBullet, 0, 1);
        }
        public void LoadBulletTexture(RTSRenderer renderer, string f) {
            tBullet = renderer.LoadTexture2D(f);
        }
        public void BuildFireModel(RTSRenderer renderer, int div) {
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

            vbFire = renderer.CreateVertexBuffer(VertexPositionTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vbFire.SetData(verts);
            ibFire = renderer.CreateIndexBuffer(IndexElementSize.SixteenBits, inds.Length, BufferUsage.WriteOnly);
            ibFire.SetData(inds);
            dvbFire = renderer.CreateDynamicVertexBuffer(VertexFireInstance.Declaration, MAX_FIRES, BufferUsage.WriteOnly);
            dvbFire.SetData(fires);

            vbbFires = new VertexBufferBinding[2];
            vbbFires[0] = new VertexBufferBinding(vbFire);
            vbbFires[1] = new VertexBufferBinding(dvbFire, 0, 1);
        }
        public void LoadFireShader(RTSRenderer renderer, string fxFile, string fNoise, string fColor, string fAlpha) {
            fxFire = new FireShader();
            fxFire.Build(
                renderer.LoadEffect(fxFile),
                renderer.LoadTexture2D(fNoise),
                renderer.LoadTexture2D(fColor),
                renderer.LoadTexture2D(fAlpha)
                );
        }

        public void Update(List<Particle> newParticles, float dt) {
            // Update Particles
            Action<Particle> fp = (p) => { p.Update(dt); };
            pBullets.
                Concat<Particle>(pFires).
                AsParallel().ForAll(fp);

            // Remove Dead Particles
            bool addB = pBullets.RemoveAll(Particle.IsParticleDead) > 0;
            bool addF = pFires.RemoveAll(Particle.IsParticleDead) > 0;

            // Add New Particles
            for(int i = 0; i < newParticles.Count; i++) {
                switch(newParticles[i].Type) {
                    case ParticleType.Bullet:
                        pBullets.Add(newParticles[i] as BulletParticle);
                        addB = true;
                        break;
                    case ParticleType.Fire:
                        pFires.Add(newParticles[i] as FireParticle);
                        addF = true;
                        break;
                }
            }

            if(addB) {
                // Make Sure We Don't Run Over
                if(pBullets.Count > MAX_BULLETS)
                    pBullets.RemoveRange(0, pBullets.Count - MAX_BULLETS);

                for(int i = 0; i < pBullets.Count; i++) {
                    bullets[i] = pBullets[i].instance;
                }
                dvbBullet.SetData(bullets);
            }

            if(addF) {
                // Make Sure We Don't Run Over
                if(pFires.Count > MAX_FIRES)
                    pFires.RemoveRange(0, pFires.Count - MAX_FIRES);

                for(int i = 0; i < pFires.Count; i++) {
                    fires[i] = pFires[i].instance;
                }
                dvbFire.SetData(fires);
            }
        }

        public void SetBullets(GraphicsDevice g) {
            g.Textures[0] = tBullet;
            g.SamplerStates[0] = SamplerState.LinearClamp;
            g.SetVertexBuffers(vbbBullets);
            g.Indices = ibBullet;
        }
        public void DrawBullets(GraphicsDevice g) {
            if(pBullets.Count > 0)
                g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, vbBullet.VertexCount, 0, ibBullet.IndexCount / 3, pBullets.Count);
        }

        public void SetFire(GraphicsDevice g, Matrix mWVP, float t) {
            fxFire.Apply(g, mWVP, t);
            g.SetVertexBuffers(vbbFires);
            g.Indices = ibFire;
        }
        public void DrawFire(GraphicsDevice g) {
            if(pFires.Count > 0)
                g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, vbFire.VertexCount, 0, ibFire.IndexCount / 3, pFires.Count);
        }
    }
}