using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTS.Mech.Building;

namespace RTS.UIInput.BuildingInput
{
    public enum EventType
    {
        None = -1,
        Production = 0,
        Research = 1
    }

    public abstract class BuildingInput
    {
        protected RTS.Mech.Building.Action action;
        public abstract void Apply(GameState g);
    }

    public class ProductionInput : BuildingInput
    {
        public override void Apply(GameState g)
        {
            throw new NotImplementedException();
        }

        public void Apply(GameState g, int unitType)
        {
            action.eventQueue.Enqueue(EventType.Production);
            action.unitQueue.Enqueue(unitType);
            action.Building.Team.Capital -= action.Building.Team.Race.Units[unitType].CapitalCost;
        }
    }

    public class ResearchInput : BuildingInput
    {
        public override void Apply(GameState g)
        {
            action.eventQueue.Enqueue(EventType.Research);
        }
    }
}
