using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;

namespace RTSEngine.Data.Parsers {
    public enum ParseType {
        // Boolean
        Boolean,

        // Integral
        Byte,
        SByte,
        Short,
        UShort,
        Int,
        UInt,
        Long,
        ULong,

        // Floating Point
        Single,
        Double,
        Decimal,

        // String
        String,

        // Custom
        Custom
    }

    public static class ZXPRegexConstructor {
        private static readonly string RGX_UNSIGNED_INTEGER = @"(?:\d+)";
        private static readonly string RGX_INTEGER = @"(?:\x2d?" + RGX_UNSIGNED_INTEGER + @")";
        private static readonly string RGX_NUMBER_NO_EXPONENT = RGX_INTEGER + @"(?:\." + RGX_UNSIGNED_INTEGER + @")?";
        private static readonly string RGX_NUMBER = RGX_NUMBER_NO_EXPONENT + @"(?:e" + RGX_INTEGER + @")?";
        private static readonly string RGX_STRING_TYPE = @"(?:[\.\w]*)";
        private static readonly string RGX_STRING = @"(?:[\x20-\x7f\x09-\x0b]*)";
        private static readonly string RGX_BOOLEAN = @"(?:(?:[Tt][Rr][Uu][Ee])|(?:[Ff][Aa][Ll][Ss][Ee]))";

        private static readonly string RGX_VALUE_GROUP_NAME = @"VALUE";
        private static readonly string RGX_TYPE_GROUP_NAME = @"TYPE";
        private static readonly string RGX_FORMAT_KEY_VALUE = @"(?:{0})\s+\x5b\s*(?<" + RGX_VALUE_GROUP_NAME + @">{1})\s*\x5d";
        private static readonly string RGX_FORMAT_KEY_VALUE_STRICT = @"(?:{0})\s+\x5b(?<" + RGX_VALUE_GROUP_NAME + @">{1})\x5d";
        private static readonly string RGX_FORMAT_KEY_VALUE_CUSTOM = @"(?:{0})\s+\x5b(?<" + RGX_TYPE_GROUP_NAME + @">" + RGX_STRING_TYPE + @")\x5d\s+\x7b(?<" + RGX_VALUE_GROUP_NAME + @">(?:(?s).*))\x7d";

        public static ParseType GetParseType(Type t) {
            switch(t.FullName) {
                case "System.Boolean": return ParseType.Boolean;
                case "System.SByte": return ParseType.SByte;
                case "System.Int16": return ParseType.Short;
                case "System.Int32": return ParseType.Int;
                case "System.Int64": return ParseType.Long;
                case "System.Byte": return ParseType.Byte;
                case "System.UInt16": return ParseType.UShort;
                case "System.UInt32": return ParseType.UInt;
                case "System.UInt64": return ParseType.ULong;
                case "System.String": return ParseType.String;
                case "System.Single": return ParseType.Single;
                case "System.Double": return ParseType.Double;
                case "System.Decimal": return ParseType.Decimal;
                default: return ParseType.Custom;
            }
        }
        public static Regex BuildRegex(ParseType pt, string key) {
            switch(pt) {
                case ParseType.Boolean:
                    return new Regex(string.Format(RGX_FORMAT_KEY_VALUE, key, RGX_BOOLEAN));
                case ParseType.SByte:
                case ParseType.Short:
                case ParseType.Int:
                case ParseType.Long:
                    return new Regex(string.Format(RGX_FORMAT_KEY_VALUE, key, RGX_INTEGER));
                case ParseType.Byte:
                case ParseType.UShort:
                case ParseType.UInt:
                case ParseType.ULong:
                    return new Regex(string.Format(RGX_FORMAT_KEY_VALUE, key, RGX_UNSIGNED_INTEGER));
                case ParseType.Single:
                case ParseType.Double:
                case ParseType.Decimal:
                    return new Regex(string.Format(RGX_FORMAT_KEY_VALUE, key, RGX_NUMBER));
                case ParseType.String:
                    return new Regex(string.Format(RGX_FORMAT_KEY_VALUE_STRICT, key, RGX_STRING));
                case ParseType.Custom:
                    return new Regex(string.Format(RGX_FORMAT_KEY_VALUE_CUSTOM, key));
                default:
                    return null;
            }
        }

        public static string GetRegexValueString(Regex r, string mStr) {
            Match m = r.Match(mStr);
            if(!m.Success) return null;
            return m.Groups[RGX_VALUE_GROUP_NAME].Value;
        }
        public static string GetRegexValueString(Regex r, string mStr, out string type) {
            Match m = r.Match(mStr);
            type = null;
            if(!m.Success) return null;
            type = m.Groups[RGX_TYPE_GROUP_NAME].Value;
            return m.Groups[RGX_VALUE_GROUP_NAME].Value;
        }
    }

    public struct ZXPData {
        public readonly string Key;
        public readonly FieldInfo Field;
        public readonly ParseType ParseType;
        public readonly Regex Regex;

