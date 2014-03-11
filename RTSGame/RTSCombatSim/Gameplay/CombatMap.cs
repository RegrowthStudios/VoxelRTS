using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSCS.Gameplay {
    public class CombatMap : IDisposable {
        // The Map Image
        public Texture2D Background { get; private set; }

        // The Tiling Of The Map
        private VertexPositionTexture[] verts;
        public Vector2 Tiling {
            get {
                return new Vector2(
                    verts[1].TextureCoordinate.X - verts[0].TextureCoordinate.X,
                    verts[2].TextureCoordinate.Y - verts[0].TextureCoordinate.Y
                    );
            }
            set {
                Vector2 min = (Vector2.One - value) * 0.5f;
                Vector2 max = (Vector2.One + value) * 0.5f;
                verts[0].TextureCoordinate.X = min.X; verts[0].TextureCoordinate.Y = min.Y;
                verts[1].TextureCoordinate.X = max.X; verts[1].TextureCoordinate.Y = min.Y;
                verts[2].TextureCoordinate.X = min.X; verts[2].TextureCoordinate.Y = max.Y;
                verts[3].TextureCoordinate.X = max.X; verts[3].TextureCoordinate.Y = max.Y;
            }
        }

        // Location And Size Of Axis-aligned Map
        private Matrix transform;
        public Matrix WorldTransform {
            get { return transform; }
        }
        public Vector3 Translation {
            get {
                return WorldTransform.Translation;
            }
            set {
                transform.Translation = value;
            }
        }
        public Vector2 Scaling {
            get {
                return new Vector2(
                    WorldTransform.Right.X,
                    WorldTransform.Up.Y
                    );
            }
            set {
                transform.Right = Vector3.UnitX * value.X;
                transform.Up = Vector3.UnitY * value.Y;
            }
        }

        public CombatMap(GraphicsDevice g, string imageFile) {
            IsDisposed = false;

            // Load Image
            using(var fs = File.OpenRead(imageFile)) {
                Background = Texture2D.FromStream(g, fs);
            }

            // Identity Transform
            transform = Matrix.Identity;

            verts = new VertexPositionTexture[4];
            verts[0].Position = new Vector3(-1, 1, 0);
            verts[0].TextureCoordinate = Vector2.Zero;
            verts[1].Position = new Vector3(1, 1, 0);
            verts[1].TextureCoordinate = Vector2.UnitX;
            verts[2].Position = new Vector3(-1, -1, 0);
            verts[2].TextureCoordinate = Vector2.UnitY;
            verts[3].Position = new Vector3(1, -1, 0);
            verts[3].TextureCoordinate = Vector2.One;
        }
        #region IDisposalNotifier
        ~CombatMap() {
            if(!IsDisposed) Dispose();
        }
        public event Action<object> OnDisposal;
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            if(IsDisposed)
                throw new ObjectDisposedException("Combat Map");

            IsDisposed = true;
            if(OnDisposal != null) OnDisposal(this);

            if(Background != null) {
                Background.Dispose();
                Background = null;
            }
        }
        #endregion

        public void CopyVertexTriangleStrip(ref VertexPositionTexture[] v, int off) {
            v[off++] = verts[0];
            v[off++] = verts[1];
            v[off++] = verts[2];
            v[off] = verts[3];
        }
    }
}