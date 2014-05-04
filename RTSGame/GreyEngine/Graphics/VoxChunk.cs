using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grey.Graphics {
    public class VoxChunk {
        public int yStart, yEnd;

        private int change;
        public bool BeginResolve() {
            int p = System.Threading.Interlocked.CompareExchange(ref change, 2, 1);
            return p == 1;
        }
        public void MarkChanged() {
            System.Threading.Interlocked.CompareExchange(ref change, 1, 0);
        }
        public void MarkResolved() {
            System.Threading.Interlocked.CompareExchange(ref change, 0, 2);
        }
    }
}