using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grey.Vox;

namespace Grey.Engine {
    public struct VoxStateConfig {
        public int WorkerCount;
    }

    public class VoxState {
        public VoxWorld World {
            get;
            private set;
        }
        public VoxWorkPool VWorkPool {
            get;
            private set;
        }

        private object lckEQ = new object();
        private List<VEvent> eQueue;

        public VoxState() {
            World = new VoxWorld(this);
            VWorkPool = new VoxWorkPool();
            eQueue = new List<VEvent>(4);
        }

        public void AddEvent(VEvent e) {
            lock(lckEQ) {
                eQueue.Add(e);
            }
        }
        public List<VEvent> GetEvents() {
            var l = new List<VEvent>();
            lock(lckEQ) {
                l = System.Threading.Interlocked.Exchange(ref eQueue, l);
            }
            return l;
        }
    }
}