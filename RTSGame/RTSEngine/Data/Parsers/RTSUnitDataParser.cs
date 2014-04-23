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
        private static readonly Regex rgxHealth = RegexHelper.GenerateInteger("HEALTH");
        private static readonly Regex rgxSpeed = RegexHelper.GenerateNumber("SPEED");
        private static readonly Regex rgxCost = RegexHelper.GenerateVec2Int("COST");
        private static readonly Regex rgxBuildTime = RegexHelper.GenerateInteger("BUILDTIME");
        private static readonly Regex rgxCarryCapacity = RegexHelper.GenerateInteger("CARRYCAPACITY");
        private static readonly Regex rgxWorker = RegexHelper.GenerateInteger("WORKER");
        private static readonly Regex rgxImpact = RegexHelper.GenerateInteger("IMPACT");
        private static readonly Regex rgxRadius = RegexHelper.GenerateNumber("RADIUS");
        private static readonly Regex rgxBBMin = RegexHelper.GenerateVec3("BBOXMIN");
        private static readonly Regex rgxBBMax = RegexHelper.GenerateVec3("BBOXMAX");
        private static readonly Regex rgxArmor = RegexHelper.GenerateInteger("ARMOR");
        private static readonly Regex rgxDamage = RegexHelper.GenerateVec2Int("DAMAGE");
        private static readonly Regex rgxRange = RegexHelper.GenerateVec2Int("RANGE");
        private static readonly Regex rgxTimer = RegexHelper.GenerateNumber("TIMER");
        private static readonly Regex rgxCritChance = RegexHelper.GenerateNumber("CRITCHANCE");
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

            // Match Tokens
            Match[] mp = {
                rgxName.Match(mStr),
                rgxHealth.Match(mStr),
                rgxImpact.Match(mStr),
                rgxBuildTime.Match(mStr),
                rgxCost.Match(mStr),
                rgxMaxCount.Match(mStr),
                rgxSpeed.Match(mStr),
                rgxCarryCapacity.Match(mStr),
                rgxWorker.Match(mStr),
                rgxRadius.Match(mStr),
                rgxBBMin.Match(mStr),
                rgxBBMax.Match(mStr),
                rgxArmor.Match(mStr),
                rgxDamage.Match(mStr),
                rgxRange.Match(mStr),
                rgxCritChance.Match(mStr),
                rgxTimer.Match(mStr),
                rgxCtrlAction.Match(mStr),
                rgxCtrlAnimation.Match(mStr),
                rgxCtrlCombat.Match(mStr),
                rgxCtrlMove.Match(mStr)
            };
            foreach(var m in mp) if(!m.Success) return null;

            // Read Data
            int[] buf;
            int ri = 0;
            RTSUnitData data = new RTSUnitData(index);
            data.InfoFile = PathHelper.GetRelativePath(infoFile.FullName);
            data.FriendlyName = RegexHelper.Extract(mp[ri++]);
            data.Health = RegexHelper.ExtractInt(mp[ri++]);
            data.Impact = RegexHelper.ExtractInt(mp[ri++]);
            data.BuildTime = RegexHelper.ExtractInt(mp[ri++]);
            buf = RegexHelper.ExtractVec2I(mp[ri++]);
            data.CapitalCost = buf[0];
            data.PopulationCost = buf[1];
            data.MaxCount = RegexHelper.ExtractInt(mp[ri++]);
            data.MovementSpeed = RegexHelper.ExtractFloat(mp[ri++]);
            data.CarryingCapacity = RegexHelper.ExtractInt(mp[ri++]);
            data.IsWorker = RegexHelper.ExtractInt(mp[ri++]) == 0 ? false : true;

            // Collision Information
            data.ICollidableShape = new CollisionCircle(
                RegexHelper.ExtractFloat(mp[ri++]),
                Vector2.Zero, false
                );
            data.BBox.Min = RegexHelper.ExtractVec3(mp[ri++]);
            data.BBox.Max = RegexHelper.ExtractVec3(mp[ri++]);

            // Read Combat Data
            data.BaseCombatData.Armor = RegexHelper.ExtractInt(mp[ri++]);
            buf = RegexHelper.ExtractVec2I(mp[ri++]);
            data.BaseCombatData.AttackDamage = buf[0];
            data.BaseCombatData.CriticalDamage = buf[1];
            buf = RegexHelper.ExtractVec2I(mp[ri++]);
            data.BaseCombatData.MinRange = buf[0];
            data.BaseCombatData.MaxRange = buf[1];
            data.BaseCombatData.CriticalChance = RegexHelper.ExtractDouble(mp[ri++]);
            data.BaseCombatData.AttackTimer = RegexHelper.ExtractFloat(mp[ri++]);

            // Get The Controllers From The Controller Dictionary
            if(controllers != null) {
                data.DefaultActionController = controllers[RegexHelper.Extract(mp[ri++])];
                data.DefaultAnimationController = controllers[RegexHelper.Extract(mp[ri++])];
                data.DefaultCombatController = controllers[RegexHelper.Extract(mp[ri++])];
                data.DefaultMoveController = controllers[RegexHelper.Extract(mp[ri++])];
            }

            return data;
        }
    }
}