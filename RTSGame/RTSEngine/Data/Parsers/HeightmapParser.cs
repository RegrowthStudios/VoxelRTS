using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using System.Drawing;
using BColor = System.Drawing.Color;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Graphics;

namespace RTSEngine.Data.Parsers {
    public struct HeightmapResult {
        public Heightmap Data;
        public HeightmapModel View;
    }

    class HeightmapReadData {
        public const int DATA_COUNT = 8;

        // Private Data
        private string dat, hmp, htp, hms, hts;
        private Vector3 size;

        // Logical Accessors
        public string HMDataBmp {
            get { return dat; }
            set {
                dat = value;
                isSet[0] = true;
            }
        }
        public string HMModelPrimary {
            get { return hmp; }
            set {
                hmp = value;
                isSet[1] = true;
            }
        }
        public string HMTexPrimary {
            get { return htp; }
            set {
                htp = value;
                isSet[2] = true;
            }
        }
        public string HMModelSecondary {
            get { return hms; }
            set {
                hms = value;
                isSet[3] = true;
            }
        }
        public string HMTexSecondary {
            get { return hts; }
            set {
                hts = value;
                isSet[4] = true;
            }
        }
        public float SizeX {
            get { return size.X; }
            set {
                size.X = value;
                isSet[5] = true;
            }
        }
        public float SizeZ {
            get { return size.Z; }
            set {
                size.Z = value;
                isSet[6] = true;
            }
        }
        public float SizeY {
            get { return size.Y; }
            set {
                size.Y = value;
                isSet[7] = true;
            }
        }

        public bool HasSecondary {
            get { return HMModelSecondary != null && HMTexSecondary != null; }
        }

        // How Much Data Has Been Set Already
        private bool[] isSet;
        public int ReadCount {
            get {
                return isSet.Aggregate(0, (i, b) => { return b ? (i + 1) : i; });
            }
        }
        public bool IsAllRead {
            get { return ReadCount >= isSet.Length; }
        }

        public HeightmapReadData() {
            isSet = new bool[DATA_COUNT];
            dat = null;
            hmp = null;
            htp = null;
            hms = null;
            hts = null;
            size = Vector3.One;
        }
    }

    public static class HeightmapParser {
        // Data Detection
        const string INFO_FILE_EXT = "map";
        static readonly Regex rgxDataFile = RegexHelper.GenerateFile(@"DAT");
        static readonly Regex rgxHModelPFile = RegexHelper.GenerateFile(@"HMP");
        static readonly Regex rgxHTexPFile = RegexHelper.GenerateFile(@"HTP");
        static readonly Regex rgxHModelSFile = RegexHelper.GenerateFile(@"HMS");
        static readonly Regex rgxHTexSFile = RegexHelper.GenerateFile(@"HTS");
        static readonly Regex rgxSizeXFile = RegexHelper.GenerateNumber(@"SZX");
        static readonly Regex rgxSizeZFile = RegexHelper.GenerateNumber(@"SZZ");
        static readonly Regex rgxSizeYFile = RegexHelper.GenerateNumber(@"SZY");

        private static HeightmapReadData GetInfo(StreamReader s, string rootDir) {
            string ms = s.ReadToEnd();
            Match[] mPrim = new Match[] {
                rgxDataFile.Match(ms),
                rgxHModelPFile.Match(ms),
                rgxHTexPFile.Match(ms),
                rgxSizeXFile.Match(ms),
                rgxSizeZFile.Match(ms),
                rgxSizeYFile.Match(ms)
            };
            Match[] mSec = new Match[] {
                rgxHModelSFile.Match(ms),
                rgxHTexSFile.Match(ms)
            };

            // Check For All Necessary Data
            foreach(var m in mPrim) {
                if(!m.Success) throw new ArgumentException("Primary Data Was Not Found");
            }
            HeightmapReadData rd = new HeightmapReadData();
            rd.HMDataBmp = Path.Combine(rootDir, mPrim[0].Groups[1].Value);
            rd.HMModelPrimary = Path.Combine(rootDir, mPrim[1].Groups[1].Value);
            rd.HMTexPrimary = Path.Combine(rootDir, mPrim[2].Groups[1].Value);
            if(mSec[0].Success) rd.HMModelSecondary = Path.Combine(rootDir, mSec[0].Groups[1].Value);
            if(mSec[1].Success) rd.HMTexSecondary = Path.Combine(rootDir, mSec[1].Groups[1].Value);
            float v;
            if(!float.TryParse(mPrim[3].Groups[1].Value, out v)) v = 1;
            rd.SizeX = v;
            if(!float.TryParse(mPrim[4].Groups[1].Value, out v)) v = 1;
            rd.SizeZ = v;
            if(!float.TryParse(mPrim[5].Groups[1].Value, out v)) v = 1;
            rd.SizeY = v;
            return rd;
        }
        private static void ConvertPixel(BColor c, float[] h, byte[] d, int i) {
            h[i] = 1f - (c.R / 255f);
            d[i] = c.G > 128 ? (byte)0x01u : (byte)0x00u;
        }
        private static HeightmapResult ParseFromInfo(GraphicsDevice g, Stream s, string rootDir) {
            HeightmapResult res = new HeightmapResult();

            // Read All Data First
            HeightmapReadData rd = GetInfo(new StreamReader(s), rootDir);

            // Must Read Primary Model
            using(var fs = File.OpenRead(rd.HMModelPrimary)) {
                // Try To Read Secondary Data
                Stream fss = null;
                if(rd.HasSecondary) {
                    fss = File.OpenRead(rd.HMModelSecondary);
                }
                res.View = new HeightmapModel(g, new Vector3(rd.SizeX, rd.SizeY, rd.SizeZ), fs, fss);
                if(fss != null)
                    fss.Dispose();
            }

            // Read Primary Texture
            using(var fs = File.OpenRead(rd.HMTexPrimary)) {
                res.View.PrimaryTexture = Texture2D.FromStream(g, fs);
            }

            // Try To Read Secondary Texture
            if(rd.HasSecondary) {
                using(var fs = File.OpenRead(rd.HMTexSecondary)) {
                    res.View.SecondaryTexture = Texture2D.FromStream(g, fs);
                }
            }

            // Read Height Data
            using(var bmp = Bitmap.FromFile(rd.HMDataBmp) as Bitmap) {
                int w = bmp.Width;
                int h = bmp.Height;
                float[] hd = new float[w * h];
                byte[] cd = new byte[w * h];
                int i = 0;
                for(int y = 0; y < h; y++) {
                    for(int x = 0; x < w; x++) {
                        ConvertPixel(bmp.GetPixel(x, y), hd, cd, i++);
                    }
                }

                res.Data = new Heightmap(hd, cd, w, h);
            }

            // Apply Heightmap Size
            res.Data.Width = rd.SizeX;
            res.Data.Depth = rd.SizeZ;
            res.Data.ScaleHeights(rd.SizeY);

            return res;
        }
        public static HeightmapResult Parse(GraphicsDevice g, DirectoryInfo dir) {
            // Find The Information File
            var files = dir.GetFiles();
            FileInfo infoFile = files.FirstOrDefault((f) => {
                return f.Extension.ToLower().EndsWith(INFO_FILE_EXT);
            });
            if(infoFile == null)
                throw new ArgumentException("Map Information File Could Not Be Found In The Directory");

            // Parse Data
            HeightmapResult res;
            using(Stream s = File.OpenRead(infoFile.FullName)) {
                res = ParseFromInfo(g, s, dir.FullName);
            }
            return res;
        }
    }
}