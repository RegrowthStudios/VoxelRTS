using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace NovaLibrary
{
    public static class NovaEventQueue
    {
        private static Queue<NovaEvent> queue = new Queue<NovaEvent>();

        public static void addEvent(NOVA_EVENT type)
        {
            lock (queue)
            {
                queue.Enqueue(new NovaEvent(type));
            }
        }
        public static void addEvent(NovaEvent e)
        {
            lock (queue)
            {
                queue.Enqueue(e);
            }
        }

        public static bool getEvent(out NovaEvent e)
        {
            lock (queue)
            {
                if (queue.Count > 0)
                {
                    e = queue.Dequeue();
                    return true;
                }
            }
            e = null;
            return false;
        }
    }
}
