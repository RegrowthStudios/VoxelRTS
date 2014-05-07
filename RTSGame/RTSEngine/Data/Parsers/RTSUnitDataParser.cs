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
    public class RTSUnitViewData {
        [ZXParse]
        private static Texture2D AnimationFromBitmap(RTSRenderer renderer, string fi) {
            Texture2D t;
            float[] sData = null;
            int w, h;
            using(var bmp = System.Drawing.Bitmap.FromFile(fi) as System.Drawing.Bitmap) {
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

        [ZXParse]
        public RTSUnitModel View;

        [ZXParse("ViewModel")]
        public void Build(RTSRenderer renderer, string rootPath, string model, string[] tex) {
            using(var sModel = File.OpenRead(Path.Combine(rootPath, model))) {
                Texture2D tAnim = AnimationFromBitmap(renderer, Path.Combine(rootPath, tex[0]));
                View = new RTSUnitModel(renderer, sModel, tAnim);
            }
            View.ModelTexture = renderer.LoadTexture2D(Path.Combine(rootPath, tex[1]));
            View.ColorCodeTexture = renderer.LoadTexture2D(Path.Combine(rootPath, tex[2]));
        }
        [ZXParse("ViewIcon")]
        public void BuildIcon(RTSRenderer renderer, RTSRace race, string name, string rootPath, string icon) {
            string key = string.Join(".", race.FriendlyName, name);
            if(!renderer.IconLibrary.ContainsKey(key))
                renderer.IconLibrary.Add(key, renderer.LoadTexture2D(Path.Combine(rootPath, icon)));
        }
    }

    public static class RTSUnitDataParser {
        // Data Detection
        public const string EXTENSION = "unit";

        public static RTSUnitModel ParseModel(RTSRenderer renderer, FileInfo infoFile, RTSRace race) {
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return null;

            ZXParser.SetEnvironment("FILEROOTDIR", infoFile.Directory.FullName);
            ZXParser.SetEnvironment("RENDERER", renderer);
            ZXParser.SetEnvironment("RACE", race);
            RTSUnitViewData vd = ZXParser.ParseFile(infoFile.FullName, typeof(RTSUnitViewData)) as RTSUnitViewData;
            return vd.View;
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