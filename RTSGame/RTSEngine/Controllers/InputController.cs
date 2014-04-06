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
        None,
        Player,
        AI,
        Environment
    }

    public abstract class InputController : IDisposable {

        //Stores The Team's Events
        private ConcurrentQueue<GameInputEvent> eventQueue;

        //Currently Selected Entities
        public readonly List<IEntity> selected;

        //The RTSTeam of the InputController
        public int TeamIndex {
            get;
            private set;
        }
        public RTSTeam Team {
            get { return GameState.teams[TeamIndex]; }
        }

        //Stores GameState
        public GameState GameState {
            get;
            private set;
        }

        //Creates An InputController For The Given RTSTeam
        public InputController(GameState g, int t) {
            GameState = g;
            TeamIndex = t;
            eventQueue = new ConcurrentQueue<GameInputEvent>();
            selected = new List<IEntity>();
        }
        public abstract void Dispose();

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