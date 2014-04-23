using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data.Parsers {
    public static class RegexHelper {
        public const string DATA_STRING_REGEX = @"[\w\s]+";
        public const string DATA_FILE_REGEX = @"[\w\.][\w|\s|.|\\|/]*";
        public const string DATA_NUM_REGEX = @"[\d\x2d]*\.*\d*";
        public const string DATA_INT_REGEX = @"[\d\x2d]+";
        public const string NUM_SPLIT = @"\s*[\x7c/,]\s*";
        static readonly Regex RGX_VEC_SPLIT = new Regex(NUM_SPLIT);
        public const string DATA_VEC2_REGEX = DATA_NUM_REGEX + NUM_SPLIT + DATA_NUM_REGEX;
        public const string DATA_VEC3_REGEX = DATA_VEC2_REGEX + NUM_SPLIT + DATA_NUM_REGEX;
        public const string DATA_VEC4_REGEX = DATA_VEC3_REGEX + NUM_SPLIT + DATA_NUM_REGEX;
        public const string DATA_VEC2I_REGEX = DATA_INT_REGEX + NUM_SPLIT + DATA_INT_REGEX;
        public const string DATA_VEC3I_REGEX = DATA_VEC2I_REGEX + NUM_SPLIT + DATA_INT_REGEX;
        public const string DATA_VEC4I_REGEX = DATA_VEC3I_REGEX + NUM_SPLIT + DATA_INT_REGEX;
        public const string DATA_ARRAYI_REGEX = DATA_INT_REGEX + "(" + NUM_SPLIT + DATA_INT_REGEX + ")*";


        public static Regex Generate(string key, string data) {
            string rs = string.Format(@"{0}\s+\x5b({1})\x5d\s*", key, data);
            return new Regex(rs);
        }
        public static Regex GenerateString(string key) {
            return Generate(key, DATA_STRING_REGEX);
        }
        public static Regex GenerateFile(string key) {
            return Generate(key, DATA_FILE_REGEX);
        }
        public static Regex GenerateNumber(string key) {
            return Generate(key, DATA_NUM_REGEX);
        }
        public static Regex GenerateInteger(string key) {
            return Generate(key, DATA_INT_REGEX);
        }
        public static Regex GenerateVec2(string key) {
            return Generate(key, DATA_VEC2_REGEX);
        }
        public static Regex GenerateVec3(string key) {
            return Generate(key, DATA_VEC3_REGEX);
        }
        public static Regex GenerateVec4(string key) {
            return Generate(key, DATA_VEC4_REGEX);
        }
        public static Regex GenerateVec2Int(string key) {
            return Generate(key, DATA_VEC2I_REGEX);
        }
        public static Regex GenerateVec3Int(string key) {
            return Generate(key, DATA_VEC3I_REGEX);
        }
        public static Regex GenerateVec4Int(string key) {
            return Generate(key, DATA_VEC4I_REGEX);
        }

        public static string Extract(Match m) {
            return m.Groups[1].Value;
        }
        public static System.IO.DirectoryInfo ExtractDirectory(Match m) {
            return new System.IO.DirectoryInfo(m.Groups[1].Value.Trim());
        }
        public static System.IO.FileInfo ExtractFile(Match m) {
            return new System.IO.FileInfo(m.Groups[1].Value.Trim());
        }
        public static System.IO.DirectoryInfo ExtractDirectory(Match m, string root) {
            return new System.IO.DirectoryInfo(System.IO.Path.Combine(root, m.Groups[1].Value.Trim()));
        }
        public static System.IO.FileInfo ExtractFile(Match m, string root) {
            return new System.IO.FileInfo(System.IO.Path.Combine(root, m.Groups[1].Value.Trim()));
        }
        public static double ExtractDouble(Match m) {
            return double.Parse(m.Groups[1].Value);
        }
        public static float ExtractFloat(Match m) {
            return float.Parse(m.Groups[1].Value);
        }
        public static long ExtractLong(Match m) {
            return long.Parse(m.Groups[1].Value);
        }
        public static int ExtractInt(Match m) {
            return int.Parse(m.Groups[1].Value);
        }
        public static Vector2 ExtractVec2(Match m) {
            string[] s = RGX_VEC_SPLIT.Split(m.Groups[1].Value, 2);
            return new Vector2(
                float.Parse(s[0]),
                float.Parse(s[1])
                );
        }
        public static Vector3 ExtractVec3(Match m) {
            string[] s = RGX_VEC_SPLIT.Split(m.Groups[1].Value, 3);
            return new Vector3(
                float.Parse(s[0]),
                float.Parse(s[1]),
                float.Parse(s[2])
                );
        }
        public static Vector4 ExtractVec4(Match m) {
            string[] s = RGX_VEC_SPLIT.Split(m.Groups[1].Value, 4);
            return new Vector4(
                float.Parse(s[0]),
                float.Parse(s[1]),
                float.Parse(s[2]),
                float.Parse(s[3])
                );
        }
        public static int[] ExtractVec2I(Match m) {
            string[] s = RGX_VEC_SPLIT.Split(m.Groups[1].Value, 2);
            return new int[] {
                int.Parse(s[0]),
                int.Parse(s[1])
            };
        }
        public static int[] ExtractVec3I(Match m) {
            string[] s = RGX_VEC_SPLIT.Split(m.Groups[1].Value, 3);
            return new int[] {
                int.Parse(s[0]),
                int.Parse(s[1]),
                int.Parse(s[2])
            };
        }
        public static int[] ExtractVec4I(Match m) {
            string[] s = RGX_VEC_SPLIT.Split(m.Groups[1].Value, 4);
            return new int[] {
                int.Parse(s[0]),
                int.Parse(s[1]),
                int.Parse(s[2]),
                int.Parse(s[3])
            };
        }

        public static string ReadFile(FileInfo f) {
            // Check File Existence
            if(f == null || !f.Exists) return null;

            // Read The Entire File
            string mStr = null;
            using(FileStream fs = File.OpenRead(f.FullName)) {
                StreamReader s = new StreamReader(fs);
                mStr = s.ReadToEnd();
            }
            return mStr;
        }
        public static string ReadFile(string f) {
            return ReadFile(new FileInfo(f));
        }
        public static Match[] FindMatches(string mStr, params Regex[] rgx) {
            if(string.IsNullOrWhiteSpace(mStr)) return null;
            Match[] m = new Match[rgx.Length];
            for(int i = 0; i < m.Length; i++) {
                m[i] = rgx[i].Match(mStr);
            }
            return m;
        }
    }
}