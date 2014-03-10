using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

namespace BlisterUI {
    public class UIEffect {
        public Effect fx;
        public EffectPass fxPass;

        public EffectParameter fxpInvHalfSize;
        public Vector2 ScreenSize {
            set {
                fxpInvHalfSize.SetValue(new Vector2(2f / value.X, 2f / value.Y));
            }
        }
        public EffectParameter fxpTexture;
        public Texture2D GlyphMap {
            set {
                fxpTexture.SetValue(value);
            }
        }

        public UIEffect(Effect _fx) {
            fx = _fx;
            fx.CurrentTechnique = fx.Techniques[0];
            fxPass = fx.CurrentTechnique.Passes[0];
            fxpInvHalfSize = fx.Parameters["IHS"];
            fxpTexture = fx.Parameters["Texture"];
        }

        public void apply() {
            fxPass.Apply();
        }
    }
}
