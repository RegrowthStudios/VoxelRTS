using RTSEngine.Data;
using RTSEngine.Data.Team;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Controllers
{
    public class EnvironmentInputController : InputController
    {

        public EnvironmentInputController(GameState g, RTSTeam t)
            : base(g, t) {

        }

        public override void Dispose() {
            throw new NotImplementedException();
        }


        

    }
}
