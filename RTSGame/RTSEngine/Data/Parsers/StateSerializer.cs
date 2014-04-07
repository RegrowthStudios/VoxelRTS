using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization.Formatters.Binary;
using System.Text;
using RTSEngine.Data.Team;
using Microsoft.Xna.Framework;

namespace RTSEngine.Data.Parsers {
    public static class StateSerializer {
        public static void Serialize(BinaryWriter s, GameState state) {
            s.Write(state.CurrentFrame);
            s.Write(state.TotalGameTime);
            s.Write(UUIDGenerator.GetUUID());
            s.Write(state.UnitControllers.Count);
            foreach(var key in state.UnitControllers.Keys) {
                s.Write(key);
            }
            s.Write(state.BuildingControllers.Count);
            foreach(var key in state.BuildingControllers.Keys) {
                s.Write(key);
            }
            s.Write(state.activeTeams.Length);
            foreach(var at in state.activeTeams) {
                s.Write(at.Index);
                Serialize(s, at.Team);
            }
        }
        private static void Serialize(BinaryWriter s, RTSTeam team) {
            RTSRace.Serialize(s, team.race);
            s.Write((int)team.Input.Type);
            team.Input.Serialize(s);
            s.Write(team.ColorScheme.Name);
            s.Write(team.ColorScheme.Primary);
            s.Write(team.ColorScheme.Secondary);
            s.Write(team.ColorScheme.Tertiary);
            s.Write(team.Buildings.Count);
            foreach(var building in team.Buildings) {
                RTSBuilding.Serialize(s, building);
            }
            s.Write(team.Units.Count);
            foreach(var unit in team.Units) {
                RTSUnit.Serialize(s, unit);
            }
            s.Write(team.Squads.Count);
            foreach(var squad in team.Squads) {
                RTSSquad.Serialize(s, squad);
            }
        }
    }
}