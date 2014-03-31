using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public struct VertexRTSAnimInst : IVertexType {
        #region Declaration
        public static readonly VertexDeclaration Declaration = new VertexDeclaration(
            new VertexElement(0, VertexElementFormat.Vector4, VertexElementUsage.Position, 1),
            new VertexElement(sizeof(float) * 4, VertexElementFormat.Vector4, VertexElementUsage.Position, 2),
            new VertexElement(sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementUsage.Position, 3),
            new VertexElement(sizeof(float) * 12, VertexElementFormat.Vector4, VertexElementUsage.Position, 4),
            new VertexElement(sizeof(float) * 16, VertexElementFormat.Single, VertexElementUsage.TextureCoordinate, 1)
            );
        public VertexDeclaration VertexDeclaration {
            get { return Declaration; }
        }
        #endregion

        public Matrix World;
        public float AnimationFrame;

        public VertexRTSAnimInst(Matrix w, float f) {
            World = w;
            AnimationFrame = f;
        }
    }

    public class RTSEffect {
        // Effect Pass Keys
        public const string PASS_KEY_SIMPLE = "Simple";
        public const string PASS_KEY_ANIMATION = "Animation";
        // Effect Parameter Keys
        public const string PARAM_KEY_WORLD = "World";
        public const string PARAM_KEY_VP = "VP";
        public const string PARAM_KEY_COLOR_PRIMARY = "CPrimary";
        public const string PARAM_KEY_COLOR_SECONDARY = "CSecondary";
        public const string PARAM_KEY_COLOR_TERTIARY = "CTertiary";
        public const string PARAM_KEY_TEXEL_SIZE = "TexelSize";

        // The Effect And Its Passes
        private Effect fx;
        private EffectPass fxPassSimple, fxPassAnimation;

        // Used For Simple Pass
        private EffectParameter fxpWorld, fxpVP;
        public Matrix World {
            set { fxpWorld.SetValue(value); }
        }
        public Matrix VP {
            set { fxpVP.SetValue(value); }
        }

        // Used For Swatched Pass
        private EffectParameter fxpColP, fxpColS, fxpColT;
        public Vector3 CPrimary {
            set { fxpColP.SetValue(value); }
        }
        public Vector3 CSecondary {
            set { fxpColS.SetValue(value); }
        }
        public Vector3 CTertiary {
            set { fxpColT.SetValue(value); }
        }

        // Used For Animation Pass
        private EffectParameter fxpTexelSize;

        public RTSEffect(Effect _fx) {
            if(_fx == null) throw new ArgumentNullException("A Null Effect Was Used");

            // Set The Effect To The First Technique
            fx = _fx;
            fx.CurrentTechnique = fx.Techniques[0];

            // Get The Passes
            fxPassSimple = fx.CurrentTechnique.Passes[PASS_KEY_SIMPLE];
            fxPassAnimation = fx.CurrentTechnique.Passes[PASS_KEY_ANIMATION];

            // Get The Parameters
            fxpWorld = fx.Parameters[PARAM_KEY_WORLD];
            fxpVP = fx.Parameters[PARAM_KEY_VP];
            fxpColP = fx.Parameters[PARAM_KEY_COLOR_PRIMARY];
            fxpColS = fx.Parameters[PARAM_KEY_COLOR_SECONDARY];
            fxpColT = fx.Parameters[PARAM_KEY_COLOR_TERTIARY];
            fxpTexelSize = fx.Parameters[PARAM_KEY_TEXEL_SIZE];
        }

        public void ApplyPassSimple() {
            fxPassSimple.Apply();
        }
        public void ApplyPassAnimation() {
            fxPassAnimation.Apply();
        }

        public void DrawPassSimple(GraphicsDevice g, VertexBuffer model, IndexBuffer indices) {
            g.SetVertexBuffer(model);
            g.Indices = indices;
            g.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, model.VertexCount, 0, indices.IndexCount / 3);
        }
        public void DrawPassAnimation(GraphicsDevice g, VertexBuffer model, DynamicVertexBuffer instances, IndexBuffer indices) {
            g.SetVertexBuffers(
                new VertexBufferBinding(model),
                new VertexBufferBinding(instances, 0, 1)
                );
            g.Indices = indices;
            g.DrawInstancedPrimitives(PrimitiveType.TriangleList, 0, 0, model.VertexCount, 0, indices.IndexCount / 3, instances.VertexCount);
        }

        public void SetTextures(GraphicsDevice g, Texture2D tAnim, Texture2D tMain, Texture2D tKey) {
            g.VertexTextures[0] = tAnim;
            g.Textures[1] = tMain;
            g.Textures[2] = tKey;
            fxpTexelSize.SetValue(new Vector2(1f / tAnim.Width, 1f / tAnim.Height));
        }
    }
}