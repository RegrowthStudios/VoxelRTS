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

        public InputController Input {
            get;
            set;
        }

        // Events
        public event Action<RTSUnit> OnUnitSpawn;
        public event Action<RTSSquad> OnSquadCreation;

        public RTSTeam() {
            ColorScheme = RTSColorScheme.Default;

            // Teams Starts Out Empty
            race = new RTSRace();
            units = new List<RTSUnit>();
            squads = new List<RTSSquad>();

            // No Input Is Available For The Team Yet
            Input = null;
        }

        // Unit Addition And Removal
        public RTSUnit AddUnit(int type, Vector2 pos) {
            RTSUnit rui = new RTSUnit(this, race.units[type], pos);
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
    }
}