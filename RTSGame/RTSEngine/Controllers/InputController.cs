using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Interfaces;
using System.Collections.Concurrent;
using RTSEngine.Data.Team;
using System.IO;

namespace RTSEngine.Controllers {
    // Types Of Teams
    public enum InputType {
        None,
        Player,
        AI,
        Environment
    }

    public abstract class InputController : IDisposable {
        public InputType Type {
            get;
            private set;
        }

        //Stores The Team's Events
        private ConcurrentQueue<GameInputEvent> eventQueue;

        //Currently Selected Entities
        public readonly List<IEntity> selected;
        public event Action<InputController, List<IEntity>> OnNewSelection;

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
        public InputController(GameState g, int t, InputType it) {
            Type = it;
            GameState = g;
            TeamIndex = t;
            eventQueue = new ConcurrentQueue<GameInputEvent>();
            selected = new List<IEntity>();
        }
        public abstract void Dispose();

        // Begins The Controller
        public abstract void Begin();

        // Adds Event To Concurrent Queue
        public void AddEvent(GameInputEvent e) {
            eventQueue.Enqueue(e);
        }

        // Appends All Events In Concurrent Queue To Given List
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

        // Perform Correct Logic For Selection
        public void Select(List<IEntity> s, bool append = false) {
            if(!append) selected.Clear();
            if(s != null) selected.AddRange(s);
            if(OnNewSelection != null) {
                OnNewSelection(this, selected);
            }
        }

        public abstract void Serialize(BinaryWriter s);
        public abstract void Deserialize(BinaryReader s);
    }
}