        public ZXPData(ZXParseAttribute attr, FieldInfo fi) {
            Key = attr.Key;
            Field = fi;
            ParseType = ZXPRegexConstructor.GetParseType(fi.FieldType);
            Regex = ZXPRegexConstructor.BuildRegex(ParseType, Key);
        }
    }

    public class ZXParseProxy {
        private static readonly Type ATTR_TYPE = typeof(ZXParseAttribute);
        public const BindingFlags SEARCH_TYPE = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly Dictionary<string, ZXParseProxy> proxies = new Dictionary<string, ZXParseProxy>();

        public static ZXParseProxy GetProxy(Type t) {
            // Try To Get The Proxy
            ZXParseProxy zpp;
            if(proxies.TryGetValue(t.FullName, out zpp)) return zpp;

            // Create A New Proxy
            zpp = new ZXParseProxy(t);
            proxies.Add(t.FullName, zpp);
            zpp.CreateProxy();
            return zpp;
        }
        public static ZXParseProxy GetProxy(string t) {
            // Try To Get The Proxy
            ZXParseProxy zpp;
            if(proxies.TryGetValue(t, out zpp)) return zpp;

            // Create A New Proxy
            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var asm in asms) {
                Type type = asm.GetType(t, false, false);
                if(type == null) continue;
                zpp = new ZXParseProxy(type);
                proxies.Add(type.FullName, zpp);
                zpp.CreateProxy();
                return zpp;
            }
            throw new Exception("Could Not Resolve The Type " + t);
        }
        private static ZXParseAttribute GetAttribute(FieldInfo fi) {
            object[] attr = fi.GetCustomAttributes(ATTR_TYPE, true);
            if(attr == null || attr.Length < 1) return null;
            return attr[0] as ZXParseAttribute;
        }

        public Type ParseType {
            get;
            private set;
        }
        public string Key {
            get;
            private set;
        }

        public List<ZXPData> Fields {
            get;
            private set;
        }

        private ZXParseProxy(Type t) {
            ParseType = t;
            Key = ParseType.FullName;

            Fields = new List<ZXPData>();
        }

