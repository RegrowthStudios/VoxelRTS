using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace System.IO {
    public static class PathHelper {
        public static string GetRelativePath(string path) {
            Uri uriExec = new Uri(Path.GetDirectoryName(System.Reflection.Assembly.GetEntryAssembly().Location));
            Uri uriPath = new Uri(path);
            string p = uriExec.MakeRelativeUri(uriPath).ToString();
            int i = p.IndexOf('/');
            if(i < 0)
                return p;
            else
                return p.Substring(i + 1);
        }
    }
}