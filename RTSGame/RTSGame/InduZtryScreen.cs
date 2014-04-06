using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Graphics;
using BlisterUI;
using BlisterUI.Input;

namespace RTS {
    public class InduZtryScreen : GameScreenIndexed {
        private const string SOUND_FILE_LIGHTNING = @"Content\Audio\lightning.wav";
        private const string SOUND_FILE_THUNDER = @"Content\Audio\thunder.wav";
        private const string TEXTURE_FILE = @"Content\Textures\InduZtry.png";

        // Lightning Fade Time
        private const float LERP_TIME = 4f;
        private const float THUNDER_TIME = 1.5f;
        private const int LIGHTNING_PLAY_PAUSE = 5;

        // Lightning Arguments
        private const float BOLT_WIDTH = 4f, BOLT_JAG = 3f, BOLT_MIN = 1f, BOLT_MAX = 6f;
        private const float BRANCH_WIDTH = 2.33333f, BRANCH_JAG = 3f, BRANCH_MIN = 6f, BRANCH_MAX = 24f, BRANCH_SLOPE = 0.0005f;

        // Lightning Color Combinations
        private static readonly Color[] textColors = {
            new Color(0.2f, 0.7f, 0.06f),
            new Color(1f, 0.1f, 0.06f)
        };
        private static readonly Color[] backColors = {
            new Color(0.1f, 0.01f, 1f),
            new Color(0.8f, 0.1f, 1f)
        };

        public InduZtryScreen(int i) : base(i) { }
        public InduZtryScreen(int p, int n) : base(p, n) { }

        // Where To Spawn Lightning Within An Image
        private List<Vector2> textPos;

        // The Lightning Bolt Piece Texture
        private Texture2D tLightning;
        private Vector2 scaleLEnd, scaleLMid;
        private Vector2 originLEndL, originLMid, originLEndR;
        private Rectangle rsLEndL, rsLMid, rsLEndR;

        // Fading Lightning Render Targets
        private RenderTarget2D rtLightning, rtLightningPrev;
        private RenderTarget2D rtLightningBr, rtLightningBrPrev;

        // The Arguments Passed Into The Lightning Generator
        private LightningGenerator.BoltArgs lBoltArgs;
        private LightningGenerator.BranchArgs lBranchArgs;
        private Random r;
        private SoundEffect seLightning, seThunder;
        private float nextThunder;
        private int lastLightning;

        // How To Calculate A Fade Between Two Types Of Lightning
        private float t;
        private float Fade {
            get { return t < 0 ? 1 : 1 - t / LERP_TIME; }
        }
        private float BoltFade {
            get {
                return MathHelper.Lerp(0.99f, 0.95f, Fade * Fade * Fade);
            }
        }
        private int LightningPerFrame {
            get {
                return (int)MathHelper.Lerp(1, 490, Fade * Fade);
            }
        }
        private int LightningNeighborChecks {
            get {
                return (int)MathHelper.Lerp(0, 130, Fade * Fade);
            }
        }

