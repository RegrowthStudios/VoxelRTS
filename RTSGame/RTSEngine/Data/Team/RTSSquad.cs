using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Xna.Framework;
using RTSEngine.Interfaces;

namespace RTSEngine.Data.Team {
    public class RTSSquad {

        private List<RTSUnitInstance> squad;

        public RTSSquad() {
            squad = new List<RTSUnitInstance>();

        }

        public void Add(RTSUnitInstance unit){
            squad.Add(unit);
        }



    }
}
