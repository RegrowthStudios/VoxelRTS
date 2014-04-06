using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;

namespace RTS {
    public class CompanyScreen : GameScreen<App> {
        private const float FIRE_MAX_TIME = 5f;
        private const float SCREEN_MAX_TIME = 15f;

        public override int Next {
            get { return game.InduZtryScreen.Index; }
            protected set { }
        }
        public override int Previous {
            get { return -1; }
            protected set { }
        }

        private Effect fx;
        private Texture2D tNoise, tColor, tAlpha;
        private float et;
        private SoundEffect seFire;
        private SoundEffectInstance seiFire;

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            fx = XNAEffect.Compile(G, @"Content\FX\Fire.fx");
            fx.CurrentTechnique = fx.Techniques[0];
            using(var s = File.OpenRead(@"Content\Textures\FireNoise.png")) {
                tNoise = Texture2D.FromStream(G, s);
            }
            using(var s = File.OpenRead(@"Content\Textures\FireColorLogo.png")) {
                tColor = Texture2D.FromStream(G, s);
            }
            using(var s = File.OpenRead(@"Content\Textures\FireAlpha.png")) {
                tAlpha = Texture2D.FromStream(G, s);
            }
            using(var s = File.OpenRead(@"Content\Audio\Fire.wav")) {
                seFire = SoundEffect.FromStream(s);
            }
            et = 0f;
            seiFire = seFire.CreateInstance();
            seiFire.Play();
            seiFire.Volume = 0f;
        }
        public override void OnExit(GameTime gameTime) {
            fx.Dispose();
            tNoise.Dispose();
            tColor.Dispose();
            tAlpha.Dispose();
            seiFire.Dispose();
            seFire.Dispose();
        }

        public override void Update(GameTime gameTime) {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            et += dt;
            seiFire.Volume = MathHelper.Clamp(et / FIRE_MAX_TIME, 0, 1);
        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);

            float l1 = MathHelper.Clamp(et / FIRE_MAX_TIME, 0, 1);
            float l2 = MathHelper.Clamp(et / SCREEN_MAX_TIME, 0, 1);

            G.Textures[0] = tNoise;
            G.SamplerStates[0] = SamplerState.LinearWrap;
            G.Textures[1] = tColor;
            G.SamplerStates[1] = SamplerState.LinearClamp;
            G.Textures[2] = tAlpha;
            G.SamplerStates[2] = SamplerState.LinearClamp;

            G.BlendState = BlendState.NonPremultiplied;
            G.DepthStencilState = DepthStencilState.None;
            G.RasterizerState = RasterizerState.CullNone;

            fx.Parameters["WVP"].SetValue(Matrix.Identity);
            fx.Parameters["time"].SetValue((float)gameTime.TotalGameTime.TotalSeconds);
            fx.Parameters["rates"].SetValue(new Vector3(1.8f, 2.7f, 6f));
            fx.Parameters["scales"].SetValue(new Vector3(1, 3, 5));
            fx.Parameters["offset1"].SetValue(new Vector2(1, 1));
            fx.Parameters["offset2"].SetValue(new Vector2(1, 1));
            fx.Parameters["offset3"].SetValue(new Vector2(1, 1));
            fx.Parameters["distortScale"].SetValue(MathHelper.Lerp(0.3f, 0, MathHelper.Clamp(l2 + 0.4f, 0, 1)));
            fx.Parameters["distortBias"].SetValue(0.01f);
            fx.Parameters["tint"].SetValue(Vector4.Lerp(Vector4.Zero, Vector4.One, l1));
            fx.CurrentTechnique.Passes[0].Apply();

            G.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionTexture[] {
                new VertexPositionTexture(new Vector3(-1, 1, 0), Vector2.Zero),
                new VertexPositionTexture(new Vector3(1, 1, 0), Vector2.UnitX),
                new VertexPositionTexture(new Vector3(-1, -1, 0), Vector2.UnitY),
                new VertexPositionTexture(new Vector3(1, -1, 0), Vector2.One)
            }, 0, 2, VertexPositionTexture.VertexDeclaration);

            if(et > SCREEN_MAX_TIME)
                State = ScreenState.ChangeNext;
        }
    }
}