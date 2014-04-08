using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RTSEngine.Data.Parsers;

namespace RTSEngine.Data {
    public enum DevCommandType {
        Spawn,
        StopMotion,
        KillUnits,
        KillBuildings,
        FOW,
        Save
    }
    public class DevCommand {
        public DevCommandType Type {
            get;
            private set;
        }

        public DevCommand(DevCommandType t) {
            Type = t;
        }
    }

    // Spawn Command
    public class DevCommandSpawn : DevCommand {
        public static readonly Regex REGEX = RegexHelper.Generate("spawn",
            RegexHelper.DATA_INT_REGEX + RegexHelper.NUM_SPLIT +
            RegexHelper.DATA_INT_REGEX + RegexHelper.NUM_SPLIT +
            RegexHelper.DATA_INT_REGEX + RegexHelper.NUM_SPLIT +
            RegexHelper.DATA_NUM_REGEX + RegexHelper.NUM_SPLIT +
            RegexHelper.DATA_NUM_REGEX
            );
        public static bool TryParse(string c, out DevCommand command) {
            Match m = REGEX.Match(c);
            if(m.Success) {
                string[] split = Regex.Split(m.Groups[1].Value, RegexHelper.NUM_SPLIT);
                DevCommandSpawn comm = new DevCommandSpawn();
                comm.TeamIndex = int.Parse(split[0]);
                comm.UnitIndex = int.Parse(split[1]);
                comm.Count = int.Parse(split[2]);
                comm.X = float.Parse(split[3]);
                comm.Z = float.Parse(split[4]);
                command = comm;
                return true;
            }
            command = null;
            return false;
        }

        public int TeamIndex;
        public int UnitIndex;
        public int Count;
        public float X;
        public float Z;

        public DevCommandSpawn()
            : base(DevCommandType.Spawn) {
        }
    }
    // Stop Command
    public class DevCommandStopMotion : DevCommand {
        public static readonly Regex REGEX = new Regex(@"stop\s+motion");

        public DevCommandStopMotion()
            : base(DevCommandType.StopMotion) {
        }

        public static bool TryParse(string c, out DevCommand command) {
            Match m = REGEX.Match(c);
            if(m.Success) {
                command = new DevCommandStopMotion();
                return true;
            }
            command = null;
            return false;
        }
    }
    // Kill Command
    public class DevCommandKillUnits : DevCommand {
        public static readonly Regex REGEX = new Regex(@"avada\s+kedavra");

        public DevCommandKillUnits()
            : base(DevCommandType.KillUnits) {
        }

        public static bool TryParse(string c, out DevCommand command) {
            Match m = REGEX.Match(c);
            if(m.Success) {
                command = new DevCommandKillUnits();
                return true;
            }
            command = null;
            return false;
        }
    }
    // Kill Command
    public class DevCommandKillBuildings : DevCommand {
        public static readonly Regex REGEX = new Regex(@"i\s+am\s+god");

        public DevCommandKillBuildings()
            : base(DevCommandType.KillBuildings) {
        }

        public static bool TryParse(string c, out DevCommand command) {
            Match m = REGEX.Match(c);
            if(m.Success) {
                command = new DevCommandKillBuildings();
                return true;
            }
            command = null;
            return false;
        }
    }
    // FOW Command
    public class DevCommandFOW : DevCommand {
        public static readonly Regex REGEX = new Regex(@"franz\s+ferdinand");

        public DevCommandFOW()
            : base(DevCommandType.FOW) {
        }

        public static bool TryParse(string c, out DevCommand command) {
            Match m = REGEX.Match(c);
            if(m.Success) {
                command = new DevCommandFOW();
                return true;
            }
            command = null;
            return false;
        }
    }
    // Save Command
    public class DevCommandSave : DevCommand {
        public static readonly Regex REGEX = RegexHelper.GenerateFile("save");

        public readonly FileInfo file;

        public DevCommandSave(FileInfo f)
            : base(DevCommandType.Save) {
            file = f;
        }

        public static bool TryParse(string c, out DevCommand command) {
            Match m = REGEX.Match(c);
            if(m.Success) {
                string cwd = Directory.GetCurrentDirectory();
                command = new DevCommandSave(RegexHelper.ExtractFile(m, cwd));
                return true;
            }
            command = null;
            return false;
        }
    }
}