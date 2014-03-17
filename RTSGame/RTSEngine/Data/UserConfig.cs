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

        public static void Load(string path) {
            FileInfo fi = new FileInfo(path);
            if(!fi.Exists) {
                changeDetected = true;
                useFullscreen = DEFAULT_FULLSCREEN;
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
        }
        public static void Save(string path) {
            if(!changeDetected) return;

            using(var s = File.Create(path)) {
                StreamWriter sw = new StreamWriter(s);

                sw.WriteLine("{0,-20} [{1}]", KEY_FULLSCREEN, UseFullscreen);

                sw.Flush();
            }
        }
    }
}