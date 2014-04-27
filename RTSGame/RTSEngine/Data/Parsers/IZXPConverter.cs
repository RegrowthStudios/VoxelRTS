using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace RTSEngine.Data.Parsers {
    public interface IZXPConverter {
        string ParsingType { get; }
        bool Convert(string s, out object value);
        string Convert(object value);
    }

    public class ZXPCBoolean : IZXPConverter {
        public string ParsingType { get { return "Boolean"; } }
        public bool Convert(string s, out object value) {
            switch(s.Trim().ToLower()) {
                case "true":
                    value = true;
                    return true;
                case "false":
                    value = false;
                    return true;
                default:
                    value = null;
                    return false;
            }
        }
        public string Convert(object value) {
            return ((bool)value).ToString();
        }
    }
    public class ZXPCSByte : IZXPConverter {
        public string ParsingType { get { return "Signed Byte"; } }
        public bool Convert(string s, out object value) {
            sbyte v;
            if(sbyte.TryParse(s, out v)) {
                value = v;
                return true;
            }
            value = null;
            return false;
        }
        public string Convert(object value) {
            return ((sbyte)value).ToString();
        }
    }
    public class ZXPCShort : IZXPConverter {
        public string ParsingType { get { return "Short"; } }
        public bool Convert(string s, out object value) {
            short v;
            if(short.TryParse(s, out v)) {
                value = v;
                return true;
            }
            value = null;
            return false;
        }
        public string Convert(object value) {
            return ((short)value).ToString();
        }
    }
    public class ZXPCInt : IZXPConverter {
        public string ParsingType { get { return "Int"; } }
        public bool Convert(string s, out object value) {
            int v;
            if(int.TryParse(s, out v)) {
                value = v;
                return true;
            }
            value = null;
            return false;
        }
        public string Convert(object value) {
            return ((int)value).ToString();
        }
    }
    public class ZXPCLong : IZXPConverter {
        public string ParsingType { get { return "Long"; } }
        public bool Convert(string s, out object value) {
            long v;
            if(long.TryParse(s, out v)) {
                value = v;
                return true;
            }
            value = null;
            return false;
        }
        public string Convert(object value) {
            return ((long)value).ToString();
        }
    }

    public class ZXPCByte : IZXPConverter {
        public string ParsingType { get { return "Byte"; } }
        public bool Convert(string s, out object value) {
            byte v;
            if(byte.TryParse(s, out v)) {
                value = v;
                return true;
            }
            value = null;
            return false;
        }
        public string Convert(object value) {
            return ((byte)value).ToString();
        }
    }
    public class ZXPCUShort : IZXPConverter {
        public string ParsingType { get { return "Unsigned Short"; } }
        public bool Convert(string s, out object value) {
            ushort v;
            if(ushort.TryParse(s, out v)) {
                value = v;
                return true;
            }
            value = null;
            return false;
        }
        public string Convert(object value) {
            return ((ushort)value).ToString();
        }
    }
    public class ZXPCUInt : IZXPConverter {
        public string ParsingType { get { return "Unsigned Int"; } }
        public bool Convert(string s, out object value) {
            uint v;
            if(uint.TryParse(s, out v)) {
                value = v;
                return true;
            }
            value = null;
            return false;
        }
        public string Convert(object value) {
            return ((uint)value).ToString();
        }
    }
    public class ZXPCULong : IZXPConverter {
        public string ParsingType { get { return "Unsigned Long"; } }
        public bool Convert(string s, out object value) {
            ulong v;
            if(ulong.TryParse(s, out v)) {
                value = v;
                return true;
            }
            value = null;
            return false;
        }
        public string Convert(object value) {
            return ((ulong)value).ToString();
        }
    }

    public class ZXPCFloat : IZXPConverter {
        public string ParsingType { get { return "Float"; } }
        public bool Convert(string s, out object value) {
            float v;
            if(float.TryParse(s, out v)) {
                value = v;
                return true;
            }
            value = null;
            return false;
        }
        public string Convert(object value) {
            return ((float)value).ToString();
        }
    }
    public class ZXPCDouble : IZXPConverter {
        public string ParsingType { get { return "Double"; } }
        public bool Convert(string s, out object value) {
            double v;
            if(double.TryParse(s, out v)) {
                value = v;
                return true;
            }
            value = null;
            return false;
        }
        public string Convert(object value) {
            return ((double)value).ToString();
        }
    }
    public class ZXPCDecimal : IZXPConverter {
        public string ParsingType { get { return "Decimal"; } }
        public bool Convert(string s, out object value) {
            decimal v;
            if(decimal.TryParse(s, out v)) {
                value = v;
                return true;
            }
            value = null;
            return false;
        }
        public string Convert(object value) {
            return ((decimal)value).ToString();
        }
    }

    public class ZXPCString : IZXPConverter {
        public string ParsingType { get { return "String"; } }
        public bool Convert(string s, out object value) {
            value = s;
            return true;
        }
        public string Convert(object value) {
            return (value as string);
        }
    }
}