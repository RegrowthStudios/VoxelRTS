using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data.Team;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data {
    public class GameInputEvent {
        public enum Action {
            Select,
            SetWaypoint,
            SetTarget,
        }
        public Action action { get; protected set; }
    }

    public class SelectEvent : GameInputEvent {
        public RTSSquad Selected { get; private set; }   
        public SelectEvent(RTSSquad s){
            Selected = s;
            action = Action.Select;
        }
    }

    public class SetWayPoint : GameInputEvent {
        public Vector2 waypoint { get; private set; }
        public SetWayPoint(Vector2 w) {
            waypoint = w;
            action = Action.SetWaypoint;
        }
    }

    public class SetTargetEvent : GameInputEvent {
        public IDestructibleEntity Target { get; private set; }
        public SetTargetEvent(IDestructibleEntity t) {
            Target = t;
            action = Action.SetTarget;
        }
    }

}
