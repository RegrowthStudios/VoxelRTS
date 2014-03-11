using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSCS.Gameplay;
namespace RTSCS.Graphics {
    public class Renderer : IDisposable {

        private BasicEffect fxBasic;
        private Effect fxUnit;

        private Matrix mView, mProj;
        public Matrix View {
            get { return mView; }
            set {
                mView = value;
                fxBasic.View = mView;
                fxUnit.Parameters["VP"].SetValue(mView * mProj);
            }
        }
        public Matrix Projection {
            get { return mProj; }
            set {
                mProj = value;
                fxBasic.Projection = mProj;
                fxUnit.Parameters["VP"].SetValue(mView * mProj);
            }
        }

        public Renderer(GraphicsDevice g, Effect e) {
            IsDisposed = false;
            mView = Matrix.Identity;
            mProj = Matrix.Identity;

            fxBasic = new BasicEffect(g);
            fxBasic.FogEnabled = false;
            fxBasic.LightingEnabled = false;
            fxBasic.VertexColorEnabled = false;
            fxBasic.TextureEnabled = true;
            fxBasic.World = Matrix.Identity;

            fxUnit = e;
        }
        #region IDisposalNotifier
        ~Renderer() {
            if(!IsDisposed) Dispose();
        }
        public event Action<object> OnDisposal;
        public bool IsDisposed { get; private set; }

        public void Dispose() {
            if(IsDisposed)
                throw new ObjectDisposedException("Renderer");

            IsDisposed = true;
            if(OnDisposal != null) OnDisposal(this);

            if(fxBasic != null) {
                fxBasic.Dispose();
                fxBasic = null;
            }
        }
        #endregion

        public void RenderMap(GraphicsDevice g, CombatMap map) {
            g.DepthStencilState = DepthStencilState.DepthRead;
            g.RasterizerState = RasterizerState.CullNone;
            g.BlendState = BlendState.Opaque;
            g.SamplerStates[0] = SamplerState.LinearWrap;

            fxBasic.Texture = map.Background;
            fxBasic.World = map.WorldTransform;
            fxBasic.VertexColorEnabled = false;
            fxBasic.TextureEnabled = true;
            fxBasic.CurrentTechnique.Passes[0].Apply();

            VertexPositionTexture[] verts = new VertexPositionTexture[4];
            map.CopyVertexTriangleStrip(ref verts, 0);
            g.DrawUserPrimitives(PrimitiveType.TriangleStrip, verts, 0, 2, VertexPositionTexture.VertexDeclaration);
        }
        public void RenderCombat(GraphicsDevice g, CombatAnimator a) {
            g.DepthStencilState = DepthStencilState.None;
            g.RasterizerState = RasterizerState.CullNone;
            g.BlendState = BlendState.Additive;
            fxBasic.VertexColorEnabled = true;
            fxBasic.TextureEnabled = true;
            fxBasic.Texture = a.Texture;
            fxBasic.World = Matrix.Identity;
            fxBasic.CurrentTechnique.Passes[0].Apply();
            a.Render(g);
            fxBasic.VertexColorEnabled = false;
        }

        public void BeginUnitPass(GraphicsDevice g) {
            g.BlendState = BlendState.AlphaBlend;
            fxUnit.CurrentTechnique.Passes[0].Apply();
        }
    }
}