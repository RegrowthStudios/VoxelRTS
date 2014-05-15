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

        public override void Init(GameState s, GameplayController c, object args) {
            tc = squad.TargetingController as Targeting;
            mc = squad.MovementController as Movement;
        }

        public override void DecideAction(GameState g, float dt) {
        }
        public override void ApplyAction(GameState g, float dt) {
        }

        public override void Serialize(BinaryWriter s) {
        }
        public override void Deserialize(BinaryReader s) {
        }
    }

    public class Targeting : ACSquadTargetingController {
        int teamIndex;

        public override void Init(GameState s, GameplayController c, object args) {
            teamIndex = squad.Team.Index;
        }
        public override void DecideTarget(GameState g, float dt) {
        }

        public override void ApplyTarget(GameState g, float dt) {
        }

        public override void Serialize(BinaryWriter s) {
        }
        public override void Deserialize(BinaryReader s) {
        }
    }

    public class Movement : ACSquadMovementController {
        public override void Init(GameState s, GameplayController c, object args) {
            pathfinder = c.pathfinder;
        }

        private void UpdateWaypoints() {
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
        }

        public override void ApplyMove(GameState g, float dt) {
        }

        public override void Serialize(BinaryWriter s) {
        }

        public override void Deserialize(BinaryReader s) {
        }
    }
}