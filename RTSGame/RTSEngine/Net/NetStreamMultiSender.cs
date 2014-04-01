using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Net;
using System.Net.Sockets;
using RTSEngine.Controllers;

namespace RTSEngine.Net {
    public class NetStreamMultiSender : IDisposable {
        Socket s;
        IPAddress ipAddr;
        IPEndPoint ipEnd;

        public NetStreamMultiSender(string ip, int port) {
            s = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp);
            ipAddr = IPAddress.Parse(ip);
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.AddMembership, new MulticastOption(ipAddr));
            s.SetSocketOption(SocketOptionLevel.IP, SocketOptionName.MulticastTimeToLive, 2);

            ipEnd = new IPEndPoint(ipAddr, port);
            s.Connect(ipEnd);
        }
        public void Dispose() {
            s.Close();
            s.Dispose();
        }

        public int Send(String m) {
            byte[] b = ASCIIEncoding.Unicode.GetBytes(m);
            s.Send(b, b.Length, SocketFlags.None);
            return b.Length;
        }
    }
}
