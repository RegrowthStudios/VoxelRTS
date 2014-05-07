using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.Text.RegularExpressions;
using System.IO;

namespace System {
    /** Data Format
     * Field:
     * KEY [Data]
     * KEY < TYPE > { Data }
     * 
     * Function:
     * KEY( < TYPE > { Data } { Data } { Data } )
     * 
     */
    public class ZXParser {
        private static readonly Dictionary<Type, IZXPConverter> conversionFunctions = new Dictionary<Type, IZXPConverter>();
        private static readonly Dictionary<string, Type> searchedTypes = new Dictionary<string, Type>();
        private static readonly Dictionary<string, object> environment = new Dictionary<string, object>();

        private static Type GetTypeFromString(string t) {
            Type type;
            if(searchedTypes.TryGetValue(t, out type))
                return type;

            var asms = AppDomain.CurrentDomain.GetAssemblies();
            foreach(var asm in asms) {
                type = asm.GetType(t, false, false);
                if(type != null) {
                    searchedTypes.Add(t, type);
                    return type;
                }
            }
            return null;
        }
        private static object CreateDefaultFromType(Type t) {
            if(t.IsValueType)
                return System.Runtime.Serialization.FormatterServices.GetUninitializedObject(t);
            ConstructorInfo ci = t.GetConstructor(System.Type.EmptyTypes);
            if(ci == null) throw new Exception("This Type Must Have A Constructor That Takes Zero Parameters");
            return ci.Invoke(null);
        }
        public static string GetKeyString(string s, List<DIndices> l, int li) {
            // This Is The End Point Of The Key
            int ei = l[li].Start - 2;
            int si = li > 0 ? l[li - 1].End + 2 : 0;

            // Move End Index Until No White Space Is Found
            while(ei > si && char.IsWhiteSpace(s[ei])) ei--;

            // All White Space Was Found
            if(si >= ei) return null;

            // Find The Beginning Of The Key
            int ksi = ei - 1;
            while(ksi >= si && !char.IsWhiteSpace(s[ksi])) ksi--;
            ksi++;

            int ls = ei - ksi + 1;
            return (ls < 1 || (ksi + ls) > s.Length) ? "" : s.Substring(ksi, ls);
        }

        public static bool GetConverter(Type t, out IZXPConverter conv) {
            return conversionFunctions.TryGetValue(t, out conv);
        }
        public static void AddConverter(Type t, IZXPConverter conv) {
            conversionFunctions[t] = conv;
        }

        public static void SetEnvironment(string s, object v) {
            environment[s] = v;
        }
        public static bool GetEnvironment(string s, out object v) {
            return environment.TryGetValue(s, out v);
        }

        static ZXParser() {
            AddConverter(typeof(bool), new ZXPCBoolean());
            AddConverter(typeof(sbyte), new ZXPCSByte());
            AddConverter(typeof(short), new ZXPCShort());
            AddConverter(typeof(int), new ZXPCInt());
            AddConverter(typeof(long), new ZXPCLong());
            AddConverter(typeof(byte), new ZXPCByte());
            AddConverter(typeof(ushort), new ZXPCUShort());
            AddConverter(typeof(uint), new ZXPCUInt());
            AddConverter(typeof(ulong), new ZXPCULong());
            AddConverter(typeof(string), new ZXPCString());
            AddConverter(typeof(float), new ZXPCFloat());
            AddConverter(typeof(double), new ZXPCDouble());
            AddConverter(typeof(decimal), new ZXPCDecimal());
        }

