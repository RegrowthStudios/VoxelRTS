using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.IO;
using RTSEngine.Data.Parsers;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data {
    public static class UserConfig {
        public const bool DEFAULT_FULLSCREEN = false;
        public const string DEFAULT_USER_NAME = "___DEFAULT_USER___";
        public const int DEFAULT_RES_WIDTH = 800;
        public const int DEFAULT_RES_HEIGHT = 600;

        public class ColorScheme {
            public Color WidgetBase;
            public Color WidgetInactive;
            public Color WidgetActive;
            public Color WidgetBorder;
            public Color Text;

            public ColorScheme() {
                WidgetBase = new Color(12, 12, 12);
                WidgetInactive = new Color(12, 12, 26);
                WidgetActive = new Color(20, 80, 120);
                WidgetBorder = new Color(15, 15, 0);
                Text = new Color(200, 250, 100);
            }
        }
        private class Args {
            [ZXParse("FULL")]
            public bool FullScreen;
            [ZXParse("USERNAME")]
            public string UserName;
            [ZXParse("RESWIDTH")]
            public int ResWidth;
            [ZXParse("RESHEIGHT")]
            public int ResHeight;

            [ZXParse("CSMAIN")]
            public ColorScheme MainScheme;

            public Args() {
                FullScreen = false;
                UserName = DEFAULT_USER_NAME;
                ResWidth = DEFAULT_RES_WIDTH;
                ResHeight = DEFAULT_RES_HEIGHT;
                MainScheme = new ColorScheme();
            }
        }

        private static Args data = new Args();
        private static bool changeDetected = false;
        public static ColorScheme MainScheme {
            get { return data.MainScheme; }
        }

        public static bool UseFullscreen {
            get { return data.FullScreen; }
            set {
                if(data.FullScreen != value) {
                    data.FullScreen = value;
                    changeDetected = true;
                }
            }
        }
        public static string UserName {
            get { return data.UserName; }
            set {
                if(data.UserName != value) {
                    data.UserName = value;
                    changeDetected = true;
                }
            }
        }
        public static int ResolutionWidth {
            get { return data.ResWidth; }
            set {
                if(data.ResWidth != value) {
                    data.ResWidth = value;
                    changeDetected = true;
                }
            }
        }
        public static int ResolutionHeight {
            get { return data.ResHeight; }
            set {
                if(data.ResHeight != value) {
                    data.ResHeight = value;
                    changeDetected = true;
                }
            }
        }

        public static void Load(string path) {
            FileInfo fi = new FileInfo(path);
            if(!fi.Exists) {
                changeDetected = true;
                data = new Args();
            }
            else {
                using(var s = fi.OpenRead()) {
                    var mStr = new StreamReader(s).ReadToEnd();
                    ZXParser.ParseInto(mStr, data);
                }
            }
        }
        public static void Save(string path) {
            if(!changeDetected) return;

            using(var s = File.Create(path)) {
                StreamWriter sw = new StreamWriter(s);
                ZXParser.Write(sw, data);
                sw.Flush();
            }
        }
    }
}