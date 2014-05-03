using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RTSEngine.Data.Parsers {
    //public static class ZXPRegex {
    //    public static readonly string RGX_UNSIGNED_INTEGER = @"(?:\d+)";
    //    public static readonly string RGX_INTEGER = @"(?:\x2d?" + RGX_UNSIGNED_INTEGER + @")";
    //    public static readonly string RGX_NUMBER_NO_EXPONENT = RGX_INTEGER + @"(?:\." + RGX_UNSIGNED_INTEGER + @")?";
    //    public static readonly string RGX_NUMBER = RGX_NUMBER_NO_EXPONENT + @"(?:e" + RGX_INTEGER + @")?";
    //    public static readonly string RGX_STRING_TYPE = @"(?:[\.\w]*)";
    //    public static readonly string RGX_STRING = @"(?:[\x20-\x7f\x09-\x0b]*)";
    //    public static readonly string RGX_BOOLEAN = @"(?:(?:[Tt][Rr][Uu][Ee])|(?:[Ff][Aa][Ll][Ss][Ee]))";
    //    public static readonly string RGX_ANY = @"(?:(?s).*)";

    //    private static readonly string RGX_VALUE_GROUP_NAME = @"VALUE";
    //    private static readonly string RGX_TYPE_GROUP_NAME = @"TYPE";
    //    private static readonly string RGX_FORMAT_KEY_VALUE = @"[^\x0a]*(?:{0})\s+\x5b\s*(?<" + RGX_VALUE_GROUP_NAME + @">{1})\s*\x5d[^\x0a]*";
    //    private static readonly string RGX_FORMAT_KEY_VALUE_STRICT = @"[^\x0a]*(?:{0})\s+\x5b(?<" + RGX_VALUE_GROUP_NAME + @">{1})\x5d[^\x0a]*";
    //    private static readonly string RGX_FORMAT_KEY_VALUE_CUSTOM = @"[^\x0a]*(?:{0})\s+\x5b(?<" + RGX_TYPE_GROUP_NAME + @">" + RGX_STRING_TYPE + @")\x5d\s+\x7b(?<" + RGX_VALUE_GROUP_NAME + @">(?:(?s).*))\x7d[^\x0a]*";
    //    private static readonly string RGX_FORMAT_FUNCTION = @"[^\x0a]*(?:{0})\x5b(?<" + RGX_VALUE_GROUP_NAME + @">" + RGX_STRING + ")\x5d[^\x0a]*";

    //    private static readonly Regex rgxValue = new Regex(@"__VALUE__(?<NUM>\d+)");
    //    private static readonly Regex rgxCustom = new Regex(@"__CUSTOM__(?<NUM>\d+)");

    //    public static Regex BuildRegex(IZXPConverter conv, string key) {
    //        if(conv == null)
    //            return new Regex(string.Format(RGX_FORMAT_KEY_VALUE_CUSTOM, key));
    //        //else if(conv.UseStrict)
    //        //    return new Regex(string.Format(RGX_FORMAT_KEY_VALUE_STRICT, key, conv.ValueRegex));
    //        else
    //            //return new Regex(string.Format(RGX_FORMAT_KEY_VALUE, key, conv.ValueRegex));
    //            return new Regex(string.Format(RGX_FORMAT_KEY_VALUE_STRICT, key, RGX_STRING));
    //    }
    //    public static Regex BuildRegexFunction(string key) {
    //        return new Regex(string.Format(RGX_FORMAT_FUNCTION, key));
    //    }

    //    public static string SubstituteBack(string s, Dictionary<string, string> dV, Dictionary<string, string> dCT) {
    //        Match mV = rgxValue.Match(s);
    //        while(mV.Success) {
    //            s = s.Replace(mV.Groups[0].Value, dV[mV.Groups[0].Value]);
    //            mV = mV.NextMatch();
    //        }
    //        Match mCT = rgxCustom.Match(s);
    //        while(mCT.Success) {
    //            s = s.Replace(mCT.Groups[0].Value, dCT[mCT.Groups[0].Value]);
    //            mCT = mCT.NextMatch();
    //        }
    //        return s;
    //    }
    //    public static string GetRegexValueString(Regex r, Regex rV, string mStr, Dictionary<string, string> dV, Dictionary<string, string> dCT) {
    //        Match m = r.Match(mStr);
    //        if(!m.Success) return null;
    //        string sValue = dV[m.Groups[RGX_VALUE_GROUP_NAME].Value];
    //        Match mV = rV.Match(sValue);
    //        if(!mV.Success) return null;
    //        return mV.Groups[0].Value;
    //    }
    //    public static string GetRegexValueString(Regex r, Regex rV, string mStr, out string type, Dictionary<string, string> dV, Dictionary<string, string> dCT) {
    //        type = null;
    //        Match m = r.Match(mStr);
    //        if(!m.Success) return null;
    //        type = dV[m.Groups[RGX_TYPE_GROUP_NAME].Value];
    //        return dCT[m.Groups[RGX_VALUE_GROUP_NAME].Value];
    //        //Match m = r.Match(mStr);
    //        //type = null;
    //        //if(!m.Success) return null;
    //        //type = m.Groups[RGX_TYPE_GROUP_NAME].Value;
    //        //return m.Groups[RGX_VALUE_GROUP_NAME].Value;
    //    }
    //    public static List<string> GetRegexValueStrings(Regex r, string mStr, Dictionary<string, string> dV) {
    //        var res = new List<string>();
    //        Match m = r.Match(mStr);
    //        while(m.Success) {
    //            res.Add(dV[m.Groups[RGX_VALUE_GROUP_NAME].Value]);
    //            m = m.NextMatch();
    //        }
    //        return res;
    //    }
    //}
}