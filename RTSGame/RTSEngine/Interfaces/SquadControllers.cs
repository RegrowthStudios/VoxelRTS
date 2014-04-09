using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Controllers;
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

        // Need This Reference For Sending Pathfinding Queries
        private GameplayController GameController;
        public void Init(GameplayController gpc) {
            GameController = gpc;
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

        public void InitPathFlow(CollisionGrid cg) {
            PathFlow = new FlowGrid(cg, false);
        }

        // Update The Path Flow With A List Of Waypoints
        public void UpdatePathFlow(List<Vector2> waypoints) {
            if(Waypoints == null || Waypoints.Count == 0) return;
            Vector2 goal = Waypoints[0];
            for(int i = 0; i < PathFlow.numCells.X; i++) {
                for(int j = 0; j < PathFlow.numCells.Y; j++) {
                    float minDistSq = float.MaxValue;
                    Vector2 seg = Vector2.Zero;
                    for(int w = Waypoints.Count - 2; w >= 0; w--) {
                        Vector2 a = Waypoints[w + 1];
                        Vector2 b = Waypoints[w];
                        seg = b - a;
                        float d = DistSq(a, b, PathFlow.MakeContinuous(i, j));
                        //    PathFlow.FlowVectors[i, j] += PathForce(seg, d);
                        if(d < minDistSq) minDistSq = d;
                    }
                    if(minDistSq < 2 * PathFlow.cellSize)
                        PathFlow.FlowVectors[i, j] += PathForce(seg, minDistSq);
                    // Add A Special Attractive Charge At The Last Waypoint
                    // TODO: Figure Out Why Everything Looks Better When Only The Goal Is Turned On
                    PathFlow.FlowVectors[i, j] += FlowGrid.pForce * FlowGrid.UnitForce(PathFlow.MakeContinuous(i, j), goal);
                }
            }
        }

        // Calculate The Path Force At A Certain Distance Away From The Path
        private Vector2 PathForce(Vector2 pathSegment, float dist) {
            Vector2 force = Vector2.Zero;
            if(dist == 0f) {
                force = new Vector2(1, 0);
            }
            else {
                force = new Vector2(1f / dist, dist);
                //force.Normalize();
            }
            float a = (float)Math.Atan2(-pathSegment.Y, pathSegment.X);
            var mr = Matrix.CreateRotationZ(a);
            force = Vector2.TransformNormal(force, mr);
            return FlowGrid.pForce * force;
        }

        // Calculate The Distance Squared Between A Line Segment (A,B) And A Point P
        private float DistSq(Vector2 a, Vector2 b, Vector2 p) {
            Vector2 seg = b - a;
            if(seg.X == 0 && seg.Y == 0) return (p - a).LengthSquared();
            float denom = seg.LengthSquared();
            float t = Vector2.Dot(p - a, seg) / denom;
            if(t < 0) return (p - a).LengthSquared();
            else if(t > 1) return (p - b).LengthSquared();
            Vector2 proj = a + t * seg;
            return (proj - p).LengthSquared();
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