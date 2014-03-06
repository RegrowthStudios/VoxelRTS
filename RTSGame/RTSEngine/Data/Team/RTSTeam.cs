using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSTeam {
        // Team Color
        public Color Color {
            get;
            set;
        }
        
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

        public RTSTeam() {
            Color = Color.White;
            units = new List<RTSUnitInstance>();
            squads = new List<RTSSquad>();
        }

        public RTSUnitInstance AddUnit(RTSUnit data, Vector3 pos) {
            RTSUnitInstance rui = new RTSUnitInstance(this, data, pos);
            units.Add(rui);
            return rui;
        }

        public void RemoveUnit(RTSUnitInstance u) {
            units.Remove(u);
        }
        public void RemoveAll(Predicate<RTSUnitInstance> f) {
            units.RemoveAll(f);
        }

        public RTSSquad AddSquad() {
            RTSSquad s = new RTSSquad();
            squads.Add(s);
            return s;
        }
    }
}