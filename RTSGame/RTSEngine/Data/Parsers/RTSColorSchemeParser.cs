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

        public static RTSColorScheme? Parse(FileInfo infoFile) {
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return null;

            // Read The Entire File
            string mStr;
            using(FileStream fs = File.OpenRead(infoFile.FullName)) {
                StreamReader s = new StreamReader(fs);
                mStr = s.ReadToEnd();
            }
            RTSColorScheme scheme = new RTSColorScheme();

            // Read All And Match
            Match[] m = {
                rgxName.Match(mStr),
                rgxPrimary.Match(mStr),
                rgxSecondary.Match(mStr),
                rgxTertiary.Match(mStr)
            };

            // Check For All Primary Data
            foreach(var match in m)
                if(!match.Success)
                    return null;

            // Extract Data
            scheme.Name = RegexHelper.Extract(m[0]);
            scheme.Primary = RegexHelper.ExtractVec3(m[1]);
            scheme.Secondary = RegexHelper.ExtractVec3(m[2]);
            scheme.Tertiary = RegexHelper.ExtractVec3(m[3]);
            return scheme;
        }
    }
}