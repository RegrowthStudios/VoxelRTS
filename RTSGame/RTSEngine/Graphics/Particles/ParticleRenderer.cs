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
        public string FireNoise;
        public string FireColor;
        public string FireAlpha;

        public int LightningMaxCount;
        public string LightningImage;
        public int LightningNumTypes;

        public int AlertMaxCount;
        public string AlertImage;
    }

    public class ParticleRenderer {
        // Lists Of Particles
        private ParticleList<BulletParticle, VertexBulletInstance> plBullets;
        private ParticleList<FireParticle, VertexFireInstance> plFires;
        private ParticleList<LightningParticle, VertexLightningInstance> plBolts;
        private ParticleList<AlertParticle, VertexAlertInstance> plAlerts;

        private ParticleEffect fxParticle;
        public Vector2 MapSize {
            set { fxParticle.MapSize = value; }
        }
        private Texture2D tBullet;
        private Texture2D tLightningMap;
        private Texture2D tFireColor;
        private Texture2D tFireNoise;
        private Texture2D tFireAlpha;
        private Texture2D tAlert;

        public ParticleRenderer(Effect fx, ParticleEffectConfig peConf) {
            fxParticle = new ParticleEffect(fx, peConf);
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
            LoadFireShader(renderer, o.FireNoise, o.FireColor, o.FireAlpha);

            // Create Lightning System
            plBolts = new ParticleList<LightningParticle, VertexLightningInstance>(renderer, o.LightningMaxCount, ParticleType.Lightning);
            BuildLightningModel(renderer);
            LoadLightningShader(renderer, o.LightningImage, o.LightningNumTypes);

            // Create Alert System
            plAlerts = new ParticleList<AlertParticle, VertexAlertInstance>(renderer, o.AlertMaxCount, ParticleType.Alert);
            BuildAlertModel(renderer);
            tAlert = renderer.LoadTexture2D(o.AlertImage);
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
        private void LoadFireShader(RTSRenderer renderer, string fNoise, string fColor, string fAlpha) {
            tFireColor = renderer.LoadTexture2D(fColor);
            tFireNoise = renderer.LoadTexture2D(fNoise);
            tFireAlpha = renderer.LoadTexture2D(fAlpha);
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
        private void LoadLightningShader(RTSRenderer renderer, string fLMap, int splits) {
            tLightningMap = renderer.LoadTexture2D(fLMap);
            fxParticle.LightningSplits = splits;
        }
        private void BuildAlertModel(RTSRenderer renderer) {
            plAlerts.VBuffer = renderer.CreateVertexBuffer(VertexPositionTexture.VertexDeclaration, 4, BufferUsage.WriteOnly);
            plAlerts.VBuffer.SetData(new VertexPositionTexture[] {
                new VertexPositionTexture(new Vector3(-1, 0, -1), Vector2.Zero),
                new VertexPositionTexture(new Vector3(1, 0, -1), Vector2.UnitX),
                new VertexPositionTexture(new Vector3(-1, 0, 1), Vector2.UnitY),
                new VertexPositionTexture(new Vector3(1, 0, 1), Vector2.One)
            });
            plAlerts.IBuffer = renderer.CreateIndexBuffer(IndexElementSize.SixteenBits, 6, BufferUsage.WriteOnly);
            plAlerts.IBuffer.SetData(new short[] { 0, 1, 2, 2, 1, 3 });
        }

        public void Update(List<Particle> newParticles, float dt) {
            plBullets.Update(newParticles, dt);
            plFires.Update(newParticles, dt);
            plBolts.Update(newParticles, dt);
            plAlerts.Update(newParticles, dt);
        }

        public void SetupAll(GraphicsDevice g, Matrix mVP, float t, Texture2D tFOW) {
            fxParticle.SetupBasic(g, mVP, t, tFOW);
        }

        public void SetBullets(GraphicsDevice g) {
            fxParticle.ApplySimple(g, tBullet);
            g.SetVertexBuffers(plBullets.VBBinds);
            g.Indices = plBullets.IBuffer;
        }
        public void DrawBullets(GraphicsDevice g) {
            if(plBullets.ParticleCount > 0)
                g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, plBullets.VertexCount, 0, plBullets.TriCount, plBullets.ParticleCount);
        }

        public void SetFire(GraphicsDevice g) {
            fxParticle.ApplyFire(g, tFireColor, tFireNoise, tFireAlpha);
            g.SetVertexBuffers(plFires.VBBinds);
            g.Indices = plFires.IBuffer;
        }
        public void DrawFire(GraphicsDevice g) {
            if(plFires.ParticleCount > 0)
                g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, plFires.VertexCount, 0, plFires.TriCount, plFires.ParticleCount);
        }

        public void SetLightning(GraphicsDevice g) {
            fxParticle.ApplyLightning(g, tLightningMap);
            g.SetVertexBuffers(plBolts.VBBinds);
            g.Indices = plBolts.IBuffer;
        }
        public void DrawLightning(GraphicsDevice g) {
            if(plBolts.ParticleCount > 0)
                g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, plBolts.VertexCount, 0, plBolts.TriCount, plBolts.ParticleCount);
        }

        public void SetAlerts(GraphicsDevice g) {
            fxParticle.ApplyAlert(g, tAlert);
            g.SetVertexBuffers(plAlerts.VBBinds);
            g.Indices = plAlerts.IBuffer;
        }
        public void DrawAlerts(GraphicsDevice g) {
            if(plAlerts.ParticleCount > 0)
                g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, plAlerts.VertexCount, 0, plAlerts.TriCount, plAlerts.ParticleCount);
        }
    }
}