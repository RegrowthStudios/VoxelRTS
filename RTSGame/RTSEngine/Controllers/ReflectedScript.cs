using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RTSEngine.Interfaces;

namespace RTSEngine.Controllers {
    public enum ScriptType {
        Unit,
        Squad,
        Building,
        Button,
        GameType,
        Input
    }
    public enum ControllerType {
        Action,
        Animation,
        Combat,
        Movement,
        Targetting
    }

    public class ReflectedScript {
        public static string GetKey(ACScript s) {
            return s.GetType().FullName;
        }

        public readonly string TypeName;

        // The Types Of Controller That This Is
        public ScriptType ScriptType {
            get;
            private set;
        }
        public ControllerType ControllerType {
            get;
            private set;
        }

        // The Constructor Of This Controller
        private ConstructorInfo constructor;

        public ReflectedScript(Type t) {
            TypeName = t.FullName;

            // Get Constructor
            constructor = t.GetConstructor(new Type[0]);

            // Conditional Information
            if(t.IsSubclassOf(typeof(ACUnitController))) {
                ScriptType = ScriptType.Unit;
                if(t.IsSubclassOf(typeof(ACUnitActionController)))
                    ControllerType = ControllerType.Action;
                else if(t.IsSubclassOf(typeof(ACUnitAnimationController)))
                    ControllerType = ControllerType.Animation;
                else if(t.IsSubclassOf(typeof(ACUnitCombatController)))
                    ControllerType = ControllerType.Combat;
                else if(t.IsSubclassOf(typeof(ACUnitMovementController)))
                    ControllerType = ControllerType.Movement;
                else
                    throw new ArgumentException("This Script Is Not Subclassing A Unit Controller");
            }
            else if(t.IsSubclassOf(typeof(ACSquadController))) {
                ScriptType = ScriptType.Squad;
                if(t.IsSubclassOf(typeof(ACSquadActionController)))
                    ControllerType = ControllerType.Action;
                else if(t.IsSubclassOf(typeof(ACSquadMovementController)))
                    ControllerType = ControllerType.Movement;
                else if(t.IsSubclassOf(typeof(ACSquadTargetingController)))
                    ControllerType = ControllerType.Targetting;
                else
                    throw new ArgumentException("This Script Is Not Subclassing A Squad Controller");
            }
            else if(t.IsSubclassOf(typeof(ACBuildingController))) {
                ScriptType = ScriptType.Building;
                if(t.IsSubclassOf(typeof(ACBuildingActionController)))
                    ControllerType = ControllerType.Action;
                else
                    throw new ArgumentException("This Script Is Not Subclassing A Building Controller");
            }
            else if(t.IsSubclassOf(typeof(ACRTSButton))) {
                ScriptType = ScriptType.Button;
            }
            else if(t.IsSubclassOf(typeof(ACGameTypeController))) {
                ScriptType = ScriptType.Button;
            }
            else if(t.IsSubclassOf(typeof(ACInputController))) {
                ScriptType = ScriptType.Input;
            }
            else {
                throw new ArgumentException("This Script Is Not A Script...");
            }
        }

        public T CreateInstance<T>() where T : ACScript {
            return constructor.Invoke(null) as T;
        }
    }
}