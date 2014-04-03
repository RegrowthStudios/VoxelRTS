using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using RTSEngine.Controllers;
using RTSEngine.Data;

namespace RTSEngine.Net {
    public class NetStateController : IDisposable {
        bool playing;
        NetStreamMultiReceiver recv;
        Thread tNet;

        public NetStateController() {
            recv = new NetStreamMultiReceiver(RTSConstants.MC_ADDR, RTSConstants.MC_GAME_PORT_MIN);
            tNet = new Thread(NetThread);
            tNet.Priority = ThreadPriority.BelowNormal;
            tNet.TrySetApartmentState(ApartmentState.MTA);
            tNet.IsBackground = true;
        }
        public void Dispose() {
            playing = false;
            tNet.Join();
            recv.Dispose();
        }

        public void Control(GameState s) {
            playing = true;
            tNet.Start();
        }

        void NetThread() {
            while(playing) {
                string c = recv.Receive();
                if(!string.IsNullOrWhiteSpace(c)) {
                    DevConsole.AddCommand(c);
                }
            }
        }
    }
}
