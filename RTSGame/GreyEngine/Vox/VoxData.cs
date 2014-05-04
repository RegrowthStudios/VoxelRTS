using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Grey.Graphics;

namespace Grey.Vox {
    public class VoxFaceType {
        public const uint ALL = 0xffffffffu;
        public const uint NONE = 0x00000000u;

        public uint[] Types;
        public uint[] AllowTypes;

        public VoxFaceType() {
            Types = new uint[] { NONE, NONE, NONE, NONE, NONE, NONE };
            AllowTypes = new uint[] { ALL, ALL, ALL, ALL, ALL, ALL };
        }

        public void SetAllTypes(uint t) {
            Types[0] = t;
            Types[1] = t;
            Types[2] = t;
            Types[3] = t;
            Types[4] = t;
            Types[5] = t;
        }
        public void SetAllMasks(uint t) {
            AllowTypes[0] = t;
            AllowTypes[1] = t;
            AllowTypes[2] = t;
            AllowTypes[3] = t;
            AllowTypes[4] = t;
            AllowTypes[5] = t;
        }

        public bool CanShowFace(VoxFaceType neighbor, int f) {
            return (Types[f] & neighbor.AllowTypes[f ^ 0x01]) != 0;
        }
    }

    public class VoxData {
        private static readonly VoxData vEmpty;
        public static VoxData Empty { get { return vEmpty; } }
        static VoxData() {
            vEmpty = new VoxData(0);
        }

        // ID Within The Library
        public readonly ushort ID;

        // What Kinds Of Faces This Voxel Displays
        public VoxFaceType FaceType;

        // Geometry Provider For This Voxel
        public IVGeoProvider GeoProvider;

        public VoxData(ushort id) {
            ID = id;
            FaceType = new VoxFaceType();
            GeoProvider = null;
        }
    }
}