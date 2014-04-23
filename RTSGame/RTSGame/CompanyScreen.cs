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
using System.Net;
using RTSEngine.Graphics;

namespace RTS {
    public class CompanyScreen : GameScreen<App> {
        private const float FIRE_MAX_TIME = 5f;
        private const float SCREEN_MAX_TIME = 15f;

        public override int Next {
            get { return game.WispScreen.Index; }
            protected set { }
        }
        public override int Previous {
            get { return -1; }
            protected set { }
        }

        private FireShader fxFire;

        private float et;
        private SoundEffect seFire;
        private SoundEffectInstance seiFire;

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            fxFire = new FireShader();
            fxFire.Build(G,
                @"Content\FX\Fire.fx",
                @"Content\Textures\FireNoise.png",
                @"Content\Textures\FireColorLogo.png",
                @"Content\Textures\FireAlpha.png"
                );

            et = 0f;
            using(var s = File.OpenRead(@"Content\Audio\Fire.wav")) seFire = SoundEffect.FromStream(s);
            seiFire = seFire.CreateInstance();
            seiFire.Play();
            seiFire.Volume = 0f;

            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;

            fxFire.Dispose();
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

            G.BlendState = BlendState.NonPremultiplied;
            G.DepthStencilState = DepthStencilState.None;
            G.RasterizerState = RasterizerState.CullNone;

            fxFire.DistortScale = MathHelper.Lerp(0.3f, 0, MathHelper.Clamp(l2 + 0.4f, 0, 1));
            fxFire.Tint = Vector4.Lerp(Vector4.Zero, Vector4.One, l1);
            fxFire.Apply(G, Matrix.Identity, (float)gameTime.TotalGameTime.TotalSeconds);

            G.DrawUserPrimitives(PrimitiveType.TriangleStrip, new VertexPositionTexture[] {
                new VertexPositionTexture(new Vector3(-1, 1, 0), Vector2.Zero),
                new VertexPositionTexture(new Vector3(1, 1, 0), Vector2.UnitX),
                new VertexPositionTexture(new Vector3(-1, -1, 0), Vector2.UnitY),
                new VertexPositionTexture(new Vector3(1, -1, 0), Vector2.One)
            }, 0, 2, VertexPositionTexture.VertexDeclaration);

            if(et > SCREEN_MAX_TIME)
                State = ScreenState.ChangeNext;
        }

        void KeyboardEventDispatcher_OnKeyPressed(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.Space:
                case Keys.Enter:
                case Keys.Escape:
                    State = ScreenState.ChangeNext;
                    break;
            }
        }
    }
}