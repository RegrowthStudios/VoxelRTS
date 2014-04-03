using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using RTSEngine.Controllers;

namespace RTSEngine.Net {
    public class NetStreamMultiSender : IDisposable {
        UdpClient client;
        IPEndPoint ipeRemote;

        public NetStreamMultiSender(string ip, int port) {
            client = new UdpClient();
            IPAddress multicastaddress = IPAddress.Parse(ip);
            client.JoinMulticastGroup(multicastaddress);
            ipeRemote = new IPEndPoint(multicastaddress, port);
            client.Connect(ipeRemote);
        }
        public void Dispose() {
            client.Close();
        }

        public void Send(String m) {
            byte[] b = Encoding.Unicode.GetBytes(m);
            client.Send(b, b.Length, ipeRemote);
        }
    }
}
