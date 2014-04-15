using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using NovaLibrary.Object;
using NovaLibrary.Gravity;
using System.Threading;
using System.Diagnostics;
namespace NovaLibrary
{
    public static class ObjectProcessor
    {
        public static List<NovaObject> OBJECTS = new List<NovaObject>();
        public static List<ObjectCollision> COLLISIONS = new List<ObjectCollision>();
        public static List<INovaMotion> MOVING = new List<INovaMotion>();
        public static List<IGravityDonor> GRAV_DONORS = new List<IGravityDonor>();
        public static List<IGravityRecipient> GRAV_RECIPIENTS = new List<IGravityRecipient>();
        public static List<GravityLink> GRAV_LINKS = new List<GravityLink>();

        public static void addObject<T>(out T obj) where T : NovaObject, new()
        {
            obj = new T();
            obj.Spawn();
            if (ObjectCollision.possible(obj.Type))
            {
                foreach (NovaObject o in OBJECTS)
                {
                    if (ObjectCollision.possible(o.Type, obj.Type))
                    {
                        COLLISIONS.Add(new ObjectCollision(o, obj));
                    }
                }
            }
            INovaMotion nm = obj as INovaMotion;
            if (nm != null)
            {
                MOVING.Add(nm);
            }
            IGravityDonor gd;
            IGravityRecipient gr;
            gd = obj as IGravityDonor;
            if (gd != null)
            {
                GRAV_DONORS.Add(gd);
                foreach (IGravityRecipient g in GRAV_RECIPIENTS)
                {
                    if (GravityLink.willLink(obj.Type, g.Type))
                    {
                        GRAV_LINKS.Add(new GravityLink(gd, g));
                    }
                }
            }
            gr = obj as IGravityRecipient;
            if (gr != null)
            {
                GRAV_RECIPIENTS.Add(gr);
                foreach (IGravityDonor g in GRAV_DONORS)
                {
                    if (GravityLink.willLink(g.Type, obj.Type))
                    {
                        GRAV_LINKS.Add(new GravityLink(g, gr));
                    }
                }
            }
            OBJECTS.Add(obj);
        }
        public static void removeObject<T>(T obj) where T : NovaObject, new()
        {
            if (ObjectCollision.possible(obj.Type))
            {
                lock (COLLISIONS)
                {
                    for (int i = 0; i < COLLISIONS.Count; i++)
                    {
                        if (COLLISIONS[i].hasObject(obj))
                        {
                            COLLISIONS.Remove(COLLISIONS[i]);
                            i--;
                        }
                    }
                }
            }
            INovaMotion nm = obj as INovaMotion;
            if (nm != null)
            {
                MOVING.Remove(nm);
            }
            IGravityDonor gd = obj as IGravityDonor;
            IGravityRecipient gr = obj as IGravityRecipient;
            if (gd != null || gr != null)
            {
                lock (GRAV_LINKS)
                {
                    for (int i = 0; i < GRAV_LINKS.Count; i++)
                    {
                        if (GRAV_LINKS[i].hasObject(obj))
                        {
                            GRAV_LINKS.Remove(GRAV_LINKS[i]);
                            i--;
                        }
                    }
                }
                if (gd != null)
                {
                    GRAV_DONORS.Remove(gd);
                }
                if (gr != null)
                {
                    GRAV_RECIPIENTS.Remove(gr);
                }
            }
            OBJECTS.Remove(obj);
        }

        public static void update(GameTime time)
        {
            StarDust.attemptSpawn();
            Asteroid.AttemptSpawn();

            for (int i = 0; i < OBJECTS.Count; i++)
            {
                OBJECTS[i].Update(time);
            }
            for (int i = 0; i < GRAV_LINKS.Count; i++)
            {
                GRAV_LINKS[i].update((float)time.ElapsedGameTime.TotalSeconds);
            }
            for (int i = 0; i < MOVING.Count; i++)
            {
                MOVING[i].Move((float)time.ElapsedGameTime.TotalSeconds);
            }
            for (int i = 0; i < COLLISIONS.Count; i++)
            {
                COLLISIONS[i].checkCollision();
            }
        }
        public static void drawAll(SpriteBatch batch)
        {
            for (int i = 0; i < OBJECTS.Count; i++)
            {
                OBJECTS[i].draw(batch);
            }
        }
        public static void destroyAll()
        {
            OBJECTS.Clear();
            COLLISIONS.Clear();
            MOVING.Clear();
            GRAV_DONORS.Clear();
            GRAV_RECIPIENTS.Clear();
            GRAV_LINKS.Clear();
            StarDust.ALL.Clear();
            Asteroid.ALL.Clear();
            PointCluster.ALL.Clear();
            PowerUp.ALL.Clear();
        }
    }
}
