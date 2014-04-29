using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics.Particles {
    public class LightningShader {
        private Effect fx;
        private EffectParameter fxpWVP, fxpTime, fxpSplitCount;

        private Texture2D tLightning;

        public LightningShader() {

        }

        public void Build(RTSRenderer renderer, string fxFile, string fLightningMap, int numSplits) {
            Build(renderer, renderer.LoadEffect(fxFile), renderer.LoadTexture2D(fLightningMap), numSplits);
        }
        public void Build(RTSRenderer renderer, Effect _fx, Texture2D _tLMap, int numSplits) {
            fx = _fx;
            fx.CurrentTechnique = fx.Techniques[0];
            tLightning = _tLMap;

            fxpWVP = fx.Parameters["WVP"];
            fxpTime = fx.Parameters["Time"];
            fxpSplitCount = fx.Parameters["Splits"];
            fxpSplitCount.SetValue(numSplits);
        }

        public void Apply(GraphicsDevice g, Matrix mWVP, float t) {
            g.Textures[1] = tLightning;
            g.SamplerStates[1] = SamplerState.LinearWrap;
            fxpWVP.SetValue(mWVP);
            fxpTime.SetValue(t);
            fx.CurrentTechnique.Passes[0].Apply();
        }
    }
}
