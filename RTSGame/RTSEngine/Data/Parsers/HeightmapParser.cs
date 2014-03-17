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
using RTSEngine.Interfaces;
using RTSEngine.Graphics;
using System.Collections.Concurrent;

namespace RTSEngine.Data.Parsers {
    public class HeightmapResult {
        public Heightmap Data;
        public HeightmapModel View;
    }

    public class HeightMapLoader : IWorkLoader {
        // Data Detection
        const string INFO_FILE_EXT = "map";
        static readonly Regex rgxDataFile = RegexHelper.GenerateFile("DAT");
        static readonly Regex rgxHModelPFile = RegexHelper.GenerateFile("HMP");
        static readonly Regex rgxHTexPFile = RegexHelper.GenerateFile("HTP");
        static readonly Regex rgxHModelSFile = RegexHelper.GenerateFile("HMS");
        static readonly Regex rgxHTexSFile = RegexHelper.GenerateFile("HTS");
        static readonly Regex rgxSize = RegexHelper.GenerateVec3("SIZE");
        private static void ConvertPixel(BColor c, float[] h, byte[] d, int i) {
            h[i] = 1f - (c.R / 255f);
            d[i] = c.G > 128 ? (byte)0x01u : (byte)0x00u;
        }

        // Load Arguments
        private GraphicsDevice g;
        private DirectoryInfo dir;
        private HeightmapResult res;

        // Work Data
        public int WorkAmount {
            get;
            private set;
        }
        public int WorkComplete {
            get;
            private set;
        }
        public bool IsFinishedWorking {
            get;
            private set;
        }
        public bool IsLoadSuccessful {
            get;
            private set;
        }
        public ConcurrentQueue<string> Messages {
            get;
            private set;
        }

        public HeightMapLoader(GraphicsDevice _g, DirectoryInfo _dir, HeightmapResult _res) {
            g = _g;
            dir = _dir;
            res = _res;
            WorkAmount = 1;
            WorkComplete = 0;
            IsFinishedWorking = false;
            IsLoadSuccessful = false;
            Messages = new ConcurrentQueue<string>();
        }

        public void Load() {
            Messages.Enqueue("Loading Heightmap");

            // Find The Information File
            if(!dir.Exists) {
                Messages.Enqueue("Directory " + dir + " Does Not Exist");
                IsFinishedWorking = true;
                return;
            }

            Messages.Enqueue("Trying To Find Info File");
            var files = dir.GetFiles();
            FileInfo infoFile = files.FirstOrDefault((f) => {
                return f.Extension.ToLower().EndsWith(INFO_FILE_EXT);
            });
            if(infoFile == null) {
                Messages.Enqueue("No Info File Present");
                IsFinishedWorking = true;
                return;
            }
            Messages.Enqueue("Found: " + infoFile.FullName);
            WorkComplete++;

            // Parse Data
            string rootDir = dir.FullName;
            using(Stream s = File.OpenRead(infoFile.FullName)) {
                StreamReader sr = new StreamReader(s);
                string ms = sr.ReadToEnd();

                // Read All Data First
                WorkAmount += 6;
                Match[] matches = {
                    rgxSize.Match(ms),
                    rgxDataFile.Match(ms),
                    rgxHModelPFile.Match(ms),
                    rgxHTexPFile.Match(ms),
                    rgxHModelSFile.Match(ms),
                    rgxHTexSFile.Match(ms)
                };
                for(int i = 0; i < 4; i++) {
                    WorkComplete++;
                    if(!matches[i].Success) {
                        Messages.Enqueue("Could Not Find Primary Data");
                        IsFinishedWorking = true;
                    }
                }

                Vector3 sz = RegexHelper.ExtractVec3(matches[0]);
                FileInfo hfi = RegexHelper.ExtractFile(matches[1], rootDir);
                FileInfo mpfi = RegexHelper.ExtractFile(matches[2], rootDir);
                FileInfo tpfi = RegexHelper.ExtractFile(matches[3], rootDir);
                if(!hfi.Exists || !mpfi.Exists || !tpfi.Exists ||
                    !hfi.Extension.EndsWith("png") ||
                    !mpfi.Extension.EndsWith("obj") ||
                    !tpfi.Extension.EndsWith("png")
                    ) {
                    Messages.Enqueue("Primary Data Files Not In Correct Format");
                }


                // Check For Secondary Info
                FileInfo msfi = null, tsfi = null;
                if(matches[4].Success && matches[5].Success) {
                    msfi = RegexHelper.ExtractFile(matches[4], rootDir);
                    tsfi = RegexHelper.ExtractFile(matches[5], rootDir);
                    if(msfi.Exists && tsfi.Exists)
                        Messages.Enqueue("Detail Model Found");
                    else {
                        msfi = tsfi = null;
                        Messages.Enqueue("No Detail Model Found");
                    }
                }
                else {
                    Messages.Enqueue("No Detail Model Found");
                }
                WorkComplete += 2;

                // Read Height Data
                Messages.Enqueue("Reading Height Data");
                using(var bmp = Bitmap.FromFile(hfi.FullName) as Bitmap) {
                    int w = bmp.Width;
                    int h = bmp.Height;
                    float[] hd = new float[w * h];
                    byte[] cd = new byte[w * h];
                    int i = 0;

                    // Convert Bitmap
                    WorkAmount += w * h;
                    for(int y = 0; y < h; y++) {
                        for(int x = 0; x < w; x++) {
                            ConvertPixel(bmp.GetPixel(x, y), hd, cd, i++);
                            WorkComplete++;
                        }
                    }
                    res.Data = new Heightmap(hd, cd, w, h);

                    // Apply Heightmap Size
                    res.Data.Width = sz.X;
                    res.Data.Depth = sz.Z;
                    res.Data.ScaleHeights(sz.Y);
                }

                // Must Read Primary Model
                using(var fs = File.OpenRead(mpfi.FullName)) {
                    // Try To Read Secondary Data
                    Stream fss = null;
                    if(msfi != null) {
                        fss = File.OpenRead(msfi.FullName);
                    }
                    res.View = new HeightmapModel(g, sz, res.Data.BuildBVH, fs, fss);
                    if(fss != null)
                        fss.Dispose();
                }

                // Read Primary Texture
                using(var fs = File.OpenRead(tpfi.FullName)) {
                    res.View.PrimaryTexture = Texture2D.FromStream(g, fs);
                }

                // Try To Read Secondary Texture
                if(tsfi != null) {
                    using(var fs = File.OpenRead(tsfi.FullName)) {
                        res.View.SecondaryTexture = Texture2D.FromStream(g, fs);
                    }
                }
            }
        }
    }

