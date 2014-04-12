using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace NovaLibrary.Object
{
    public enum POWER_UP
    {
        GRAV_UP,
        GRAV_DOWN,
        JACKPOT,
        SINK_HOLE
    }

    public class PowerUp : NovaObject
    {
        public static List<PowerUp> ALL = new List<PowerUp>();
        public static void attemptSpawn()
        {
            if (ALL.Count < 10 && NovaObjectContent.RAND.Next(2000) > 980)
            {
                ObjectProcessor.addObject<PowerUp>(out last);
            }
        }
        public static PowerUp last;

        protected POWER_UP powerUp;

        public override void Spawn()
        {
            type = NOVA_TYPE.POWER_UP;
            Radius = 13f;
            Mass = 100f;
            center = new Vector2(
                NovaObjectContent.RAND.Next(GameArea.GameBounds.Width) + GameArea.GameBounds.Left,
                NovaObjectContent.RAND.Next(GameArea.GameBounds.Height) + GameArea.GameBounds.Top
                );
            switch (NovaObjectContent.RAND.Next(4))
            {
                case 0:
                    powerUp = POWER_UP.GRAV_UP;
                    setTexture(NovaObjectContent.Texture(5));
                    color = Color.Green;
                    break;
                case 1:
                    powerUp = POWER_UP.GRAV_DOWN;
                    setTexture(NovaObjectContent.Texture(6));
                    color = Color.Red;
                    break;
                case 2:
                    powerUp = POWER_UP.JACKPOT;
                    setTexture(NovaObjectContent.Texture(7));
                    color = Color.Gold;
                    break;
                case 3:
                    powerUp = POWER_UP.SINK_HOLE;
                    setTexture(NovaObjectContent.Texture(7));
                    color = Color.LightBlue;
                    break;
            }
            ALL.Add(this);
        }

        public override void Update(GameTime time)
        {
        }

        public override void OnCollision(NovaObject o)
        {
            if (o.Type == NOVA_TYPE.STAR)
            {
                NovaEventQueue.addEvent(new NovaEventPowerUp(powerUp));
                ObjectProcessor.removeObject<PowerUp>(this);
                ALL.Remove(this);
            }
        }
    }
}
