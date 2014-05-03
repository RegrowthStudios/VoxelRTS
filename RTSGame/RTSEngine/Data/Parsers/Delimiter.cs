using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Data.Parsers {
    public enum DelimitType {
        None = 0x00,
        Paranthesis = 0x01,
        Bracket = 0x02,
        Curly = 0x04,
        Angled = 0x08,
        Any = Paranthesis | Bracket | Curly | Angled
    }
    public struct DIndices {
        public DelimitType Type;
        public int Start;
        public int End;
        public int Length {
            get { return End - Start + 1; }
        }
    }
    public static class Delimiter {
        public static bool IsOpener(char c, DelimitType dt) {
            return
                (dt.HasFlag(DelimitType.Paranthesis) && c == '(') ||
                (dt.HasFlag(DelimitType.Bracket) && c == '[') ||
                (dt.HasFlag(DelimitType.Curly) && c == '{') ||
                (dt.HasFlag(DelimitType.Angled) && c == '<');
        }
        public static bool IsCloser(char c, DelimitType dt) {
            return
                (dt.HasFlag(DelimitType.Paranthesis) && c == ')') ||
                (dt.HasFlag(DelimitType.Bracket) && c == ']') ||
                (dt.HasFlag(DelimitType.Curly) && c == '}') ||
                (dt.HasFlag(DelimitType.Angled) && c == '>');
        }
        public static DelimitType GetDType(char c) {
            switch(c) {
                case '(':
                case ')':
                    return DelimitType.Paranthesis;
                case '[':
                case ']':
                    return DelimitType.Bracket;
                case '{':
                case '}':
                    return DelimitType.Curly;
                case '<':
                case '>':
                    return DelimitType.Angled;
                default:
                    return DelimitType.None;
            }
        }

        public static List<DIndices> Delimit(string s, DelimitType dt) {
            var l = new List<DIndices>();
            int i = 0;
            while(i < s.Length) {
                DIndices di = GetSubstring(s, ref i, dt);
                if(di.Type == DelimitType.None) break;
                l.Add(di);
            }
            return l;
        }
        public static DIndices GetSubstring(string s, ref int i, DelimitType dt) {
            DIndices di = new DIndices();
            di.Type = DelimitType.None;
            if(i < 0) {
                di.Start = -1;
                return di;
            }

            // Find The First Character
            while(i < s.Length - 1) {
                if(IsOpener(s[i], dt)) {
                    di.Type = GetDType(s[i]);
                    i++;
                    break;
                }
                i++;
            }

            // Could Not Find The First Character
            if(di.Type == DelimitType.None)
                return di;

            // Find The Last Character
            di.Start = i;
            int open = 1;
            while(i < s.Length && open > 0) {
                if(IsOpener(s[i], di.Type)) {
                    open++;
                }
                else if(IsCloser(s[i], di.Type)) {
                    open--;
                    if(open == 0) {
                        di.End = i - 1;
                        return di;
                    }
                }
                i++;
            }

            // Could Not Find The Last Character
            di.Type = DelimitType.None;
            return di;
        }
    }
}