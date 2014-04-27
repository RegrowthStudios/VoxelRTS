using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using RTSEngine.Data.Team;
using RTSEngine.Graphics;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Controllers;

namespace RTSEngine.Data.Parsers {
    public static class RTSUnitDataParser {
        // Data Detection
        public const string EXTENSION = "unit";
        private static readonly Regex rgxName = RegexHelper.Generate("NAME", @"[\w\s]+");
        private static readonly Regex rgxModel = RegexHelper.GenerateFile("MODEL");
        private static readonly Regex rgxMainTex = RegexHelper.GenerateFile("TEXMAIN");
        private static readonly Regex rgxColorTex = RegexHelper.GenerateFile("TEXCOLOR");
        private static readonly Regex rgxAnimation = RegexHelper.GenerateFile("ANIMATION");
        private static readonly Regex rgxIcon = RegexHelper.GenerateFile("ICON");
        private static readonly Regex rgxMaxCount = RegexHelper.GenerateInteger("MAXCOUNT");
        private static readonly Regex rgxRadius = RegexHelper.GenerateNumber("RADIUS");
        private static readonly Regex rgxBBMin = RegexHelper.GenerateVec3("BBOXMIN");
        private static readonly Regex rgxBBMax = RegexHelper.GenerateVec3("BBOXMAX");
        private static readonly Regex rgxCtrlMove = RegexHelper.Generate("CTRLMOVE", @"[\w\s\.]+");
        private static readonly Regex rgxCtrlAction = RegexHelper.Generate("CTRLACTION", @"[\w\s\.]+");
        private static readonly Regex rgxCtrlAnimation = RegexHelper.Generate("CTRLANIM", @"[\w\s\.]+");
        private static readonly Regex rgxCtrlCombat = RegexHelper.Generate("CTRLCOMBAT", @"[\w\s\.]+");

        private static Texture2D AnimationFromBitmap(RTSRenderer renderer, FileInfo fi) {
            Texture2D t;
            float[] sData = null;
            int w, h;
            using(var bmp = System.Drawing.Bitmap.FromFile(fi.FullName) as System.Drawing.Bitmap) {
                w = bmp.Width;
                h = bmp.Height;
                sData = new float[w * h];
                System.Drawing.Imaging.BitmapData data = bmp.LockBits(new System.Drawing.Rectangle(0, 0, w, h), System.Drawing.Imaging.ImageLockMode.ReadOnly, bmp.PixelFormat);
                System.Runtime.InteropServices.Marshal.Copy(data.Scan0, sData, 0, (data.Stride * data.Height) >> 2);
                bmp.UnlockBits(data);
            }
            t = renderer.CreateTexture2D(w, h, SurfaceFormat.Single);
            t.SetData(sData);
            return t;
        }
        public static RTSUnitModel ParseModel(RTSRenderer renderer, FileInfo infoFile, RTSRace race) {
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return null;

            // Read The Entire File
            string mStr;
            using(FileStream fs = File.OpenRead(infoFile.FullName)) {
                StreamReader s = new StreamReader(fs);
                mStr = s.ReadToEnd();
            }

            // Set Environment Variables
            ZXParser.SetEnvironment("FILEROOTDIR", infoFile.Directory.FullName);


            // Match Tokens
            Match[] mp = {
                rgxModel.Match(mStr),
                rgxAnimation.Match(mStr),
                rgxMainTex.Match(mStr),
                rgxColorTex.Match(mStr),
                rgxIcon.Match(mStr),
                rgxName.Match(mStr)
            };

            // Check Existence
            foreach(var m in mp) if(!m.Success) return null;
            FileInfo fiModel = RegexHelper.ExtractFile(mp[0], infoFile.Directory.FullName);
            FileInfo fiAnim = RegexHelper.ExtractFile(mp[1], infoFile.Directory.FullName);
            FileInfo fiTexMain = RegexHelper.ExtractFile(mp[2], infoFile.Directory.FullName);
            FileInfo fiTexKey = RegexHelper.ExtractFile(mp[3], infoFile.Directory.FullName);
            FileInfo fiIcon = RegexHelper.ExtractFile(mp[4], infoFile.Directory.FullName);
            if(!fiModel.Exists || !fiAnim.Exists || !fiTexMain.Exists || !fiTexKey.Exists || !fiIcon.Exists)
                return null;

            RTSUnitModel model;
            using(var sModel = File.OpenRead(fiModel.FullName)) {
                Texture2D tAnim = AnimationFromBitmap(renderer, fiAnim);
                model = new RTSUnitModel(renderer, sModel, tAnim);
            }
            model.ModelTexture = renderer.LoadTexture2D(fiTexMain.FullName);
            model.ColorCodeTexture = renderer.LoadTexture2D(fiTexKey.FullName);

            if(race != null) {
                string key = string.Join(".", race.FriendlyName, RegexHelper.Extract(mp[5]));
                if(!renderer.IconLibrary.ContainsKey(key))
                    renderer.IconLibrary.Add(key, renderer.LoadTexture2D(fiIcon.FullName));
            }

            return model;
        }
        public static RTSUnitData ParseData(Dictionary<string, ReflectedScript> controllers, FileInfo infoFile, int index) {
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return null;

            // Read The Entire File
            string mStr;
            using(FileStream fs = File.OpenRead(infoFile.FullName)) {
                StreamReader s = new StreamReader(fs);
                mStr = s.ReadToEnd();
            }

            // Set Environment Variables
            ZXParser.SetEnvironment("FILEROOTDIR", infoFile.Directory.FullName);
            ZXParser.SetEnvironment("DICTSCRIPTS", controllers);

            // Read Data
            RTSUnitData data = new RTSUnitData(index);
            ZXParser.ParseInto(mStr, data);
            data.InfoFile = PathHelper.GetRelativePath(infoFile.FullName);
            return data;
        }
    }
}