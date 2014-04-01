using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using RTSEngine.Controllers;

namespace RTSEngine.Net {
    public class NetStreamMultiReceiver : IDisposable {
        UdpClient client;
        IPEndPoint ipeLocal;

        public NetStreamMultiReceiver(string ip, int port) {
            client = new UdpClient();
            client.ExclusiveAddressUse = false;
            ipeLocal = new IPEndPoint(IPAddress.Any, port);
            client.Client.ReceiveTimeout = 1000;
            client.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            client.ExclusiveAddressUse = false;
            client.Client.Bind(ipeLocal);
            IPAddress multicastaddress = IPAddress.Parse(ip);
            client.JoinMulticastGroup(multicastaddress);
        }
        public void Dispose() {
            client.Close();
        }

        public string Receive() {
            byte[] b = client.Receive(ref ipeLocal);
            return Encoding.Unicode.GetString(b);
        }
    }
}