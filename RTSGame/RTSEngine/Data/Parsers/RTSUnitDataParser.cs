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
    public struct RTSUnitResult {
        public RTSUnitData Data;
        public RTSUnitModel View;
    }

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

        public static RTSUnitResult Parse(GameEngine ge, FileInfo infoFile) {
            // Parse Data
            RTSUnitResult res;
            using(Stream s = File.OpenRead(infoFile.FullName)) {
                res = ParseFromInfo(ge, new StreamReader(s), infoFile.Directory.FullName);
            }
            return res;
        }
        private static RTSUnitResult ParseFromInfo(GameEngine ge, StreamReader s, string rootDir) {
            RTSUnitResult res = new RTSUnitResult();
            string ms = s.ReadToEnd();
            int[] buf;

            // Read Data
            res.Data = new RTSUnitData();
            res.Data.FriendlyName = RegexHelper.Extract(rgxName.Match(ms));
            res.Data.Health = RegexHelper.ExtractInt(rgxHealth.Match(ms));
            buf = RegexHelper.ExtractVec2I(rgxCost.Match(ms));
            res.Data.CapitalCost = buf[0];
            res.Data.PopulationCost = buf[1];
            res.Data.MaxCount = RegexHelper.ExtractInt(rgxMaxCount.Match(ms));
            res.Data.MovementSpeed = RegexHelper.ExtractFloat(rgxSpeed.Match(ms));
            res.Data.ICollidableShape = new CollisionCircle(
                RegexHelper.ExtractFloat(rgxRadius.Match(ms)),
                Vector2.Zero, false
                );
            res.Data.BBox.Min = RegexHelper.ExtractVec3(rgxBBMin.Match(ms));
            res.Data.BBox.Max = RegexHelper.ExtractVec3(rgxBBMax.Match(ms));
            res.Data.BaseCombatData.Armor = RegexHelper.ExtractInt(rgxArmor.Match(ms));
            buf = RegexHelper.ExtractVec2I(rgxDamage.Match(ms));
            res.Data.BaseCombatData.AttackDamage = buf[0];
            res.Data.BaseCombatData.CriticalDamage = buf[1];
            buf = RegexHelper.ExtractVec2I(rgxRange.Match(ms));
            res.Data.BaseCombatData.MinRange = buf[0];
            res.Data.BaseCombatData.MaxRange = buf[1];
            res.Data.BaseCombatData.CriticalChance = RegexHelper.ExtractDouble(rgxCritChance.Match(ms));
            res.Data.BaseCombatData.AttackTimer = RegexHelper.ExtractFloat(rgxTimer.Match(ms));

            // Get The Controllers From The Controller Dictionary
            if(ge.State != null) {
                res.Data.DefaultActionController = ge.State.UnitControllers[RegexHelper.Extract(rgxCtrlAction.Match(ms))];
                res.Data.DefaultAnimationController = ge.State.UnitControllers[RegexHelper.Extract(rgxCtrlAnimation.Match(ms))];
                res.Data.DefaultCombatController = ge.State.UnitControllers[RegexHelper.Extract(rgxCtrlCombat.Match(ms))];
                res.Data.DefaultMoveController = ge.State.UnitControllers[RegexHelper.Extract(rgxCtrlMove.Match(ms))];
            }

            FileInfo fiModel = RegexHelper.ExtractFile(rgxModel.Match(ms), rootDir);
            FileInfo fiAnim = RegexHelper.ExtractFile(rgxAnimation.Match(ms), rootDir);
            using(var sModel = File.OpenRead(fiModel.FullName)) {
                Texture2D tAnim = AnimationFromBitmap(ge, fiAnim);
                res.View = new RTSUnitModel(ge, res.Data, sModel, tAnim);
            }

            FileInfo fiTex = RegexHelper.ExtractFile(rgxMainTex.Match(ms), rootDir);
            res.View.ModelTexture = ge.LoadTexture2D(fiTex.FullName);
            fiTex = RegexHelper.ExtractFile(rgxColorTex.Match(ms), rootDir);
            res.View.ColorCodeTexture = ge.LoadTexture2D(fiTex.FullName);
            return res;
        }

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
    }
}