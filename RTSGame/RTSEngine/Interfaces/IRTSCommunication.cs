using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace RTSEngine.Interfaces {
    public interface IRTSInputSender {
    }
    public interface IRTSInputReceiver {
    }

    public interface IRTSVisualizationSender {
    }
    public interface IRTSVisualizationReceiver {
    }

    public interface IRTSClient : IRTSInputSender, IRTSVisualizationReceiver {
    }
    public interface IRTSServer : IRTSInputReceiver, IRTSVisualizationSender {
    }
}
