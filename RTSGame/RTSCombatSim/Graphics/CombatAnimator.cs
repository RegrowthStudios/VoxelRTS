using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSCS.Graphics {
    public class CombatAnimation {
        private float startTime;
        public float TimeAvailable {
            get;
            private set;
        }

        public bool IsVisible {
            get { return TimeAvailable >= 0; }
        }

        private VertexPositionColorTexture[] verts;

        public int VertexCount {
            get { return 4; }
        }
        public int IndexCount {
            get { return 6; }
        }

        public CombatAnimation(Vector3 source, Vector3 target, float width, Color sc, Color tc, float t) {
            startTime = t;
            TimeAvailable = startTime;

            Vector3 d = target - source;
            float dist = d.Length();
            d /= dist;

            Vector3 y = Vector3.Cross(Vector3.UnitZ, d);
            y.Normalize();
            y *= width * 0.5f;

            verts = new VertexPositionColorTexture[4];
            verts[0].Position = source + y;
            verts[0].TextureCoordinate = new Vector2(0, 0);
            verts[0].Color = sc;
            verts[1].Position = target + y;
            verts[1].TextureCoordinate = new Vector2(1, 0);
            verts[1].Color = tc;
            verts[2].Position = source - y;
            verts[2].TextureCoordinate = new Vector2(0, 1);
            verts[2].Color = sc;
            verts[3].Position = target - y;
            verts[3].TextureCoordinate = new Vector2(1, 1);
            verts[3].Color = tc;
        }

        public void Update(float dt) {
            TimeAvailable -= dt;
        }

        public void Append(VertexPositionColorTexture[] v, ref int vi, int[] i, ref int ii) {
            i[ii++] = vi + 0;
            i[ii++] = vi + 1;
            i[ii++] = vi + 2;
            i[ii++] = vi + 2;
            i[ii++] = vi + 1;
            i[ii++] = vi + 3;
            v[vi] = verts[0];
            v[vi++].Color.A = (byte)(255 * TimeAvailable / startTime);
            v[vi] = verts[1];
            v[vi++].Color.A = (byte)(255 * TimeAvailable / startTime);
            v[vi] = verts[2];
            v[vi++].Color.A = (byte)(255 * TimeAvailable / startTime);
            v[vi] = verts[3];
            v[vi++].Color.A = (byte)(255 * TimeAvailable / startTime);
        }
    }


    public class CombatAnimator {
        // Texture To Be Drawn
        public Texture2D Texture {
            get;
            private set;
        }

        // Number Of Vertices And Indices To Render
        private int vc, ic;

        // Render Data
        private VertexPositionColorTexture[] verts;
        private int[] inds;

        // Animations
        private List<CombatAnimation> anims, toAdd;

        public CombatAnimator(GraphicsDevice g, string texFile) {
            anims = new List<CombatAnimation>();
            toAdd = new List<CombatAnimation>();
            verts = new VertexPositionColorTexture[0];
            inds = new int[0];
            vc = 0;
            ic = 0;
            using(var s = System.IO.File.OpenRead(texFile)) {
                Texture = Texture2D.FromStream(g, s);
            }
        }

        public void Add(CombatAnimation a) {
            toAdd.Add(a);
        }

        public void Update(float dt) {
            int ovc = vc, oic = ic;
            List<CombatAnimation> l = new List<CombatAnimation>(anims.Count + toAdd.Count);

            // Add New Animations
            foreach(CombatAnimation ca in toAdd) {
                l.Add(ca);
                vc += ca.VertexCount;
                ic += ca.IndexCount;
            }
            toAdd.Clear();
            // Remove Dead Animations
            foreach(CombatAnimation ca in anims) {
                if(ca.IsVisible) {
                    l.Add(ca);
                    ca.Update(dt);
                }
                else {
                    vc -= ca.VertexCount;
                    ic -= ca.IndexCount;
                }
            }

            // Check For Resize
            if(ovc != vc)
                Array.Resize(ref verts, vc);
            if(oic != ic)
                Array.Resize(ref inds, ic);
            int vi = 0, ii = 0;

            // Append New Data
            anims = l;
            foreach(CombatAnimation ca in anims) {
                ca.Append(verts, ref vi, inds, ref ii);
            }
        }

        public void Render(GraphicsDevice g) {
            if(ic < 3) return;
            g.DrawUserIndexedPrimitives(PrimitiveType.TriangleList, verts, 0, vc, inds, 0, ic / 3, VertexPositionColorTexture.VertexDeclaration);
        }
    }
}