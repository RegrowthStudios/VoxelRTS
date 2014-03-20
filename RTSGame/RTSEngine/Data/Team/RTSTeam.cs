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
        // Team Color
        public RTSColorScheme ColorScheme {
            get;
            set;
        }

        // Unit Data
        public readonly List<RTSUnitData> unitData;

        // Entity Data
        public readonly List<RTSUnit> units;
        public readonly List<RTSSquad> squads;
        public string DSAC, DSTC;

        public InputController Input {
            get;
            set;
        }

        // Events
        public event Action<RTSUnit> OnNewUnitSpawn;

        public RTSTeam() {
            ColorScheme = RTSColorScheme.Default;

            // Teams Starts Out Empty
            unitData = new List<RTSUnitData>();
            units = new List<RTSUnit>();
            squads = new List<RTSSquad>();

            // No Input Is Available For The Team Yet
            Input = null;
        }

        // For Adding Parsed Unit Data
        public void AddUnitData(RTSUnitData t) {
            unitData.Add(t);
        }

        // Unit Addition And Removal
        public RTSUnit AddUnit(int type, Vector2 pos) {
            RTSUnit rui = new RTSUnit(this, unitData[type], pos);
            units.Add(rui);
            if(OnNewUnitSpawn != null)
                OnNewUnitSpawn(rui);
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
            squads.Add(squad);
            return squad;
        }
        public void RemoveSquad(RTSSquad u) {
            squads.Remove(u);
        }
        public void RemoveAll(Predicate<RTSSquad> f) {
            squads.RemoveAll(f);
        }
    }
}