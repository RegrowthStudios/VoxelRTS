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
        Dictionary<int, int> origTargetOrders = new Dictionary<int, int>();

        public override void Init(GameState s, GameplayController c) {
            tc = squad.TargetingController as Targeting;
            mc = squad.MovementController as Movement;
            for(int i = 0; i < squad.Units.Count; i++) {
                RTSUnit unit = squad.Units[i];
                origTargetOrders[unit.UUID] = unit.TargetingOrders;
            }
        }

        public override void DecideAction(GameState g, float dt) {
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
            if(tc != null)
                tc.DecideTarget(g, dt);
            if(mc != null) {
                foreach(var unit in squad.Units) {
                    mc.DecideMove(g, dt, unit);
                    var doMove = mc.doMove;
                    if(doMove.ContainsKey(unit.UUID) && (doMove[unit.UUID])) {
                        unit.State = BehaviorFSM.Walking;
                        unit.TargetingOrders = BehaviorFSM.TargetNone;
                    }
                    else if(!(unit.State == BehaviorFSM.CombatMelee || unit.State == BehaviorFSM.CombatRanged)) {
                        unit.State = BehaviorFSM.Rest;
                        unit.TargetingOrders = origTargetOrders[unit.UUID];
                    }
                }
            }
        }
        public override void ApplyAction(GameState g, float dt) {
            if(tc != null)
                squad.TargetingController.ApplyTarget(g, dt);
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

    public class Movement : ACSquadMovementController {
        // Whether Units In This Squad Have Decided To Move (Key: UUID)
        public readonly Dictionary<int, bool> doMove = new Dictionary<int, bool>();

        // The Whole Squad Will Move At The Min Default Movespeed
        float minDefaultMoveSpeed;

        // Squad Radius Squared
        float squadRadiusSquared = 0f;

        // The Centroid Of The Squad
        Vector2 centroid;

        // Get The Force Between A Unit And A Path Made Of Waypoints 
        private Vector2 GetForce(RTSUnit unit, List<Vector2> waypoints) {
            if(Waypoints == null || Waypoints.Count == 0) return Vector2.Zero;
            Vector2 goal = Waypoints[0];
            float minDistSq = float.MaxValue;
            Vector2 seg = Vector2.Zero;
            for(int w = Waypoints.Count - 2; w >= 0; w--) {
                Vector2 a = Waypoints[w + 1];
                Vector2 b = Waypoints[w];
                float d = DistSq(a, b, unit.GridPosition);
                if(d < minDistSq) {
                    seg = b - a;
                    minDistSq = d;
                }
            }
            return PathForce(seg, minDistSq);
        }

        // Calculate The Path Force At A Certain Distance Away From The Path
        private Vector2 PathForce(Vector2 pathSegment, float dist) {
            Vector2 force = Vector2.Zero;
            if(dist == 0f) {
                force = new Vector2(1, 0);
            }
            else {
                force = new Vector2(1f / dist, dist);
                force.Normalize();
            }
            float a = (float)Math.Atan2(pathSegment.Y, pathSegment.X) + (float)Math.PI/2;
            var mr = Matrix.CreateRotationZ(a);
            force = Vector2.TransformNormal(force, mr);
            return aForce*force;
        }

        // Calculate The Distance Squared Between A Line Segment (A,B) And A Point P
        private float DistSq(Vector2 a, Vector2 b, Vector2 p) {
            Vector2 seg = b - a;
            if(seg.X == 0 && seg.Y == 0) return (p - a).LengthSquared();
            float mag = seg.LengthSquared();
            float t = Vector2.Dot(p - a, seg) / mag;
            if(t < 0) return (p - a).LengthSquared();
            else if(t > mag) return (p - b).LengthSquared();
            Vector2 proj = a + t * seg/mag;
            return (proj - p).LengthSquared();
        }

        private void UpdateCentroid() {
            centroid = Vector2.Zero;
            foreach(var unit in squad.Units) {
                centroid += unit.GridPosition;
            }
            centroid /= squad.Units.Count;
        }

        public override void Init(GameState s, GameplayController c) {
            minDefaultMoveSpeed = float.MaxValue;
            squadRadiusSquared = 0f;
            foreach(var unit in squad.Units) {
                float moveSpeed = unit.MovementSpeed / unit.MovementMultiplier;
                if(moveSpeed < minDefaultMoveSpeed)
                    minDefaultMoveSpeed = moveSpeed;
                squadRadiusSquared += unit.CollisionGeometry.BoundingRadius * unit.CollisionGeometry.BoundingRadius;
            }
        }

        public override void DecideMove(GameState g, float dt, RTSUnit unit) {
            //UpdateCentroid();
            doMove[unit.UUID] = CurrentWaypointIndices.ContainsKey(unit.UUID) && IsValid(CurrentWaypointIndices[unit.UUID]);
            if(!doMove[unit.UUID]) return;
            // Pathfinding Has Not Finished: Make The Formation At The Average Squad Position
            if(Waypoints == null || Waypoints.Count == 0) {
                SetNetForceAndWaypoint(g, unit, squad.GridPosition, null);
            }
            // Regular Movement 
            else {
                if(CurrentWaypointIndices.ContainsKey(unit.UUID) && IsValid(CurrentWaypointIndices[unit.UUID])) {
                    Vector2 waypoint = squad.MovementController.Waypoints[CurrentWaypointIndices[unit.UUID]];
                    SetNetForceAndWaypoint(g, unit, waypoint, null);
                }
            }
        }
        public override void ApplyMove(GameState g, float dt, RTSUnit unit) {
            AddToHistory(unit, unit.GridPosition);
            Vector2 change = NetForces.ContainsKey(unit.UUID) ? NetForces[unit.UUID] : Vector2.Zero;
            if(change != Vector2.Zero) {
                float magnitude = change.Length();
                Vector2 scaledChange = (change / magnitude) * minDefaultMoveSpeed * dt;
                if(scaledChange.LengthSquared() > magnitude * magnitude)
                    unit.Move(change);
                else
                    unit.Move(scaledChange);
            }
        }

        private void SetNetForceAndWaypoint(GameState g, RTSUnit unit, Vector2 waypoint, List<Vector2> targetFormation) {
            // Set Net Force...
            //Vector2 netForce = GetForce(unit, Waypoints);
            Vector2 netForce = squad.Units.Count * Force(unit, waypoint);
            CollisionGrid cg = g.CGrid;
            Point unitCell = HashHelper.Hash(unit.GridPosition, cg.numCells, cg.size);
            // Apply Forces From Other Units In This One's Cell
            foreach(var otherUnit in cg.EDynamic[unitCell.X, unitCell.Y]) {
                netForce += Force(unit, otherUnit);
            }
            // Apply Forces From Buildings And Other Units Near This One
            IEnumerable<Point> neighborhood = Pathfinder.NeighborhoodAlign(unitCell).Concat<Point>(Pathfinder.NeighborhoodDiag(unitCell));
            foreach(Point n in neighborhood) {
                RTSBuilding b = cg.EStatic[n.X, n.Y];
                if(b != null)
                    netForce += Force(unit, b);
                foreach(var otherUnit in cg.EDynamic[n.X, n.Y]) {
                    netForce += Force(unit, otherUnit);
                }
            }
            if(UnitHistory.ContainsKey(unit.UUID)) {
                foreach(var prevLocation in UnitHistory[unit.UUID]) {
                    netForce -= Force(unit, prevLocation);
                }
            }
            NetForces[unit.UUID] = netForce;

            // Set Waypoint...
            Point currWaypointCell = HashHelper.Hash(waypoint, cg.numCells, cg.size);
            bool inGoalCell = unitCell.X == currWaypointCell.X && unitCell.Y == currWaypointCell.Y;
            if(inGoalCell || (waypoint - unit.GridPosition).LengthSquared() < 1.5*squadRadiusSquared) {
                CurrentWaypointIndices[unit.UUID]--;
            }
        }

        // TODO: Make This Predict (Or Omnisciently Read) Where The Target Will Be Because It Could Be Moving
        private void DoTargeting(GameState g, float dt, RTSUnit unit, RTSUnit target, float r) {
            if(CurrentWaypointIndices.ContainsKey(unit.UUID) && IsValid(CurrentWaypointIndices[unit.UUID])) {
                Vector2 waypoint = Waypoints[CurrentWaypointIndices[unit.UUID]];
                bool waypointIsTarget = waypoint.X == target.GridPosition.X && waypoint.Y == target.GridPosition.Y;
                if(waypointIsTarget) {
                    List<Vector2> targetFormation = new List<Vector2>();
                    float step = (float)Math.PI / 16;
                    float angle = 0;
                    while(angle < 2 * Math.PI) {
                        targetFormation.Add(new Vector2((float)(r * Math.Sin(angle)), (float)(r * Math.Sin(angle))));
                        angle += step;
                    }
                    SetNetForceAndWaypoint(g, unit, waypoint, targetFormation);
                }
                else {
                    SetNetForceAndWaypoint(g, unit, waypoint, null);
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
        public override void Init(GameState s, GameplayController c) {
            teamIndex = squad.Team.Index;
        }

        public override void DecideTarget(GameState g, float dt) {
            if(target == null) {
                FindTargetUnit(g);
            }
            else if(!target.IsAlive) {
                target = null;
            }
            else if(g.CGrid.GetFogOfWar(target.GridPosition, teamIndex) != FogOfWar.Active) {
                target = null;
            }
        }
        private void FindTargetSquad(GameState g) {
            targetSquad = null;
            float minDist = float.MaxValue;
            for(int i = 0; i < g.activeTeams.Length; i++) {
                RTSTeam team = g.activeTeams[i].Team;
                if(team == squad.Team) continue;
                foreach(var sq in team.Squads) {
                    float d = (sq.GridPosition - squad.GridPosition).LengthSquared();
                    if(d < minDist) {
                        targetSquad = sq;
                        minDist = d;
                    }
                }
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
        public override void ApplyTarget(GameState g, float dt) {
            foreach(var unit in squad.Units) {
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
}
