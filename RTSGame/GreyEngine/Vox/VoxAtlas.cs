using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grey.Vox {
    public class VoxAtlas {
        public const int LIB_BUF_START = 100;

        private VoxData[] data;
        public VoxData this[ushort id] {
            get { return data[id]; }
        }
        public int Count {
            get;
            private set;
        }

        public VoxAtlas() {
            data = new VoxData[LIB_BUF_START];
            data[0] = VoxData.Empty;
            Count = 1;
        }

        public VoxData Create() {
            if(Count == data.Length)
                Array.Resize(ref data, data.Length * 2);
            VoxData d = new VoxData((ushort)Count);
            data[Count++] = d;
            return d;
        }
    }
}
