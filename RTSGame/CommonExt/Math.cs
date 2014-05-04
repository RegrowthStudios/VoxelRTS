using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System {
    public static class Flags {
        public const byte BIT0 = 0x01;
        public const byte BIT1 = 0x02;
        public const byte BIT2 = 0x04;
        public const byte BIT3 = 0x08;
        public const byte BIT4 = 0x10;
        public const byte BIT5 = 0x20;
        public const byte BIT6 = 0x40;
        public const byte BIT7 = 0x80;
        public static readonly byte[] BITS = { BIT0, BIT1, BIT2, BIT3, BIT4, BIT5, BIT6, BIT7 };

        public static bool HasFlags(byte b, byte flags) {
            return (b & flags) == flags;
        }
        public static bool HasFlags(byte b, int flags) {
            return (b & flags) == flags;
        }

        public static bool HasSomeFlags(byte b, byte flags) {
            return (b & flags) != 0;
        }
        public static bool HasSomeFlags(byte b, int flags) {
            return (b & flags) != 0;
        }

        public static void SetFlagsOn(ref byte b, byte flags) {
            b |= flags;
        }
        public static void SetFlagsOff(ref byte b, byte flags) {
            b &= (byte)~flags;
        }
        public static void ToggleFlags(ref byte b, byte flags) {
            b ^= flags;
        }

        public static void SetFlagsOn(ref byte b, int flags) {
            b |= (byte)flags;
        }
        public static void SetFlagsOff(ref byte b, int flags) {
            b &= (byte)~flags;
        }
        public static void ToggleFlags(ref byte b, int flags) {
            b ^= (byte)flags;
        }
    }
}