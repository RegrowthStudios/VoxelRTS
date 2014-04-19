using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Runtime.InteropServices;
using RTSEngine.Controllers;
using RTSEngine.Graphics;

namespace RTSEngine.Net {
    public static class NetSerializer {
        public const string HEADER_VISUAL_TEAM = "visteam";

        public static void Write(ref byte[] buf, ref int off, int v) {
            Marshal.WriteInt32(buf, off, v);
            off += sizeof(int);
        }
        public static void Write(ref byte[] buf, ref int off, string v) {
            byte[] b = ASCIIEncoding.Unicode.GetBytes(v);

            // Write String Length In Bytes
            Write(ref buf, ref off, b.Length);

            // Write String
            b.CopyTo(buf, off);
            off += b.Length;
        }

        public static int ReadInt(ref byte[] buf, ref int off) {
            off += sizeof(int);
            return Marshal.ReadInt32(buf, off);
        }
        public static string ReadString(ref byte[] buf, ref int off) {
            // Read Size Of String
            int l = ReadInt(ref buf, ref off);

            // Read String
            byte[] b = new byte[l];
            string s = ASCIIEncoding.Unicode.GetString(buf, off, l);
            off += l;

            return s;
        }

        public static void Serialize(ref byte[] buf, ref int off) {
        }
        public static void Deserialize(ref byte[] buf, ref int off) {
        }
    }
}