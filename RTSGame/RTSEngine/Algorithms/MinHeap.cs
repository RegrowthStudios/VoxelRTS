using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Algorithms {
    public class MinHeap<T> : List<T> {
        Comparison<T> f;

        public MinHeap(Comparison<T> _f, int capacity = 10)
            : base(capacity) { f = _f; }
        public MinHeap(Comparison<T> _f, IEnumerable<T> collection, int collectionCount)
            : base(collectionCount) {
            f = _f;
            foreach(T o in collection) { Insert(o); }
        }

        public void Insert(T o) {
            Add(default(T));
            int i = Count - 1;
            while(i > 0 && f(this[i / 2], o) > 0) {
                this[i] = this[i / 2];
                i = i / 2;
            }
            this[i] = o;
        }
        public T Pop() {
            if(Count < 0) {
                throw new ArgumentOutOfRangeException();
            }

            T min = this[0];
            this[0] = this[Count - 1];
            RemoveAt(Count - 1);
            this.Heapify(0);
            return min;
        }

        // TODO: Verify
        new public void Remove(T o) {
            int i = 0;
            bool done = false;
            while(!done) {
                if(this[i].Equals(o)) {
                    this[i] = this[Count - 1];
                    RemoveAt(Count - 1);
                    this.Heapify(i);
                    return;
                }
                else {
                    int left = 2 * i + 1;
                    // Look At Left Child Next
                    if(f(o, this[i]) < 0) {
                        i = left;
                    }
                    // Look At Right Child Next
                    else {
                        i = left + 1;
                    }
                    done = !(i < this.Count);
                }
            }
        }

        // TODO: Verify (=)
        new public bool Contains(T o) {
            int i = 0;
            bool done = false;
            while(!done) {
                if(this[i].Equals(o)) return true;
                else {
                    int left = 2 * i + 1;
                    // Look At Left Child Next
                    if(f(o, this[i]) < 0) {
                        i = left;
                    }
                    // Look At Right Child Next
                    else {
                        i = left + 1;
                    }
                    done = !(i < this.Count);
                }
            }
            return false;
        }

        private void Heapify(int i) {
            int smallest;
            int l = 2 * i;
            int r = 2 * i + 1;

            if(l < Count && f(this[l], this[i]) < 0) {
                smallest = l;
            }
            else {
                smallest = i;
            }

            if(r < Count && f(this[r], this[smallest]) < 0) {
                smallest = r;
            }

            if(smallest != i) {
                T tmp = this[i];
                this[i] = this[smallest];
                this[smallest] = tmp;
                this.Heapify(smallest);
            }
        }

        public void ToArray(out T[] a) {
            if(Count < 0) { throw new ArgumentOutOfRangeException(); }
            else {
                int c = Count;
                a = new T[c];
                for(int i = 0; i < c; i++) {
                    a[i] = Pop();
                }
            }
        }
    }
}