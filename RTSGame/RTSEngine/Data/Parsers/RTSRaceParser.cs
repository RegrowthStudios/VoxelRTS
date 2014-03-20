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
        private static readonly Regex rgxCtrlTargetting = RegexHelper.Generate("CTRLTARGET", @"[\w\s\.]+");
        static readonly Regex rgxUnit = RegexHelper.GenerateFile("UNIT");

        private static bool IsFile(FileInfo fi) {
            return fi.Extension.EndsWith(EXTENSION);
        }

        public static List<RTSRaceData> ParseAll(DirectoryInfo dirPacks) {
            if(!dirPacks.Exists)
                throw new DirectoryNotFoundException("Packs Directory Doesn't Exist");

            var res = new List<RTSRaceData>();
            foreach(var dir in dirPacks.GetDirectories()) {
                // Check For A Race Directory
                DirectoryInfo dt = new DirectoryInfo(Path.Combine(dir.FullName, DIRECTORY));
                if(!dt.Exists) continue;

                // Read All Races
                foreach(FileInfo fi in dt.GetFiles().Where(IsFile)) {
                    using(FileStream s = File.OpenRead(fi.FullName)) {
                        res.Add(Parse(s, dt.FullName));
                    }
                }
            }
            return res;
        }
        private static RTSRaceData Parse(Stream s, string rootDir) {
            RTSRaceData res = new RTSRaceData();

            StreamReader sr = new StreamReader(s);
            string ms = sr.ReadToEnd();

            // Read Name
            res.Name = RegexHelper.Extract(rgxName.Match(ms));
            res.DefaultSquadActionController = RegexHelper.Extract(rgxCtrlAction.Match(ms));
            res.DefaultSquadTargettingController = RegexHelper.Extract(rgxCtrlTargetting.Match(ms));

            // Read All Units
            Match m = rgxUnit.Match(ms);
            while(m.Success) {
                res.UnitTypes.Add(RegexHelper.ExtractDirectory(m, rootDir));
                m = m.NextMatch();
            }

            return res;
        }
    }
}