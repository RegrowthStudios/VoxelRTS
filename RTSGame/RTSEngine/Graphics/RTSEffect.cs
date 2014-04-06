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

    public class RTSFXEntity {
        // Effect Pass Keys
        public const string PASS_KEY_BUILDING = "Building";
        public const string PASS_KEY_UNIT = "Unit";
        // Effect Parameter Keys
        public const string PARAM_KEY_WORLD = "World";
        public const string PARAM_KEY_VP = "VP";
        public const string PARAM_KEY_COLOR_PRIMARY = "CPrimary";
        public const string PARAM_KEY_COLOR_SECONDARY = "CSecondary";
        public const string PARAM_KEY_COLOR_TERTIARY = "CTertiary";
        public const string PARAM_KEY_TEXEL_SIZE = "TexelSize";

        // The Effect And Its Passes
        private Effect fx;
        private EffectPass fxPassBuilding, fxPassUnit;

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

        public RTSFXEntity(Effect _fx) {
            if(_fx == null) throw new ArgumentNullException("A Null Effect Was Used");

            // Set The Effect To The First Technique
            fx = _fx;
            fx.CurrentTechnique = fx.Techniques[0];

            // Get The Passes
            fxPassBuilding = fx.CurrentTechnique.Passes[PASS_KEY_BUILDING];
            fxPassUnit = fx.CurrentTechnique.Passes[PASS_KEY_UNIT];

            // Get The Parameters
            fxpWorld = fx.Parameters[PARAM_KEY_WORLD];
            fxpVP = fx.Parameters[PARAM_KEY_VP];
            fxpColP = fx.Parameters[PARAM_KEY_COLOR_PRIMARY];
            fxpColS = fx.Parameters[PARAM_KEY_COLOR_SECONDARY];
            fxpColT = fx.Parameters[PARAM_KEY_COLOR_TERTIARY];
            fxpTexelSize = fx.Parameters[PARAM_KEY_TEXEL_SIZE];
        }

        public void ApplyPassBuilding() {
            fxPassBuilding.Apply();
        }
        public void ApplyPassUnit() {
            fxPassUnit.Apply();
        }

        public void SetTextures(GraphicsDevice g, Texture2D tAnim, Texture2D tMain, Texture2D tKey) {
            g.VertexTextures[0] = tAnim;
            g.Textures[1] = tMain;
            g.Textures[2] = tKey;
            fxpTexelSize.SetValue(new Vector2(1f / tAnim.Width, 1f / tAnim.Height));
        }
        public void SetTextures(GraphicsDevice g, Texture2D tMain, Texture2D tKey) {
            g.Textures[1] = tMain;
            g.Textures[2] = tKey;
        }
    }

    public class RTSFXMap {
        // Effect Pass Keys
        public const string PASS_KEY_PRIMARY = "Primary";
        public const string PASS_KEY_SECONDARY = "Secondary";
        // Effect Parameter Keys
        public const string PARAM_KEY_TEXELSIZE = "TexelSize";
        public const string PARAM_KEY_MAPSIZE = "MapSize";
        public const string PARAM_KEY_VP = "VP";

        // The Effect And Its Passes
        private Effect fx;
        private EffectPass fxPassPrimary, fxPassSecondary;

        private EffectParameter fxpMapSize, fxpTexelSize, fxpVP;
        public Vector2 MapSize {
            set { fxpMapSize.SetValue(value); }
        }
        public Matrix VP {
            set { fxpVP.SetValue(value); }
        }

        public RTSFXMap(Effect _fx) {
            if(_fx == null) throw new ArgumentNullException("A Null Effect Was Used");

            // Set The Effect To The First Technique
            fx = _fx;
            fx.CurrentTechnique = fx.Techniques[0];

            // Get The Passes
            fxPassPrimary = fx.CurrentTechnique.Passes[PASS_KEY_PRIMARY];
            fxPassSecondary = fx.CurrentTechnique.Passes[PASS_KEY_SECONDARY];

            // Get The Parameters
            fxpTexelSize = fx.Parameters[PARAM_KEY_TEXELSIZE];
            fxpMapSize = fx.Parameters[PARAM_KEY_MAPSIZE];
            fxpVP = fx.Parameters[PARAM_KEY_VP];
        }

        public void SetTextures(GraphicsDevice g, Texture2D tColor, Texture2D tFOW) {
            g.Textures[0] = tColor;
            g.SamplerStates[0] = SamplerState.LinearClamp;
            g.Textures[1] = tFOW;
            g.SamplerStates[1] = SamplerState.PointClamp;
            fxpTexelSize.SetValue(new Vector2(1f / tFOW.Width, 1f / tFOW.Height));
        }

        public void ApplyPassPrimary() {
            fxPassPrimary.Apply();
        }
        public void ApplyPassSecondary() {
            fxPassSecondary.Apply();
        }
    }
}