    public static class HeightmapParser {
        // Data Detection
        const string INFO_FILE_EXT = "map";
        static readonly Regex rgxDataFile = RegexHelper.GenerateFile("DAT");
        static readonly Regex rgxHModelPFile = RegexHelper.GenerateFile("HMP");
        static readonly Regex rgxHTexPFile = RegexHelper.GenerateFile("HTP");
        static readonly Regex rgxHModelSFile = RegexHelper.GenerateFile("HMS");
        static readonly Regex rgxHTexSFile = RegexHelper.GenerateFile("HTS");
        static readonly Regex rgxSize = RegexHelper.GenerateVec3("SIZE");

        private static void ConvertPixel(BColor c, float[] h, byte[] d, int i) {
            h[i] = 1f - (c.R / 255f);
            d[i] = c.G > 128 ? (byte)0x01u : (byte)0x00u;
        }
        private static HeightmapResult ParseFromInfo(GraphicsDevice g, Stream s, string rootDir) {
            HeightmapResult res = new HeightmapResult();
            StreamReader sr = new StreamReader(s);
            string ms = sr.ReadToEnd();

            // Read All Data First
            Vector3 sz = RegexHelper.ExtractVec3(rgxSize.Match(ms));
            FileInfo hfi = RegexHelper.ExtractFile(rgxDataFile.Match(ms), rootDir);
            FileInfo mpfi = RegexHelper.ExtractFile(rgxHModelPFile.Match(ms), rootDir);
            FileInfo tpfi = RegexHelper.ExtractFile(rgxHTexPFile.Match(ms), rootDir);

            FileInfo msfi = null, tsfi = null;
            Match s1 = rgxHModelSFile.Match(ms);
            Match s2 = rgxHTexSFile.Match(ms);
            if(s1.Success && s2.Success) {
                msfi = RegexHelper.ExtractFile(s1, rootDir);
                tsfi = RegexHelper.ExtractFile(s2, rootDir);
            }

            // Read Height Data
            using(var bmp = Bitmap.FromFile(hfi.FullName) as Bitmap) {
                int w = bmp.Width;
                int h = bmp.Height;
                float[] hd = new float[w * h];
                byte[] cd = new byte[w * h];
                int i = 0;

                // Convert Bitmap
                for(int y = 0; y < h; y++) {
                    for(int x = 0; x < w; x++) {
                        ConvertPixel(bmp.GetPixel(x, y), hd, cd, i++);
                    }
                }
                res.Data = new Heightmap(hd, cd, w, h);

                // Apply Heightmap Size
                res.Data.Width = sz.X;
                res.Data.Depth = sz.Z;
                res.Data.ScaleHeights(sz.Y);
            }

            // Must Read Primary Model
            using(var fs = File.OpenRead(mpfi.FullName)) {
                // Try To Read Secondary Data
                Stream fss = null;
                if(msfi != null) {
                    fss = File.OpenRead(msfi.FullName);
                }
                res.View = new HeightmapModel(g, sz, res.Data.BuildBVH, fs, fss);
                if(fss != null)
                    fss.Dispose();
            }

            // Read Primary Texture
            using(var fs = File.OpenRead(tpfi.FullName)) {
                res.View.PrimaryTexture = Texture2D.FromStream(g, fs);
            }

            // Try To Read Secondary Texture
            if(tsfi != null) {
                using(var fs = File.OpenRead(tsfi.FullName)) {
                    res.View.SecondaryTexture = Texture2D.FromStream(g, fs);
                }
            }
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