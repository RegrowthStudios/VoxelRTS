using System;
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
        public void SetSquad(RTSSquad s) {
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

    // A Struct Relating A Unit To Its Post (Offset) In This Squad's Formation
    public struct FormationAssignment {
        public int UUID;
        public Vector2 Post;

        public FormationAssignment(int uuid, Vector2 post) {
            UUID = uuid;
            Post = post;
        }
    }

    // The Movement Controller That Dictates The General Movement Behavior Of Units In The Squad
    public abstract class ACSquadMovementController : ACSquadController {
        // Box Formation Follows The Golden Ratio Phi
        private static float phi = (float)((1.0f + Math.Sqrt(5.0f)) / 2.0f);

        // Waypoints That Units In This Squad Will Generally Follow
        private List<Vector2> waypoints = new List<Vector2>();
        public List<Vector2> Waypoints {
            get { return waypoints; }
            set { waypoints = value; }
        }        
    
        // Units' Displacements From The Squad's Waypoints At Origin
        public readonly List<FormationAssignment> formationAssignments = new List<FormationAssignment>();

        // TODO: Implement
        // Decide Where Units In This Squad Should Go When Moving
        public void ApplyMovementFormation(int movementOrder) {
            switch(movementOrder) {
                case BehaviorFSM.BoxFormation:
                    // Determine Spacing Bewteen Units In Formation
                    float spacing = float.MinValue;
                    foreach(var unit in squad.Units) {
                        if(unit.CollisionGeometry.BoundingRadius > spacing) {
                            spacing = unit.CollisionGeometry.BoundingRadius;
                        }
                    }
                    spacing *= 2;
                    int numUnits = squad.Units.Count;
                    int numFullRows = (int)Math.Floor(Math.Sqrt(numUnits * spacing / phi));
                    int unitsPerRow = (int)Math.Ceiling(phi * numFullRows);

#if DEBUG
                    RTSEngine.Controllers.DevConsole.AddCommand("numFullRows: " + numFullRows + "\n unitsPerRow: " + unitsPerRow);
#endif

                    // Special Spacing For The Last Row
                    float lastSpacing = spacing;
                    int numLastUnits = numUnits - numFullRows * unitsPerRow;
                    lastSpacing = ((float)unitsPerRow) / ((float)numLastUnits);

                    // Calculate Formation As Offsets From Squad Waypoint
                    List<Vector2> formation = new List<Vector2>();
                    float rOffset = (numLastUnits > 0) ? -numFullRows * spacing / 2.0f : -(numFullRows - 1) * spacing / 2.0f;
                    float cOffset = -(unitsPerRow - 1) * spacing / 2.0f;
                    for(int r = 0; r < numFullRows; r++) {
                        rOffset += r * spacing;
                        for(int c = 0; c < unitsPerRow; c++) {
                            cOffset += c * spacing;
                            formation.Add(new Vector2(rOffset, cOffset));
                        }
                    }
                    rOffset += spacing;
                    cOffset = -(numLastUnits - 1) * lastSpacing / 2.0f;
                    if(numLastUnits > 0) {
                        for(int c = 0; c < numLastUnits; c++) {
                            cOffset += c * lastSpacing;
                            formation.Add(new Vector2(rOffset, cOffset));
                        }
                    }

                    // Assign The Units To Posts In The Formation
                    bool[] assigned = new bool[formation.Count];
                    foreach(var unit in squad.Units) {
                        Vector2 pos = unit.GridPosition;
                        float minDistSq = float.MaxValue;
                        int assignment = 0;
                        for(int i = 0; i < formation.Count; i++) {
                            float distSq = Vector2.DistanceSquared(pos, formation[i]);
                            if(!assigned[i] && distSq < minDistSq) {
                                minDistSq = distSq;
                                assignment = i;
                            }
                        }
                        assigned[assignment] = true;
                        formationAssignments.Add(new FormationAssignment(unit.UUID, formation[assignment]));
                    }
                    break;
                case BehaviorFSM.FreeFormation:
                    break;
            }
            foreach(var fa in formationAssignments) {
//#if DEBUG
//                RTSEngine.Controllers.DevConsole.AddCommand("unit "+fa.UUID+" : "+fa.Post);
//#endif
            }
        }
        // TODO: Implement
        // Decide Where Units In This Squad Should Go When Close To Their Target
        public void CalculateTargetFormation() {

        }
    }

    // Controls The Targetting That A Squad Performs
    public abstract class ACSquadTargettingController : ACSquadController {
        // A Squad Target
        protected RTSSquad targetSquad;

        // A Unit Target
        protected RTSUnit targetUnit;
        public RTSUnit Target {
            get { return targetUnit; }
            set { targetUnit = value; }
        }

        // Find And Setting A Target For This Squad
        public abstract void DecideTarget(GameState g, float dt);
        public abstract void ApplyTarget(GameState g, float dt);
    }
}