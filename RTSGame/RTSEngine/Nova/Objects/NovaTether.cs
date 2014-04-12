using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NovaLibrary.Gravity;
using Microsoft.Xna.Framework.Graphics;

namespace NovaLibrary.Object {
    public class NovaTether : NovaMovingObject, IGravityDonor {
        protected bool active = false;
        const float activityGrowthRate = 0.5f;
        const float activityDecayRate = 0.35f;
        protected float percentActive = 0f;

        protected Color fade1Color = Color.Yellow;
        protected Color fade2Color = Color.Red;

        protected float fade1Size = 13f;
        protected float fade2Size = 18f;

        public override void Spawn() {
            type = NOVA_TYPE.TETHER;
            layerDepth = 0.9f;
            setTexture(NovaObjectContent.Texture(2));
            color = fade1Color;
            Center = new Vector2(GameArea.GameBounds.Center.X + 2, GameArea.GameBounds.Center.Y);
            Radius = fade1Size;
            Mass = 840f;
            friction = 1.06f;
        }

        public override void Update(GameTime time) {
            if(isActive()) {
                if(percentActive < 1f) {
                    percentActive += activityGrowthRate * (float)time.ElapsedGameTime.TotalSeconds;
                    if(percentActive > 1f) {
                        percentActive = 1f;
                    }
                    color = Color.Lerp(fade1Color, fade2Color, percentActive);
                    Radius = MathHelper.Lerp(fade1Size, fade2Size, percentActive);
                }
            }
            else {
                if(percentActive > 0f) {
                    percentActive -= activityDecayRate * (float)time.ElapsedGameTime.TotalSeconds;
                    if(percentActive < 0f) {
                        percentActive = 0f;
                    }
                    color = Color.Lerp(fade1Color, fade2Color, percentActive);
                    Radius = MathHelper.Lerp(fade1Size, fade2Size, percentActive);
                }
            }
            Rotation = MathHelper.WrapAngle(Rotation + 0.18f * percentActive);
        }

        public override void OnCollision(NovaObject o) {
            switch(o.Type) {
                case NOVA_TYPE.STAR:
                    Vector2 off = Center - o.Center;
                    off.Normalize();
                    Center = o.Center + off * (Radius + o.Radius);
                    Velocity = Vector2.Zero;
                    break;
                default:
                    return;
            }
        }

        public void setActive(bool b) {
            active = b;
        }
        public float getPercentActive() {
            return percentActive;
        }
        public bool isActive() {
            return active;
        }
        public Vector2 gAcceleration(IGravityRecipient o) {
            if(active) {
                double theta = Math.Atan2(Center.Y - o.Center.Y, Center.X - o.Center.X);
                float force = GravityLink.G_Constant * Mass * o.Mass / (Center - o.Center).LengthSquared();
                return new Vector2(
                    (float)Math.Cos(theta),
                    (float)Math.Sin(theta)
                    ) * force * percentActive;
            }
            return Vector2.Zero;
        }
    }
}
