using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Threading.Tasks;
using RTSEngine.Data.Parsers;
using RTSEngine.Data;

namespace RTSEngineTests {

    public class ZXPTestObj {
        [ZXParse("MyValue")]
        public int Value;

        [ZXParse("Inner")]
        public ZXPTestObj Rec;

        public ZXPTestObj() {
            Value = -1;
        }

        [ZXParse("Add")]
        public void Add(int i) {
            Value += i;
        }
    }

    class Program {
        static void Main(string[] args) {
            ConvertHeightData(@"C:\InduZtry\Git\Guardian-Entertainment\RTSGame\RTSGame\Packs\Default\maps\0\height.png");
            Console.Read();
        }

        static void TestZXP() {
            string data =
@"    MyValue [10]
    Add({10})
    Add({10})
    Add({10})
    Inner <RTSEngineTests.ZXPTestObj> {
        MyValue [12]
        Add({10})
        Add({10})
        Add({10})
        Inner <RTSEngineTests.ZXPTestObj> {
            MyValue [14]
            Add({10})
            Add({10})
            Add({10})
        }
    }"
;
            ZXPTestObj o = new ZXPTestObj();
            ZXParser.ParseInto(data, o);
            Console.WriteLine(o.Value);
            Console.WriteLine(o.Rec.Value);
            Console.WriteLine(o.Rec.Rec.Value);
            var sw = new StreamWriter("Test.txt");
            ZXParser.Write(sw, o);
            sw.Flush();
            sw.Dispose();
        }
        static void ConvertHeightData(string bmpPath) {
            byte[] col;
            int w, h;
            using(var bmp = Bitmap.FromFile(bmpPath) as Bitmap) {
                w = bmp.Width;
                h = bmp.Height;
                col = new byte[w * h * 4];

                // Convert Bitmap
                System.Drawing.Imaging.BitmapData bd = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(bd.Scan0, col, 0, bd.Stride * bd.Height);
                bmp.UnlockBits(bd);
            }

            // Make Pixels As Floating Point Values
            int i = 0, ci = 0;
            float[] hd = new float[w * h];
            for(int y = 0; y < h; y++) {
                for(int x = 0; x < w; x++) {
                    hd[i++] = 1f - (col[ci + 2] / 255f);
                    ci += 4;
                }
            }

            HeightTile[] tiles = new HeightTile[(w - 1) * (h - 1)];
            int ti = 0;
            for(int y = 0; y < h - 1; y++) {
                for(int x = 0; x < w - 1; x++) {
                    i = y * w + x;
                    tiles[ti].XNZN = hd[i];
                    i++;
                    tiles[ti].XPZN = hd[i];
                    i += w;
                    tiles[ti].XPZP = hd[i];
                    i--;
                    tiles[ti].XNZP = hd[i];
                    ti++;
                }
            }

            ci = 0;
            byte[] convData = new byte[8 + tiles.Length * 16];
            BitConverter.GetBytes(w - 1).CopyTo(convData, ci); ci += 4;
            BitConverter.GetBytes(h - 1).CopyTo(convData, ci); ci += 4;
            for(i = 0; i < tiles.Length; i++) {
                BitConverter.GetBytes(tiles[i].XNZN).CopyTo(convData, ci); ci += 4;
                BitConverter.GetBytes(tiles[i].XPZP).CopyTo(convData, ci); ci += 4;
                BitConverter.GetBytes(tiles[i].XNZP).CopyTo(convData, ci); ci += 4;
                BitConverter.GetBytes(tiles[i].XPZP).CopyTo(convData, ci); ci += 4;
            }
            using(var s = File.Create(bmpPath + ".conv")) {
                var bw = new BinaryWriter(s);
                bw.Write(convData.Length);
                bw.Flush();
                var gs = new GZipStream(s, CompressionMode.Compress);
                gs.Write(convData, 0, convData.Length);
                gs.Flush();
            }
        }
    }
}
