using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RTSEngine.Data.Parsers {
    public struct EnvironmentInitData {
        public static EnvironmentInitData Default {
            get {
                return new EnvironmentInitData() {
                    FloraType = 0,
                    OreType = 1,
                    MinionType = 0,
                    TankType = 1,
                    TitanType = 2
                };
            }
        }

        public int FloraType;
        public int OreType;
        public int MinionType;
        public int TankType;
        public int TitanType;
    }

    public class EnvironmentDataParser {
        // Data Detection
        private static readonly Regex rgxFlora = RegexHelper.GenerateInteger("FLORA");
        private static readonly Regex rgxOre = RegexHelper.GenerateInteger("ORE");
        private static readonly Regex rgxMinion = RegexHelper.GenerateInteger("MINION");
        private static readonly Regex rgxTank = RegexHelper.GenerateInteger("TANK");
        private static readonly Regex rgxTitan = RegexHelper.GenerateInteger("TITAN");

        public static EnvironmentInitData Parse(FileInfo infoFile) {
            // Check File Existence
            if(infoFile == null || !infoFile.Exists) return EnvironmentInitData.Default;

            // Read The Entire File
            string mStr;
            using(FileStream fs = File.OpenRead(infoFile.FullName)) {
                StreamReader s = new StreamReader(fs);
                mStr = s.ReadToEnd();
            }

            // Match Tokens
            Match[] mp = {
                rgxFlora.Match(mStr),
                rgxOre.Match(mStr),
                rgxMinion.Match(mStr),
                rgxTank.Match(mStr),
                rgxTitan.Match(mStr)
            };
            foreach(var m in mp) if(!m.Success) return EnvironmentInitData.Default;

            EnvironmentInitData eid;
            eid.FloraType = RegexHelper.ExtractInt(mp[0]);
            eid.OreType = RegexHelper.ExtractInt(mp[1]);
            eid.MinionType = RegexHelper.ExtractInt(mp[2]);
            eid.TankType = RegexHelper.ExtractInt(mp[3]);
            eid.TitanType = RegexHelper.ExtractInt(mp[4]);
            return eid;
        }
    }
}