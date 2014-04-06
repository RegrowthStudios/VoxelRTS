using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using RTSEngine.Data.Parsers;

namespace RTSEngine.Data {
    public static class UserConfig {
        private static bool changeDetected = false;

        // Fullscreen Option
        private static bool useFullscreen;
        public static bool UseFullscreen {
            get { return useFullscreen; }
            set {
                if(useFullscreen != value) {
                    useFullscreen = value;
                    changeDetected = true;
                }
            }
        }
        public const bool DEFAULT_FULLSCREEN = false;
        public const string KEY_FULLSCREEN = "FULL";
        private static readonly Regex rgxFS = RegexHelper.Generate(KEY_FULLSCREEN, @"[\w\s]+");

        // User Name Option
        private static string userName;
        public static string UserName {
            get { return userName; }
            set {
                if(userName != value) {
                    userName = value;
                    changeDetected = true;
                }
            }
        }
        public const string DEFAULT_USER_NAME = "___DEFAULT_USER___";
        public const string KEY_USER_NAME = "USERNAME";
        private static readonly Regex rgxUN = RegexHelper.GenerateString(KEY_USER_NAME);

        // Resolution Options
        private static int resWidth;
        public static int ResolutionWidth {
            get { return resWidth; }
            set {
                if(resWidth != value) {
                    resWidth = value;
                    changeDetected = true;
                }
            }
        }
        public const int DEFAULT_RES_WIDTH = 800;
        public const string KEY_RES_WIDTH = "RESWIDTH";
        private static readonly Regex rgxRW = RegexHelper.GenerateInteger(KEY_RES_WIDTH);
        private static int resHeight;
        public static int ResolutionHeight {
            get { return resHeight; }
            set {
                if(resHeight != value) {
                    resHeight = value;
                    changeDetected = true;
                }
            }
        }
        public const int DEFAULT_RES_HEIGHT = 600;
        public const string KEY_RES_HEIGHT = "RESHEIGHT";
        private static readonly Regex rgxRH = RegexHelper.GenerateInteger(KEY_RES_HEIGHT);

        public static void Load(string path) {
            FileInfo fi = new FileInfo(path);
            if(!fi.Exists) {
                changeDetected = true;
                useFullscreen = DEFAULT_FULLSCREEN;
                userName = DEFAULT_USER_NAME;
                resWidth = DEFAULT_RES_WIDTH;
                resHeight = DEFAULT_RES_HEIGHT;
            }
            else {
                using(var s = File.OpenRead(fi.FullName)) {
                    Load(s);
                }
            }
        }
        private static void Load(Stream s) {
            StreamReader sr = new StreamReader(s);
            string ms = sr.ReadToEnd();
            Match m;

            // Get Fullscreen
            m = rgxFS.Match(ms);
            try {
                bool.TryParse(m.Groups[1].Value, out useFullscreen);
            }
            catch(Exception) {
                useFullscreen = DEFAULT_FULLSCREEN;
                changeDetected = true;
            }

            // Get User Name
            m = rgxUN.Match(ms);
            try {
                userName = RegexHelper.Extract(m);
            }
            catch(Exception) {
                userName = DEFAULT_USER_NAME;
                changeDetected = true;
            }

            // Get Resolution
            m = rgxRW.Match(ms);
            try {
                resWidth = RegexHelper.ExtractInt(m);
            }
            catch(Exception) {
                resWidth = DEFAULT_RES_WIDTH;
                changeDetected = true;
            }
            m = rgxRH.Match(ms);
            try {
                resHeight = RegexHelper.ExtractInt(m);
            }
            catch(Exception) {
                resHeight = DEFAULT_RES_HEIGHT;
                changeDetected = true;
            }
        }
        public static void Save(string path) {
            if(!changeDetected) return;

            using(var s = File.Create(path)) {
                StreamWriter sw = new StreamWriter(s);

                sw.WriteLine("{0,-20} [{1}]", KEY_FULLSCREEN, UseFullscreen);
                sw.WriteLine("{0,-20} [{1}]", KEY_USER_NAME, UserName);
                sw.WriteLine("{0,-20} [{1}]", KEY_RES_WIDTH, ResolutionWidth);
                sw.WriteLine("{0,-20} [{1}]", KEY_RES_HEIGHT, ResolutionHeight);

                sw.Flush();
            }
        }
    }
}