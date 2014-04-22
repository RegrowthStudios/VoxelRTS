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
        // Subordinate Controllers
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
                mc.DecideMove(g, dt);
            }
        }
        public override void ApplyAction(GameState g, float dt) {
            if(tc != null) {
                tc.ApplyTarget(g, dt);
            }
            if(mc != null) {
                mc.ApplyMove(g, dt);
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
        //public readonly Dictionary<int, int> origTargetOrders = new Dictionary<int, int>();

        public override void Init(GameState s, GameplayController c) {
            teamIndex = squad.Team.Index;
            //for(int i = 0; i < squad.Units.Count; i++) {
            //    RTSUnit unit = squad.Units[i];
            //    origTargetOrders[unit.UUID] = unit.TargetingOrders;
            //}
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
            // TODO: Think About Targeting Buildings & Melee Units
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
        public override void Init(GameState s, GameplayController c) {
            pathfinder = c.pathfinder;
            MinDefaultMoveSpeed = float.MaxValue;
            SquadRadiusSquared = 0f;
            foreach(var unit in squad.Units) {
                float moveSpeed = unit.MovementSpeed / unit.MovementMultiplier;
                if(moveSpeed < MinDefaultMoveSpeed)
                    MinDefaultMoveSpeed = moveSpeed;
                SquadRadiusSquared += unit.CollisionGeometry.BoundingRadius * unit.CollisionGeometry.BoundingRadius;
            }
        }

        private void UpdatePathQuery() {
            if(Query != null && !Query.IsOld && Query.IsComplete) {
                Query.IsOld = true; // Only Do This Once Per Query
                Waypoints = Query.waypoints;
                foreach(var unit in squad.Units) {
                    List<Vector2> waypoints = new List<Vector2>();
                    foreach(var wp in Waypoints) {
                        waypoints.Add(wp);
                    }
                    unit.MovementController.Waypoints = waypoints;
                    unit.MovementController.CurrentWaypointIndex = Waypoints.Count - 1;
                }
            }
        }

        public override void DecideMove(GameState g, float dt) {
            // TODO: Decide Whether To Send A New Path Query
        }
        public override void ApplyMove(GameState g, float dt) {
            UpdatePathQuery();
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