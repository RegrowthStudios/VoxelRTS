using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RTSEngine.Controllers;
using RTSEngine.Data.Team;

namespace RTSEngine.Data.Parsers {
    // This Is How A Team Should Be Made
    public struct TeamInitOption {
        public string PlayerName;
        public InputType InputType;
        public string Race;
        public RTSColorScheme Colors;
    }

    // The Data The Engine Needs To Know About To Properly Create A Game
    public struct EngineLoadData {
        // Teams In The Battle
        public TeamInitOption[] Teams;

        // Where To Load The Map
        public FileInfo MapFile;
    }

    public static class EngineLoadParser {
        // Data Detection
        public const string EXTENSION = "eld";
        private static readonly Regex rgxName = RegexHelper.Generate("NAME", @"[\w\s]+");
        private static readonly Regex rgxInputType = RegexHelper.GenerateString("");


        public static void Parse(FileInfo infoFile) {

        }
    }
}