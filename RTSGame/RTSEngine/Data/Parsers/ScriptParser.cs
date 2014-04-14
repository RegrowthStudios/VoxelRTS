using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using RTSEngine.Interfaces;
using RTSEngine.Controllers;

namespace RTSEngine.Data.Parsers {
    public static class ScriptParser {
        // Create The Compiler
        private static readonly CSharpCodeProvider compiler = new CSharpCodeProvider();

        public static Dictionary<string, ReflectedScript> Compile(string[] files, string[] references, out string error) {
            // No Error Default
            error = null;

            // Compile
            CompilerParameters compParams = new CompilerParameters(references, null, false);
            compParams.CompilerOptions = "/optimize";
            compParams.GenerateExecutable = false;
            compParams.GenerateInMemory = true;
            compParams.TreatWarningsAsErrors = false;
#if DEBUG
            compParams.IncludeDebugInformation = true;
#endif
            CompilerResults cr = compiler.CompileAssemblyFromFile(compParams, files);

            // Check For Errors
            if(cr.Errors.Count > 0) {
                error = "";
                foreach(var e in cr.Errors)
                    error += e + "\n";
                return null;
            }

            // Dictionaries
            var res = new Dictionary<string, ReflectedScript>();

            // Loop Through All Visible Types
            Assembly a = cr.CompiledAssembly;
            Type[] types = a.GetExportedTypes();
            foreach(Type t in types) {
                // We Don't Want Abstract Classes Or Interfaces
                if(t.IsAbstract || t.IsInterface) continue;

                // Check For The Superclass
                if(t.IsSubclassOf(typeof(ACScript))) {
                    var rs = new ReflectedScript(t);
                    res.Add(rs.TypeName, rs);
                }
            }
            return res;
        }
    }
}