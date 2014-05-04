using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Grey.Vox {
    public struct Voxel {
        // Size Of The Struct In Bytes
        public const int BYTE_SIZE = 3;
        // Face Indexing Information
        public const int FACE_NX = 0;
        public const int FACE_PX = 1;
        public const int FACE_NY = 2;
        public const int FACE_PY = 3;
        public const int FACE_NZ = 4;
        public const int FACE_PZ = 5;
        public const int FACE_MASK = ~(System.Flags.BIT6 | System.Flags.BIT7);
        // The Empty Voxel
        private static readonly Voxel vEmpty = new Voxel(0);
        public static Voxel Empty { get { return vEmpty; } }

        // The ID Within The Library
        public ushort ID;

        /* = Bitfield Data =
        // +-+-------------+
        // |0|Voxel Face NX|
        // +-+-------------+
        // |1|Voxel Face PX|
        // +-+-------------+
        // |2|Voxel Face NY|
        // +-+-------------+
        // |3|Voxel Face PY|
        // +-+-------------+
        // |4|Voxel Face NZ|
        // +-+-------------+
        // |5|Voxel Face PZ|
        // +-+-------------+
        ================= */
        public byte Flags;

        public Voxel(ushort id) {
            ID = id;
            Flags = 0x00;
        }
    }
}
