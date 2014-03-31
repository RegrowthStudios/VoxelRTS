using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RTSEngine.Data.Team {
    public class RTSRaceData {
        public string Name;
        public List<FileInfo> UnitTypes;
        public string DefaultSquadActionController;
        public string DefaultSquadTargettingController;

        public RTSRaceData() {
            Name = null;
            UnitTypes = new List<FileInfo>();
        }
    }
}