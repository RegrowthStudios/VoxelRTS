using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSSquad {
        // This Squad's Team
        public RTSTeam Team {
            get;
            private set;
        }

        // Units In The Squad
        private List<RTSUnit> units;
        public List<RTSUnit> Units {
            get { return units; }
        }

        // The Average Position Of The Squad
        private Vector2 gridPos;
        public Vector2 GridPosition {
            get { return gridPos; }
        }

        // Events When Squad Is Altered
        public event Action<RTSSquad, RTSUnit> OnCombatantAddition;
        public event Action<RTSSquad, RTSUnit> OnCombatantRemoval;

        // The Action Controller For This Squad
        private ACSquadActionController aController;
        public ACSquadActionController ActionController {
            get { return aController; }
            set {
                aController = value;
                if(aController != null)
                    aController.SetSquad(this);
            }
        }

        // The Targetting Controller For This Squad
        private ACSquadTargettingController tController;
        public ACSquadTargettingController TargettingController {
            get { return tController; }
            set {
                tController = value;
                if(tController != null)
                    tController.SetSquad(this);
            }
        }

        public RTSSquad() {
            units = new List<RTSUnit>();
        }

        // Adds A Combatant To This Squad
        public void AddUnit(RTSUnit u) {
            // Squad Invariant Performed Here
            if(u.Squad != null) {
                u.Squad.units.Remove(u);
                if(u.Squad.OnCombatantRemoval != null)
                    u.Squad.OnCombatantRemoval(u.Squad, u);
            }

            u.Squad = this;
            units.Add(u);
            if(OnCombatantAddition != null)
                OnCombatantAddition(this, u);
        }

        // Removes All Combatants From This Squad That Match A Predicate
        public void RemoveAll(Predicate<RTSUnit> f) {
            List<RTSUnit> nUnits = new List<RTSUnit>(units.Count);
            for(int i = 0; i < units.Count; i++) {
                if(f(units[i])) {
                    if(OnCombatantRemoval != null)
                        OnCombatantRemoval(this, units[i]);
                }
                else
                    nUnits.Add(units[i]);
            }
        }

        // Should Be Done At The Beginning Of Each Frame (Only Once)
        public void RecalculateGridPosition() {
            if(units.Count > 0) {
                gridPos = Vector2.Zero;
                foreach(var u in units)
                    gridPos += u.GridPosition;
                gridPos /= units.Count;
            }
        }
    }
}