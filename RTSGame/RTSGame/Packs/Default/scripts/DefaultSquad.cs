using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Algorithms;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using RTSEngine.Interfaces;

namespace RTS.Default.Squad {
    public class Action : ACSquadActionController {
        Targeting tc;
        Movement mc;

        public override void Init(GameState s, GameplayController c) {
            tc = squad.TargetingController as Targeting;
            mc = squad.MovementController as Movement;
        }

        public override void DecideAction(GameState g, float dt) {
            if(tc != null) { 
                tc.DecideTarget(g, dt);
            }
            if(mc != null) {
                var doMove = mc.doMove;
                foreach(var unit in squad.Units) {
                    mc.DecideMove(g, dt, unit);
                    if(doMove.ContainsKey(unit.UUID) && (doMove[unit.UUID])) {
                        unit.State = BehaviorFSM.Walking;
                        // TODO: Change This To Allow A-Move
                        unit.TargetingOrders = BehaviorFSM.TargetNone;
                    }
                    else if(!(unit.State == BehaviorFSM.CombatMelee || unit.State == BehaviorFSM.CombatRanged)) {
                        unit.State = BehaviorFSM.Rest;
                        unit.TargetingOrders = tc.origTargetOrders[unit.UUID];
                    }
                }
            }
        }
        public override void ApplyAction(GameState g, float dt) {
            if(tc != null) {
                tc.ApplyTarget(g, dt);
            }
            if(mc != null) {
                var doMove = mc.doMove;
                foreach(var unit in squad.Units) {
                    if(doMove.ContainsKey(unit.UUID) && (doMove[unit.UUID])) {
                        mc.ApplyMove(g, dt, unit);
                    }
                }
            }
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }

    public class Targeting : ACSquadTargetingController {
        int teamIndex;
        public readonly Dictionary<int, int> origTargetOrders = new Dictionary<int, int>();

        public override void Init(GameState s, GameplayController c) {
            teamIndex = squad.Team.Index;
            for(int i = 0; i < squad.Units.Count; i++) {
                RTSUnit unit = squad.Units[i];
                origTargetOrders[unit.UUID] = unit.TargetingOrders;
            }
        }
        public override void DecideTarget(GameState g, float dt) {
            if(target == null) {
                FindTargetUnit(g);
                if(target == null) // No Units Found
                    FindTargetBuilding(g);
            }
            else {
                bool inFog = g.CGrid.GetFogOfWar(target.GridPosition, teamIndex) != FogOfWar.Active;
                if(!target.IsAlive || inFog)
                    target = null;
            }
        }
        private void FindTargetUnit(GameState g) {
            target = null;
            float minDist = float.MaxValue;
            for(int ti = 0; ti < g.activeTeams.Length; ti++) {
                // Don't Automatically Self-Target
                if(g.activeTeams[ti].Index == teamIndex)
                    continue;
                RTSTeam team = g.activeTeams[ti].Team;
                for(int i = 0; i < team.Units.Count; i++) {
                    // This Check Makes Sure The Candidate Target Is In Range Of The Squad
                    if(g.CGrid.GetFogOfWar(team.Units[i].GridPosition, teamIndex) != FogOfWar.Active)
                        continue;
                    float d = (team.Units[i].GridPosition - squad.GridPosition).LengthSquared();
                    if(d < minDist) {
                        TargetUnit = team.Units[i];
                        minDist = d;
                    }
                }
            }
        }
        private void FindTargetBuilding(GameState g) {
            target = null;
            float minDist = float.MaxValue;
            for(int ti = 0; ti < g.activeTeams.Length; ti++) {
                // Don't Automatically Self-Target
                if(g.activeTeams[ti].Index == teamIndex)
                    continue;
                RTSTeam team = g.activeTeams[ti].Team;
                for(int i = 0; i < team.Units.Count; i++) {
                    // This Check Makes Sure The Candidate Target Is In Range Of The Squad
                    if(g.CGrid.GetFogOfWar(team.Buildings[i].GridPosition, teamIndex) != FogOfWar.Active)
                        continue;
                    float d = (team.Buildings[i].GridPosition - squad.GridPosition).LengthSquared();
                    if(d < minDist) {
                        TargetBuilding = team.Buildings[i];
                        minDist = d;
                    }
                }
            }
        }

