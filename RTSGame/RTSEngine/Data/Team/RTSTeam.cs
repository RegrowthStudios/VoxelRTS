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
        private List<RTSUnit> units;
        public List<RTSUnit> Units {
            get { return units; }
        }
        private List<RTSSquad> squads;
        public List<RTSSquad> Squads {
            get { return squads; }
        }
        private List<RTSBuilding> buildings;
        public List<RTSBuilding> Buildings {
            get { return buildings; }
        }

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
            if(race.Units[type].CurrentCount >= race.Units[type].MaxCount) return null;

            RTSUnit unit = new RTSUnit(this, race.Units[type], pos);
            unit.UnitData.CurrentCount++;
            unit.ActionController = race.Units[type].DefaultActionController.CreateInstance<ACUnitActionController>();
            unit.AnimationController = race.Units[type].DefaultAnimationController.CreateInstance<ACUnitAnimationController>();
            unit.MovementController = race.Units[type].DefaultMoveController.CreateInstance<ACUnitMovementController>();
            unit.CombatController = race.Units[type].DefaultCombatController.CreateInstance<ACUnitCombatController>();
            Units.Add(unit);
            if(OnUnitSpawn != null)
                OnUnitSpawn(unit);
            return unit;
        }
        public void RemoveAll(Predicate<RTSUnit> f) {
            var nu = new List<RTSUnit>(units.Count);
            for(int i = 0; i < units.Count; i++) {
                if(f(units[i]))
                    units[i].UnitData.CurrentCount--;
                else
                    nu.Add(units[i]);
            }
            System.Threading.Interlocked.Exchange(ref units, nu);
        }

        // Squad Addition And Removal
        public RTSSquad AddSquad() {
            RTSSquad squad = new RTSSquad(this);
            squad.ActionController = race.SCAction.CreateInstance<ACSquadActionController>();
            squad.MovementController = race.SCMovement.CreateInstance<ACSquadMovementController>();
            squad.TargettingController = race.SCTargetting.CreateInstance<ACSquadTargettingController>();
            Squads.Add(squad);
            if(OnSquadCreation != null)
                OnSquadCreation(squad);
            return squad;
        }
        public void RemoveAll(Predicate<RTSSquad> f) {
            Squads.RemoveAll(f);
        }

        // Building Addition And Removal
        public RTSBuilding AddBuilding(int type, Vector2 pos) {
            if(race.Buildings[type].CurrentCount >= race.Buildings[type].MaxCount) return null;

            RTSBuilding b = new RTSBuilding(this, race.Buildings[type], pos);
            b.BuildingData.CurrentCount++;
            b.ActionController = race.Buildings[type].DefaultActionController.CreateInstance<ACBuildingActionController>();
            Buildings.Add(b);
            if(OnBuildingSpawn != null)
                OnBuildingSpawn(b);
            return b;
        }
        public void RemoveAll(Predicate<RTSBuilding> f) {
            var nb = new List<RTSBuilding>(buildings.Count);
            for(int i = 0; i < buildings.Count; i++) {
                if(f(buildings[i]))
                    buildings[i].BuildingData.CurrentCount--;
                else
                    nb.Add(buildings[i]);
            }
            System.Threading.Interlocked.Exchange(ref buildings, nb);

            Buildings.RemoveAll(f);
        }
    }
}