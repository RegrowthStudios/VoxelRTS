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
    public struct HeightMapResult {
        public Heightmap HeightData;
        public HeightmapModel Model;
    }

    public static class HeightmapParser {
        const string INFO_FILE_NAME = "info.dat";

        // Keys To Search For
        const string KEY_DATA = "dat";
        const string KEY_MODEL_P = "hmp";
        const string KEY_TEX_P = "htp";
        const string KEY_MODEL_S = "hms";
        const string KEY_TEX_S = "hts";
        const string KEY_SCALE_X = "scx";
        const string KEY_SCALE_Z = "scz";
        const string KEY_SCALE_Y = "scy";
        const string KEY_END = "end";

        class ReadData {
            public const int DATA_COUNT = 8;

            // Private Data
            private string dat, hmp, htp, hms, hts;
            private Vector3 scale;

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
            public float ScaleX {
                get { return scale.X; }
                set {
                    scale.X = value;
                    isSet[5] = true;
                }
            }
            public float ScaleZ {
                get { return scale.Z; }
                set {
                    scale.Z = value;
                    isSet[6] = true;
                }
            }
            public float ScaleY {
                get { return scale.Y; }
                set {
                    scale.Y = value;
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

            public ReadData() {
                isSet = new bool[DATA_COUNT];
                dat = null;
                hmp = null;
                htp = null;
                hms = null;
                hts = null;
                scale = Vector3.One;
            }
        }

        // How To Detect Data
        const string DATA_REGEX = @"(\w{3})\s+(\w[\w|\s|.|\\|/]*)";
        static readonly Regex dataSplitRegex = new Regex(DATA_REGEX, RegexOptions.None);

        private static ReadData GetInfo(StreamReader s, string rootDir) {
            ReadData rd = new ReadData();
            string line;
            while(!s.EndOfStream && !rd.IsAllRead) {
                // Read The Next Line And Get Rid Of Surrounding Whitespace
                line = s.ReadLine().Trim();

                // Check To See If We Should Terminate Parsing
                if(line.ToLower().Equals(KEY_END))
                    break;

                // Get A Proper Value
                Match m = dataSplitRegex.Match(line);
                if(m.Success) {
                    // Get The File Value If Provided
                    string file = m.Groups[2].Value.Trim();
                    switch(file.ToLower()) {
                        case "null":
                        case "none":
                            file = null;
                            break;
                    }
                    float v;

                    // Set Read Data By Key
                    string key = m.Groups[1].Value;
                    switch(key.ToLower()) {
                        case KEY_DATA: rd.HMDataBmp = Path.Combine(rootDir, file); break;
                        case KEY_MODEL_P: rd.HMModelPrimary = Path.Combine(rootDir, file); break;
                        case KEY_MODEL_S: rd.HMModelSecondary = Path.Combine(rootDir, file); break;
                        case KEY_TEX_P: rd.HMTexPrimary = Path.Combine(rootDir, file); break;
                        case KEY_TEX_S: rd.HMTexSecondary = Path.Combine(rootDir, file); break;
                        case KEY_SCALE_X:
                            if(!float.TryParse(file, out v)) v = 1;
                            rd.ScaleX = v;
                            break;
                        case KEY_SCALE_Y:
                            if(!float.TryParse(file, out v)) v = 1;
                            rd.ScaleY = v;
                            break;
                        case KEY_SCALE_Z:
                            if(!float.TryParse(file, out v)) v = 1;
                            rd.ScaleZ = v;
                            break;
                        default: break;
                    }
                }
            }
            return rd;
        }
        private static void ConvertPixel(BColor c, float[] h, byte[] d, int i) {
            h[i] = 1f - (c.R / 255f);
            d[i] = c.G > 128 ? (byte)0x01u : (byte)0x00u;
        }
        private static HeightMapResult ParseFromInfo(GraphicsDevice g, Stream s, string rootDir) {
            HeightMapResult res = new HeightMapResult();

            // Read All Data First
            ReadData rd = GetInfo(new StreamReader(s), rootDir);

            // Must Read Primary Model
            using(var fs = File.OpenRead(rd.HMModelPrimary)) {
                // Try To Read Secondary Data
                Stream fss = null;
                if(rd.HasSecondary) {
                    fss = File.OpenRead(rd.HMModelSecondary);
                }
                res.Model = new HeightmapModel(g, fs, fss);
                if(fss != null)
                    fss.Dispose();
            }

            // Read Primary Texture
            using(var fs = File.OpenRead(rd.HMTexPrimary)) {
                res.Model.PrimaryTexture = Texture2D.FromStream(g, fs);
            }

            // Try To Read Secondary Texture
            if(rd.HasSecondary) {
                using(var fs = File.OpenRead(rd.HMTexSecondary)) {
                    res.Model.SecondaryTexture = Texture2D.FromStream(g, fs);
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
                res.HeightData = new Heightmap(hd, cd, w, h);
            }

            // Apply Scaling
            res.HeightData.ScaleX = rd.ScaleX;
            res.HeightData.ScaleZ = rd.ScaleZ;
            res.HeightData.ScaleHeights(rd.ScaleY);

            return res;
        }
        public static HeightMapResult Parse(GraphicsDevice g, DirectoryInfo dir) {
            var files = dir.GetFiles();
            foreach(var file in files) {
                // Check For The Info File
                if(file.Name.ToLower().Equals(INFO_FILE_NAME)) {
                    Stream s = File.OpenRead(file.FullName);
                    HeightMapResult res = ParseFromInfo(g, s, dir.FullName);
                    s.Dispose();
                    return res;
                }
            }
            throw new ArgumentException("\"info.dat\" Could Be Found In The Directory");
        }

    }
}