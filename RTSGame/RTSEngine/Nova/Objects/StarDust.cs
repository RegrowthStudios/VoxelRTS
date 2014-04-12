using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NovaLibrary.Gravity;

namespace NovaLibrary.Object
{
    public class StarDust : NovaMovingObject, IGravityRecipient, IBoundable
    {
        public static List<StarDust> ALL = new List<StarDust>();
        public static bool canSpawnStar()
        {
            return ALL.Count < 200;
        }
        public static void attemptSpawn()
        {
            if (canSpawnStar() && NovaObjectContent.RAND.Next(200) > 180)
            {
                ObjectProcessor.addObject<StarDust>(out last);
            }
        }
        public static StarDust last;

        public override void Spawn()
        {
            type = NOVA_TYPE.STAR_DUST;
            setTexture(NovaObjectContent.Texture(3));
            color = new Color(
                NovaObjectContent.RAND.Next(255),
                NovaObjectContent.RAND.Next(255),
                NovaObjectContent.RAND.Next(255),
                NovaObjectContent.RAND.Next(60) + 20
                );
            Center = new Vector2(
                NovaObjectContent.RAND.Next(GameArea.GameBounds.Width) + GameArea.GameBounds.Left,
                NovaObjectContent.RAND.Next(GameArea.GameBounds.Height) + GameArea.GameBounds.Top
                );
            Radius = NovaObjectContent.RAND.Next(8) + 5f;
            Mass = Radius * 20f;
            friction = 1.008f;
            ALL.Add(this);
        }

        public override void Update(GameTime time)
        {
            Rotation = MathHelper.WrapAngle(Rotation + 0.05f);
        }

        public override void OnCollision(NovaObject o)
        {
            switch (o.Type)
            {
                case NOVA_TYPE.TETHER:
                    ObjectProcessor.removeObject<StarDust>(this);
                    ALL.Remove(this);
                    return;
                default:
                    return;
            }
        }

        public void accelerateG(Vector2 g, float dTime)
        {
            accelerate(g, dTime);
        }

        public override void Move(float dTime)
        {
            base.Move(dTime);
            CheckBound(GameArea.GameBounds);
        }

        public void CheckBound(Rectangle bounds)
        {
            if (center.Y + radius > bounds.Bottom)
            {
                velocity.Y = -0.8f * velocity.Y;
                Center = new Vector2(center.X, bounds.Bottom - radius - 1f);
            }
            else if (center.Y - radius < bounds.Top)
            {
                velocity.Y = -0.8f * velocity.Y;
                Center = new Vector2(center.X, bounds.Top + radius + 1f);
            }

            if (center.X + radius > bounds.Right)
            {
                velocity.X = -0.8f * velocity.X;
                Center = new Vector2(bounds.Right - radius - 1f, center.Y);
            }
            else if (center.X - radius < bounds.Left)
            {
                velocity.X = -0.8f * velocity.X;
                Center = new Vector2(bounds.Left + radius + 1f, center.Y);
            }
        }
    }
}
