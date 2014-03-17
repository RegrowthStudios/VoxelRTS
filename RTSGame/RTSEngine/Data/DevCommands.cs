using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using RTSEngine.Data.Parsers;

namespace RTSEngine.Data {
    public enum DevCommandType {
        Spawn
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
}