        public static void ParseInto(string s, object o, ZXPProxy zpp) {
            List<DIndices> delimiters = Delimiter.Delimit(s, DelimitType.Any);
            for(int i = 0; i < delimiters.Count; ) {
                switch(delimiters[i].Type) {
                    // Plain Data That Should Be Able To Be Parsed In
                    case DelimitType.Bracket:
                        ParseSimpleData(o, zpp, s, delimiters, ref i);
                        break;
                    // This Is A Special Type
                    case DelimitType.Angled:
                        ParseComplexData(o, zpp, s, delimiters, ref i);
                        break;
                    // This Is A Function
                    case DelimitType.Paranthesis:
                        // Find The Key
                        ParseFunction(o, zpp, s, delimiters, ref i);
                        break;
                }
                i++;
            }
            return;
        }
        private static bool ReadValue(string s, IZXPConverter converter, out object val) {
            string sFormatted = s.Trim();
            switch(sFormatted.ToLower()) {
                case "null":
                    val = null;
                    return true;
            }

            // We Found An Environment Key To Use
            if(sFormatted.StartsWith("~~") && sFormatted.EndsWith("~~") && sFormatted.Length > 4) {
                string sEnvKey = sFormatted.Substring(2, sFormatted.Length - 4);
                return GetEnvironment(sEnvKey, out val);
            }

            // Try To Convert The Value
            if(converter != null) return converter.Convert(s, out val);
            else {
                val = null;
                return false;
            }
        }
        private static bool ReadValue(string s, IZXPConverter converter, Type t, out object val) {
            string sFormatted = s.Trim();
            switch(sFormatted.ToLower()) {
                case "null":
                    val = null;
                    return true;
            }

            // We Found An Environment Key To Use
            if(sFormatted.StartsWith("~~") && sFormatted.EndsWith("~~") && sFormatted.Length > 4) {
                string sEnvKey = sFormatted.Substring(2, sFormatted.Length - 4);
                return GetEnvironment(sEnvKey, out val);
            }

            // Try To Convert The Value
            if(converter != null) return converter.Convert(s, out val);
            else {
                val = ParseNew(s, t);
                return true;
            }
        }
        private static void ParseSimpleData(object o, ZXPProxy zpp, string s, List<DIndices> ld, ref int li) {
            // Check If A Key Is Available
            string key = GetKeyString(s, ld, li);
            if(string.IsNullOrWhiteSpace(key))
                return;

            // Find The Field That Matches To This Key
            ZXPDatum datum = null;
            if(zpp.DataDict.TryGetValue(key, out datum)) {
                object val = null;

                // Check For Array
                if(datum.Type.IsArray) {
                    if(!datum.Type.HasElementType) return;
                    Type eType = datum.Type.GetElementType();
                    if(ParseArray(eType, s.Substring(ld[li].Start, ld[li].Length), out val))
                        datum.SetValue(o, val);
                }

                // Check For A Possible Conversion
                if(datum.Converter == null) return;

                // Try To Convert
                string sValue = s.Substring(ld[li].Start, ld[li].Length);
                if(ReadValue(sValue, datum.Converter, out val))
                    datum.SetValue(o, val);
            }
        }
        private static void ParseComplexData(object o, ZXPProxy zpp, string s, List<DIndices> ld, ref int li) {
            // Check For Data Availability
            if(li + 1 >= ld.Count || ld[li].Length < 1 || ld[li + 1].Type != DelimitType.Curly)
                return;

            // Check If A Key Is Available
            string key = GetKeyString(s, ld, li);
            if(string.IsNullOrWhiteSpace(key))
                return;

            // Get Substrings
            DIndices diType = ld[li], diData = ld[li + 1];
            li++;

            // Find The Field That Matches To This Key
            ZXPDatum datum = null;
            if(!zpp.DataDict.TryGetValue(key, out datum)) return;

            // Check For A Possible Conversion
            if(datum.Converter != null) {
                // Try To Convert
                object val = null;
                string sValue = s.Substring(ld[li].Start, ld[li].Length);
                if(ReadValue(sValue, datum.Converter, out val))
                    datum.SetValue(o, val);
                return;
            }
            else {
                // Get The Special Type
                string sType = s.Substring(diType.Start, diType.Length);
                if(string.IsNullOrWhiteSpace(sType)) return;
                Type cType = GetTypeFromString(sType);
                if(cType == null) return;

                // Create And Set The Data
                object val = null;
                string sValue = s.Substring(diData.Start, diData.Length);
                if(ReadValue(sValue, datum.Converter, cType, out val))
                    datum.SetValue(o, val);
            }
        }
        private static void ParseFunction(object o, ZXPProxy zpp, string s, List<DIndices> ld, ref int li) {
            // Check If A Key Is Available
            string key = GetKeyString(s, ld, li);
            if(string.IsNullOrWhiteSpace(key))
                return;

            // Find The Method That Matches To This Key
            ZXPFunc func;
            MethodInfo method = null;
            if(!zpp.FuncsDict.TryGetValue(key, out func)) return;
            method = func.Method;

            // Check For Simple Invoke
            ParameterInfo[] paramInfo = method.GetParameters();
            if(paramInfo.Length < 1 || paramInfo == null) {
                method.Invoke(o, null);
                return;
            }

            // Parse Parameters
            string sArgs = s.Substring(ld[li].Start, ld[li].Length);
            object[] args = new object[paramInfo.Length];
            var nl = Delimiter.Delimit(sArgs, DelimitType.Angled | DelimitType.Curly);

            // Check Number Of Arguments
            int ca = 0;
            foreach(var ndi in nl) {
                if(ndi.Type == DelimitType.Curly) ca++;
            }
            if(ca != args.Length) return;

            // Create Parameters
            int ai = 0, pi = 0;
            for(; pi < args.Length; ) {
                if(nl[ai].Type == DelimitType.Angled) {
                    if(ai >= nl.Count - 1 || nl[ai + 1].Type != DelimitType.Curly) {
                        ai++;
                        continue;
                    }

                    // Get The Argument Type
                    string atValue = sArgs.Substring(nl[ai].Start, nl[ai].Length);
                    if(string.IsNullOrWhiteSpace(atValue))
                        break;
                    Type aType = GetTypeFromString(atValue);
                    if(aType == null)
                        break;


                    // Get The Argument
                    string sValue = sArgs.Substring(nl[ai + 1].Start, nl[ai + 1].Length);
                    IZXPConverter conv;
                    ZXParser.GetConverter(aType, out conv);
                    if(ReadValue(sValue, conv, aType, out args[pi])) {
                        pi++;
                    }
                    ai += 2;
                }
                else {
                    // Simple Parse
                    Type aType = paramInfo[pi].ParameterType;
                    string sValue = sArgs.Substring(nl[ai].Start, nl[ai].Length);
                    IZXPConverter conv;
                    ZXParser.GetConverter(aType, out conv);
                    if(ReadValue(sValue, conv, aType, out args[pi])) {
                        pi++;
                    }
                    ai++;
                }
            }
            // Check That All Arguments Are OK
            if(pi == args.Length) {
                method.Invoke(o, args);
            }
        }
        private static bool ParseArray(Type eType, string sArray, out object val) {
            val = null;
            IZXPConverter etConv;
            ZXParser.GetConverter(eType, out etConv);

            var nl = Delimiter.Delimit(sArray, DelimitType.Angled | DelimitType.Curly);

            // Create Parameters
            object element;
            var elements = new List<object>();
            for(int ai = 0; ai < nl.Count; ) {
                if(nl[ai].Type == DelimitType.Angled) {
                    if(ai >= nl.Count - 1 || nl[ai + 1].Type != DelimitType.Curly) {
                        ai++;
                        continue;
                    }

                    // Get The Argument Type
                    string atValue = sArray.Substring(nl[ai].Start, nl[ai].Length);
                    if(string.IsNullOrWhiteSpace(atValue))
                        break;
                    Type aType = GetTypeFromString(atValue);
                    if(aType == null)
                        break;


                    // Get The Argument
                    string sValue = sArray.Substring(nl[ai + 1].Start, nl[ai + 1].Length);
                    IZXPConverter conv;
                    ZXParser.GetConverter(aType, out conv);
                    if(ReadValue(sValue, conv, aType, out element)) {
                        elements.Add(element);
                    }
                    ai += 2;
                }
                else {
                    // Simple Parse
                    string sValue = sArray.Substring(nl[ai].Start, nl[ai].Length);
                    if(ReadValue(sValue, etConv, eType, out element)) {
                        elements.Add(element);
                    }
                    ai++;
                }
            }

            if(elements.Count < 1) return true;
            var valArr = Array.CreateInstance(eType, elements.Count);
            for(int i = 0; i < elements.Count; i++) {
                valArr.SetValue(elements[i], i);
            }
            val = valArr;
            return true;
        }

