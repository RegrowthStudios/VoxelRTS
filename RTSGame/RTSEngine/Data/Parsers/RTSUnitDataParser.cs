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
        private static readonly Regex rgxMaxCount = RegexHelper.GenerateFile("MAXCOUNT");
        private static readonly Regex rgxHealth = RegexHelper.GenerateInteger("HEALTH");
        private static readonly Regex rgxSpeed = RegexHelper.GenerateNumber("SPEED");
        private static readonly Regex rgxCost = RegexHelper.GenerateVec2Int("COST");
        private static readonly Regex rgxBuildTime = RegexHelper.GenerateInteger("BUILDTIME");
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

        private static Texture2D AnimationFromBitmap(GameEngine ge, FileInfo fi) {
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
            t = ge.CreateTexture2D(w, h, SurfaceFormat.Single);
            t.SetData(sData);
            return t;
        }
        public static RTSUnitModel ParseModel(GameEngine ge, RTSUnitData data, FileInfo infoFile) {
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
                rgxColorTex.Match(mStr)
            };

            // Check Existence
            foreach(var m in mp) if(!m.Success) return null;
            FileInfo fiModel = RegexHelper.ExtractFile(mp[0], infoFile.Directory.FullName);
            FileInfo fiAnim = RegexHelper.ExtractFile(mp[1], infoFile.Directory.FullName);
            FileInfo fiTexMain = RegexHelper.ExtractFile(mp[2], infoFile.Directory.FullName);
            FileInfo fiTexKey = RegexHelper.ExtractFile(mp[3], infoFile.Directory.FullName);
            if(!fiModel.Exists || !fiAnim.Exists || !fiTexMain.Exists || !fiTexKey.Exists)
                return null;

            RTSUnitModel model;
            using(var sModel = File.OpenRead(fiModel.FullName)) {
                Texture2D tAnim = AnimationFromBitmap(ge, fiAnim);
                model = new RTSUnitModel(ge, data, sModel, tAnim);
            }
            model.ModelTexture = ge.LoadTexture2D(fiTexMain.FullName);
            model.ColorCodeTexture = ge.LoadTexture2D(fiTexKey.FullName);
            return model;
        }
        public static RTSUnitData ParseData(Dictionary<string, ReflectedUnitController> controllers, FileInfo infoFile) {
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
                rgxBuildTime.Match(mStr),
                rgxCost.Match(mStr),
                rgxMaxCount.Match(mStr),
                rgxSpeed.Match(mStr),
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
            RTSUnitData data = new RTSUnitData();
            data.FriendlyName = RegexHelper.Extract(mp[ri++]);
            data.Health = RegexHelper.ExtractInt(mp[ri++]);
            data.BuildTime = RegexHelper.ExtractInt(mp[ri++]);
            buf = RegexHelper.ExtractVec2I(mp[ri++]);
            data.CapitalCost = buf[0];
            data.PopulationCost = buf[1];
            data.MaxCount = RegexHelper.ExtractInt(mp[ri++]);
            data.MovementSpeed = RegexHelper.ExtractFloat(mp[ri++]);

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