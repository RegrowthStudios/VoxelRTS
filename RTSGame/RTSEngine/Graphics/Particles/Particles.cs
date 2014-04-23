using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public enum ParticleType {
        Bullet,
        Fire
    }
    public abstract class Particle {
        public static bool IsParticleDead(Particle p) {
            return p.IsDead;
        }

        public ParticleType Type {
            get;
            private set;
        }

        // Temporal Information
        protected float duration, timeAlive;
        public bool IsDead {
            get { return timeAlive >= duration; }
        }

        // 0 -> 1 From Birth To Death
        public float TimeRatio {
            get { return timeAlive / duration; }
        }

        public Particle(float t, ParticleType pt) {
            duration = t;
            timeAlive = 0;
            Type = pt;
        }

        public void Update(float dt) {
            timeAlive += dt;
        }
    }

    #region Bullet Instancing
    public struct VertexBulletInstance : IVertexType {
        #region Declaration
        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.Position, 2),
            new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.Position, 3),
            new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector4, VertexElementUsage.Position, 4),
            new VertexElement(sizeof(float) * 16, VertexElementFormat.Color, VertexElementUsage.Color, 0)
        );
        public VertexDeclaration VertexDeclaration {
            get { return Declaration; }
        }
        #endregion

        public Matrix Transform;
        public Color Tint;

        public VertexBulletInstance(Matrix m, Color c) {
            Transform = m;
            Tint = c;
        }
    }
    #endregion
    public class BulletParticle : Particle {
        public Vector3 origin;
        public Vector3 direction;
        public float angle, distance;

        // Instance Transform Of Bullet
        public VertexBulletInstance instance;

        public BulletParticle(Vector3 o, Vector3 d, float ang, float dist, float t)
            : base(t, ParticleType.Bullet) {
            origin = o;
            direction = d;
            angle = ang;
            distance = dist;

            // Create Instance Matrix
            float s = (float)Math.Tan(angle);
            instance.Transform =
                Matrix.CreateScale(distance * s, distance * s, distance) *
                Matrix.CreateWorld(origin, direction, Vector3.Up);
            instance.Tint = Color.White;
        }
    }

    #region Fire Instancing
    public struct VertexFireInstance : IVertexType {
        #region Declaration
        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(sizeof(float) * 0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.Position, 2),
            new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.Position, 3),
            new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector4, VertexElementUsage.Position, 4),
            new VertexElement(sizeof(float) * 16, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1)
        );
        public VertexDeclaration VertexDeclaration {
            get { return Declaration; }
        }
        #endregion

        public Matrix Transform;
        public float Time;

        public VertexFireInstance(Matrix m, float t) {
            Transform = m;
            Time = t;
        }
    }
    #endregion
    public class FireParticle : Particle {
        public Vector3 origin;
        public float radius, height;

        // Instance Transform Of Bullet
        public VertexFireInstance instance;

        public FireParticle(Vector3 o, float r, float h, float rotY, float t)
            : base(t, ParticleType.Fire) {
            origin = o;

            // Create Instance Matrix
            instance.Transform =
                Matrix.CreateScale(r, h, r) *
                Matrix.CreateRotationY(rotY) *
                Matrix.CreateTranslation(o);
            instance.Time = 0;
        }
    }
}