        public static object ParseNew(string s, Type t) {
            // Check Arguments
            if(t == null || string.IsNullOrEmpty(s))
                return null;

            // Attempt A Conversion
            IZXPConverter conv;
            if(GetConverter(t, out conv)) {
                object value = null;
                conv.Convert(s, out value);
                return value;
            }
            else if(t.IsArray) {
                object val = null;
                if(!t.HasElementType) return null;
                Type eType = t.GetElementType();
                if(ParseArray(eType, s, out val))
                    return val;
                else return null;
            }

            // Create A Complex Value
            ZXPProxy zpp = ZXPProxy.GetProxy(t);
            object o = CreateDefaultFromType(zpp.Type);
            ParseInto(s, o, zpp);
            return o;
        }
        public static object ParseNew(string s, string t) {
            return ParseNew(s, GetTypeFromString(t));
        }
        public static void ParseInto(string s, object o) {
            ZXPProxy zpp = ZXPProxy.GetProxy(o.GetType());
            ParseInto(s, o, zpp);
        }
        public static object ParseFile(string f, Type t) {
            FileInfo fi = new FileInfo(f);
            if(!fi.Exists) return null;
            object val = null;
            using(var fs = fi.OpenRead()) {
                val = ParseNew(new StreamReader(fs).ReadToEnd(), t);
            }
            return val;
        }

        public static void Write(StreamWriter writer, object o, ZXPProxy zpp, int spaces = 0) {
            foreach(var f in zpp.Data) {
                object value = f.GetValue(o);
                if(f.Converter == null) {
                    if(value == null) writer.WriteLine("{2}{0} <{1}> {{ NULL }}", f.Key, f.Type.FullName, new string(' ', spaces));
                    else {
                        Type vType = value.GetType();
                        writer.WriteLine("{2}{0} <{1}> {{", f.Key, vType.FullName, new string(' ', spaces));
                        Write(writer, value, ZXPProxy.GetProxy(vType), spaces + 4);
                        writer.WriteLine("{0}", new string(' ', spaces) + "}");
                    }
                }
                else writer.WriteLine("{2}{0} [{1}]", f.Key, f.Converter.Convert(value), new string(' ', spaces));
            }
        }
        public static void Write(Stream s, object o, ZXPProxy zpp, int spaces = 0) {
            Write(new StreamWriter(s), o, zpp, spaces);
        }
        public static void Write(StreamWriter writer, object o) {
            Write(writer, o, ZXPProxy.GetProxy(o.GetType()));
        }
        public static void Write(Stream s, object o) {
            Write(new StreamWriter(s), o);
        }
    }
}