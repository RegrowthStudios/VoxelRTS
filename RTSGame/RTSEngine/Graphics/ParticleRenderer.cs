using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public class ParticleRenderer {
        public const int MAX_BULLETS = 1000;

        // Lists Of Particles
        private List<BulletParticle> pBullets;
        private readonly VertexBulletInstance[] bullets;

        // Models
        private VertexBuffer vbBullet;
        private IndexBuffer ibBullet;
        private DynamicVertexBuffer dvbBullet;
        private VertexBufferBinding[] vbbBullets;

        public ParticleRenderer() {
            pBullets = new List<BulletParticle>();
            bullets = new VertexBulletInstance[MAX_BULLETS];
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

        public void Update(List<Particle> newParticles, float dt) {
            // Update Particles
            Action<Particle> fp = (p) => { p.Update(dt); };
            pBullets.AsParallel().ForAll(fp);

            // Remove Dead Particles
            pBullets.RemoveAll(Particle.IsParticleDead);

            // Add New Particles
            for(int i = 0; i < newParticles.Count; i++) {
                switch(newParticles[i].Type) {
                    case ParticleType.Bullet:
                        pBullets.Add(newParticles[i] as BulletParticle);
                        break;
                }
            }
        }

        public void SetBulletBuffers(GraphicsDevice g) {
            g.SetVertexBuffers(vbbBullets);
            g.Indices = ibBullet;
        }
    }
}