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
    public class ReflectedUnitController {
        // The Types Of Controller That This Is
        public UnitControllerType ControllerType {
            get;
            private set;
        }

        // The Constructor Of This Controller
        private ConstructorInfo constructor;

        public ReflectedUnitController(Type t) {
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

    public enum SquadControllerType {
        Action,
        Targetting
    }
    public class ReflectedSquadController {
        // The Types Of Controller That This Is
        public SquadControllerType ControllerType {
            get;
            private set;
        }

        // The Constructor Of This Controller
        private ConstructorInfo constructor;

        public ReflectedSquadController(Type t) {
            // Get Constructor
            constructor = t.GetConstructor(new Type[0]);
            if(t.IsSubclassOf(typeof(ACSquadActionController)))
                ControllerType = SquadControllerType.Action;
            else if(t.IsSubclassOf(typeof(ACSquadTargettingController)))
                ControllerType = SquadControllerType.Targetting;
            else
                throw new ArgumentException("This Script Is Not Subclassing A Squad Controller");
        }

        public T CreateInstance<T>() where T : ACSquadController {
            return constructor.Invoke(null) as T;
        }
    }

    public struct DynCompiledResults {
        public Dictionary<string, ReflectedUnitController> UnitControllers;
        public Dictionary<string, ReflectedSquadController> SquadControllers;
    }
    #endregion

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