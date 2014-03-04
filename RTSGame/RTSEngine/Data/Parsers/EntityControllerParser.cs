using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using System.CodeDom.Compiler;
using Microsoft.CSharp;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Parsers {
    #region Compiled Arguments
    public class ReflectedEntityController {
        // The Types Of Controller That This Is
        public EntityControllerType ControllerType {
            get;
            private set;
        }

        // The Constructor Of This Controller
        private ConstructorInfo constructor;

        public ReflectedEntityController(Type t) {
            // Get Constructor
            constructor = t.GetConstructor(new Type[0]);

            // Find Controller Types
            ControllerType = EntityControllerType.None;
            Type[] interfaces = t.GetInterfaces();
            foreach(var ti in interfaces) {
                if(ti.IsEquivalentTo(typeof(IActionController)))
                    ControllerType |= EntityControllerType.Action;
                else if(ti.IsEquivalentTo(typeof(IMovementController)))
                    ControllerType |= EntityControllerType.Movement;
                else if(ti.IsEquivalentTo(typeof(ITargettingController)))
                    ControllerType |= EntityControllerType.Targetting;
                else if(ti.IsEquivalentTo(typeof(ICombatController)))
                    ControllerType |= EntityControllerType.Combat;
            }
        }

        public IEntityController CreateInstance() {
            return constructor.Invoke(null) as IEntityController;
        }
    }
    public class CompiledEntityControllers {
        // The Dictionary Of Controllers
        public readonly Dictionary<string, ReflectedEntityController> Controllers;

        public CompiledEntityControllers() {
            Controllers = new Dictionary<string, ReflectedEntityController>();
        }
    }
    #endregion

    public static class EntityControllerParser {
        // Create The Compiler
        private static readonly CSharpCodeProvider compiler = new CSharpCodeProvider();

        public static CompiledEntityControllers Compile(string file, string[] references, out string error) {
            // No Error Default
            error = null;

            // Compile
            CompilerParameters compParams = new CompilerParameters(references, null, false);
            compParams.CompilerOptions = "/optimize";
            compParams.GenerateExecutable = false;
            compParams.GenerateInMemory = true;
            compParams.TreatWarningsAsErrors = false;
            CompilerResults cr = compiler.CompileAssemblyFromFile(compParams, file);

            // Check For Errors
            if(cr.Errors.Count > 0) {
                error = "";
                foreach(var e in cr.Errors)
                    error += e + "\n";
                return null;
            }
            CompiledEntityControllers cec = new CompiledEntityControllers();
            Assembly a = cr.CompiledAssembly;
            Type[] types = a.GetExportedTypes();
            foreach(Type t in types) {
                // We Don't Want Abstract Classes Or Interfaces
                if(t.IsAbstract || t.IsInterface) continue;

                Type[] interfaces = t.GetInterfaces();
                foreach(Type ti in interfaces) {
                    if(ti.Equals(typeof(IEntityController))) {
                        cec.Controllers.Add(t.FullName, new ReflectedEntityController(t));
                    }
                }
            }
            return cec;
        }
    }
}