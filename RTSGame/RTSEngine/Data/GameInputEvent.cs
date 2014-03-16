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

        public GameInputEvent(GameEventType a) {
            Action = a;
        }
    }

    public class SelectEvent : GameInputEvent {
        public List<IEntity> Selected {
            get;
            private set;
        }

        public SelectEvent(List<IEntity> s)
            : base(GameEventType.Select) {
            Selected = s;
        }
    }

    public class SetWayPoint : GameInputEvent {
        public Vector2 Waypoint {
            get;
            private set;
        }

        public SetWayPoint(Vector2 w)
            : base(GameEventType.SetWaypoint) {
            Waypoint = w;
        }
    }

    public class SetTargetEvent : GameInputEvent {
        public IDestructibleEntity Target {
            get;
            private set;
        }

        public SetTargetEvent(IDestructibleEntity t)
            : base(GameEventType.SetTarget) {
            Target = t;
        }
    }
}