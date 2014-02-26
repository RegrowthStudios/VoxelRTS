using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using RTSEngine.Data;
namespace RTSEngine.Interfaces {
    public delegate bool GameStateChanger(ref GameState gs);

    public interface IResourceExchanger {
        long Exchange(IResource r);
    }
}
