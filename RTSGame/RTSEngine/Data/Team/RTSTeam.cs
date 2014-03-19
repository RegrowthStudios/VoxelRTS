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
        public List<RTSUnit> UnitData {
            get;
            private set;
        }

        // This Is All The Units In The Team
        private List<RTSUnitInstance> units;
        public List<RTSUnitInstance> Units {
            get { return units; }
        }

        // This Is All The Squads In The Team
        private List<RTSSquad> squads;
        public List<RTSSquad> Squads {
            get { return squads; }
        }

        public IInputController Input {
            get;
            set;
        }

        public event Action<RTSUnitInstance> OnNewUnitSpawn;

        public RTSTeam() {
            ColorSheme = RTSTeamColorScheme.Default;

            // Teams Starts Out Empty
            UnitData = new List<RTSUnit>();
            units = new List<RTSUnitInstance>();
            squads = new List<RTSSquad>();

            // No Input Is Available For The Team Yet
            Input = null;
        }

        public void AddUnitType(RTSUnit t) {
            UnitData.Add(t);
        }


        // Unit Addition And Removal
        public RTSUnitInstance AddUnit(int type, Vector2 pos) {
            RTSUnitInstance rui = new RTSUnitInstance(this, UnitData[type], pos);
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
        public RTSSquad AddSquad(RTSSquad s) {
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