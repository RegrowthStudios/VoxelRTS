using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace RTSEngine.Graphics {
    public class ParticleEffectConfig {
        public string PassSimple;
        public string ParamVP;
        public string ParamTime;
        public string ParamMapSize;

        public string PassLightning;
        public string ParamSplits;

        public string PassFire;
        public string ParamRates;
        public string ParamScales;
        public string ParamOffset1;
        public string ParamOffset2;
        public string ParamOffset3;
        public string ParamDistortScale;
        public string ParamDistortBias;

        public string PassAlert;

        public ParticleEffectConfig() {
            PassSimple = "Simple";
            ParamVP = "VP";
            ParamTime = "Time";
            ParamMapSize = "MapSize";

            PassLightning = "Lightning";
            ParamSplits = "Splits";

            PassFire = "Fire";
            ParamRates = "Rates";
            ParamScales = "Scales";
            ParamOffset1 = "Offset1";
            ParamOffset2 = "Offset2";
            ParamOffset3 = "Offset3";
            ParamDistortScale = "DistortScale";
            ParamDistortBias = "DistortBias";

            PassAlert = "Alert";
        }
    }

    public class ParticleEffect {
        public const int DEFAULT_SPLITS = 8;
        public static readonly Vector2 DEFAULT_OFFSET = Vector2.One;
        public static readonly Vector3 DEFAULT_RATES = new Vector3(1.8f, 2.7f, 6f);
        public static readonly Vector3 DEFAULT_SCALES = new Vector3(1, 3, 5);
        public const float DEFAULT_DISTORT_SCALE = 0.2f;
        public const float DEFAULT_DISTORT_BIAS = 0.01f;

        // The Effect
        Effect fx;

        // Basic
        EffectPass fxPassSimple;
        EffectParameter fxpVP, fxpTime, fxpMapSize;
        public Matrix VP {
            set { fxpVP.SetValue(value); }
        }
        public float Time {
            set { fxpTime.SetValue(value); }
        }
        public Vector2 MapSize {
            set { fxpMapSize.SetValue(value); }
        }

        // Lightning
        EffectPass fxPassLightning;
        EffectParameter fxpLSplits;
        public float LightningSplits {
            set { fxpLSplits.SetValue(value); }
        }

        // Lightning
        EffectPass fxPassFire;
        EffectParameter fxpFRates, fxpFDistortScale, fxpFDistortBias, fxpFScales, fxpFOff1, fxpFOff2, fxpFOff3;
        public Vector3 FireRates {
            set { fxpFRates.SetValue(value); }
        }
        public Vector3 FireScales {
            set { fxpFScales.SetValue(value); }
        }
        public Vector2 FireOffset1 {
            set { fxpFOff1.SetValue(value); }
        }
        public Vector2 FireOffset2 {
            set { fxpFOff2.SetValue(value); }
        }
        public Vector2 FireOffset3 {
            set { fxpFOff3.SetValue(value); }
        }
        public float FireDistortScale {
            set { fxpFDistortScale.SetValue(value); }
        }
        public float FireDistortBias {
            set { fxpFDistortBias.SetValue(value); }
        }

        // Alert
        EffectPass fxPassAlert;


        public ParticleEffect(Effect _fx, ParticleEffectConfig conf) {
            fx = _fx;
            fx.CurrentTechnique = fx.Techniques[0];

            fxPassSimple = fx.CurrentTechnique.Passes[conf.PassSimple];
            fxpVP = fx.Parameters[conf.ParamVP];
            fxpTime = fx.Parameters[conf.ParamTime];
            fxpMapSize = fx.Parameters[conf.ParamMapSize];

            fxPassLightning = fx.CurrentTechnique.Passes[conf.PassLightning];
            fxpLSplits = fx.Parameters[conf.ParamSplits];

            fxPassFire = fx.CurrentTechnique.Passes[conf.PassFire];
            fxpFRates = fx.Parameters[conf.ParamRates];
            fxpFScales = fx.Parameters[conf.ParamScales];
            fxpFOff1 = fx.Parameters[conf.ParamOffset1];
            fxpFOff2 = fx.Parameters[conf.ParamOffset2];
            fxpFOff3 = fx.Parameters[conf.ParamOffset3];
            fxpFDistortScale = fx.Parameters[conf.ParamDistortScale];
            fxpFDistortBias = fx.Parameters[conf.ParamDistortBias];

            fxPassAlert = fx.CurrentTechnique.Passes[conf.PassAlert];

            // Set Default Values
            LightningSplits = DEFAULT_SPLITS;
            FireDistortScale = DEFAULT_DISTORT_SCALE;
            FireDistortBias = DEFAULT_DISTORT_BIAS;
            FireOffset1 = DEFAULT_OFFSET;
            FireOffset2 = DEFAULT_OFFSET;
            FireOffset3 = DEFAULT_OFFSET;
            FireRates = DEFAULT_RATES;
            FireScales = DEFAULT_SCALES;
        }

        public void SetupBasic(GraphicsDevice g, Matrix mVP, float t, Texture2D tFOW) {
            VP = mVP;
            Time = t;
            g.Textures[0] = tFOW;
            g.SamplerStates[0] = SamplerState.PointClamp;
        }

        public void ApplySimple(GraphicsDevice g, Texture2D tColor) {
            g.Textures[1] = tColor;
            g.SamplerStates[1] = SamplerState.LinearWrap;
            fxPassSimple.Apply();
        }
        public void ApplyLightning(GraphicsDevice g, Texture2D tColor) {
            g.Textures[1] = tColor;
            g.SamplerStates[1] = SamplerState.LinearWrap;
            fxPassLightning.Apply();
        }
        public void ApplyFire(GraphicsDevice g, Texture2D tColor, Texture2D tNoise, Texture2D tAlpha) {
            g.Textures[1] = tColor;
            g.SamplerStates[1] = SamplerState.LinearClamp;
            g.Textures[2] = tNoise;
            g.SamplerStates[2] = SamplerState.LinearWrap;
            g.Textures[3] = tAlpha;
            g.SamplerStates[3] = SamplerState.LinearClamp;
            fxPassFire.Apply();
        }
        public void ApplyAlert(GraphicsDevice g, Texture2D tColor) {
            g.Textures[1] = tColor;
            g.SamplerStates[1] = SamplerState.LinearWrap;
            fxPassAlert.Apply();
        }
    }
}