        public override void ApplyTarget(GameState g, float dt) {
            // TODO: Think About Targeting Buildings. Melee Units
            for(int u = 0; u < squad.Units.Count; u++) {
                RTSUnit unit = squad.Units[u];
                if(unit.TargetingOrders != BehaviorFSM.TargetNone)
                    unit.Target = target;
            }
        }

        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }

    public class Movement : ACSquadMovementController {
        // The Constants Used In Flow Field Calculations
        protected const float rForce = 10f;
        protected const float aForce = -200f;

        // How Many Waypoints This Squad Should Lookahead When Updating
        protected const int lookahead = 2;

        // The Whole Squad Will Move At The Min Default Movespeed
        float minDefaultMoveSpeed;

        // Used For Movement Halting Logic
        float squadRadiusSquared = 0f;

        // The Net Force On Each Unit In This Squad
        // Key: UUID; Value: Net Force
        private Dictionary<int, Vector2> netForces = new Dictionary<int, Vector2>();
        public Dictionary<int, Vector2> NetForces {
            get { return netForces; }
            set { netForces = value; }
        }

        // Whether Units In This Squad Have Decided To Move (Key: UUID)
        public readonly Dictionary<int, bool> doMove = new Dictionary<int, bool>();

        // Calculate The Force Between Two Locations
        public Vector2 Force(Vector2 a, Vector2 b) {
            Vector2 diff = a - b;
            float denom = diff.LengthSquared();
            return diff.X != 0 && diff.Y != 0 ? 1 / denom * Vector2.Normalize(diff) : Vector2.Zero;
        }

        // Calculate The Force Between Two IEntities
        public Vector2 Force(IEntity e1, IEntity e2) {
            return rForce * Force(e1.GridPosition, e2.GridPosition);
        }

