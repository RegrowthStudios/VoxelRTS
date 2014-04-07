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

        public readonly RTSUnitData[] units;
        public IndexedUnitType[] activeUnits;

        public readonly RTSBuildingData[] buildings;
        public IndexedBuildingType[] activeBuildings;

        public ReflectedSquadController scAction;
        public ReflectedSquadController scMovement;
        public ReflectedSquadController scTargeting;

        public RTSRace() {
            units = new RTSUnitData[MAX_UNIT_TYPES];
            activeUnits = new IndexedUnitType[0];
            buildings = new RTSBuildingData[MAX_BUILDING_TYPES];
            activeBuildings = new IndexedBuildingType[0];
        }

        public void UpdateActiveUnits() {
            int c = 0;
            for(int i = 0; i < MAX_UNIT_TYPES; i++) {
                if(units[i] != null) c++;
            }
            activeUnits = new IndexedUnitType[c];
            c = 0;
            for(int i = 0; i < MAX_UNIT_TYPES; i++) {
                if(units[i] != null) activeUnits[c++] = new IndexedUnitType(i, units[i]);
            }
        }
        public void UpdateActiveBuildings() {
            int c = 0;
            for(int i = 0; i < MAX_BUILDING_TYPES; i++) {
                if(buildings[i] != null) c++;
            }
            activeBuildings = new IndexedBuildingType[c];
            c = 0;
            for(int i = 0; i < MAX_BUILDING_TYPES; i++) {
                if(buildings[i] != null) activeBuildings[c++] = new IndexedBuildingType(i, buildings[i]);
            }
        }
    }
}
