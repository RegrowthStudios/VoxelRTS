using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSSquad : ISquad {

        // This Squad's Team
        public RTSTeam Team {
            get;
            private set;
        }

        //List of Combatants In The Squad
        private List<ICombatEntity> combatants;
        public IEnumerable<ICombatEntity> Combatants {
            get { return combatants; }
        }

        //Number of Combatants In The Squad
        public int EntityCount {
            get { return combatants.Count; }
        }

        // The Average Position Of The Squad
        private Vector2 gridPos;
        public Vector2 GridPosition {
            get { return gridPos; }
        }

        // Events When Squad Is Altered
        public event Action<ISquad, ICombatEntity> OnCombatantAddition;

        public event Action<ISquad, ICombatEntity> OnCombatantRemoval;

        // The Targetting Controller For This Squad
        private ITargettingController tController;
        public ITargettingController TargettingController {
            get { return tController; }
            set {
                tController = value;
                if(tController != null)
                    tController.SetSquad(this);
            }
        }

        // Adds A Combatant To This Squad
        public void AddCombatant(ICombatEntity e) {
            combatants.Add(e);
            if(OnCombatantAddition != null)
                OnCombatantAddition(this,e);
        }

        // Removes All Combatants From This Squad That Match A Predicate
        public void RemoveAll(Predicate<ICombatEntity> f) {
            foreach(var c in combatants) {
                if(f(c)) {
                    combatants.Remove(c);
                    if(OnCombatantRemoval != null)
                        OnCombatantRemoval(this, c);
                }
            }
        }

        // Should Be Done At The Beginning Of Each Frame (Only Once)
        public void RecalculateGridPosition() {
            float sumX = 0;
            float sumY = 0;
            foreach(var c in Combatants){
                sumX += c.GridPosition.X;
                sumY += c.GridPosition.Y;
            }
            gridPos.X = sumX / EntityCount;
            gridPos.Y = sumY / EntityCount;
        }
    }

}
