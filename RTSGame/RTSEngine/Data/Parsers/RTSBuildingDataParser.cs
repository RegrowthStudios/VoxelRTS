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
    public static class RTSBuildingDataParser {
        // Data Detection
        public const string EXTENSION = "building";
        private static readonly Regex rgxName = RegexHelper.GenerateString("NAME");
        private static readonly Regex rgxHealth = RegexHelper.GenerateInteger("HEALTH");
        private static readonly Regex rgxCost = RegexHelper.GenerateInteger("COST");
        private static readonly Regex rgxMaxCount = RegexHelper.GenerateInteger("MAXCOUNT");
        private static readonly Regex rgxImpact = RegexHelper.GenerateInteger("IMPACT");
        private static readonly Regex rgxBuildTime = RegexHelper.GenerateInteger("BUILDTIME");
        private static readonly Regex rgxSightRadius = RegexHelper.GenerateInteger("SIGHTRADIUS");
        private static readonly Regex rgxGridSize = RegexHelper.GenerateVec2Int("GRIDSIZE");
        private static readonly Regex rgxBBMin = RegexHelper.GenerateVec3("BBOXMIN");
        private static readonly Regex rgxBBMax = RegexHelper.GenerateVec3("BBOXMAX");
        private static readonly Regex rgxCRect = RegexHelper.GenerateVec4("CRECT");
        private static readonly Regex rgxCtrlAction = RegexHelper.Generate("CTRLACTION", @"[\w\s\.]+");

        private static readonly Regex rgxModel = RegexHelper.GenerateFile("MODEL");
        private static readonly Regex rgxMainTex = RegexHelper.GenerateFile("TEXMAIN");
        private static readonly Regex rgxColorTex = RegexHelper.GenerateFile("TEXCOLOR");
        private static readonly Regex rgxIcon = RegexHelper.GenerateFile("ICON");

        public static RTSBuildingModel ParseModel(RTSRenderer renderer, FileInfo infoFile, RTSRace race) {
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
            }

            return model;
        }
        public static RTSBuildingData ParseData(Dictionary<string, ReflectedScript> controllers, FileInfo infoFile) {
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
                rgxName.Match(mStr),
                rgxHealth.Match(mStr),
                rgxCost.Match(mStr),
                rgxMaxCount.Match(mStr),
                rgxImpact.Match(mStr),
                rgxBuildTime.Match(mStr),
                rgxSightRadius.Match(mStr),
                rgxGridSize.Match(mStr),
                rgxBBMin.Match(mStr),
                rgxBBMax.Match(mStr),
                rgxCRect.Match(mStr),
                rgxCtrlAction.Match(mStr)
            };
            foreach(var m in mp) if(!m.Success) return null;

            // Read Data
            int[] buf;
            int ri = 0;
            RTSBuildingData data = new RTSBuildingData();
            data.InfoFile = PathHelper.GetRelativePath(infoFile.FullName);
            data.FriendlyName = RegexHelper.Extract(mp[ri++]);
            data.Health = RegexHelper.ExtractInt(mp[ri++]);
            data.CapitalCost = RegexHelper.ExtractInt(mp[ri++]);
            data.MaxCount = RegexHelper.ExtractInt(mp[ri++]);
            data.Impact = RegexHelper.ExtractInt(mp[ri++]);
            data.BuildTime = RegexHelper.ExtractInt(mp[ri++]);
            data.SightRadius = RegexHelper.ExtractInt(mp[ri++]);
            buf = RegexHelper.ExtractVec2I(mp[ri++]);
            data.GridSize.X = buf[0];
            data.GridSize.Y = buf[1];
            data.BBox.Min = RegexHelper.ExtractVec3(mp[ri++]);
            data.BBox.Max = RegexHelper.ExtractVec3(mp[ri++]);
            Vector4 cr = RegexHelper.ExtractVec4(mp[ri++]);
            //data.ICollidableShape = new CollisionCircle(cr.Length() * 0.5f, Vector2.Zero, true);
            data.ICollidableShape = new CollisionRect(cr.X, cr.Y, new Vector2(cr.Z, cr.W), true);

            // Get The Controllers From The Controller Dictionary
            if(controllers != null) {
                data.DefaultActionController = controllers[RegexHelper.Extract(mp[ri++])];
            }

            return data;
        }
    }
}