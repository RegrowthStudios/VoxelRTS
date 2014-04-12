using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using BlisterUI;
using BlisterUI.Input;
using BlisterUI.Widgets;
using NovaLibrary;
using NovaLibrary.Object;
using NovaLibrary.GUI;
using System.IO;

namespace Nova.Screens {
    public class PlayScreen : GameScreenIndexed {
        public PlayScreen(string root, int i) : base(i) { RootPath = root; }
        public PlayScreen(string root, int p, int n) : base(p, n) { RootPath = root; }

        NovaStar player;
        NovaTether tether;
        float elapsedTime = 0f;
        float timeAddition;
        float distance = 0f;
        float pAdd = 0f;
        bool isGameOver = false;

        InputManager input = null;
        NovaEvent ne;
        NovaEventPoint nep;
        NovaEventPowerUp nepu;

        TimeBar timeBar;
        PointCounter pointCounter;
        ComboText comboText;
        CenterText centerText;
        IDisposable sf;

        bool hasSinkhole;
        Sinkhole lastSinkHoleAdded;

        public string RootPath {
            get;
            set;
        }
        public Rectangle GameBounds {
            get { return GameArea.bounds; }
            set { GameArea.bounds = value; }
        }

        public override void Build() {
            input = new InputManager();
            if(!NovaObjectContent.isInitialized)
                NovaObjectContent.Initialize(G, new string[] {
                    Path.Combine(RootPath, @"Textures\Pixel.png"),
                    Path.Combine(RootPath, @"Textures\Circle.png"),
                    Path.Combine(RootPath, @"Textures\Star.png"),
                    Path.Combine(RootPath, @"Textures\CircleBlur.png"),
                    Path.Combine(RootPath, @"Textures\CircleCrazy.png"),
                    Path.Combine(RootPath, @"Textures\PowerUpGravUp.png"),
                    Path.Combine(RootPath, @"Textures\PowerUpGravDown.png"),
                    Path.Combine(RootPath, @"Textures\PowerUpJackpot.png")
                });
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            input.Refresh();

            SpriteFont f = XNASpriteFont.Compile(G, "Chintzy CPU BRK", 16, out sf);

            timeBar = new TimeBar();
            timeBar.build(GameBounds.Width);
            timeBar.setTime(TimeBar.maxTime);

            comboText = new ComboText();
            comboText.build(f);
            centerText = new CenterText();
            centerText.build(f);
            centerText.setText(CenterTextOptions.MAIN.setText("Survive"));

            pointCounter = new PointCounter();
            pointCounter.build(f, GameBounds.Width);
            pAdd = 0f;

            isGameOver = false;
            ObjectProcessor.addObject<NovaStar>(out player);
            ObjectProcessor.addObject<NovaTether>(out tether);
        }
        public override void OnExit(GameTime gameTime) {
            ObjectProcessor.destroyAll();
            while(NovaEventQueue.getEvent(out ne)) { }
            player = null;
            tether = null;
            sf.Dispose();
        }