        // Calculate The Force Between An IEntity And A Waypoint
        public Vector2 Force(IEntity e, Vector2 wp) {
            return aForce * Force(e.GridPosition, wp);
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

        public override void Init(GameState s, GameplayController c) {
            pathfinder = c.pathfinder;
            minDefaultMoveSpeed = float.MaxValue;
            squadRadiusSquared = 0f;
            foreach(var unit in squad.Units) {
                float moveSpeed = unit.MovementSpeed / unit.MovementMultiplier;
                if(moveSpeed < minDefaultMoveSpeed)
                    minDefaultMoveSpeed = moveSpeed;
                squadRadiusSquared += unit.CollisionGeometry.BoundingRadius * unit.CollisionGeometry.BoundingRadius;
            }
        }

        private void UpdatePathQuery() {
            if(Query != null && !Query.IsOld && Query.IsComplete) {
                Query.IsOld = true; // Only Do This Once Per Query
                Waypoints = Query.waypoints;
                foreach(var unit in squad.Units) {
                    CurrentWaypointIndices[unit.UUID] = Query.waypoints.Count - 1;
                }
            }
        }

        public override void DecideMove(GameState g, float dt, RTSUnit unit) {
            UpdatePathQuery();
            doMove[unit.UUID] = CurrentWaypointIndices.ContainsKey(unit.UUID) && IsValid(CurrentWaypointIndices[unit.UUID]);
            if(!doMove[unit.UUID]) return;
            // If The Old Path Has Become Invalidated, Send A New Query
            bool invalid = false;
            int start = CurrentWaypointIndices[unit.UUID];
            int end = Math.Max(start - lookahead, 0);
            for(int i = CurrentWaypointIndices[unit.UUID]; i > end; i--) {
                Vector2 wp = Waypoints[i];
                Point wpCell = HashHelper.Hash(wp, g.CGrid.numCells, g.CGrid.size);
                if(g.CGrid.GetCollision(wpCell.X, wpCell.Y)) {
                    invalid = true;
                    break;
                }
            }
            if(invalid) {
                Vector2 goal = Waypoints[0];
                SendPathQuery(g, new SetWayPointEvent(squad.Team.Index, goal));
            }
            else {
                Vector2 waypoint = squad.MovementController.Waypoints[CurrentWaypointIndices[unit.UUID]];
                SetNetForceAndWaypoint(g, unit, waypoint);
            }
        }
        public override void ApplyMove(GameState g, float dt, RTSUnit unit) {
            AddToHistory(unit, unit.GridPosition);
            Vector2 netForce = NetForces.ContainsKey(unit.UUID) ? NetForces[unit.UUID] : Vector2.Zero;
            if(netForce != Vector2.Zero) {
                float magnitude = netForce.Length();
                Vector2 scaledChange = (netForce / magnitude) * minDefaultMoveSpeed * dt;
                // TODO: Make Sure We Don't Overshoot The Goal But Otherwise Move At Max Speed
                if(scaledChange.LengthSquared() > magnitude * magnitude)
                    unit.Move(netForce);
                else
                    unit.Move(scaledChange);
            }
        }

        private void SetNetForceAndWaypoint(GameState g, RTSUnit unit, Vector2 waypoint) {
            // Set Net Force...
            Vector2 netForce = squad.Units.Count * Force(unit, waypoint);
            CollisionGrid cg = g.CGrid;
            Point unitCell = HashHelper.Hash(unit.GridPosition, cg.numCells, cg.size);
            // Apply Forces From Other Units In This One's Cell
            foreach(var otherUnit in cg.EDynamic[unitCell.X, unitCell.Y]) {
                netForce += Force(unit, otherUnit);
            }
            // Apply Forces From Buildings And Other Units Near This One
            foreach(Point n in Pathfinder.Neighborhood(unitCell)) {
                RTSBuilding b = cg.EStatic[n.X, n.Y];
                if(b != null)
                    netForce += Force(unit, b);
                foreach(var otherUnit in cg.EDynamic[n.X, n.Y]) {
                    netForce += Force(unit, otherUnit);
                }
            }
            // Apply Forces From Units Recent Locations To Push It Forward
            if(UnitHistory.ContainsKey(unit.UUID)) {
                foreach(var prevLocation in UnitHistory[unit.UUID]) {
                    netForce -= Force(unit, prevLocation);
                }
            }
            NetForces[unit.UUID] = netForce;
            // Set Waypoint...
            Point currWaypointCell = HashHelper.Hash(waypoint, cg.numCells, cg.size);
            bool inGoalCell = unitCell.X == currWaypointCell.X && unitCell.Y == currWaypointCell.Y;
            if(inGoalCell || (waypoint - unit.GridPosition).LengthSquared() < 1.5 * squadRadiusSquared) {
                CurrentWaypointIndices[unit.UUID]--;
            }
        }
        public override void Serialize(BinaryWriter s) {
            // TODO: Implement Serialize
        }
        public override void Deserialize(BinaryReader s) {
            // TODO: Implement Deserialize
        }
    }
}

// // Having A Target Trumps Regular Movement
//else if(squad.TargetingController != null && squad.TargetingController.Target != null) {
//    RTSUnit target = unit.Target as RTSUnit;
//    if(target != null) {
//        switch(unit.CombatOrders) {
//            case BehaviorFSM.UseMeleeAttack:
//                float r = unit.CollisionGeometry.BoundingRadius + target.CollisionGeometry.BoundingRadius;
//                r *= 1.3f;
//                DoTargeting(g, dt, unit, target, r);
//                if(!doMove[unit.UUID] && unit.State == BehaviorFSM.Walking)
//                    unit.State = BehaviorFSM.CombatMelee;
//                break;
//            default: // case BehaviorFSM.UseRangedAttack:
//                r = unit.UnitData.BaseCombatData.MaxRange;
//                DoTargeting(g, dt, unit, target, r);
//                if(!doMove[unit.UUID] && unit.State == BehaviorFSM.Walking)
//                    unit.State = BehaviorFSM.CombatRanged;
//                break;
//        }
//    }
//}

