using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NovaLibrary.Gravity;

namespace NovaLibrary.Object {
    public class NovaStar : NovaMovingObject, IGravityRecipient, IBoundable {
        public override void Spawn() {
            type = NOVA_TYPE.STAR;
            layerDepth = 0.95f;
            setTexture(NovaObjectContent.Texture(2));
            color = Color.Purple;
            Center = new Vector2(GameArea.GameBounds.Center.X, GameArea.GameBounds.Center.Y);
            Radius = 24f;
            friction = 1.018f;
            Mass = 740f;
        }

        public override void Update(GameTime time) {
            Rotation = MathHelper.WrapAngle(Rotation + 0.05f);
        }

        public override void OnCollision(NovaObject o) {
            switch(o.Type) {
                case NOVA_TYPE.ASTEROID:
                    NovaEventQueue.addEvent(NOVA_EVENT.GAME_OVER);
                    break;
            }
        }

        public void accelerateG(Vector2 g, float dTime) {
            accelerate(g, dTime);
        }

        public override void Move(float dTime) {
            base.Move(dTime);
            CheckBound(GameArea.GameBounds);
        }

        public void CheckBound(Rectangle bounds) {
            if(center.Y + radius > bounds.Bottom) {
                velocity.Y = -0.8f * velocity.Y;
                Center = new Vector2(center.X, bounds.Bottom - radius - 1f);
            }
            else if(center.Y - radius < bounds.Top) {
                velocity.Y = -0.8f * velocity.Y;
                Center = new Vector2(center.X, bounds.Top + radius + 1f);
            }

            if(center.X + radius > bounds.Right) {
                velocity.X = -0.8f * velocity.X;
                Center = new Vector2(bounds.Right - radius - 1f, center.Y);
            }
            else if(center.X - radius < bounds.Left) {
                velocity.X = -0.8f * velocity.X;
                Center = new Vector2(bounds.Left + radius + 1f, center.Y);
            }
        }
    }
}