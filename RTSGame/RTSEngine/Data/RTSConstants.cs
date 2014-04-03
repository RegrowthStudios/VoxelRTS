﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Data {
    public static class RTSConstants {
        public static readonly string[] REFERENCES = {
            "System.dll",
            "System.Core.dll",
            "System.Data.dll",
            "System.Xml.dll",
            "System.Xml.Linq.dll",
            @"lib\Microsoft.Xna.Framework.dll",
            "RTSEngine.dll"
        };

        public const float GAME_DELTA_TIME = 1f / 60f;
        public const float CGRID_SIZE = 2f;

        public const string MC_ADDR = "228.8.8.8";
        public const int MC_LOBBY_PORT = 22880;
        public const int MC_GAME_PORT_MIN = 23000;
        public const int MC_CLIENT_PORT_MIN = 23100;
    }
}
