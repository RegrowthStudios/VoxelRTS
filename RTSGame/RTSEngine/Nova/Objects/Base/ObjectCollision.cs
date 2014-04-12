using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NovaLibrary.Object
{
    public class ObjectCollision
    {
        NovaObject obj1;
        NovaObject obj2;

        public ObjectCollision(NovaObject o1, NovaObject o2)
        {
            obj1 = o1;
            obj2 = o2;
        }

        public bool hasObject(NovaObject o)
        {
            return obj1 == o || obj2 == o;
        }

        public void checkCollision()
        {
            float rSq = obj1.Radius + obj2.Radius;
            rSq *= rSq;
            if ((obj1.Center - obj2.Center).LengthSquared() < rSq)
            {
                //Collision Happened
                obj1.OnCollision(obj2);
                obj2.OnCollision(obj1);
            }
        }

        public static bool possible(NOVA_TYPE t)
        {
            switch (t)
            {
                case NOVA_TYPE.NONE:
                    return false;
                default:
                    return true;
            }
        }
        public static bool possible(NOVA_TYPE t1, NOVA_TYPE t2)
        {
            switch (t1)
            {
                case NOVA_TYPE.NONE:
                    return false;
                case NOVA_TYPE.STAR_DUST:
                    switch (t2)
                    {
                        case NOVA_TYPE.TETHER:
                        case NOVA_TYPE.SINK_HOLE:
                            return true;
                        default:
                            return false;
                    }
                case NOVA_TYPE.ASTEROID:
                    switch (t2)
                    {
                        case NOVA_TYPE.STAR:
                        case NOVA_TYPE.SINK_HOLE:
                            return true;
                        default:
                            return false;
                    }
                case NOVA_TYPE.POINT_CLUSTER:
                    switch (t2)
                    {
                        case NOVA_TYPE.STAR:
                        case NOVA_TYPE.SINK_HOLE:
                            return true;
                        default:
                            return false;
                    }
                case NOVA_TYPE.POWER_UP:
                    switch (t2)
                    {
                        case NOVA_TYPE.STAR:
                            return true;
                        default:
                            return false;
                    }
                case NOVA_TYPE.SINK_HOLE:
                    switch (t2)
                    {
                        case NOVA_TYPE.TETHER:
                        case NOVA_TYPE.SINK_HOLE:
                        case NOVA_TYPE.POWER_UP:
                            return false;
                        default:
                            return true;
                    }
                default:
                    return possible(t2);
            }
        }
    }
}
