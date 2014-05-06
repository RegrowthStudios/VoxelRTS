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

        public void Build(RTSRenderer renderer, string rootPath, string model, string[] tex) {
            using(var sModel = File.OpenRead(Path.Combine(rootPath, model))) {
                View = new RTSBuildingModel(renderer, sModel);
            }
            View.ModelTexture = renderer.LoadTexture2D(Path.Combine(rootPath, tex[1]));
            View.ColorCodeTexture = renderer.LoadTexture2D(Path.Combine(rootPath, tex[2]));
        }
        public void BuildIcon(RTSRenderer renderer, RTSRace race, string name, string rootPath, string icon) {
            string key = string.Join(".", race.FriendlyName, name);
            if(!renderer.IconLibrary.ContainsKey(key))
                renderer.IconLibrary.Add(key, renderer.LoadTexture2D(Path.Combine(rootPath, icon)));
        }
    }

    public static class RTSBuildingDataParser {
        // Data Detection
        public const string EXTENSION = "building";
        private static readonly Regex rgxName = RegexHelper.GenerateString("NAME");
        private static readonly Regex rgxHealth = RegexHelper.GenerateInteger("HEALTH");
        private static readonly Regex rgxCost = RegexHelper.GenerateInteger("COST");
        private static readonly Regex rgxMaxCount = RegexHelper.GenerateInteger("MAXCOUNT");
        private static readonly Regex rgxResource = RegexHelper.GenerateInteger("RESOURCE");
        private static readonly Regex rgxPPC = RegexHelper.GenerateInteger("POPCAPCHANGE");
        private static readonly Regex rgxImpact = RegexHelper.GenerateInteger("IMPACT");
        private static readonly Regex rgxBuildAmount = RegexHelper.GenerateInteger("BUILDAMOUNT");
        private static readonly Regex rgxDepositable = RegexHelper.GenerateInteger("DEPOSITABLE");
        private static readonly Regex rgxSightRadius = RegexHelper.GenerateInteger("SIGHTRADIUS");
        private static readonly Regex rgxGridSize = RegexHelper.GenerateVec2Int("GRIDSIZE");
        private static readonly Regex rgxBBMin = RegexHelper.GenerateVec3("BBOXMIN");
        private static readonly Regex rgxBBMax = RegexHelper.GenerateVec3("BBOXMAX");
        private static readonly Regex rgxCRect = RegexHelper.GenerateVec4("CRECT");
        private static readonly Regex rgxCtrlAction = RegexHelper.Generate("CTRLACTION", @"[\w\s\.]+");
        private static readonly Regex rgxCtrlButton = RegexHelper.Generate("CTRLBUTTON", @"[\w\s\.]+");
        private static readonly Regex rgxCtrlButtonIcon = RegexHelper.GenerateFile("CTRLBUTTONICON");

        private static readonly Regex rgxModel = RegexHelper.GenerateFile("MODEL");
        private static readonly Regex rgxMainTex = RegexHelper.GenerateFile("TEXMAIN");
        private static readonly Regex rgxColorTex = RegexHelper.GenerateFile("TEXCOLOR");
        private static readonly Regex rgxIcon = RegexHelper.GenerateFile("ICON");

        public static RTSBuildingModel ParseModel(RTSRenderer renderer, FileInfo infoFile, RTSRace race) {
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return null;

            ZXParser.SetEnvironment("FILEROOTDIR", infoFile.Directory.FullName);
            ZXParser.SetEnvironment("RENDERER", renderer);
            ZXParser.SetEnvironment("RACE", race);
            RTSBuildingViewData vd = ZXParser.ParseFile(infoFile.FullName, typeof(RTSBuildingViewData)) as RTSBuildingViewData;
            return vd.View;
            
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return null;

            // Read The Entire File
            string mStr;
            using(FileStream fs = File.OpenRead(infoFile.FullName)) {
                StreamReader s = new StreamReader(fs);
                mStr = s.ReadToEnd();
            }

            // Match Tokens
            Match[] mp = {
                rgxModel.Match(mStr),
                rgxMainTex.Match(mStr),
                rgxColorTex.Match(mStr),
                rgxIcon.Match(mStr),
                rgxName.Match(mStr)
            };

            // Check Existence
            foreach(var m in mp) if(!m.Success) return null;
            FileInfo fiModel = RegexHelper.ExtractFile(mp[0], infoFile.Directory.FullName);
            FileInfo fiTexMain = RegexHelper.ExtractFile(mp[1], infoFile.Directory.FullName);
            FileInfo fiTexKey = RegexHelper.ExtractFile(mp[2], infoFile.Directory.FullName);
            FileInfo fiIcon = RegexHelper.ExtractFile(mp[3], infoFile.Directory.FullName);
            if(!fiModel.Exists || !fiTexMain.Exists || !fiTexKey.Exists || !fiIcon.Exists)
                return null;

            RTSBuildingModel model;
            using(var sModel = File.OpenRead(fiModel.FullName)) {
                model = new RTSBuildingModel(renderer, sModel);
            }
            model.ModelTexture = renderer.LoadTexture2D(fiTexMain.FullName);
            model.ColorCodeTexture = renderer.LoadTexture2D(fiTexKey.FullName);

            if(race != null) {
                string key = string.Join(".", race.FriendlyName, RegexHelper.Extract(mp[4]));
                if(!renderer.IconLibrary.ContainsKey(key))
                    renderer.IconLibrary.Add(key, renderer.LoadTexture2D(fiIcon.FullName));

                var mButton = rgxCtrlButton.Match(mStr);
                var mButtonIcon = rgxCtrlButtonIcon.Match(mStr);
                while(mButton.Success && mButtonIcon.Success) {
                    key = string.Join(".", race.FriendlyName, RegexHelper.Extract(mButton));
                    if(!renderer.IconLibrary.ContainsKey(key))
                        renderer.IconLibrary.Add(key, renderer.LoadTexture2D(RegexHelper.ExtractFile(mButtonIcon, infoFile.Directory.FullName).FullName));

                    mButton = mButton.NextMatch();
                    mButtonIcon = mButtonIcon.NextMatch();
                }
            }


            return model;
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