using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using Microsoft.Xna.Framework;
using RTSEngine.Data.Team;
using RTSEngine.Controllers;

namespace RTSEngine.Data.Parsers {
    public static class RTSRaceParser {
        const string DIRECTORY = "races";
        const string EXTENSION = "race";
        static readonly Regex rgxName = RegexHelper.GenerateString("NAME");
        private static readonly Regex rgxCtrlAction = RegexHelper.Generate("CTRLACTION", @"[\w\s\.]+");
        private static readonly Regex rgxCtrlMovement = RegexHelper.Generate("CTRLMOVEMENT", @"[\w\s\.]+");
        private static readonly Regex rgxCtrlTargeting = RegexHelper.Generate("CTRLTARGET", @"[\w\s\.]+");
        static readonly Regex rgxUnit = RegexHelper.GenerateFile("UNIT");
        static readonly Regex rgxBuilding = RegexHelper.GenerateFile("BUILDING");

        private static bool IsFile(FileInfo fi) {
            return fi.Extension.EndsWith(EXTENSION);
        }

        public static List<RTSRace> ParseAll(DirectoryInfo dirPacks, Dictionary<string, ReflectedScript> scripts) {
            if(!dirPacks.Exists)
                throw new DirectoryNotFoundException("Packs Directory Doesn't Exist");

            var res = new List<RTSRace>();
            foreach(var dir in dirPacks.GetDirectories()) {
                // Check For A Race Directory
                DirectoryInfo dt = new DirectoryInfo(Path.Combine(dir.FullName, DIRECTORY));
                if(!dt.Exists) continue;

                // Read All Races
                foreach(FileInfo fi in dt.GetFiles().Where(IsFile)) {
                    res.Add(Parse(fi, null));
                }
            }
            return res;
        }
        public static RTSRace Parse(FileInfo fi, Dictionary<string, ReflectedScript> scripts) {
            if(fi == null || !fi.Exists) return null;

            string mStr = null;
            using(FileStream s = File.OpenRead(fi.FullName)) {
                mStr = new StreamReader(s).ReadToEnd();
            }

            RTSRace res = new RTSRace();
            res.InfoFile = fi;

            // Read Name
            res.FriendlyName = RegexHelper.Extract(rgxName.Match(mStr));
            if(scripts != null) {
                res.SCAction = scripts[RegexHelper.Extract(rgxCtrlAction.Match(mStr))];
                res.SCMovement = scripts[RegexHelper.Extract(rgxCtrlMovement.Match(mStr))];
                res.SCTargeting = scripts[RegexHelper.Extract(rgxCtrlTargeting.Match(mStr))];
            }

            // Read All Units
            Match m = rgxUnit.Match(mStr);
            int type = 0;
            while(m.Success) {
                RTSUnitData data = RTSUnitDataParser.ParseData(scripts, RegexHelper.ExtractFile(m, fi.Directory.FullName));
                res.Units[type++] = data;
                m = m.NextMatch();
            }
            res.UpdateActiveUnits();

            // Read All Buildings
            m = rgxBuilding.Match(mStr);
            type = 0;
            while(m.Success) {
                RTSBuildingData data = RTSBuildingDataParser.ParseData(scripts, RegexHelper.ExtractFile(m, fi.Directory.FullName));
                res.Buildings[type++] = data;
                m = m.NextMatch();
            }
            res.UpdateActiveBuildings();

            return res;
        }
        public static string ParseName(FileInfo fi) {
            if(fi == null || !fi.Exists) return null;

            string mStr = null;
            using(FileStream s = File.OpenRead(fi.FullName)) {
                mStr = new StreamReader(s).ReadToEnd();
            }

            return RegexHelper.Extract(rgxName.Match(mStr));
        }
    }
}