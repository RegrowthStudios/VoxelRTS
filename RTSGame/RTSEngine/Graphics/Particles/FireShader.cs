using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public class FireShader : IDisposable {
        public static readonly Vector4 DEFAULT_TINT = new Vector4(1f, 0.6f, 0.2f, 1f);
        public static readonly Vector2 DEFAULT_OFFSET = Vector2.One;
        public static readonly Vector3 DEFAULT_RATES = new Vector3(1.8f, 2.7f, 6f);
        public static readonly Vector3 DEFAULT_SCALES = new Vector3(1, 3, 5);
        public const float DEFAULT_DISTORT_SCALE = 0.2f;
        public const float DEFAULT_DISTORT_BIAS = 0.01f;

        private Effect fx;
        private EffectParameter fxpWVP, fxpTime, fxpRates, fxpTint, fxpDistortScale, fxpDistortBias;

        private Texture2D tNoise, tColor, tAlpha;

        private float t;
        public float Time {
            get { return t; }
            set {
                t = value;
                fxpTime.SetValue(t);
            }
        }
        public Vector3 Rates {
            set { fxpRates.SetValue(value); }
        }
        public Vector4 Tint {
            set { fxpTint.SetValue(value); }
        }
        public float DistortScale {
            set { fxpDistortScale.SetValue(value); }
        }
        public float DistortBias {
            set { fxpDistortBias.SetValue(value); }
        }

        public FireShader() {
            t = 0f;
        }
        public void Dispose() {
            if(fx != null) {
                fx.Dispose();
                fx = null;
            }
            if(tNoise != null) {
                tNoise.Dispose();
                tNoise = null;
            }
            if(tColor != null) {
                tColor.Dispose();
                tColor = null;
            }
            if(tAlpha != null) {
                tAlpha.Dispose();
                tAlpha = null;
            }
        }

        public void Build(GraphicsDevice g, string fxFile, string fNoise, string fColor, string fAlpha) {
            // Load Resources
            fx = XNAEffect.Compile(g, fxFile);
            using(var s = File.OpenRead(fNoise)) tNoise = Texture2D.FromStream(g, s);
            using(var s = File.OpenRead(fColor)) tColor = Texture2D.FromStream(g, s);
            using(var s = File.OpenRead(fAlpha)) tAlpha = Texture2D.FromStream(g, s);
            Build(fx, tNoise, tColor, tAlpha);
        }
        public void Build(Effect _fx, Texture2D _tNoise, Texture2D _tColor, Texture2D _tAlpha) {
            fx = _fx;
            fx.CurrentTechnique = fx.Techniques[0];
            tNoise = _tNoise;
            tColor = _tColor;
            tAlpha = _tAlpha;

            // Set Default Parameters
            fxpWVP = fx.Parameters["WVP"];
            fxpWVP.SetValue(Matrix.Identity);
            fxpTime = fx.Parameters["time"];
            fxpTime.SetValue(Time);
            fxpRates = fx.Parameters["rates"];
            fxpRates.SetValue(DEFAULT_RATES);
            fx.Parameters["scales"].SetValue(DEFAULT_SCALES);
            fx.Parameters["offset1"].SetValue(DEFAULT_OFFSET);
            fx.Parameters["offset2"].SetValue(DEFAULT_OFFSET);
            fx.Parameters["offset3"].SetValue(DEFAULT_OFFSET);
            fxpDistortScale = fx.Parameters["distortScale"];
            fxpDistortScale.SetValue(DEFAULT_DISTORT_SCALE);
            fxpDistortBias = fx.Parameters["distortBias"];
            fxpDistortBias.SetValue(DEFAULT_DISTORT_BIAS);
            fxpTint = fx.Parameters["tint"];
            fxpTint.SetValue(DEFAULT_TINT);
        }

        public void Apply(GraphicsDevice g, Matrix mWVP, float t) {
            g.Textures[0] = tNoise;
            g.SamplerStates[0] = SamplerState.LinearWrap;
            g.Textures[1] = tColor;
            g.SamplerStates[1] = SamplerState.LinearClamp;
            g.Textures[2] = tAlpha;
            g.SamplerStates[2] = SamplerState.LinearClamp;

            Time = t;
            fxpWVP.SetValue(mWVP);
            fx.CurrentTechnique.Passes[0].Apply();
        }
    }
}