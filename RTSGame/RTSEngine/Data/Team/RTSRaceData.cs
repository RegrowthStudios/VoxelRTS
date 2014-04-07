using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;

namespace RTSEngine.Data.Team {
    public class RTSRaceData {
        public string Name;
        public FileInfo InfoFile;
        public List<FileInfo> UnitTypes;
        public List<FileInfo> BuildingTypes;
        public string DefaultSquadActionController;
        public string DefaultSquadMovementController;
        public string DefaultSquadTargetingController;

        public RTSRaceData() {
            UnitTypes = new List<FileInfo>();
            BuildingTypes = new List<FileInfo>();
        }
    }
}