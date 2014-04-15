using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using NovaLibrary.Gravity;

namespace NovaLibrary.Object
{
    public class Sinkhole : NovaObject, IGravityDonor
    {
        public override void Spawn()
        {
            type = NOVA_TYPE.SINK_HOLE;
            Mass = 700f;
            setTexture(NovaObjectContent.Texture(2));
            color = Color.CadetBlue;
            Radius = 13f;

            NovaEventQueue.addEvent(NOVA_EVENT.SINK_HOLE_ADDED);
        }

        public override void Update(GameTime time)
        {
        }

        public override void OnCollision(NovaObject o)
        {
            //Place It On The Outer Edge
            Vector2 off = o.Center - Center;
            off.Normalize();
            o.Center = Center + off * (o.Radius + Radius);

            //Bounce Off Moving Objects
            NovaMovingObject mo = o as NovaMovingObject;
            if (mo != null)
            {
                mo.Velocity = Vector2.Reflect(mo.Velocity, off) * 0.9f;
            }
        }

        public Vector2 gAcceleration(IGravityRecipient o)
        {
            double theta = Math.Atan2(Center.Y - o.Center.Y, Center.X - o.Center.X);
            float force = GravityLink.G_Constant * Mass * o.Mass / (Center - o.Center).LengthSquared();
            return new Vector2(
                (float)Math.Cos(theta),
                (float)Math.Sin(theta)
                ) * force;
        }
    }
}
