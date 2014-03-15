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
        public GameState GameState { get; private set; }

        public PlayerInputController(GameState g, RTSTeam t)
            : base(g) {
            eventQueue = new ConcurrentQueue<GameInputEvent>();
            Team = t;
            GameState = g;
        }

        //Adds Event To Concurrent Queue
        public void AddEvent(GameInputEvent e) {
            eventQueue.Enqueue(e);
        }

        //Appends All Events In Concurrent Queue To Given List
        void AppendEvents(LinkedList<GameInputEvent> l) {
            int count = eventQueue.Count;
            GameInputEvent e;

            while(count > 0) {
                if(eventQueue.TryDequeue(out e)) {
                    l.AddLast(e);
                }
                count--;
            }
        }
    }
}
