using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace System {
    public static class ZXPCExt {
        public static readonly char[] VALUE_DELIMITERS = { ',', '|', ';' };
        public static void AddXNATypes() {
            ZXParser.AddConverter(typeof(Vector2), new ZXPCVector2());
            ZXParser.AddConverter(typeof(Vector3), new ZXPCVector3());
            ZXParser.AddConverter(typeof(Vector4), new ZXPCVector4());
            ZXParser.AddConverter(typeof(Point), new ZXPCPoint());
            ZXParser.AddConverter(typeof(Color), new ZXPCColor());
            ZXParser.AddConverter(typeof(Rectangle), new ZXPCRectangle());
            ZXParser.AddConverter(typeof(BoundingBox), new ZXPCBBox());
        }
    }

    public class ZXPCVector2 : IZXPConverter {
        public string ParsingType { get { return "Vector2"; } }
        public bool Convert(string s, out object value) {
            Vector2 v = Vector2.Zero;
            value = v;
            float f;
            string[] splits = s.Split(ZXPCExt.VALUE_DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
            int vi = 0;
            foreach(var sv in splits) {
                if(vi == 2) break;
                if(string.IsNullOrWhiteSpace(sv))
                    continue;
                if(float.TryParse(sv, out f)) {
                    switch(vi) {
                        case 0: v.X = f; break;
                        case 1: v.Y = f; break;
                    }
                    vi++;
                }
            }
            if(vi < 2) return false;

            value = v;
            return true;
        }
        public string Convert(object value) {
            if(value == null) return "Null";
            Vector2 v = (Vector2)value;
            if(v == null) return "Null";
            return string.Format("{0} , {1}", v.X, v.Y);
        }
    }
    public class ZXPCVector3 : IZXPConverter {
        public string ParsingType { get { return "Vector3"; } }
        public bool Convert(string s, out object value) {
            Vector3 v = Vector3.Zero;
            value = v;
            float f;
            string[] splits = s.Split(ZXPCExt.VALUE_DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
            int vi = 0;
            foreach(var sv in splits) {
                if(vi == 3) break;
                if(string.IsNullOrWhiteSpace(sv))
                    continue;
                if(float.TryParse(sv, out f)) {
                    switch(vi) {
                        case 0: v.X = f; break;
                        case 1: v.Y = f; break;
                        case 2: v.Z = f; break;
                    }
                    vi++;
                }
            }
            if(vi < 3) return false;

            value = v;
            return true;
        }
        public string Convert(object value) {
            if(value == null) return "Null";
            Vector3 v = (Vector3)value;
            if(v == null) return "Null";
            return string.Format("{0} , {1} , {2}", v.X, v.Y, v.Z);
        }
    }
    public class ZXPCVector4 : IZXPConverter {
        public string ParsingType { get { return "Vector4"; } }
        public bool Convert(string s, out object value) {
            Vector4 v = Vector4.Zero;
            value = v;
            float f;
            string[] splits = s.Split(ZXPCExt.VALUE_DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
            int vi = 0;
            foreach(var sv in splits) {
                if(vi == 4) break;
                if(string.IsNullOrWhiteSpace(sv))
                    continue;
                if(float.TryParse(sv, out f)) {
                    switch(vi) {
                        case 0: v.X = f; break;
                        case 1: v.Y = f; break;
                        case 2: v.Z = f; break;
                        case 3: v.W = f; break;
                    }
                    vi++;
                }
            }
            if(vi < 4) return false;

            value = v;
            return true;
        }
        public string Convert(object value) {
            if(value == null) return "Null";
            Vector4 v = (Vector4)value;
            if(v == null) return "Null";
            return string.Format("{0} , {1} , {2}, {3}", v.X, v.Y, v.Z, v.W);
        }
    }
    public class ZXPCPoint : IZXPConverter {
        public string ParsingType { get { return "Point"; } }
        public bool Convert(string s, out object value) {
            Point v = Point.Zero;
            value = v;
            int f;
            string[] splits = s.Split(ZXPCExt.VALUE_DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
            int vi = 0;
            foreach(var sv in splits) {
                if(vi == 2) break;
                if(string.IsNullOrWhiteSpace(sv))
                    continue;
                if(int.TryParse(sv, out f)) {
                    switch(vi) {
                        case 0: v.X = f; break;
                        case 1: v.Y = f; break;
                    }
                    vi++;
                }
            }
            if(vi < 2) return false;

            value = v;
            return true;
        }
        public string Convert(object value) {
            if(value == null) return "Null";
            Point v = (Point)value;
            if(v == null) return "Null";
            return string.Format("{0} , {1}", v.X, v.Y);
        }
    }
    public class ZXPCColor : IZXPConverter {
        public string ParsingType { get { return "Color"; } }
        public bool Convert(string s, out object value) {
            Color v = Color.Transparent;
            value = v;
            byte f;
            string[] splits = s.Split(ZXPCExt.VALUE_DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
            int vi = 0;
            foreach(var sv in splits) {
                if(vi == 4) break;
                if(string.IsNullOrWhiteSpace(sv))
                    continue;
                if(byte.TryParse(sv, out f)) {
                    switch(vi) {
                        case 0: v.R = f; break;
                        case 1: v.G = f; break;
                        case 2: v.B = f; break;
                        case 3: v.A = f; break;
                    }
                    vi++;
                }
            }
            if(vi < 4) return false;

            value = v;
            return true;
        }
        public string Convert(object value) {
            if(value == null) return "Null";
            Color v = (Color)value;
            if(v == null) return "Null";
            return string.Format("{0} , {1} , {2}, {3}", v.R, v.G, v.B, v.A);
        }
    }
    public class ZXPCRectangle : IZXPConverter {
        public string ParsingType { get { return "Rectangle"; } }
        public bool Convert(string s, out object value) {
            Rectangle v = Rectangle.Empty;
            value = v;
            int f;
            string[] splits = s.Split(ZXPCExt.VALUE_DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
            int vi = 0;
            foreach(var sv in splits) {
                if(vi == 4) break;
                if(string.IsNullOrWhiteSpace(sv))
                    continue;
                if(int.TryParse(sv, out f)) {
                    switch(vi) {
                        case 0: v.X = f; break;
                        case 1: v.Y = f; break;
                        case 2: v.Width = f; break;
                        case 3: v.Height = f; break;
                    }
                    vi++;
                }
            }
            if(vi < 4) return false;

            value = v;
            return true;
        }
        public string Convert(object value) {
            if(value == null) return "Null";
            Rectangle v = (Rectangle)value;
            if(v == null) return "Null";
            return string.Format("{0} , {1} , {2}, {3}", v.X, v.Y, v.Width, v.Height);
        }
    }
    public class ZXPCBBox : IZXPConverter {
        public static readonly char[] VALUE_DELIMITERS = { '-' };
        private ZXPCVector3 vec3Conv = new ZXPCVector3();
        public string ParsingType { get { return "Bounding Box"; } }
        public bool Convert(string s, out object value) {
            BoundingBox v = new BoundingBox();
            value = v;
            object corner;
            string[] splits = s.Split(VALUE_DELIMITERS, StringSplitOptions.RemoveEmptyEntries);
            if(splits.Length < 2) return false;
            int vi = 0;
            foreach(var sv in splits) {
                if(vi == 2) break;
                if(string.IsNullOrWhiteSpace(sv))
                    continue;
                if(vec3Conv.Convert(sv, out corner)) {
                    switch(vi) {
                        case 0: v.Min = (Vector3)corner; break;
                        case 1: v.Max = (Vector3)corner; break;
                    }
                    vi++;
                }
            }
            if(vi < 2) return false;

            value = v;
            return true;
        }
        public string Convert(object value) {
            if(value == null) return "Null";
            BoundingBox v = (BoundingBox)value;
            if(v == null) return "Null";
            return string.Format("{0} - {1}", vec3Conv.Convert(v.Min), vec3Conv.Convert(v.Max));
        }
    }
}