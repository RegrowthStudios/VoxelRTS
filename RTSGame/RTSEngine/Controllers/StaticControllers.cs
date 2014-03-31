using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using RTSEngine.Interfaces;

namespace RTSEngine.Controllers
{
    public enum BuildingControllerType {
        Action,
        Combat
    }

    public class ReflectedBuildingController {
        // The Types Of Controller This Is
        public BuildingControllerType ControllerType {
            get;
            private set;
        }

        // The Constructor Of This Controller
        private ConstructorInfo constructor;

        public ReflectedBuildingController(Type t) {
            // Get Constructor
            constructor = t.GetConstructor(new Type[0]);
            if (t.IsSubclassOf(typeof(ACUnitActionController)))
                ControllerType = BuildingControllerType.Action;
            else if (t.IsSubclassOf(typeof(ACUnitCombatController)))
                ControllerType = BuildingControllerType.Combat;
            else
                throw new ArgumentException("This Script Is Not Subclassing A Unit Controller");
        }

        public T CreateInstance<T>() where T : ACUnitController {
            return constructor.Invoke(null) as T;
        }
    }
}
