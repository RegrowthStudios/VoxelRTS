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
    public enum UnitControllerType {
        Action,
        Animation,
        Combat,
        Movement
    }
    public class ReflectedEntityController {
        // The Types Of Controller That This Is
        public UnitControllerType ControllerType {
            get;
            private set;
        }

        // The Constructor Of This Controller
        private ConstructorInfo constructor;

        public ReflectedEntityController(Type t) {
            // Get Constructor
            constructor = t.GetConstructor(new Type[0]);
            if(t.IsSubclassOf(typeof(ACUnitActionController)))
                ControllerType = UnitControllerType.Action;
            else if(t.IsSubclassOf(typeof(ACUnitAnimationController)))
                ControllerType = UnitControllerType.Animation;
            else if(t.IsSubclassOf(typeof(ACUnitCombatController)))
                ControllerType = UnitControllerType.Combat;
            else if(t.IsSubclassOf(typeof(ACUnitMovementController)))
                ControllerType = UnitControllerType.Movement;
            else
                throw new ArgumentException("This Script Is Not Subclassing A Unit Controller");
        }

        public T CreateInstance<T>() where T : ACUnitController {
            return constructor.Invoke(null) as T;
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

            // Loop Through All Visible Types
            CompiledEntityControllers cec = new CompiledEntityControllers();
            Assembly a = cr.CompiledAssembly;
            Type[] types = a.GetExportedTypes();
            foreach(Type t in types) {
                // We Don't Want Abstract Classes Or Interfaces
                if(t.IsAbstract || t.IsInterface) continue;

                // Check For The Superclass Of ACUnitController
                if(t.IsSubclassOf(typeof(ACUnitController)))
                    cec.Controllers.Add(t.FullName, new ReflectedEntityController(t));
            }
            return cec;
        }

        public static CompiledEntityControllers CompileText(string text, string[] references, out string error) {
            // No Error Default
            error = null;

            // Compile
            CompilerParameters compParams = new CompilerParameters(references, null, false);
            compParams.CompilerOptions = "/optimize";
            compParams.GenerateExecutable = false;
            compParams.GenerateInMemory = true;
            compParams.TreatWarningsAsErrors = false;
            CompilerResults cr = compiler.CompileAssemblyFromSource(compParams, text);

            // Check For Errors
            if(cr.Errors.Count > 0) {
                error = "";
                foreach(var e in cr.Errors)
                    error += e + "\n";
                return null;
            }

            // Loop Through All Visible Types
            CompiledEntityControllers cec = new CompiledEntityControllers();
            Assembly a = cr.CompiledAssembly;
            Type[] types = a.GetExportedTypes();
            foreach(Type t in types) {
                // We Don't Want Abstract Classes Or Interfaces
                if(t.IsAbstract || t.IsInterface) continue;

                // Check For The Superclass Of ACUnitController
                if(t.IsSubclassOf(typeof(ACUnitController)))
                    cec.Controllers.Add(t.FullName, new ReflectedEntityController(t));
            }
            return cec;
        }
    }
}