        public override void Build() {
            Random rColor = new Random();
            int ci = rColor.Next(0, 2);

            lBoltArgs.JagDisplacement = BOLT_JAG;
            lBoltArgs.LineMinLength = BOLT_MIN;
            lBoltArgs.LineMaxLength = BOLT_MAX;
            lBoltArgs.Color = textColors[ci];

            lBranchArgs.JagDisplacement = BRANCH_JAG;
            lBranchArgs.LineMinLength = BRANCH_MIN;
            lBranchArgs.LineMaxLength = BRANCH_MAX;
            lBranchArgs.BranchSlope = BRANCH_SLOPE;
            lBranchArgs.MinBounds = Vector2.Zero;
            lBranchArgs.MaxBounds = ViewSize;
            lBranchArgs.Color = backColors[ci];
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;

            t = LERP_TIME;
            r = new Random(343);

            rtLightning = new RenderTarget2D(G, G.Viewport.Width, G.Viewport.Height, false, SurfaceFormat.HdrBlendable, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            rtLightningPrev = new RenderTarget2D(G, G.Viewport.Width, G.Viewport.Height, false, SurfaceFormat.HdrBlendable, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            rtLightningBr = new RenderTarget2D(G, G.Viewport.Width, G.Viewport.Height, false, SurfaceFormat.HdrBlendable, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            rtLightningBrPrev = new RenderTarget2D(G, G.Viewport.Width, G.Viewport.Height, false, SurfaceFormat.HdrBlendable, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);

            using(var fs = System.IO.File.OpenRead("Content\\Textures\\Lightning.png")) {
                tLightning = Texture2D.FromStream(G, fs);
            }
            textPos = new List<Vector2>();
            using(var bmp = System.Drawing.Bitmap.FromFile(TEXTURE_FILE) as System.Drawing.Bitmap) {
                for(int y = 0; y < bmp.Height; y++) {
                    for(int x = 0; x < bmp.Width; x++) {
                        if(bmp.GetPixel(x, y).A > 60) {
                            Vector2 p = ViewSize / 2f;
                            p.X += x - bmp.Width * 0.5f;
                            p.Y += y - bmp.Height * 0.5f;
                            textPos.Add(p);
                        }
                    }
                }
            }
            rsLEndL = new Rectangle(0, 0, tLightning.Width / 4, tLightning.Height);
            rsLMid = rsLEndL;
            rsLMid.X += rsLEndL.Width;
            rsLMid.Width *= 2;
            rsLEndR = rsLMid;
            rsLEndR.X += rsLMid.Width;
            rsLEndR.Width /= 2;
            scaleLEnd = new Vector2(4f / tLightning.Width, 1f / tLightning.Height);
            scaleLMid = new Vector2(2f / tLightning.Width, 1f / tLightning.Height);
            originLEndL = new Vector2(rsLEndL.X + rsLEndL.Width, rsLEndL.Y + rsLEndL.Height / 2f);
            originLMid = new Vector2(0, rsLMid.Y + rsLMid.Height / 2f);
            originLEndR = new Vector2(0, rsLEndR.Y + rsLEndR.Height / 2f);

            using(var s = System.IO.File.OpenRead(SOUND_FILE_LIGHTNING)) {
                seLightning = SoundEffect.FromStream(s);
            }
            using(var s = System.IO.File.OpenRead(SOUND_FILE_THUNDER)) {
                seThunder = SoundEffect.FromStream(s);
            }
            nextThunder = 0;
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;

            rtLightning.Dispose();
            rtLightningPrev.Dispose();
            tLightning.Dispose();
            seLightning.Dispose();
            seThunder.Dispose();
        }

        private void DrawLightning(LightningData data, float w) {
            SB.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.LinearClamp, DepthStencilState.None, RasterizerState.CullNone);
            for(int i = 0; i < data.Lines.Count; ) {
                Vector2 s = data.Lines[i++];
                Vector2 e = data.Lines[i++];
                Vector2 d = e - s;
                float dist = d.Length();
                d /= dist;
                float r = (float)Math.Atan2(d.Y, d.X);
                SB.Draw(tLightning, s, rsLEndL, data.Color, r, originLEndL, new Vector2(w, w) * scaleLEnd, SpriteEffects.None, 0);
                SB.Draw(tLightning, s, rsLMid, data.Color, r, originLMid, new Vector2(dist, w) * scaleLMid, SpriteEffects.None, 0);
                SB.Draw(tLightning, e, rsLEndR, data.Color, r, originLEndR, new Vector2(w, w) * scaleLEnd, SpriteEffects.None, 0);
            }
            SB.End();
        }

        public override void Update(GameTime gameTime) {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
            t -= dt;
            nextThunder -= dt;
            if(nextThunder < 0) {
                seThunder.Play();
                nextThunder = (float)(r.NextDouble() * THUNDER_TIME + 1.5);
            }
        }
        public override void Draw(GameTime gameTime) {
            if(lastLightning > 0)
                lastLightning--;
            bool playSound = false;

            // Compute Lightning
            LightningData bolts = new LightningData(lBoltArgs.Color);
            LightningData branches = new LightningData(lBranchArgs.Color);
            int rC = r.Next(LightningPerFrame);
            for(int i = 0; i < rC; i++) {
                Vector2 ts = Vector2.Zero, te = ts;
                if(r.Next(4000) < 3999) {
                    te = ViewSize / 2f;
                    ts = textPos[r.Next(textPos.Count)];
                    float d2 = float.MaxValue;
                    for(int ri = 0; ri < LightningNeighborChecks; ri++) {
                        Vector2 tet = textPos[r.Next(textPos.Count)];
                        float d2t = (tet - ts).LengthSquared();
                        if(d2t < d2) {
                            d2 = d2t;
                            te = tet;
                        }
                    }
                    lBoltArgs.Start = ts;
                    lBoltArgs.End = te;
                    LightningGenerator.CreateLightning(lBoltArgs, ref bolts, r);
                }
                else {
                    playSound = true;
                    float bsx = (float)r.NextDouble() * ViewSize.X;
                    lBranchArgs.Start = new Vector2(bsx, 0);
                    lBranchArgs.End = new Vector2(bsx, ViewSize.Y * 10f);
                    LightningGenerator.CreateBranch(lBranchArgs, ref branches, r);
                }
            }
            playSound &= lastLightning == 0;
            if(playSound) {
                lastLightning = LIGHTNING_PLAY_PAUSE;
                seLightning.Play();
            }
            // Set Render Target To Lightning
            G.SetRenderTarget(rtLightning);
            G.Clear(Color.Transparent);

            // Draw Faded Lightning
            SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            SB.Draw(rtLightningPrev, G.Viewport.TitleSafeArea, new Color(BoltFade, BoltFade, BoltFade, BoltFade));
            SB.End();
            DrawLightning(bolts, BOLT_WIDTH);

            // Set Render Target To Lightning Branches
            G.SetRenderTarget(rtLightningBr);
            G.Clear(Color.Transparent);

            // Draw Faded Lightning Branches
            SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            SB.Draw(rtLightningBrPrev, G.Viewport.TitleSafeArea, new Color(BoltFade, BoltFade, BoltFade, BoltFade));
            SB.End();
            DrawLightning(branches, BRANCH_WIDTH);
            SB.Begin(SpriteSortMode.Immediate, BlendState.AlphaBlend, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            SB.Draw(tLightning, ViewSize / 2f, null, Color.Black, 0, new Vector2(tLightning.Width, tLightning.Height) / 2f, new Vector2(4f, 0.7f), SpriteEffects.None, 0);
            SB.End();

            // Draw Lightning To User
            G.SetRenderTarget(null);
            G.Clear(Color.Black);
            SB.Begin(SpriteSortMode.Immediate, BlendState.Additive, SamplerState.PointClamp, DepthStencilState.None, RasterizerState.CullNone);
            SB.Draw(rtLightning, G.Viewport.TitleSafeArea, Color.White);
            SB.Draw(rtLightningBr, G.Viewport.TitleSafeArea, Color.White);
            SB.End();


            // Swap Lightning Render Targets
            var b = rtLightning;
            rtLightning = rtLightningPrev;
            rtLightningPrev = b;
            b = rtLightningBr;
            rtLightningBr = rtLightningBrPrev;
            rtLightningBrPrev = b;


        }

        void KeyboardEventDispatcher_OnKeyPressed(object sender, KeyEventArgs args) {
            State = ScreenState.ChangeNext;
        }
    }
}