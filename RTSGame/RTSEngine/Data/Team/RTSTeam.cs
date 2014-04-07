using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Controllers;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    // "Army Painter" Color Values
    public struct RTSColorScheme {
        public static RTSColorScheme Default {
            get {
                return new RTSColorScheme() {
                    Name = "Default",
                    Primary = Vector3.One,
                    Secondary = Vector3.One * 0.8f,
                    Tertiary = Vector3.One * 0.1f
                };
            }
        }

        public string Name;
        public Vector3 Primary;
        public Vector3 Secondary;
        public Vector3 Tertiary;
    }

    public class RTSTeam {
        // Team Colors
        public RTSColorScheme ColorScheme {
            get;
            set;
        }

        // Team Race
        public readonly RTSRace race;

        // Entity Data
        public readonly List<RTSUnit> units;
        public readonly List<RTSSquad> squads;
        public readonly List<RTSBuilding> buildings;

        public InputController Input {
            get;
            set;
        }

        // Events
        public event Action<RTSUnit> OnUnitSpawn;
        public event Action<RTSBuilding> OnBuildingSpawn;
        public event Action<RTSSquad> OnSquadCreation;

        public RTSTeam() {
            ColorScheme = RTSColorScheme.Default;

            // Teams Starts Out Empty
            race = new RTSRace();
            units = new List<RTSUnit>();
            squads = new List<RTSSquad>();
            buildings = new List<RTSBuilding>();

            // No Input Is Available For The Team Yet
            Input = null;
        }

        // Unit Addition And Removal
        public RTSUnit AddUnit(int type, Vector2 pos) {
            RTSUnit rui = new RTSUnit(this, race.units[type], pos);
            rui.ActionController = race.units[type].DefaultActionController.CreateInstance<ACUnitActionController>();
            rui.AnimationController = race.units[type].DefaultAnimationController.CreateInstance<ACUnitAnimationController>();
            rui.MovementController = race.units[type].DefaultMoveController.CreateInstance<ACUnitMovementController>();
            rui.CombatController = race.units[type].DefaultCombatController.CreateInstance<ACUnitCombatController>();
            units.Add(rui);
            if(OnUnitSpawn != null)
                OnUnitSpawn(rui);
            return rui;
        }
        public void RemoveUnit(RTSUnit u) {
            units.Remove(u);
        }
        public void RemoveAll(Predicate<RTSUnit> f) {
            units.RemoveAll(f);
        }

        // Squad Addition And Removal
        public RTSSquad AddSquad() {
            RTSSquad squad = new RTSSquad(this);
            squad.ActionController = race.scAction.CreateInstance<ACSquadActionController>();
            squad.MovementController = race.scMovement.CreateInstance<ACSquadMovementController>();
            squad.TargetingController = race.scTargetting.CreateInstance<ACSquadTargetingController>();
            squads.Add(squad);
            if(OnSquadCreation != null)
                OnSquadCreation(squad);
            return squad;
        }
        public void RemoveSquad(RTSSquad u) {
            squads.Remove(u);
        }
        public void RemoveAll(Predicate<RTSSquad> f) {
            squads.RemoveAll(f);
        }

        // Building Addition And Removal
        public RTSBuilding AddBuilding(int type, Vector2 pos) {
            RTSBuilding b = new RTSBuilding(this, race.buildings[type], pos);
            b.ActionController = race.buildings[type].DefaultActionController.CreateInstance<ACBuildingActionController>();
            buildings.Add(b);
            if(OnBuildingSpawn != null)
                OnBuildingSpawn(b);
            return b;
        }
        public void RemoveBuilding(RTSBuilding b) {
            buildings.Remove(b);
        }
        public void RemoveAll(Predicate<RTSBuilding> f) {
            buildings.RemoveAll(f);
        }
    }
}