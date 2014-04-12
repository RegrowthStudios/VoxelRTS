using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NovaLibrary.Object
{
    public class PointCluster : NovaObject
    {
        public static List<PointCluster> ALL = new List<PointCluster>();
        public static void attemptSpawn()
        {
            if (ALL.Count < 30 && NovaObjectContent.RAND.Next(200) > 180)
            {
                ObjectProcessor.addObject<PointCluster>(out last);
            }
        }
        public static PointCluster last;

        public override void Spawn()
        {
            type = NOVA_TYPE.POINT_CLUSTER;
            setTexture(NovaObjectContent.Texture(4));
            center = new Vector2(
                NovaObjectContent.RAND.Next(GameArea.GameBounds.Width) + GameArea.GameBounds.Left,
                NovaObjectContent.RAND.Next(GameArea.GameBounds.Height) + GameArea.GameBounds.Top
                );
            switch (NovaObjectContent.RAND.Next(4))
            {
                case 0:
                    Mass = 1000;
                    Radius = 3;
                    color = Color.White;
                    break;
                case 1:
                    Mass = 5000;
                    Radius = 6;
                    color = Color.Red;
                    break;
                case 2:
                    Mass = 10000;
                    Radius = 8;
                    color = Color.Orange;
                    break;
                case 3:
                    Mass = 25000;
                    Radius = 12;
                    color = Color.DimGray;
                    break;
            }
            ALL.Add(this);
        }

        public override void Update(GameTime time)
        {
            Rotation = MathHelper.WrapAngle(Rotation + 0.05f);
        }

        public override void OnCollision(NovaObject o)
        {
            if (o.Type == NOVA_TYPE.STAR)
            {
                ObjectProcessor.removeObject<PointCluster>(this);
                ALL.Remove(this);
                NovaEventQueue.addEvent(new NovaEventPoint((int)Mass));
            }
        }
    }
}
