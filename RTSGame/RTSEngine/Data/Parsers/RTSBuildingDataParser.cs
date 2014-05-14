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
    public class RTSBuildingViewData {
        [ZXParse]
        public RTSBuildingModel View;

        [ZXParse("ViewModel")]
        public void Build(RTSRenderer renderer, string rootPath, string model, string[] tex) {
            using(var sModel = File.OpenRead(Path.Combine(rootPath, model))) {
                View = new RTSBuildingModel(renderer, sModel);
            }
            View.ModelTexture = renderer.LoadTexture2D(Path.Combine(rootPath, tex[0]));
            View.ColorCodeTexture = renderer.LoadTexture2D(Path.Combine(rootPath, tex[1]));
        }
        [ZXParse("ViewIcon")]
        public void BuildIcon(RTSRenderer renderer, RTSRace race, string name, string rootPath, string icon) {
            string key = string.Join(".", race.FriendlyName, name);
            if(!renderer.IconLibrary.ContainsKey(key))
                renderer.IconLibrary.Add(key, renderer.LoadTexture2D(Path.Combine(rootPath, icon)));
        }
        [ZXParse("ViewButton")]
        public void BuildButton(RTSRenderer renderer, RTSRace race, string name, int i, string rootPath, string icon) {
            string key = string.Join(".", race.FriendlyName, name, i);
            if(!renderer.IconLibrary.ContainsKey(key))
                renderer.IconLibrary.Add(key, renderer.LoadTexture2D(Path.Combine(rootPath, icon)));
        }
    }

    public static class RTSBuildingDataParser {
        // Data Detection
        public const string EXTENSION = "building";

        public static RTSBuildingModel ParseModel(RTSRenderer renderer, FileInfo infoFile, RTSRace race) {
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return null;

            ZXParser.SetEnvironment("FILEROOTDIR", infoFile.Directory.FullName);
            ZXParser.SetEnvironment("RENDERER", renderer);
            ZXParser.SetEnvironment("RACE", race);
            RTSBuildingViewData vd = ZXParser.ParseFile(infoFile.FullName, typeof(RTSBuildingViewData)) as RTSBuildingViewData;
            return vd.View;
        }
        public static RTSBuildingData ParseData(Dictionary<string, ReflectedScript> controllers, FileInfo infoFile, int index) {
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
            RTSBuildingData data = new RTSBuildingData(index);
            ZXParser.ParseInto(mStr, data);
            data.InfoFile = PathHelper.GetRelativePath(infoFile.FullName);
            return data;
        }
    }
}