using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data.Team;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data {
    public enum GameEventType {
        Select,
        SetWaypoint,
        SetTarget
    }

    public class GameInputEvent {
        public GameEventType Action {
            get;
            private set;
        }

        public RTSTeam Team {
            get;
            private set;
        }

        public GameInputEvent(GameEventType a, RTSTeam t) {
            Action = a;
            Team = t;
        }
    }

    public class SelectEvent : GameInputEvent {
        public List<IEntity> Selected {
            get;
            private set;
        }

        public SelectEvent(List<IEntity> s, RTSTeam t)
            : base(GameEventType.Select, t) {
            Selected = s;
        }
    }

    public class SetWayPointEvent : GameInputEvent {
        public Vector2 Waypoint {
            get;
            private set;
        }

        public SetWayPointEvent(Vector2 w, RTSTeam t)
            : base(GameEventType.SetWaypoint, t) {
            Waypoint = w;
        }
    }

    public class SetTargetEvent : GameInputEvent {
        public IDestructibleEntity Target {
            get;
            private set;
        }

        public SetTargetEvent(IDestructibleEntity target, RTSTeam t)
            : base(GameEventType.SetTarget, t) {
            Target = target;
        }
    }
}
