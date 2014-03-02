using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;
using RTSEngine.Data.Team;

namespace RTSCS {
    public interface IDataForm {
        event Action<GameRestartArgs> OnGameRestart;
    }
}
