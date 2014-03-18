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
    public struct RTSTeamPlayResult {
        public RTSTeamResult TeamType;
        public RTSTeamColorScheme Colors;
        public InputType InputType;
    }

    public class RTSTeamResult {
        public string Name;
        public List<DirectoryInfo> UnitTypes;

        public RTSTeamResult() {
            Name = null;
            UnitTypes = new List<DirectoryInfo>();
        }
    }

    public static class RTSTeamParser {
        const string TEAM_DIR = "teams";
        const string INFO_FILE_EXT = "team";
        static readonly Regex rgxName = RegexHelper.Generate("NAME", @"[\w\s]+");
        static readonly Regex rgxCP = RegexHelper.GenerateVec3("PRIMARY");
        static readonly Regex rgxCS = RegexHelper.GenerateVec3("SECONDARY");
        static readonly Regex rgxCT = RegexHelper.GenerateVec3("TERTIARY");
        static readonly Regex rgxUnit = RegexHelper.GenerateFile("UNIT");

        public static List<RTSTeamResult> ParseAll(DirectoryInfo dirPacks) {
            var res = new List<RTSTeamResult>();
            foreach(var dir in dirPacks.GetDirectories()) {
                DirectoryInfo dt = new DirectoryInfo(Path.Combine(dir.FullName, TEAM_DIR));
                if(!dt.Exists) continue;
                List<RTSTeamResult> l = Parse(dt);
                res.AddRange(l);
            }
            return res;
        }
        private static List<RTSTeamResult> Parse(DirectoryInfo dir) {
            var results = new List<RTSTeamResult>();
            foreach(FileInfo file in dir.GetFiles()) {
                if(file.Extension.EndsWith(INFO_FILE_EXT))
                    results.Add(ParseInfo(file, dir.FullName));
            }
            return results;
        }
        private static RTSTeamResult ParseInfo(FileInfo file, string rootDir) {
            RTSTeamResult res = new RTSTeamResult();
            using(var fs = File.OpenRead(file.FullName)) {
                StreamReader s = new StreamReader(fs);
                string ms = s.ReadToEnd();

                // Read Name
                res.Name = RegexHelper.Extract(rgxName.Match(ms));

                // Read Color Scheme
                //res.Colors.Primary = RegexHelper.ExtractVec3(rgxCP.Match(ms));
                //res.Colors.Secondary = RegexHelper.ExtractVec3(rgxCS.Match(ms));
                //res.Colors.Tertiary = RegexHelper.ExtractVec3(rgxCT.Match(ms));

                // Read All Units
                Match m = rgxUnit.Match(ms);
                while(m.Success) {
                    res.UnitTypes.Add(RegexHelper.ExtractDirectory(m, rootDir));
                    m = m.NextMatch();
                }
            }
            return res;
        }
    }
}