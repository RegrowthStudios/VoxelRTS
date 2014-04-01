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
    public struct DynCompiledResults {
        public Dictionary<string, ReflectedUnitController> UnitControllers;
        public Dictionary<string, ReflectedSquadController> SquadControllers;
    }

    public static class DynControllerParser {
        // Create The Compiler
        private static readonly CSharpCodeProvider compiler = new CSharpCodeProvider();

        public static DynCompiledResults Compile(string[] files, string[] references, out string error) {
            // No Error Default
            error = null;

            // Compile
            CompilerParameters compParams = new CompilerParameters(references, null, false);
            compParams.CompilerOptions = "/optimize";
            compParams.GenerateExecutable = false;
            compParams.GenerateInMemory = true;
            compParams.TreatWarningsAsErrors = false;
            CompilerResults cr = compiler.CompileAssemblyFromFile(compParams, files);

            // Check For Errors
            if(cr.Errors.Count > 0) {
                error = "";
                foreach(var e in cr.Errors)
                    error += e + "\n";
                return new DynCompiledResults();
            }

            // Loop Through All Visible Types
            DynCompiledResults res = new DynCompiledResults();
            res.UnitControllers = new Dictionary<string, ReflectedUnitController>();
            res.SquadControllers = new Dictionary<string, ReflectedSquadController>();
            Assembly a = cr.CompiledAssembly;
            Type[] types = a.GetExportedTypes();
            foreach(Type t in types) {
                // We Don't Want Abstract Classes Or Interfaces
                if(t.IsAbstract || t.IsInterface) continue;

                // Check For The Superclass
                if(t.IsSubclassOf(typeof(ACUnitController)))
                    res.UnitControllers.Add(t.FullName, new ReflectedUnitController(t));
                else if(t.IsSubclassOf(typeof(ACSquadController)))
                    res.SquadControllers.Add(t.FullName, new ReflectedSquadController(t));
            }
            return res;
        }
    }
}