using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Graphics {
    public class ParticleRenderer {
        // Lists Of Particles
        private List<BulletParticle> pBullets;

        public ParticleRenderer() {
            pBullets = new List<BulletParticle>();
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
    }
}
