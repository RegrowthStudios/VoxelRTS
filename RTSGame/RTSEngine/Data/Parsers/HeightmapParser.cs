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
        const string INFO_FILE_EXT = "map";

        // Keys To Search For
        const string KEY_DATA = "dat";
        const string KEY_MODEL_P = "hmp";
        const string KEY_TEX_P = "htp";
        const string KEY_MODEL_S = "hms";
        const string KEY_TEX_S = "hts";
        const string KEY_SCALE_X = "szx";
        const string KEY_SCALE_Z = "szz";
        const string KEY_SCALE_Y = "szy";
        const string KEY_END = "end";

        // How To Detect Data
        const string DATA_REGEX = @"(\w{3})\s+(\w[\w|\s|.|\\|/]*)";
        static readonly Regex dataSplitRegex = new Regex(DATA_REGEX, RegexOptions.None);

        private static HeightmapReadData GetInfo(StreamReader s, string rootDir) {
            HeightmapReadData rd = new HeightmapReadData();
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
                            rd.SizeX = v;
                            break;
                        case KEY_SCALE_Y:
                            if(!float.TryParse(file, out v)) v = 1;
                            rd.SizeY = v;
                            break;
                        case KEY_SCALE_Z:
                            if(!float.TryParse(file, out v)) v = 1;
                            rd.SizeZ = v;
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
            HeightmapReadData rd = GetInfo(new StreamReader(s), rootDir);

            // Must Read Primary Model
            using(var fs = File.OpenRead(rd.HMModelPrimary)) {
                // Try To Read Secondary Data
                Stream fss = null;
                if(rd.HasSecondary) {
                    fss = File.OpenRead(rd.HMModelSecondary);
                }
                res.Model = new HeightmapModel(g, new Vector3(rd.SizeX, rd.SizeY, rd.SizeZ), fs, fss);
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

            // Apply Heightmap Size
            res.HeightData.Width = rd.SizeX;
            res.HeightData.Depth = rd.SizeZ;
            res.HeightData.ScaleHeights(rd.SizeY);

            return res;
        }
        public static HeightMapResult Parse(GraphicsDevice g, DirectoryInfo dir) {
            // Find The Information File
            var files = dir.GetFiles();
            FileInfo infoFile = files.FirstOrDefault((f) => {
                return f.Extension.ToLower().EndsWith(INFO_FILE_EXT);
            });
            if(infoFile == null)
                throw new ArgumentException("Map Information File Could Not Be Found In The Directory");

            // Parse Data
            HeightMapResult res;
            using(Stream s = File.OpenRead(infoFile.FullName)) {
                res = ParseFromInfo(g, s, dir.FullName);
            }
            return res;
        }
    }
}