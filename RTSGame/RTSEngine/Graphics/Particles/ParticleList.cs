using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
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
                if(newParticles[i] == null) continue;
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
}