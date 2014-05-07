using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RTSEngine.Controllers;
using RTSEngine.Data.Parsers;

namespace RTSEngine.Data.Team {
    public class RTSRace {
        public static void Serialize(BinaryWriter s, RTSRace race) {
            s.Write(race.FriendlyName);
            s.Write(race.ActiveUnits.Length);
            foreach(var d in race.ActiveUnits) {
                s.Write(d.Index);
                RTSUnitData.Serialize(s, d);
            }
            s.Write(race.ActiveBuildings.Length);
            foreach(var d in race.ActiveBuildings) {
                s.Write(d.Index);
                RTSBuildingData.Serialize(s, d);
            }
            s.Write(race.SCAction.TypeName);
            s.Write(race.SCMovement.TypeName);
            s.Write(race.SCTargeting.TypeName);
        }
        public static RTSRace Deserialize(BinaryReader s, GameState state) {
            RTSRace race = new RTSRace();
            race.FriendlyName = s.ReadString();
            int c = s.ReadInt32();
            for(int i = 0; i < c; i++) {
                int ui = s.ReadInt32();
                race.Units[ui] = RTSUnitData.Deserialize(s, state, ui);
            }
            race.UpdateActiveUnits();
            c = s.ReadInt32();
            for(int i = 0; i < c; i++) {
                int bi = s.ReadInt32();
                race.Buildings[bi] = RTSBuildingData.Deserialize(s, state, bi);
            }
            race.UpdateActiveBuildings();
            race.SCAction = state.Scripts[s.ReadString()];
            race.SCMovement = state.Scripts[s.ReadString()];
            race.SCTargeting = state.Scripts[s.ReadString()];
            return race;
        }

        public const int MAX_UNIT_TYPES = 24;
        public const int MAX_BUILDING_TYPES = 36;

        [ZXParse("Name")]
        public string FriendlyName;
        public FileInfo InfoFile;

        public readonly RTSUnitData[] Units;
        public RTSUnitData[] ActiveUnits;

        public readonly RTSBuildingData[] Buildings;
        public RTSBuildingData[] ActiveBuildings;

        public ReflectedScript SCAction;
        public ReflectedScript SCMovement;
        public ReflectedScript SCTargeting;

        public RTSRace() {
            Units = new RTSUnitData[MAX_UNIT_TYPES];
            ActiveUnits = new RTSUnitData[0];
            Buildings = new RTSBuildingData[MAX_BUILDING_TYPES];
            ActiveBuildings = new RTSBuildingData[0];
        }

        public void UpdateActiveUnits() {
            int c = 0;
            for(int i = 0; i < MAX_UNIT_TYPES; i++) {
                if(Units[i] != null) c++;
            }
            ActiveUnits = new RTSUnitData[c];
            c = 0;
            for(int i = 0; i < MAX_UNIT_TYPES; i++) {
                if(Units[i] != null) ActiveUnits[c++] = Units[i];
            }
        }
        public void UpdateActiveBuildings() {
            int c = 0;
            for(int i = 0; i < MAX_BUILDING_TYPES; i++) {
                if(Buildings[i] != null) c++;
            }
            ActiveBuildings = new RTSBuildingData[c];
            c = 0;
            for(int i = 0; i < MAX_BUILDING_TYPES; i++) {
                if(Buildings[i] != null) ActiveBuildings[c++] = Buildings[i];
            }
        }

        public void SetController(Dictionary<string, ReflectedScript> d, string cType, string key) {
            switch(cType.Trim().ToLower()) {
                case "act":
                case "action":
                    d.TryGetValue(key, out SCAction);
                    break;
                case "move":
                case "movement":
                case "motion":
                    d.TryGetValue(key, out SCMovement);
                    break;
                case "target":
                case "targeting":
                    d.TryGetValue(key, out SCTargeting);
                    break;
            }
        }
        public void LoadUnit(Dictionary<string, ReflectedScript> d, int index, string rootPath, string file) {
            Units[index] = RTSUnitDataParser.ParseData(d, new FileInfo(Path.Combine(rootPath, file)), index);
        }
        public void LoadBuilding(Dictionary<string, ReflectedScript> d, int index, string rootPath, string file) {
            Buildings[index] = RTSBuildingDataParser.ParseData(d, new FileInfo(Path.Combine(rootPath, file)), index);
        }
    }
}