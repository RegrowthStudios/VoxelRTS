using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Data.Team;

namespace RTSEngine.Interfaces {
    public interface IInputController {

        // Currently Selected Entities
        List<IEntity> Selected { get; set; }

        // Input Controllers Operate On A Team For Reference
        RTSTeam Team { get; }

        // Adding And Removing Events Should Be Concurrent
        void AddEvent(GameInputEvent e);
        void AppendEvents(LinkedList<GameInputEvent> l);
    }
}
