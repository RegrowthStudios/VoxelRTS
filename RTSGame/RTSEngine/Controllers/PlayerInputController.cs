using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Data.Team;

namespace RTSEngine.Controllers {
    public class PlayerInputController : InputController {

        private ConcurrentQueue<GameInputEvent> eventQueue;
        public RTSTeam Team { get; private set;  }

        public PlayerInputController(GameState g)
            : base(g) {
            eventQueue = new ConcurrentQueue<GameInputEvent>();
            
        }

        //Adds Event To Concurrent Queue
        public void AddEvent(GameInputEvent e) {
            eventQueue.Enqueue(e);
        }

        //Appends All Events In Concurrent Queue To Given List
        void AppendEvents(LinkedList<GameInputEvent> l) {
            GameInputEvent e;
            while(eventQueue.TryDequeue(out e)) {
                l.AddLast(e);
            }
        }
    }
}
