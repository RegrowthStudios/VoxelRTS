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
        SetTarget,
        SpawnUnit,
        SpawnBuilding,
        Impact,
        Capital,
        SetOrders
    }

    public class GameInputEvent {
        public GameEventType Action {
            get;
            private set;
        }

        public int Team {
            get;
            private set;
        }

        public GameInputEvent(GameEventType a, int t) {
            Action = a;
            Team = t;
        }
    }

    public class SelectEvent : GameInputEvent {
        public List<IEntity> Selected {
            get;
            private set;
        }
        public bool Append {
            get;
            private set;
        }

        public SelectEvent(int t, List<IEntity> s, bool append = false)
            : base(GameEventType.Select, t) {
            Selected = s;
            Append = append;
        }
    }

    public class SetWayPointEvent : GameInputEvent {
        public Vector2 Waypoint {
            get;
            private set;
        }

        public SetWayPointEvent(int t, Vector2 w)
            : base(GameEventType.SetWaypoint, t) {
            Waypoint = w;
        }
    }

    public class SetTargetEvent : GameInputEvent {
        public IEntity Target {
            get;
            private set;
        }

        public SetTargetEvent(int t, IEntity target)
            : base(GameEventType.SetTarget, t) {
            Target = target;
        }
    }

    public class SpawnUnitEvent : GameInputEvent {
        public int Type {
            get;
            private set;
        }

        public Vector2 Position {
            get;
            private set;
        }

        public SpawnUnitEvent(int t, int type, Vector2 pos)
            : base(GameEventType.SpawnUnit, t) {
            Type = type;
            Position = pos;
        }

    }

    public class SpawnBuildingEvent : GameInputEvent {
        public int Type {
            get;
            private set;
        }

        public Point GridPosition {
            get;
            private set;
        }
        public bool InstantBuild {
            get;
            private set;
        }

        public SpawnBuildingEvent(int t, int type, Point gPos, bool ib = false)
            : base(GameEventType.SpawnBuilding, t) {
            Type = type;
            GridPosition = gPos;
            InstantBuild = ib;
        }
    }

    public class ImpactEvent : GameInputEvent {
        public Vector2 Position {
            get;
            private set;
        }
        public int ChangeAmount {
            get;
            private set;
        }

        public ImpactEvent(int t, Vector2 p, int amount)
            : base(GameEventType.Impact, t) {
            Position = p;
            ChangeAmount = amount;
        }
    }

    public class CapitalEvent : GameInputEvent {
        public int ChangeAmount {
            get;
            private set;
        }

        public CapitalEvent(int t, int amount)
            : base(GameEventType.Capital, t) {
            ChangeAmount = amount;
        }
    }

    public class SetOrdersEvent : GameInputEvent {
        public int UnitID {
            get;
            private set;
        }

        public int Orders {
            get;
            private set;
        }

        public int Index {
            get;
            private set;
        }

        public SetOrdersEvent(int t, int id, int o, int i)
            : base(GameEventType.SetOrders, t) {
            UnitID = id;
            Orders = o;
            Index = i;
        }
    }
}