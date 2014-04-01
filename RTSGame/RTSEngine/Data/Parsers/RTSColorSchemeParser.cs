using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using RTSEngine.Data.Team;

namespace RTSEngine.Data.Parsers {
    public static class RTSColorSchemeParser {
        public const string EXTENSION = "scheme";
        private static readonly Regex rgxName = RegexHelper.GenerateString("NAME");
        private static readonly Regex rgxPrimary = RegexHelper.GenerateVec3("PRIMARY");
        private static readonly Regex rgxSecondary = RegexHelper.GenerateVec3("SECONDARY");
        private static readonly Regex rgxTertiary = RegexHelper.GenerateVec3("TERTIARY");

        private static bool IsFile(FileInfo fi) {
            return fi.Extension.EndsWith(EXTENSION);
        }

        public static List<RTSColorScheme> ParseAll(DirectoryInfo dir) {
            if(!dir.Exists)
                throw new DirectoryNotFoundException("Color Scheme Directory Doesn't Exist");

            var l = new List<RTSColorScheme>();
            RTSColorScheme buf;
            foreach(FileInfo fi in dir.GetFiles().Where(IsFile)) {
                using(FileStream s = File.OpenRead(fi.FullName)) {
                    if(Parse(s, out buf)) l.Add(buf);
                }
            }
            return l;
        }
        private static bool Parse(Stream s, out RTSColorScheme scheme) {
            scheme = new RTSColorScheme();

            // Read All And Match
            StreamReader sr = new StreamReader(s);
            string ms = sr.ReadToEnd();
            Match[] m = {
                rgxName.Match(ms),
                rgxPrimary.Match(ms),
                rgxSecondary.Match(ms),
                rgxTertiary.Match(ms)
            };

            // Check For All Primary Data
            foreach(var match in m)
                if(!match.Success)
                    return false;

            // Extract Data
            scheme.Name = RegexHelper.Extract(m[0]);
            scheme.Primary = RegexHelper.ExtractVec3(m[1]);
            scheme.Secondary = RegexHelper.ExtractVec3(m[2]);
            scheme.Tertiary = RegexHelper.ExtractVec3(m[3]);
            return true;
        }
    }
}