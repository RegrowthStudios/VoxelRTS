using System;
using System.Threading;
using System.Collections.Generic;

namespace System.Collections.Generic {
    public abstract class ACThreadPool<T> : IDisposable where T : class {
        private readonly object lckQueue = new object();

        private Thread[] workers;
        public ThreadPriority Priority {
            get;
            private set;
        }

        private Queue<T> queue = new Queue<T>();

        public void Dispose() {
            End(true);
        }

        public void Start(int count, ThreadPriority tp = ThreadPriority.Normal) {
            if(workers != null) Dispose();

            workers = new Thread[count];
            Priority = tp;
            for(int i = 0; i < workers.Length; i++) {
                workers[i] = new Thread(Work);
                workers[i].IsBackground = true;
                workers[i].Priority = Priority;
                workers[i].Start();
            }
        }
        public void End(bool wait = true) {
            foreach(Thread worker in workers)
                AddWork(null);

            if(wait)
                foreach(Thread worker in workers)
                    worker.Join();
        }

        public void AddWork(T o) {
            lock(lckQueue) {
                queue.Enqueue(o);
                Monitor.Pulse(lckQueue);
            }
        }
        private void Work() {
            while(true) {
                T item;
                lock(lckQueue) {
                    while(queue.Count == 0)
                        Monitor.Wait(lckQueue);
                    item = queue.Dequeue();
                }
                // Exit Condition
                if(item == null) return;

                // Work Task
                DoWork(item);
            }
        }

        protected abstract void DoWork(T o);
    }
}