        public override void Update(GameTime gameTime) {
            elapsedTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            if(!isGameOver) {
                //Other Figures
                distance = (player.Center - tether.Center).Length() - player.Radius - tether.Radius;

                //Spawns
                StarDust.attemptSpawn();
                Asteroid.AttemptSpawn();
                PointCluster.attemptSpawn();
                PowerUp.attemptSpawn();

                //Updates
                UpdatePoints();
                UpdateTime();
                ObjectProcessor.update(gameTime);
                comboText.update(elapsedTime);
                timeAddition = 0f;
                UpdateInput(gameTime);

                //Events
                while(!isGameOver && NovaEventQueue.getEvent(out ne)) {
                    switch(ne.type) {
                        case NOVA_EVENT.GAME_OVER:
                            NovaEventQueue.addEvent(new NovaEventCenterText("Game Over"));
                            NovaEventQueue.addEvent(new NovaEventCenterText(" - Score - "));
                            NovaEventQueue.addEvent(new NovaEventCenterText(Convert.ToString(pointCounter.points)));
                            isGameOver = true;
                            break;
                        case NOVA_EVENT.POWER_UP:
                            nepu = ne as NovaEventPowerUp;
                            if(nepu != null) {
                                switch(nepu.PowerUp) {
                                    case POWER_UP.GRAV_UP:
                                        centerText.setText(CenterTextOptions.GOOD_POWER_UP.setText("Gravity +"));
                                        tether.Mass += 300;
                                        break;
                                    case POWER_UP.GRAV_DOWN:
                                        centerText.setText(CenterTextOptions.BAD_POWER_UP.setText("Gravity -"));
                                        tether.Mass -= 300;
                                        break;
                                    case POWER_UP.JACKPOT:
                                        centerText.setText(CenterTextOptions.GOOD_POWER_UP.setText("Jackpot"));
                                        pointCounter.addPoints(1000000);
                                        comboText.setCombo(comboText.combo + 3);
                                        break;
                                    case POWER_UP.SINK_HOLE:
                                        hasSinkhole = true;
                                        break;
                                }
                            }
                            break;
                        case NOVA_EVENT.POINT_CHANGE:
                            nep = ne as NovaEventPoint;
                            if(nep != null) {
                                int p = nep.getPoints();
                                pointCounter.addPoints(p);
                                if(p == 25000) {
                                    comboText.setCombo(comboText.combo + 1);
                                    centerText.setText(CenterTextOptions.MAIN.setText("Combo +"));
                                }
                            }
                            break;
                        case NOVA_EVENT.SINK_HOLE_ADDED:
                            lastSinkHoleAdded.Center = new Vector2(
                                input.Mouse.Current.X,
                                input.Mouse.Current.Y
                                );
                            break;
                    }
                }
            }
            else {
                //Game Over
                State = ScreenState.ExitApplication;
            }
            centerText.update(elapsedTime);
            input.Refresh();
        }
        public override void Draw(GameTime gameTime) {
            Viewport pv = G.Viewport;
            G.Viewport = new Viewport(GameArea.bounds);
            Rectangle dr = new Rectangle(0, 0, G.Viewport.Bounds.Width, G.Viewport.Bounds.Height);

            SB.Begin(SpriteSortMode.BackToFront, BlendState.NonPremultiplied);
            SB.Draw(NovaObjectContent.Texture(0), dr, null, Color.Black, 0, Vector2.Zero, SpriteEffects.None, 1f);
            ObjectProcessor.drawAll(SB);
            SB.End();

            SB.Begin();

            timeBar.draw(SB);
            comboText.draw(SB);
            pointCounter.draw(SB);
            centerText.draw(SB);

            SB.End();

            G.Viewport = pv;
        }

        void UpdateInput(GameTime gameTime) {
            //Escape Sequence
            if(input.Keyboard.IsKeyJustPressed(Keys.R)) {
                State = ScreenState.ExitApplication;
                return;
            }
            else if(input.Keyboard.IsKeyJustPressed(Keys.Back)) {
                State = ScreenState.ExitApplication;
                return;
            }

            //Tether
            tether.setActive(input.Mouse.Current.LeftButton == ButtonState.Pressed);
            if(tether.isActive()) {
                Vector2 off = new Vector2(input.Mouse.Current.X - GameArea.bounds.X, input.Mouse.Current.Y - GameArea.bounds.Y) - tether.Center;
                float mDist = MathHelper.Clamp(off.Length(), 0f, 10f);
                if(mDist > 0.6f) {
                    tether.Velocity = Vector2.Normalize(off) * mDist;
                }
            }

            if(input.Mouse.IsButtonJustPressed(MouseButton.Right)) {
                if(hasSinkhole) {
                    ObjectProcessor.addObject<Sinkhole>(out lastSinkHoleAdded);
                    hasSinkhole = false;
                }
            }
        }
        void UpdatePoints() {
            pAdd += 10f * elapsedTime * (comboText.combo + 1);
            if(tether.isActive() && distance < 160f) {
                pAdd += (160f - distance) * elapsedTime * tether.getPercentActive() * (comboText.combo + 1);
            }
            if(pAdd >= 1f) {
                int p = System.ZMath.fastFloor(pAdd);
                pointCounter.addPoints(p);
                pAdd -= p;
            }
        }
        void UpdateTime() {
            if(tether.isActive()) {
                if(distance < 60f) {
                    timeAddition += 4f * (60f - distance) * tether.getPercentActive() * (comboText.combo + 1);
                }
            }

            if(timeAddition > 0.1f) {
                timeBar.setTime(timeBar.time + timeAddition * elapsedTime);
            }
            timeBar.update(elapsedTime);

            if(timeBar.time <= 0) {
                NovaEventQueue.addEvent(NOVA_EVENT.GAME_OVER);
            }
        }
    }
}