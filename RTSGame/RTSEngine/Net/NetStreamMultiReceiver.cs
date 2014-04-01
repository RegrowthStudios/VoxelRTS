using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using RTSEngine.Controllers;

namespace RTSEngine.Net {
    public class NetStreamMultiReceiver : IDisposable {
        Socket s;
        IPAddress ipAddrRemote;
        IPEndPoint ipEndLocal;

        public NetStreamMultiReceiver(string ip, int port) {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipEndLocal = new IPEndPoint(IPAddress.Any, port);
            s.Bind(ipEndLocal);

            ipAddrRemote = IPAddress.Parse(ip);

            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ipAddrRemote, IPAddress.Any));
        }
        public void Dispose() {
            s.Close();
            s.Dispose();
        }

        public string Receive(int maxLength) {
            byte[] b = new byte[maxLength];
            int bc = s.Receive(b);
            return ASCIIEncoding.Unicode.GetString(b, 0, bc);
        }
    }
}