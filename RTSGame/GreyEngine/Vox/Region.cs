using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace Grey.Vox {
    public class RegionBoundaryState {
        readonly object lck = new object();
        public int State;

        public RegionBoundaryState() {
            State = 0;
        }

        public bool DoWork() {
            bool b = false;
            lock(lck) {
                if(State == 0) {
                    State = 1;
                    b = true;
                }
            }
            return b;
        }
    }

    public class Region {
        // Size Of Array
        public const int XZ_SHIFT = 4;
        public const int Y_SHIFT = 8;
        public const int WIDTH = 0x01 << XZ_SHIFT;
        public const int HEIGHT = 0x01 << Y_SHIFT;
        public const int DEPTH = 0x01 << XZ_SHIFT;
        public const int PLANE_SIZE = WIDTH * DEPTH;
        public const int VOXEL_COUNT = PLANE_SIZE * HEIGHT;

        public const int INDZ_SHIFT = XZ_SHIFT;
        public const int INDY_SHIFT = INDZ_SHIFT * 2;

        public const int REGION_VOXBYTE_SIZE = VOXEL_COUNT * Voxel.BYTE_SIZE;

        // Precondition That All Value Fall Within Range
        public static int ToIndex(int x, int y, int z) {
            return (y << INDY_SHIFT) | (z << INDZ_SHIFT) | x;
        }
        public static int ToIndex(Vector3I v) {
            return ToIndex(v.X, v.Y, v.Z);
        }

        // The World And Its Location Within It
        public VoxWorld World {
            get;
            private set;
        }
        public Point loc;

        // The Important Data
        public Voxel[] voxels;
        public int changedCount;
        public event Action<Region> OnFacesChanged;

        public bool IsLoaded {
            get;
            private set;
        }
        public readonly RegionBoundaryState[] rBS;

        public Region rNX, rPX, rNZ, rPZ;

        public Region(VoxWorld w) {
            World = w;
            loc = Point.Zero;

            // Create Empty Region
            voxels = new Voxel[VOXEL_COUNT];
            for(int i = 0; i < VOXEL_COUNT; i++)
                voxels[i] = Voxel.Empty;

            // Non-calculated Boundaries
            rBS = new RegionBoundaryState[4];
            for(int i = 0; i < 4; i++)
                rBS[i] = new RegionBoundaryState();

            // No Neighbors
            rNX = rPX = rNZ = rPZ = null;
        }

        public void ApplyToAll(Func<Voxel, Vector3I, Voxel> f) {
            int i = 0;
            Vector3I v = Vector3I.Zero;
            for(v.Y = 0; v.Y < HEIGHT; v.Y++) {
                for(v.Z = 0; v.Z < DEPTH; v.Z++) {
                    for(v.X = 0; v.X < WIDTH; v.X++) {
                        voxels[i] = f(voxels[i], v);
                        i++;
                    }
                }
            }
        }

        public bool RecalculateSeam(int vi1, int vi2, int f) {
            VoxData vd1 = World.Atlas[voxels[vi1].ID];
            VoxData vd2 = World.Atlas[voxels[vi2].ID];
            byte flag = Flags.BITS[f];
            if(vd1.FaceType.CanShowFace(vd2.FaceType, f)) {
                if(Flags.HasFlags(voxels[vi1].Flags, flag))
                    return false;
                else {
                    Flags.SetFlagsOn(ref voxels[vi1].Flags, flag);
                    return true;
                }
            }
            else {
                if(!Flags.HasFlags(voxels[vi1].Flags, flag))
                    return false;
                else {
                    Flags.SetFlagsOff(ref voxels[vi1].Flags, flag);
                    return true;
                }
            }
        }
        public bool RecalculateSeams(int vi1, int vi2, int f) {
            return RecalculateSeam(vi1, vi2, f) | RecalculateSeam(vi2, vi1, f ^ 0x01);
        }

        public void SetVoxel(int x, int y, int z, ushort id) {
            voxels[ToIndex(x, y, z)].ID = id;
        }
        public void AddVoxel(int x, int y, int z, ushort id) {
            // Check If A Voxel Exists There
            int i = ToIndex(x, y, z);
            if(voxels[i].ID == id) return;

            // Set The Voxel To Be Empty
            bool change = Flags.HasSomeFlags(voxels[i].Flags, Voxel.FACE_MASK);
            voxels[i].ID = id;
            voxels[i].Flags = 0x00;

            // TODO: Check Surrounding Faces
            if(x < WIDTH - 1)
                change |= RecalculateSeams(i + 1, i, Voxel.FACE_NX);
            if(x > 0)
                change |= RecalculateSeams(i - 1, i, Voxel.FACE_PX);
            if(z < DEPTH - 1)
                change |= RecalculateSeams(i + WIDTH, i, Voxel.FACE_NZ);
            if(z > 0)
                change |= RecalculateSeams(i - WIDTH, i, Voxel.FACE_PZ);
            if(y < HEIGHT - 1)
                change |= RecalculateSeams(i + PLANE_SIZE, i, Voxel.FACE_NY);
            if(y > 0)
                change |= RecalculateSeams(i - PLANE_SIZE, i, Voxel.FACE_PY);

            // TODO: Send Update To Neighboring Regions

            // TODO: Use The Change
            NotifyFacesChanged();
        }
        public void RemoveVoxel(int x, int y, int z) {
            // Check If A Voxel Exists There
            int i = ToIndex(x, y, z);
            if(voxels[i].ID == 0) return;

            // Set The Voxel To Be Empty
            bool change = Flags.HasSomeFlags(voxels[i].Flags, Voxel.FACE_MASK);
            voxels[i] = Voxel.Empty;

            // TODO: Check Surrounding Faces
            if(x < WIDTH - 1)
                change |= RecalculateSeam(i + 1, i, Voxel.FACE_NX);
            if(x > 0)
                change |= RecalculateSeam(i - 1, i, Voxel.FACE_PX);
            if(z < DEPTH - 1)
                change |= RecalculateSeam(i + WIDTH, i, Voxel.FACE_NZ);
            if(z > 0)
                change |= RecalculateSeam(i - WIDTH, i, Voxel.FACE_PZ);
            if(y < HEIGHT - 1)
                change |= RecalculateSeam(i + PLANE_SIZE, i, Voxel.FACE_NY);
            if(y > 0)
                change |= RecalculateSeam(i - PLANE_SIZE, i, Voxel.FACE_PY);

            // TODO: Send Update To Neighboring Regions

            // TODO: Use The Change
            NotifyFacesChanged();
        }

        public void LoadComplete() {
            IsLoaded = true;
        }
        public void NotifyFacesChanged() {
            System.Threading.Interlocked.Increment(ref changedCount);
            if(OnFacesChanged != null)
                OnFacesChanged(this);
        }
    }
}