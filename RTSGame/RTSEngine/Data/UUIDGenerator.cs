using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Data {
    public static class UUIDGenerator {
        private static int curID = 0;
        public static void SetUUID(int id) {
            curID = id;
        }
        public static int GetUUID() {
            int id = curID;
            curID++;
            return id;
        }
    }
}