        private void CreateProxy() {
            CreateProxyFields();
            PropertyInfo[] props = ParseType.GetProperties(SEARCH_TYPE);
            MethodInfo[] methods = ParseType.GetMethods(SEARCH_TYPE);
        }
        private void CreateProxyFields() {
            FieldInfo[] fields = ParseType.GetFields(SEARCH_TYPE);
            for(int i = 0; i < fields.Length; i++) {
                var field = fields[i];
                ZXParseAttribute attr = GetAttribute(field);
                if(attr != null)
                    Fields.Add(new ZXPData(attr, field));
            }
        }
    }

    public class ZXParser {
        private static object CreateDefaultFromType(Type t) {
            ConstructorInfo ci = t.GetConstructor(System.Type.EmptyTypes);
            if(ci == null) throw new Exception("This Type Must Have A Constructor That Takes Zero Parameters");
            return ci.Invoke(null);
        }
        public static object Parse(string s, Type t) {
            ZXParseProxy zpp = ZXParseProxy.GetProxy(t);
            object o = CreateDefaultFromType(zpp.ParseType);
            Parse(s, o, zpp);
            return o;
        }
        public static object Parse(string s, string t) {
            ZXParseProxy zpp = ZXParseProxy.GetProxy(t);
            object o = CreateDefaultFromType(zpp.ParseType);
            Parse(s, o, zpp);
            return o;
        }
        public static void Parse(string s, object o, ZXParseProxy zpp) {
            foreach(var f in zpp.Fields) {
                object value = null;
                string v;
                if(f.ParseType == ParseType.Custom) {
                    string t;
                    v = ZXPRegexConstructor.GetRegexValueString(f.Regex, s, out t);
                    if(v == null) continue;
                    if(string.IsNullOrWhiteSpace(v) || v.Trim().ToLower().Equals("null"))
                        value = null;
                    else {
                        value = Parse(v, t);
                    }
                }
                else {
                    v = ZXPRegexConstructor.GetRegexValueString(f.Regex, s);
                    if(v == null) continue;
                    Parse(f.ParseType, v, out value);
                }
                f.Field.SetValue(o, value);
            }
        }
        public static void Parse(string s, object o) {
            ZXParseProxy zpp = ZXParseProxy.GetProxy(o.GetType());
            Parse(s, o, zpp);
        }

        public static void Write(TextWriter writer, object o, ZXParseProxy zpp, int tabs = 0) {
            foreach(var f in zpp.Fields) {
                object value = f.Field.GetValue(o);
                if(f.ParseType == ParseType.Custom) {
                    if(value == null) writer.WriteLine("{2}{0} [{1}] {{ NULL }}", f.Key, f.Field.FieldType.FullName, new string('\t', tabs));
                    else {
                        writer.WriteLine("{2}{0} [{1}] {{", f.Key, f.Field.FieldType.FullName, new string('\t', tabs));
                        Write(writer, value, ZXParseProxy.GetProxy(value.GetType()), tabs + 1);
                        writer.WriteLine("{0}", new string('\t', tabs) + "}");
                    }
                }
                else writer.WriteLine("{2}{0} [{1}]", f.Key, value, new string('\t', tabs));
            }
        }
        public static void Write(TextWriter writer, object o) {
            Write(writer, o, ZXParseProxy.GetProxy(o.GetType()));
        }

        private static void Parse(ParseType pt, string s, out object v) {
            switch(pt) {
                case ParseType.Boolean:
                    if(!ParseBoolean(s, out v))
                        throw new Exception("Could Not Parse Value Expression [" + s + "] To Boolean");
                    break;
                case ParseType.SByte:
                    if(!ParseSByte(s, out v))
                        throw new Exception("Could Not Parse Value Expression [" + s + "] To SByte");
                    break;
                case ParseType.Short:
                    if(!ParseShort(s, out v))
                        throw new Exception("Could Not Parse Value Expression [" + s + "] To Short");
                    break;
                case ParseType.Int:
                    if(!ParseInt(s, out v))
                        throw new Exception("Could Not Parse Value Expression [" + s + "] To Int");
                    break;
                case ParseType.Long:
                    if(!ParseLong(s, out v))
                        throw new Exception("Could Not Parse Value Expression [" + s + "] To Long");
                    break;
                case ParseType.Byte:
                    if(!ParseByte(s, out v))
                        throw new Exception("Could Not Parse Value Expression [" + s + "] To Byte");
                    break;
                case ParseType.UShort:
                    if(!ParseUShort(s, out v))
                        throw new Exception("Could Not Parse Value Expression [" + s + "] To UShort");
                    break;
                case ParseType.UInt:
                    if(!ParseUInt(s, out v))
                        throw new Exception("Could Not Parse Value Expression [" + s + "] To UInt");
                    break;
                case ParseType.ULong:
                    if(!ParseULong(s, out v))
                        throw new Exception("Could Not Parse Value Expression [" + s + "] To ULong");
                    break;
                case ParseType.Single:
                    if(!ParseSingle(s, out v))
                        throw new Exception("Could Not Parse Value Expression [" + s + "] To Single");
                    break;
                case ParseType.Double:
                    if(!ParseDouble(s, out v))
                        throw new Exception("Could Not Parse Value Expression [" + s + "] To Double");
                    break;
                case ParseType.Decimal:
                    if(!ParseDecimal(s, out v))
                        throw new Exception("Could Not Parse Value Expression [" + s + "] To Decimal");
                    break;
                case ParseType.String:
                    v = s;
                    break;
                case ParseType.Custom:
                    throw new Exception("Custom Should Have Been Parsed Elsewhere");
                default:
                    throw new Exception("This Cannot Be Parsed");
            }
        }
        private static bool ParseBoolean(string s, out object v) {
            switch(s.Trim().ToLower()) {
                case "true":
                    v = true;
                    return true;
                case "false":
                    v = false;
                    return true;
                default:
                    v = null;
                    return false;
            }
        }
        private static bool ParseSByte(string s, out object v) {
            sbyte t;
            if(!sbyte.TryParse(s, out t)) {
                v = null;
                return false;
            }

            v = t;
            return true;
        }
        private static bool ParseShort(string s, out object v) {
            short t;
            if(!short.TryParse(s, out t)) {
                v = null;
                return false;
            }

            v = t;
            return true;
        }
        private static bool ParseInt(string s, out object v) {
            int t;
            if(!int.TryParse(s, out t)) {
                v = null;
                return false;
            }

            v = t;
            return true;
        }
        private static bool ParseLong(string s, out object v) {
            long t;
            if(!long.TryParse(s, out t)) {
                v = null;
                return false;
            }

            v = t;
            return true;
        }
        private static bool ParseByte(string s, out object v) {
            byte t;
            if(!byte.TryParse(s, out t)) {
                v = null;
                return false;
            }

            v = t;
            return true;
        }
        private static bool ParseUShort(string s, out object v) {
            ushort t;
            if(!ushort.TryParse(s, out t)) {
                v = null;
                return false;
            }

            v = t;
            return true;
        }
        private static bool ParseUInt(string s, out object v) {
            uint t;
            if(!uint.TryParse(s, out t)) {
                v = null;
                return false;
            }

            v = t;
            return true;
        }
        private static bool ParseULong(string s, out object v) {
            ulong t;
            if(!ulong.TryParse(s, out t)) {
                v = null;
                return false;
            }

            v = t;
            return true;
        }
        private static bool ParseSingle(string s, out object v) {
            float t;
            if(!float.TryParse(s, out t)) {
                v = null;
                return false;
            }

            v = t;
            return true;
        }
        private static bool ParseDouble(string s, out object v) {
            double t;
            if(!double.TryParse(s, out t)) {
                v = null;
                return false;
            }

            v = t;
            return true;
        }
        private static bool ParseDecimal(string s, out object v) {
            decimal t;
            if(!decimal.TryParse(s, out t)) {
                v = null;
                return false;
            }

            v = t;
            return true;
        }
    }
}