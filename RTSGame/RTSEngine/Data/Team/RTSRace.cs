using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Controllers;

namespace RTSEngine.Data.Team {
    public struct IndexedUnitType {
        public readonly int Index;
        public readonly RTSUnitData Data;

        public IndexedUnitType(int i, RTSUnitData d) {
            Index = i;
            Data = d;
        }
    }
    public struct IndexedBuildingType {
        public readonly int Index;
        public readonly RTSBuildingData Data;

        public IndexedBuildingType(int i, RTSBuildingData d) {
            Index = i;
            Data = d;
        }
    }

    public class RTSRace {
        public const int MAX_UNIT_TYPES = 24;
        public const int MAX_BUILDING_TYPES = 36;

        public string FriendlyName;

        public readonly RTSUnitData[] Units;
        public IndexedUnitType[] ActiveUnits;

        public readonly RTSBuildingData[] Buildings;
        public IndexedBuildingType[] ActiveBuildings;

        public ReflectedSquadController SCAction;
        public ReflectedSquadController SCMovement;
        public ReflectedSquadController SCTargeting;

        public RTSRace() {
            Units = new RTSUnitData[MAX_UNIT_TYPES];
            ActiveUnits = new IndexedUnitType[0];
            Buildings = new RTSBuildingData[MAX_BUILDING_TYPES];
            ActiveBuildings = new IndexedBuildingType[0];
        }

        public void UpdateActiveUnits() {
            int c = 0;
            for(int i = 0; i < MAX_UNIT_TYPES; i++) {
                if(Units[i] != null) c++;
            }
            ActiveUnits = new IndexedUnitType[c];
            c = 0;
            for(int i = 0; i < MAX_UNIT_TYPES; i++) {
                if(Units[i] != null) ActiveUnits[c++] = new IndexedUnitType(i, Units[i]);
            }
        }
        public void UpdateActiveBuildings() {
            int c = 0;
            for(int i = 0; i < MAX_BUILDING_TYPES; i++) {
                if(Buildings[i] != null) c++;
            }
            ActiveBuildings = new IndexedBuildingType[c];
            c = 0;
            for(int i = 0; i < MAX_BUILDING_TYPES; i++) {
                if(Buildings[i] != null) ActiveBuildings[c++] = new IndexedBuildingType(i, Buildings[i]);
            }
        }
    }
}
