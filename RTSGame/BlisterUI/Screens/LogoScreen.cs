using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace BlisterUI
{
    public class LogoScreen : GameScreenIndexed
    {
        protected LogoList logoList;
        public LogoScreen(int previousScreen, int nextScreen, params Logo[] logos)
            : base(previousScreen, nextScreen)
        {
            logoList = new LogoList(logos);
        }

        public override void Build()
        {
            logoList.build(game.GraphicsDevice);
        }
        public override void Destroy(GameTime gameTime)
        {
            logoList.Dispose();
        }

        public override void OnEntry(GameTime gameTime)
        {
            logoList.reset();
        }
        public override void OnExit(GameTime gameTime)
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (logoList.update((float)gameTime.ElapsedGameTime.TotalSeconds))
            {
                State = ScreenState.ChangeNext;
                return;
            }
        }
        public override void Draw(GameTime gameTime)
        {
            game.GraphicsDevice.Clear(Color.Black);
            game.SpriteBatch.Begin();
            logoList.draw(game.SpriteBatch);
            game.SpriteBatch.End();
        }

        public class Logo : IDisposable
        {
            public float Duration;
            protected float timeIn, fade;
            protected LinkedList<FadeOptions> fades;

            public Color Color;
            public float Rotation;
            protected Vector2 size, scale, location, tSize, tCenter;
            protected string tPath;
            protected Texture2D texture;

            protected Logo next, previous;

            public Logo(string path, float duration)
            {
                fades = new LinkedList<FadeOptions>();
                Duration = duration;
                reset();

                tPath = path;
            }
            public Logo(string path, float duration, Vector2 size)
                : this(path, duration)
            {
                this.size = size;
            }

            public void build(GraphicsDevice g)
            {
                using (FileStream fs = File.Open(tPath, FileMode.Open))
                {
                    texture = Texture2D.FromStream(g, fs);
                }
                tSize = new Vector2(texture.Width, texture.Height);
                tCenter = tSize / 2f;
                location = new Vector2(g.Viewport.Width, g.Viewport.Height) / 2f;

                setSize(tSize);
            }

            public void reset()
            {
                timeIn = Duration;
                fade = 0f;
                foreach (FadeOptions fo in fades)
                {
                    fo.reset();
                }
            }

            public void setNext(Logo l)
            {
                next = l;
            }
            public void setPrevious(Logo l)
            {
                previous = l;
            }

            public void addFade(FadeOptions opt)
            {
                fades.AddLast(opt);
            }

            public void setSize(Vector2 s)
            {
                size = s;
                scale = size / tSize;
            }

            public bool update(float elapsedTime)
            {
                timeIn -= elapsedTime;
                fade += elapsedTime / Duration;
                foreach (FadeOptions fo in fades)
                {
                    fo.applyFade(fade);
                }
                return timeIn <= 0f;
            }
            public bool goNext(out Logo l)
            {
                l = next;
                return next != null;
            }

            public void draw(SpriteBatch batch)
            {
                batch.Draw(texture, location, null, Color, Rotation, tCenter, scale, SpriteEffects.None, 0f);
            }

            public abstract class FadeOptions
            {
                protected Logo logo;

                public FadeOptions(Logo l)
                {
                    logo = l;
                    reset();
                }

                public abstract void applyFade(float fade);
                public abstract void reset();
            }
            public abstract class FadeTimelineOptions : FadeOptions
            {
                float pS, pE, pDif;
                bool setLast;

                public FadeTimelineOptions(Logo l, float pStart, float pEnd)
                    : base(l)
                {
                    setLast = false;
                    pS = pStart;
                    pE = pEnd;
                    pDif = pE - pS;
                }

                public override void applyFade(float percent)
                {
                    if (percent >= pS && !setLast)
                    {
                        if (percent <= pE)
                        {
                            applyTrueFade((percent - pS) / pDif);
                        }
                        else
                        {
                            applyTrueFade(1f);
                        }
                    }
                }
                public abstract void applyTrueFade(float pFade);

                public override void reset()
                {
                    setLast = false;
                }
            }
            public class FadeColorOptions : FadeTimelineOptions
            {
                Color c1, c2;

                public FadeColorOptions(Logo l, float pStart, float pEnd, Color col1, Color col2)
                    : base(l, pStart, pEnd)
                {
                    c1 = col1;
                    c2 = col2;
                }

                public override void applyTrueFade(float pFade)
                {
                    logo.Color = Color.Lerp(c1, c2, pFade);
                }

            }
            public class FadeSizeOptions : FadeTimelineOptions
            {
                Vector2 s1, s2;

                public FadeSizeOptions(Logo l, float pStart, float pEnd, Vector2 siz1, Vector2 siz2)
                    : base(l, pStart, pEnd)
                {
                    s1 = siz1;
                    s2 = siz2;
                }

                public override void applyTrueFade(float pFade)
                {
                    logo.setSize(Vector2.Lerp(s1, s2, pFade));
                }
            }
            public class FadeRotationOptions : FadeTimelineOptions
            {
                float r1, r2, rDif;

                public FadeRotationOptions(Logo l, float pStart, float pEnd, float rot1, float rot2)
                    : base(l, pStart, pEnd)
                {
                    r1 = rot1;
                    r2 = rot2;
                    rDif = r2 - r1;
                }

                public override void applyTrueFade(float pFade)
                {
                    logo.Rotation = r1 + rDif * pFade;
                }
            }

            public void Dispose()
            {
                texture.Dispose();
            }
        }
        public class LogoList : IDisposable
        {
            Logo[] logos;
            Logo current;

            public LogoList(params Logo[] l)
            {
                logos = new Logo[l.Length];
                Array.Copy(l, logos, l.Length);

                current = logos[0];

                for (int i = 0; i < logos.Length; i++)
                {
                    logos[i].setNext((i < logos.Length - 1) ? logos[i + 1] : null);
                    logos[i].setPrevious((i > 0) ? logos[i - 1] : null);
                }
                reset();
            }

            public void build(GraphicsDevice g)
            {
                foreach (Logo l in logos)
                {
                    l.build(g);
                }
            }

            public void reset()
            {
                foreach (Logo l in logos)
                {
                    l.reset();
                }
            }
            public bool update(float elapsedTime)
            {
                if (current != null && current.update(elapsedTime))
                {
                    if (current.goNext(out current))
                    {
                        return false;
                    }
                    return true;
                }
                return current == null;
            }
            public void draw(SpriteBatch batch)
            {
                current.draw(batch);
            }

            public void Dispose()
            {
                foreach (Logo l in logos)
                {
                    l.Dispose();
                }
            }
        }
    }
}
