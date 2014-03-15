using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    // "Army Painter" Color Values
    public struct RTSTeamColorScheme {
        public static RTSTeamColorScheme Default {
            get {
                return new RTSTeamColorScheme() {
                    Primary = Vector3.One,
                    Secondary = Vector3.One * 0.8f,
                    Tertiary = Vector3.One * 0.3f
                };
            }
        }

        public Vector3 Primary;
        public Vector3 Secondary;
        public Vector3 Tertiary;
    }

    public class RTSTeam {
        // Team Color
        public RTSTeamColorScheme ColorSheme {
            get;
            set;
        }

        // Unit Data
        private List<RTSUnit> unitData;

        // This Is All The Units In The Team
        private List<RTSUnitInstance> units;
        public IEnumerable<RTSUnitInstance> Units {
            get { return units; }
        }
        public int UnitCount {
            get { return units.Count; }
        }

        // This Is All The Squads In The Team
        private List<RTSSquad> squads;
        public IEnumerable<RTSSquad> Squads {
            get { return squads; }
        }
        public int SquadCount {
            get { return squads.Count; }
        }

        public IInputController Input {
            get;
            set;
        }

        public event Action<RTSUnitInstance> OnNewUnitSpawn;

        public RTSTeam() {
            ColorSheme = RTSTeamColorScheme.Default;

            // Teams Starts Out Empty
            unitData = new List<RTSUnit>();
            units = new List<RTSUnitInstance>();
            squads = new List<RTSSquad>();

            // No Input Is Available For The Team Yet
            Input = null;
        }

        public void AddUnitType(RTSUnit t) {
            unitData.Add(t);
        }

        // Unit Addition And Removal
        public RTSUnitInstance AddUnit(int type, Vector3 pos) {
            RTSUnitInstance rui = new RTSUnitInstance(this, unitData[type], pos);
            units.Add(rui);
            if(OnNewUnitSpawn != null)
                OnNewUnitSpawn(rui);
            return rui;
        }
        public void RemoveUnit(RTSUnitInstance u) {
            units.Remove(u);
        }
        public void RemoveAll(Predicate<RTSUnitInstance> f) {
            units.RemoveAll(f);
        }

        // Squad Addition And Removal
        public RTSSquad AddSquad() {
            RTSSquad s = new RTSSquad();
            squads.Add(s);
            return s;
        }
        public void RemoveSquad(RTSSquad u) {
            squads.Remove(u);
        }
        public void RemoveAll(Predicate<RTSSquad> f) {
            squads.RemoveAll(f);
        }
    }
}