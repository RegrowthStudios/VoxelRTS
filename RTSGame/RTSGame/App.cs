using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using RTSEngine.Graphics;

namespace RTS {
    public class App : Game {
        protected GraphicsDeviceManager graphics;
        protected SpriteBatch spriteBatch;
        SpriteFont font;

        RTSEffect fx;
        Texture2D t, tColor, tModel;
        VertexBuffer vbModel;
        DynamicVertexBuffer dvbInst;
        IndexBuffer ib;

        VertexRTSAnimInst[] instances = new VertexRTSAnimInst[2000];
        Random r = new Random(676);
        int counterPause = 0;

        public App()
            : base() {
            graphics = new GraphicsDeviceManager(this);
            graphics.GraphicsProfile = GraphicsProfile.HiDef;
            graphics.ApplyChanges();
        }

        protected override void Initialize() {
            base.Initialize();
        }
        protected override void LoadContent() {
            Content = new ContentManager(Services, @"Content");
            spriteBatch = new SpriteBatch(GraphicsDevice);

            font = XNASpriteFont.Compile(GraphicsDevice, @"Content\Font.spritefont");
            fx = new RTSEffect(XNAEffect.Compile(GraphicsDevice, @"Content\FX\RTS.fx"));
            using(FileStream fs = File.OpenRead("res/g/anim/Monkey.png")) {
                t = Texture2D.FromStream(GraphicsDevice, fs);
            }
            using(var bmp = System.Drawing.Bitmap.FromFile("res/g/anim/Monkey.png") as System.Drawing.Bitmap) {
                tModel = new Texture2D(GraphicsDevice, t.Width, t.Height, false, SurfaceFormat.Single);
                float[] sData = new float[t.Width * t.Height];
                byte[] datac = new byte[4];
                for(int i = 0; i < sData.Length; i++) {
                    var col = bmp.GetPixel(i % t.Width, i / t.Width);
                    datac[0] = col.B;
                    datac[1] = col.G;
                    datac[2] = col.R;
                    datac[3] = col.A;
                    sData[i] = BitConverter.ToSingle(datac, 0);
                }
                tModel.SetData(sData);
            }
            Vector2 texelSize = new Vector2(1f / (t.Width), 1f / (t.Height));


            VertexPositionNormalTexture[] verts;
            int[] inds;
            using(FileStream fs = File.OpenRead("res/g/model/Monkey.obj")) {
                ObjParser.TryParse(fs, out verts, out inds, ParsingFlags.ConversionOpenGL);
            }
            for(int i = 0; i < verts.Length; i++) {
                verts[i].Position = new Vector3(((float)i / (float)t.Width), 0, 0);
            }
            vbModel = new VertexBuffer(GraphicsDevice, VertexPositionNormalTexture.VertexDeclaration, verts.Length, BufferUsage.WriteOnly);
            vbModel.SetData(verts);
            ib = new IndexBuffer(GraphicsDevice, IndexElementSize.ThirtyTwoBits, inds.Length, BufferUsage.WriteOnly);
            ib.SetData(inds);
            for(int i = 0; i < instances.Length; i++) {
                instances[i].World = Matrix.CreateScale(r.Next(50, 200) / 80f) *
                    Matrix.CreateFromYawPitchRoll(
                        r.Next(0, 700) / 100f,
                        r.Next(0, 700) / 100f,
                        r.Next(0, 700) / 100f
                        ) *
                    Matrix.CreateTranslation(
                        r.Next(-200, 200) / 10f,
                        r.Next(-200, 200) / 10f,
                        r.Next(-500, 0) / 10f
                        );
                instances[i].AnimationFrame = r.Next(0, 64);
            }
            instances[0].World =
                Matrix.CreateScale(3f) *
                Matrix.CreateRotationY(-MathHelper.PiOver2 - 0.7f) *
                Matrix.CreateTranslation(0, -1.4f * 3, 5);
            dvbInst = new DynamicVertexBuffer(GraphicsDevice, VertexRTSAnimInst.Declaration, instances.Length, BufferUsage.WriteOnly);
            dvbInst.SetData(instances);



            using(FileStream fs = File.OpenRead("res/g/tex/Monkey.png")) {
                tColor = Texture2D.FromStream(GraphicsDevice, fs);
            }

            fx.CPrimary = Vector3.UnitX;
            fx.CSecondary = Vector3.UnitY;
            fx.CTertiary = Vector3.UnitZ;
            fx.TexColor = tColor;
            fx.TexModelMap = tModel;
            fx.TexOverlay = tColor;
            fx.World = Matrix.Identity;
            fx.VP = Matrix.CreateLookAt(Vector3.Backward * 12.4f, Vector3.Zero, Vector3.Up) *
                Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, GraphicsDevice.Viewport.AspectRatio, 0.01f, 1000f);
            base.LoadContent();
        }
        protected override void UnloadContent() {

            base.UnloadContent();
        }

        protected override void Update(GameTime gameTime) {

            base.Update(gameTime);
        }
        protected override void Draw(GameTime gameTime) {
            GraphicsDevice.Clear(Color.Black);
            GraphicsDevice.DepthStencilState = DepthStencilState.Default;
            GraphicsDevice.BlendState = BlendState.NonPremultiplied;
            GraphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            //RasterizerState wire = new RasterizerState();
            //wire.FillMode = FillMode.WireFrame;
            //wire.CullMode = CullMode.None;
            //GraphicsDevice.RasterizerState = wire;

            counterPause++;
            if(counterPause > 1) {
                for(int i = 0; i < instances.Length; i++) {
                    instances[i].AnimationFrame = instances[i].AnimationFrame + 1f;
                    if(instances[i].AnimationFrame > 31) instances[i].AnimationFrame = 0;
                }
                counterPause = 0;
            }
            dvbInst.SetData(instances);
            fx.ApplyPassAnimation();
            fx.DrawPassAnimation(GraphicsDevice, vbModel, dvbInst, ib);

            spriteBatch.Begin();
            spriteBatch.DrawString(font, "FPS: " + (1.0 / gameTime.ElapsedGameTime.TotalSeconds), Vector2.One * 9, Color.Black);
            spriteBatch.DrawString(font, "FPS: " + (1.0 / gameTime.ElapsedGameTime.TotalSeconds), Vector2.One * 11, Color.Black);
            spriteBatch.DrawString(font, "FPS: " + (1.0 / gameTime.ElapsedGameTime.TotalSeconds), Vector2.One * 10, Color.Green);
            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
