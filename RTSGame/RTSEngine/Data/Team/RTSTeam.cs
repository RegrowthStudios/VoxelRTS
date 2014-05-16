using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Algorithms;
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

        [ZXParse("NAME")]
        public string Name;
        [ZXParse("PRIMARY")]
        public Vector3 Primary;
        [ZXParse("SECONDARY")]
        public Vector3 Secondary;
        [ZXParse("TERTIARY")]
        public Vector3 Tertiary;
    }

    public class ViewedBuilding {
        public int Team;
        public int Type;
        public Point CellPoint;
        public Vector3 WorldPosition;
        public Vector2 ViewDirection;
        public float BuildAmount;
    }

    public class RTSTeam {
        public static void Serialize(BinaryWriter s, RTSTeam team) {
            s.Write(team.Type);
            RTSRace.Serialize(s, team.Race);
            if(team.Input != null) {
                s.Write(true);
                s.Write(ReflectedScript.GetKey(team.Input));
                team.Input.Serialize(s);
            }
            else {
                s.Write(false);
            }
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
            int t = s.ReadInt32();
            RTSTeam team = new RTSTeam(index, t);
            team.Race = RTSRace.Deserialize(s, state);
            if(s.ReadBoolean()) {
                string it = s.ReadString();
                team.Input = state.Scripts[it].CreateInstance<ACInputController>();
                team.Input.Deserialize(s);
                team.Input.Init(state, index, null);
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
                state.CGrid.Add(building);
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

        // Index Into Game State
        public int Index {
            get;
            private set;
        }
        public int Type {
            get;
            private set;
        }

        // Team Colors
        public RTSColorScheme ColorScheme {
            get;
            set;
        }

        // Team Race
        public RTSRace Race {
            get;
            set;
        }

        // Team Capital (Always Non-negative)
        private int capital;
        public int Capital {
            get { return capital; }
            set {
                if(value < 0) value = 0;
                if(capital != value) {
                    capital = value;
                    if(OnCapitalChange != null)
                        OnCapitalChange(this, Capital);
                }
            }
        }

        // Population Information
        private int population, populationCap;
        public int Population {
            get { return population; }
            set {
                if(value < 0) value = 0;
                if(population != value) {
                    population = value;
                    if(OnPopulationChange != null)
                        OnPopulationChange(this, Population);
                }
            }
        }
        public int PopulationCap {
            get { return populationCap; }
            set {
                if(value < 0) value = 0;
                if(populationCap != value) {
                    populationCap = value;
                    if(OnPopulationCapChange != null)
                        OnPopulationCapChange(this, PopulationCap);
                }
            }
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

        // Input Controller
        public ACInputController Input {
            get;
            set;
        }

        // Used For Visuals And Input Logic
        public List<ViewedBuilding> ViewedEnemyBuildings {
            get;
            private set;
        }

        // Events
        public event Action<RTSUnit> OnUnitSpawn;
        public event Action<RTSBuilding> OnBuildingSpawn;
        public event Action<RTSSquad> OnSquadCreation;
        public event Action<RTSTeam, int> OnCapitalChange;
        public event Action<RTSTeam, int> OnPopulationChange;
        public event Action<RTSTeam, int> OnPopulationCapChange;

        public RTSTeam(int i, int t) {
            Index = i;
            ColorScheme = RTSColorScheme.Default;
            Type = t;

            // Teams Starts Out Empty
            Race = new RTSRace();
            units = new List<RTSUnit>();
            squads = new List<RTSSquad>();
            buildings = new List<RTSBuilding>();
            ViewedEnemyBuildings = new List<ViewedBuilding>();
            Capital = 0;
            Population = 0;
            PopulationCap = 0;

            // No Input Is Available For The Team Yet
            Input = null;
        }

        // Unit Addition And Removal
        public RTSUnit AddUnit(int type, Vector2 pos) {
            // Check For Unit Type Existence
            RTSUnitData data = Race.Units[type];
            if (data == null) {
                DevConsole.AddCommand("data null");
                return null;
            }
            // Check For Unit Cap
            if(data.CurrentCount >= data.MaxCount) return null;

            // Check For Population Cap
            if(data.PopulationCost + Population > PopulationCap) return null;

            // Check For Capital Cost
            if(data.CapitalCost > Capital) return null;

            // Produce Unit
            Capital -= data.CapitalCost;
            Population += data.PopulationCost;
            data.CurrentCount++;

            // Create Unit
            RTSUnit unit = new RTSUnit(this, data, pos);
            unit.ActionController = data.DefaultActionController.CreateInstance<ACUnitActionController>();
            unit.AnimationController = data.DefaultAnimationController.CreateInstance<ACUnitAnimationController>();
            unit.MovementController = data.DefaultMoveController.CreateInstance<ACUnitMovementController>();
            unit.CombatController = data.DefaultCombatController.CreateInstance<ACUnitCombatController>();
            Units.Add(unit);
            if(OnUnitSpawn != null)
                OnUnitSpawn(unit);
            return unit;
        }
        public void RemoveAll(Predicate<RTSUnit> f, Action<int> fRemoval) {
            var nu = new List<RTSUnit>(units.Count);
            int pc = 0;
            for(int i = 0; i < units.Count; i++) {
                if(f(units[i])) {
                    pc += units[i].Data.PopulationCost;
                    units[i].Data.CurrentCount--;
                    fRemoval(units[i].UUID);
                }
                else
                    nu.Add(units[i]);
            }
            if(pc != 0) Population -= pc;
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
            // Check For Building Type Existence
            RTSBuildingData data = Race.Buildings[type];
            if(data == null) return null;

            // Check For Building Cap
            if(data.CurrentCount >= data.MaxCount) return null;

            // Check For Capital Cost
            if(data.CapitalCost > Capital) return null;

            // Produce Building
            Capital -= data.CapitalCost;
            data.CurrentCount++;

            RTSBuilding b = new RTSBuilding(this, data, pos);
            b.OnBuildingFinished += OnBuildingFinished;
            b.ActionController = data.DefaultActionController.CreateInstance<ACBuildingActionController>();
            for(int i = 0; i < data.DefaultButtonControllers.Count; i++) {
                b.AddButton(data.DefaultButtonControllers[i].CreateInstance<ACBuildingButtonController>());
            }
            Buildings.Add(b);
            if(OnBuildingSpawn != null)
                OnBuildingSpawn(b);
            return b;
        }
        public void RemoveAll(Predicate<RTSBuilding> f, Action<int> fRemoval) {
            var nb = new List<RTSBuilding>(buildings.Count);
            int pc = 0;
            for(int i = 0; i < buildings.Count; i++) {
                if(f(buildings[i])) {
                    if(buildings[i].IsBuilt) {
                        pc += buildings[i].Data.PopCapChange;
                    }
                    buildings[i].Data.CurrentCount--;
                    fRemoval(buildings[i].UUID);
                }
                else
                    nb.Add(buildings[i]);
            }
            if(pc != 0) PopulationCap -= pc;
            System.Threading.Interlocked.Exchange(ref buildings, nb);
        }
        private void OnBuildingFinished(RTSBuilding b) {
            if(b.Data.PopCapChange != 0)
                PopulationCap += b.Data.PopCapChange;
        }
    }
}