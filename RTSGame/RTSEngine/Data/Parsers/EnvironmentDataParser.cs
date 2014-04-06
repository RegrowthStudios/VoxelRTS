using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RTSEngine.Data.Parsers {
    public class EnvironmentDataParser {
        // Data Detection
        private static readonly Regex rgxFlora = RegexHelper.GenerateInteger("FLORA");
        private static readonly Regex rgxOre = RegexHelper.GenerateInteger("ORE");
        private static readonly Regex rgxMinion = RegexHelper.GenerateInteger("MINION");
        private static readonly Regex rgxTank = RegexHelper.GenerateInteger("TANK");
        private static readonly Regex rgxTitan = RegexHelper.GenerateInteger("TITAN");
    }
}