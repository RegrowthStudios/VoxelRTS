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
        static readonly Regex rgxName = RegexHelper.GenerateString("Name");

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
        public static RTSRace Parse(FileInfo infoFile, Dictionary<string, ReflectedScript> scripts) {
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
            ZXParser.SetEnvironment("DICTSCRIPTS", scripts);

            // Read Data
            RTSRace data = new RTSRace();
            ZXParser.ParseInto(mStr, data);
            data.InfoFile = new FileInfo(PathHelper.GetRelativePath(infoFile.FullName));
            data.UpdateActiveUnits();
            data.UpdateActiveBuildings();
            return data;
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