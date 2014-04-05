using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using RTSEngine.Data;

namespace RTSEngine.Controllers {
    public abstract class GameTypeController : IDisposable {
        // The Same File As The Map File
        public abstract void Load(GameState s, FileInfo infoFile);

        public abstract int? GetVictoriousTeam(GameState s);

        public void Start(GameState s) {

        }
        public void Dispose() {

        }
        
        public void UpdateThread() {

        }
        public abstract void Tick(GameState s);

        public abstract void Serialize(Stream s);
        public abstract void Deserialize(Stream s);

    }
}