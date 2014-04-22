using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Interfaces;
using System.Collections.Concurrent;
using RTSEngine.Data.Team;
using System.IO;

namespace RTSEngine.Interfaces {
    // Types Of Teams
    public static class RTSInputType {
        public const int None = 0;
        public const int Player = 1;
        public const int AI = 2;
        public const int Environment = 3;
    }

    public abstract class ACInputController : ACScript, IDisposable {
        // Input Type Metadata
        public int Type {
            get;
            set;
        }

        // Stores The Team's Events
        private ConcurrentQueue<GameInputEvent> eventQueue;

        // Currently Selected Entities
        public readonly List<IEntity> selected;
        public event Action<ACInputController, List<IEntity>> OnNewSelection;

        // The RTSTeam of the InputController
        public int TeamIndex {
            get;
            set;
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
        public ACInputController() {
            eventQueue = new ConcurrentQueue<GameInputEvent>();
            selected = new List<IEntity>();

            Type = RTSInputType.None;
            TeamIndex = -1;
            GameState = null;
        }
        public abstract void Dispose();

        // Called After Controller Is Created
        public virtual void Init(GameState s, int t) {
            GameState = s;
            TeamIndex = t;
        }

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
            // Remove Old Units
            if(!append && selected != null && selected.Count > 0) {
                for(int i = 0; i < selected.Count; i++) {
                    selected[i].OnDestruction -= OnEntityDestruction;
                }
                selected.Clear();
            }

            // Add New Units
            if(s != null && s.Count > 0) {
                selected.AddRange(s);
                for(int i = 0; i < s.Count; i++) {
                    s[i].OnDestruction += OnEntityDestruction;
                }
            }

            // Raise Event
            if(OnNewSelection != null) {
                OnNewSelection(this, selected);
            }
        }
        public void OnEntityDestruction(IEntity e) {
            selected.Remove(e);
            e.OnDestruction -= OnEntityDestruction;
            if(OnNewSelection != null)
                OnNewSelection(this, selected);
        }

        public abstract void Serialize(BinaryWriter s);
        public abstract void Deserialize(BinaryReader s);
    }
}