using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Text.RegularExpressions;

namespace RTSEngine.Data.Parsers {
    public class ZXPDatum {
        public readonly string Key;
        private FieldInfo field;
        private PropertyInfo prop;
        public readonly IZXPConverter Converter;

        public bool IsProperty {
            get { return prop != null; }
        }
        public bool CanWrite {
            get { return IsProperty ? prop.CanWrite != null : true; }
        }
        public bool CanRead {
            get { return IsProperty ? prop.CanRead != null : true; }
        }
        public Type Type {
            get { return IsProperty ? prop.PropertyType : field.FieldType; }
        }

        public ZXPDatum(string key, FieldInfo fi) {
            Key = key;
            field = fi;
            prop = null;
            if(!ZXParser.GetConverter(Type, out Converter))
                Converter = null;
        }
        public ZXPDatum(string key, PropertyInfo pi) {
            Key = key;
            field = null;
            prop = pi;
            if(!ZXParser.GetConverter(Type, out Converter))
                Converter = null;
        }

        public void SetValue(object o, object v) {
            if(IsProperty) {
                if(prop.CanWrite) prop.SetValue(o, v, null);
            }
            else field.SetValue(o, v);
        }
        public object GetValue(object o) {
            if(IsProperty) {
                if(prop.CanRead) return prop.GetValue(o, null);
                else return null;
            }
            else return field.GetValue(o);
        }
    }
    public class ZXPFunc {
        public readonly string Key;
        public readonly MethodInfo Method;

        public ZXPFunc(string key, MethodInfo mi) {
            Key = key;
            Method = mi;
        }
    }

    public class ZXPProxy {
        private static readonly Type ATTR_TYPE = typeof(ZXParseAttribute);
        public const BindingFlags SEARCH_TYPE = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;
        private static readonly Dictionary<string, ZXPProxy> proxies = new Dictionary<string, ZXPProxy>();

        public static ZXPProxy GetProxy(Type t) {
            // Try To Get The Proxy
            ZXPProxy zpp;
            if(proxies.TryGetValue(t.FullName, out zpp)) return zpp;

            // Create A New Proxy
            zpp = new ZXPProxy(t);
            proxies.Add(t.FullName, zpp);
            zpp.CreateProxy();
            return zpp;
        }
        private static ZXParseAttribute GetAttribute(FieldInfo fi) {
            object[] attr = fi.GetCustomAttributes(ATTR_TYPE, true);
            if(attr == null || attr.Length < 1) return null;
            return attr[0] as ZXParseAttribute;
        }
        private static ZXParseAttribute GetAttribute(PropertyInfo pi) {
            object[] attr = pi.GetCustomAttributes(ATTR_TYPE, true);
            if(attr == null || attr.Length < 1) return null;
            return attr[0] as ZXParseAttribute;
        }
        private static ZXParseAttribute GetAttribute(MethodInfo mi) {
            object[] attr = mi.GetCustomAttributes(ATTR_TYPE, true);
            if(attr == null || attr.Length < 1) return null;
            return attr[0] as ZXParseAttribute;
        }

        public Type Type {
            get;
            private set;
        }
        public string Key {
            get { return Type.FullName; }
        }

        public List<ZXPDatum> Data {
            get;
            private set;
        }
        public Dictionary<string, ZXPDatum> DataDict {
            get;
            private set;
        }
        public List<ZXPFunc> Funcs {
            get;
            private set;
        }
        public Dictionary<string, ZXPFunc> FuncsDict {
            get;
            private set;
        }

        private ZXPProxy(Type t) {
            Type = t;
            Data = new List<ZXPDatum>();
            DataDict = new Dictionary<string, ZXPDatum>();
            Funcs = new List<ZXPFunc>();
            FuncsDict = new Dictionary<string, ZXPFunc>();
        }

        private void CreateProxy() {
            CreateProxyFields();
            CreateProxyProps();
            CreateProxyFuncs();
        }
        private void CreateProxyFields() {
            FieldInfo[] fields = Type.GetFields(SEARCH_TYPE);
            for(int i = 0; i < fields.Length; i++) {
                var data = fields[i];
                ZXParseAttribute attr = GetAttribute(data);

                // Add The Field
                if(attr != null) {
                    ZXPDatum v = new ZXPDatum(attr.Key, data);
                    Data.Add(v);
                    DataDict[v.Key] = v;
                }
            }
        }
        private void CreateProxyProps() {
            PropertyInfo[] props = Type.GetProperties(SEARCH_TYPE);
            for(int i = 0; i < props.Length; i++) {
                var data = props[i];
                ZXParseAttribute attr = GetAttribute(data);

                // Add The Property
                if(attr != null) {
                    ZXPDatum v = new ZXPDatum(attr.Key, data);
                    Data.Add(v);
                    DataDict[v.Key] = v;
                }
            }
        }
        private void CreateProxyFuncs() {
            MethodInfo[] methods = Type.GetMethods(SEARCH_TYPE);
            for(int i = 0; i < methods.Length; i++) {
                var method = methods[i];
                ZXParseAttribute attr = GetAttribute(method);

                // Add The Method
                if(attr != null) {
                    ZXPFunc v = new ZXPFunc(attr.Key, method);
                    Funcs.Add(v);
                    FuncsDict[v.Key] = v;
                }
            }
        }
    }
}