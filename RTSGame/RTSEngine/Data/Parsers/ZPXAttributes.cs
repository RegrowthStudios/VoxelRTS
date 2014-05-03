using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data.Parsers {
    [AttributeUsage(AttributeTargets.All)]
    public class ZXParseAttribute : System.Attribute {
        public string Key {
            get;
            private set;
        }

        public ZXParseAttribute() {
            Key = null;
        }
        public ZXParseAttribute(string key) {
            Key = key;
        }
    }
}