 //// There Is A Straigt-Line Path From A To B That Intersects No Collidable Objects
 //       private bool CoastIsClear(Vector2 a, Vector2 b, float stepSize, float radius, CollisionGrid cg) {
 //           Vector2 dist = b - a;
 //           float slope = dist.Y / dist.X;
 //           float x = a.X;
 //           float y = a.Y;
 //           float rSq = (float)Math.Sqrt(radius);
 //           while(x < b.X && y < b.Y) {
 //               // TODO: Check Collisions Based On Radius
 //               bool collision = cg.GetCollision(new Vector2(x, y));
 //               // Cardinal Stuff
 //               collision |= cg.GetCollision(new Vector2(x + radius, y));
 //               collision |= cg.GetCollision(new Vector2(x + radius / 2.0f, y));
 //               collision |= cg.GetCollision(new Vector2(x - radius, y));
 //               collision |= cg.GetCollision(new Vector2(x - radius / 2.0f, y));
 //               collision |= cg.GetCollision(new Vector2(x, y + radius));
 //               collision |= cg.GetCollision(new Vector2(x, y + radius / 2.0f));
 //               collision |= cg.GetCollision(new Vector2(x, y - radius));
 //               collision |= cg.GetCollision(new Vector2(x, y - radius / 2.0f));
 //               collision |= cg.GetCollision(new Vector2(x + rSq, y + rSq));
 //               collision |= cg.GetCollision(new Vector2(x + rSq, y - rSq));
 //               collision |= cg.GetCollision(new Vector2(x - rSq, y + rSq));
 //               collision |= cg.GetCollision(new Vector2(x - rSq, y - rSq));
 //               if(collision)
 //                   return false;
 //               x += stepSize;
 //               y += stepSize;
 //           }
 //           return true;
 //       }

//// Get The Force Between A Unit And A Path Made Of Waypoints 
//private Vector2 GetForce(RTSUnit unit, List<Vector2> waypoints) {
//    if(Waypoints == null || Waypoints.Count == 0) return Vector2.Zero;
//    Vector2 goal = Waypoints[0];
//    float minDistSq = float.MaxValue;
//    Vector2 seg = Vector2.Zero;
//    for(int w = Waypoints.Count - 2; w >= 0; w--) {
//        Vector2 a = Waypoints[w + 1];
//        Vector2 b = Waypoints[w];
//        float d = DistSq(a, b, unit.GridPosition);
//        if(d < minDistSq) {
//            seg = b - a;
//            minDistSq = d;
//        }
//    }
//    return PathForce(seg, minDistSq);
//}

//// Calculate The Path Force At A Certain Distance Away From The Path
//private Vector2 PathForce(Vector2 pathSegment, float dist) {
//    Vector2 force = Vector2.Zero;
//    if(dist == 0f) {
//        force = new Vector2(1, 0);
//    }
//    else {
//        force = new Vector2(1f / dist, dist);
//        force.Normalize();
//    }
//    float a = (float)Math.Atan2(pathSegment.Y, pathSegment.X) + (float)Math.PI/2;
//    var mr = Matrix.CreateRotationZ(a);
//    force = Vector2.TransformNormal(force, mr);
//    return aForce*force;
//}

//// Calculate The Distance Squared Between A Line Segment (A,B) And A Point P
//private float DistSq(Vector2 a, Vector2 b, Vector2 p) {
//    Vector2 seg = b - a;
//    if(seg.X == 0 && seg.Y == 0) return (p - a).LengthSquared();
//    float mag = seg.LengthSquared();
//    float t = Vector2.Dot(p - a, seg) / mag;
//    if(t < 0) return (p - a).LengthSquared();
//    else if(t > mag) return (p - b).LengthSquared();
//    Vector2 proj = a + t * seg/mag;
//    return (proj - p).LengthSquared();
//}