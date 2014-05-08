using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;

namespace BEU.Data {
    public class Team {
        public const int TYPE_PLAYER = 0;
        public const int TYPE_COMPUTER = 1;

        // Index Into Game State
        public int Index {
            get;
            private set;
        }
        public int Type {
            get;
            private set;
        }

        // Team Race
        public Race Race {
            get;
            set;
        }

        // Population Information
        private int population;
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

        private List<Tank> units;
        public List<Tank> Units {
            get { return units; }
        }

        public event Action<Team, Tank> OnSpawn;
        public event Action<Team, int> OnPopulationChange;

        public Team(int i, int t, Race r) {
            Index = i;
            Type = t;
            Race = r;
            population = 0;
            units = new List<Tank>();
        }

        // Unit Addition And Removal
        public Tank AddUnit(int type, Vector2 pos) {
            // Check For Unit Type Existence
            TankData data = Race.TankTypes[type];
            if(data == null) return null;

            // Check For Unit Cap
            if(data.Population >= data.MaxPopulation) return null;

            // Produce Unit
            Population++;

            // Create Unit
            Tank unit = new Tank(this, data, pos);
            Units.Add(unit);
            if(OnSpawn != null)
                OnSpawn(this, unit);
            return unit;
        }
        public void RemoveAll(Predicate<Tank> f) {
            var nu = new List<Tank>(units.Count);
            int pc = 0;
            for(int i = 0; i < units.Count; i++) {
                if(f(units[i])) pc++;
                else nu.Add(units[i]);
            }
            if(pc != 0) Population -= pc;
            System.Threading.Interlocked.Exchange(ref units, nu);
        }
    }
}