﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Data;
using RTSEngine.Data.Team;

namespace RTSEngine.Interfaces {
    // Base Controller Functionality
    public abstract class ACSquadController {
        // The Squad That Is Being Controlled
        protected RTSSquad squad;
        public RTSSquad Squad {
            get { return squad; }
        }

        // Will Set Once And Then Fail On Later Occurrences
        public virtual void SetSquad(RTSSquad s) {
            if(s == null) return;
            if(squad != null)
                throw new ArgumentException("Cannot Rebind This Controller To Another Squad");
            squad = s;
            return;
        }
        public T SetSquad<T>(RTSSquad s) where T : ACSquadController {
            SetSquad(s);
            return this as T;
        }
    }

    // A Super Controller Called By The Gameplay Controller
    public abstract class ACSquadActionController : ACSquadController {
        // Scripted Super-Controller Logic
        public abstract void DecideAction(GameState g, float dt);
        public abstract void ApplyAction(GameState g, float dt);
    }

    // The Movement Controller That Dictates The General Movement Behavior Of Units In The Squad
    public abstract class ACSquadMovementController : ACSquadController {
        // Waypoints That Units In This Squad Will Generally Follow
        private List<Vector2> waypoints = new List<Vector2>();
        public List<Vector2> Waypoints {
            get { return waypoints; }
            set { waypoints = value; }
        }

        // The Flows Due To This Squad Controller's Current Waypoints
        public FlowGrid PathFlow {
            get;
            private set;
        }

        // Update The Path Flow With This Controller's Waypoints
        public void InitPathFlow(CollisionGrid cg) {
            PathFlow = new FlowGrid(cg, false);
            if(Waypoints == null || Waypoints.Count == 0) return;
            for(int i = 0; i < PathFlow.numCells.X; i++) {
                for(int j = 0; j < PathFlow.numCells.Y; j++) {
                    for(int w = 1; w < Waypoints.Count; w++) {
                        Vector2 a = Waypoints[w - 1];
                        Vector2 pathSegment = Waypoints[w] - a;
                        float d = Dist(a, pathSegment, PathFlow.MakeContinuous(i, j));
                        PathFlow.FlowVectors[i, j] = PathForce(d);
                    }
                }
            }
        }

        // Calculate The Path Force At A Certain Distance Away From The Path
        private Vector2 PathForce(float dist) {
            return FlowGrid.pForce * new Vector2(1f/dist, dist);
        }

        // Calculate The Distance Between A Line Starting At A And A Point P
        private float Dist(Vector2 a, Vector2 line, Vector2 p) {
            float mag = line.Length();
            Vector2 n = mag > 0 ? line / mag : Vector2.Zero;
            return ((a - p) - ((a - p) * n) * n).Length();
        }

        // The Index Of The Current Waypoint Each Unit In This Squad Is Supposed To Head Toward
        // Key: UUID; Value: CurrentWaypointIndex
        private Dictionary<int, int> currentWaypointIndices = new Dictionary<int, int>();
        public Dictionary<int, int> CurrentWaypointIndices {
            get { return currentWaypointIndices; }
            set { currentWaypointIndices = value; }
        }

        protected const int historySize = 20;
        // The Last Few Locations Each Unit Has Been To
        private Dictionary<int, Queue<Vector2>> unitHistory = new Dictionary<int, Queue<Vector2>>();
        public Dictionary<int, Queue<Vector2>> UnitHistory {
            get { return unitHistory; }
            set { unitHistory = value; }
        }

        public void AddToHistory(RTSUnit unit, Vector2 location) {
            if(UnitHistory.ContainsKey(unit.UUID)) {
                if(UnitHistory[unit.UUID].Count >= historySize)
                    UnitHistory[unit.UUID].Dequeue();
                UnitHistory[unit.UUID].Enqueue(location);
            }
        }

        // The Net Force On Each Unit In This Squad
        // Key: UUID; Value: Net Force
        private Dictionary<int, Vector2> netForces = new Dictionary<int, Vector2>();
        public Dictionary<int,Vector2> NetForces {
            get { return netForces; }
            set { netForces = value; }
        }

        // Does The Provided Index Point To A Valid Squad Waypoint?
        public bool IsValid(int idx) {
            return Waypoints != null && idx >= 0 && idx < Waypoints.Count;
        }
        
        // Scripted Logic For Movement
        public abstract void DecideMoves(GameState g, float dt);
        public abstract void ApplyMoves(GameState g, float dt);
    }

    // Controls The Targetting That A Squad Performs
    public abstract class ACSquadTargetingController : ACSquadController {
        // A Squad Target
        protected RTSSquad targetSquad;

        // A Unit Target
        protected RTSUnit targetUnit;
        public RTSUnit Target {
            get { return targetUnit; }
            set { 
                targetUnit = value;
                targetSquad = targetUnit != null ? targetUnit.Squad : null;
            }
        }

        // Find And Setting A Target For This Squad
        public abstract void DecideTarget(GameState g, float dt);
        public abstract void ApplyTarget(GameState g, float dt);
    }
}