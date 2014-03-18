using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Interfaces;
using System.Collections.Concurrent;
using RTSEngine.Data.Team;

namespace RTSEngine.Controllers {
    // Types Of Teams
    public enum InputType {
        Player,
        AI,
        Environment
    }

    public class InputController : IInputController {

        //Stores The Team's Events
        private ConcurrentQueue<GameInputEvent> eventQueue;

        //Currently Selected Entities
        public List<IEntity> Selected {
            get;
            set;
        }

        //The RTSTeam of the InputController
        public RTSTeam Team { get; private set; }

        //Stores GameState
        public GameState GameState { get; private set; }

        //Creates An InputController For The Given RTSTeam
        public InputController(GameState g, RTSTeam t) {
            GameState = g;
            Team = t;
            eventQueue = new ConcurrentQueue<GameInputEvent>();
        }

        //Adds Event To Concurrent Queue
        public void AddEvent(GameInputEvent e) {
            eventQueue.Enqueue(e);
        }

        //Appends All Events In Concurrent Queue To Given List
        public void AppendEvents(LinkedList<GameInputEvent> l) {
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