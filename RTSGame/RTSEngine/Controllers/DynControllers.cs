using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Reflection;
using RTSEngine.Interfaces;

namespace RTSEngine.Controllers {
    #region Unit
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
    #endregion

    #region Squad
    public enum SquadControllerType {
        Action,
        Movement,
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
            else if(t.IsSubclassOf(typeof(ACSquadMovementController)))
                ControllerType = SquadControllerType.Movement;
            else if(t.IsSubclassOf(typeof(ACSquadTargettingController)))
                ControllerType = SquadControllerType.Targetting;
            else
                throw new ArgumentException("This Script Is Not Subclassing A Squad Controller");
        }

        public T CreateInstance<T>() where T : ACSquadController {
            return constructor.Invoke(null) as T;
        }
    }
    #endregion

    #region Building
    public enum BuildingControllerType {
        Action
    }
    public class ReflectedBuildingController {
        // The Types Of Controller That This Is
        public BuildingControllerType ControllerType {
            get;
            private set;
        }

        // The Constructor Of This Controller
        private ConstructorInfo constructor;

        public ReflectedBuildingController(Type t) {
            // Get Constructor
            constructor = t.GetConstructor(new Type[0]);
            if(t.IsSubclassOf(typeof(ACBuildingActionController)))
                ControllerType = BuildingControllerType.Action;
            else
                throw new ArgumentException("This Script Is Not Subclassing A Building Controller");
        }

        public T CreateInstance<T>() where T : ACBuildingController {
            return constructor.Invoke(null) as T;
        }
    }
    #endregion
}