using System;
using System.Collections.Generic;
using System.IO;
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
        public static void Serialize(BinaryWriter s, RTSTeam team) {
            RTSRace.Serialize(s, team.Race);
            s.Write((int)team.Input.Type);
            team.Input.Serialize(s);
            s.Write(team.ColorScheme.Name);
            s.Write(team.ColorScheme.Primary);
            s.Write(team.ColorScheme.Secondary);
            s.Write(team.ColorScheme.Tertiary);
            s.Write(team.Buildings.Count);
            foreach(var building in team.Buildings) {
                RTSBuilding.Serialize(s, building);
            }
            s.Write(team.Units.Count);
            foreach(var unit in team.Units) {
                RTSUnit.Serialize(s, unit);
            }
            s.Write(team.Squads.Count);
            foreach(var squad in team.Squads) {
                RTSSquad.Serialize(s, squad);
            }
        }
        public static RTSTeam Deserialize(BinaryReader s, int index, GameState state) {
            RTSTeam team = new RTSTeam();
            team.Race = RTSRace.Deserialize(s, state);
            InputType it = (InputType)s.ReadInt32();
            switch(it) {
                case InputType.AI:
                    AIInputController aic = new AIInputController(state, index);
                    team.Input = aic;
                    team.Input.Deserialize(s);
                    break;
                case InputType.Environment:
                    EnvironmentInputController eic = new EnvironmentInputController(state, index);
                    team.Input = eic;
                    team.Input.Deserialize(s);
                    break;
                case InputType.Player:
                    PlayerInputController pic = new PlayerInputController(state, index);
                    team.Input = pic;
                    team.Input.Deserialize(s);
                    break;
                default:
                    throw new Exception("A Team That Was Never Supposed To Be Created Was Made");
            }
            RTSColorScheme scheme = new RTSColorScheme();
            scheme.Name = s.ReadString();
            scheme.Primary = s.ReadVector3();
            scheme.Secondary = s.ReadVector3();
            scheme.Tertiary = s.ReadVector3();
            team.ColorScheme = scheme;

            int? target;
            var du = new Dictionary<int, RTSUnit>();
            List<int> su;

            int c = s.ReadInt32();
            RTSBuilding building;
            for(int i = 0; i < c; i++) {
                building = RTSBuilding.Deserialize(s, team, out target);
                team.buildings.Add(building);
                if(target.HasValue) {
                    // TODO: Add A Target Binding
                }
            }

            c = s.ReadInt32();
            RTSUnit unit;
            for(int i = 0; i < c; i++) {
                unit = RTSUnit.Deserialize(s, team, out target);
                du.Add(unit.UUID, unit);
                team.units.Add(unit);
                if(target.HasValue) {
                    // TODO: Add A Target Binding
                }
            }

            c = s.ReadInt32();
            RTSSquad squad;
            for(int i = 0; i < c; i++) {
                squad = RTSSquad.Deserialize(s, team, out su);
                team.squads.Add(squad);
                foreach(int uuid in su) {
                    if(du.TryGetValue(uuid, out unit)) {
                        squad.Add(unit);
                    }
                    else {
                        throw new Exception("Could Not Find A Unit With The Specified UUID");
                    }
                }
            }
            return team;
        }

        // Team Colors
        public RTSColorScheme ColorScheme {
            get;
            set;
        }

        // Team Race
        public RTSRace Race {
            get;
            private set;
        }

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
            Race = new RTSRace();
            units = new List<RTSUnit>();
            squads = new List<RTSSquad>();
            buildings = new List<RTSBuilding>();

            // No Input Is Available For The Team Yet
            Input = null;
        }

        // Unit Addition And Removal
        public RTSUnit AddUnit(int type, Vector2 pos) {
            if(Race.Units[type].CurrentCount >= Race.Units[type].MaxCount) return null;

            RTSUnit unit = new RTSUnit(this, Race.Units[type], pos);
            unit.UnitData.CurrentCount++;
            unit.ActionController = Race.Units[type].DefaultActionController.CreateInstance<ACUnitActionController>();
            unit.AnimationController = Race.Units[type].DefaultAnimationController.CreateInstance<ACUnitAnimationController>();
            unit.MovementController = Race.Units[type].DefaultMoveController.CreateInstance<ACUnitMovementController>();
            unit.CombatController = Race.Units[type].DefaultCombatController.CreateInstance<ACUnitCombatController>();
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
            squad.ActionController = Race.SCAction.CreateInstance<ACSquadActionController>();
            squad.MovementController = Race.SCMovement.CreateInstance<ACSquadMovementController>();
            squad.TargetingController = Race.SCTargeting.CreateInstance<ACSquadTargetingController>();
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
            if(Race.Buildings[type].CurrentCount >= Race.Buildings[type].MaxCount) return null;

            RTSBuilding b = new RTSBuilding(this, Race.Buildings[type], pos);
            b.BuildingData.CurrentCount++;
            b.ActionController = Race.Buildings[type].DefaultActionController.CreateInstance<ACBuildingActionController>();
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