using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;
using RTSEngine.Net;
using Lidgren.Network;

namespace BandWidthTest {
    class Program {
        public static int ParseInt(string question) {
            int i;
            while(true) {
                Console.WriteLine(question);
                if(int.TryParse(Console.ReadLine(), out i)) return i;
            }
        }
        public static IPAddress ParseAddr(string question) {
            IPAddress i;
            while(true) {
                Console.WriteLine(question);
                if(IPAddress.TryParse(Console.ReadLine(), out i)) return i;
            }
        }

        static string message = "";

        private static string GetPublicIpAddress() {
            var request = (HttpWebRequest)WebRequest.Create("http://checkip.dyndns.org/");
            string publicIPAddress;
            request.Method = "GET";
            using(WebResponse response = request.GetResponse()) {
                using(var reader = new System.IO.StreamReader(response.GetResponseStream())) {
                    publicIPAddress = reader.ReadToEnd();
                }
            }
            string addr = publicIPAddress.Split(new char[] { ':' })[1].Split(new char[] { '<' })[0];
            return addr.Trim();
        }

        static void Main(string[] args) {
            IPHostEntry ipHostInfo = Dns.GetHostEntry(Dns.GetHostName());
            foreach(IPAddress addr in ipHostInfo.AddressList) {
                Console.WriteLine("{0}", addr);
            }
            bool entered = false;
            bool isListener = false;
            while(!entered) {
                Console.WriteLine("Is Listener?");
                switch(Console.ReadLine().ToLower()) {
                    case "y": isListener = true; entered = true; break;
                    case "n": isListener = false; entered = true; break;
                }
            }

            if(isListener) {
                while(true) {
                    SendMCL();
                    Thread.Sleep(100);
                }
            }
            else {
                message = null;
                Thread tc = new Thread(SendMCS);
                tc.Start();
                while(message == null) Thread.Sleep(100);
                while(true) {
                    Console.WriteLine("New Message:");
                    message = Console.ReadLine();
                }
            }
        }

        static NetPeer peer;
        static NetConnection peerConnect;
        public static void SendMCL() {
            Console.WriteLine("Enter Port: ");
            int port = int.Parse(Console.ReadLine());

            NetPeerConfiguration config = new NetPeerConfiguration("InduZtry");
            config.Port = port;
            config.EnableMessageType(NetIncomingMessageType.DiscoveryResponse);
            config.EnableMessageType(NetIncomingMessageType.Data);
            config.AcceptIncomingConnections = true;
            //config.EnableUPnP = true;
            peer = new NetPeer(config);
            peer.Start();
            //Console.WriteLine("UPnP IP: " + peer.UPnP.GetExternalIP());

            Thread t = new Thread(RecvMCL);
            t.IsBackground = true;
            t.Start();

            //Console.WriteLine("Enter Server IP: ");
            //string sip = Console.ReadLine();
            Console.WriteLine("Enter Server Port: ");
            port = int.Parse(Console.ReadLine());
            //peer.DiscoverKnownPeer(sip, port);
            peer.DiscoverLocalPeers(port);
            //while(true) {
            //Thread.Sleep(10);
            //Console.WriteLine("Port: ");
            //int rport = int.Parse(Console.ReadLine());
            //peer.DiscoverLocalPeers(rport);
            //}
        }
        public static void RecvMCL() {
            while(true) {
                Thread.Sleep(10);
                NetIncomingMessage msg;
                while((msg = peer.ReadMessage()) != null) {
                    switch(msg.MessageType) {
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            Console.WriteLine(msg.ReadString());
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            Console.WriteLine(((NetConnectionStatus)msg.ReadByte()).ToString());
                            break;
                        case NetIncomingMessageType.DiscoveryResponse:
                            Console.WriteLine("Found server at " + msg.SenderEndPoint + " name: " + msg.ReadString());
                            peerConnect = peer.Connect(msg.SenderEndPoint);
                            break;
                        case NetIncomingMessageType.Data:
                            Console.WriteLine(msg.LengthBytes);
                            break;
                        default:
                            Console.WriteLine("Unhandled type: " + msg.MessageType);
                            break;
                    }
                }
            }
        }

        static NetConnection recipient;
        static NetPeer server;
        public static void SendMCS() {
            Console.WriteLine("Enter Port: ");
            int port = int.Parse(Console.ReadLine());

            NetPeerConfiguration config = new NetPeerConfiguration("InduZtry");
            config.Port = port;
            config.EnableMessageType(NetIncomingMessageType.DiscoveryRequest);
            config.EnableMessageType(NetIncomingMessageType.Data);
            //config.EnableUPnP = true;
            server = new NetPeer(config);
            server.Start();
            //Console.WriteLine(server.UPnP.Status);
            //server.UPnP.ForwardPort(port, "InduZtry");
            //Console.WriteLine(server.UPnP.Status);
            //Console.WriteLine("UPnP IP: " + server.UPnP.GetExternalIP());

            Thread t = new Thread(RecvMCS);
            t.IsBackground = true;
            t.Start();

            message = "NULL";

            while(true) {
                Thread.Sleep(10);
                if(recipient != null) {
                    NetOutgoingMessage sendMsg = server.CreateMessage();
                    sendMsg.Write(message);
                    server.SendMessage(sendMsg, recipient, NetDeliveryMethod.ReliableOrdered);
                }
            }
        }
        public static void RecvMCS() {
            while(true) {
                Thread.Sleep(10);
                NetIncomingMessage msg;
                while((msg = server.ReadMessage()) != null) {
                    switch(msg.MessageType) {
                        case NetIncomingMessageType.VerboseDebugMessage:
                        case NetIncomingMessageType.DebugMessage:
                        case NetIncomingMessageType.WarningMessage:
                        case NetIncomingMessageType.ErrorMessage:
                            Console.WriteLine(msg.ReadString());
                            break;
                        case NetIncomingMessageType.StatusChanged:
                            Console.WriteLine(((NetConnectionStatus)msg.ReadByte()).ToString());
                            break;
                        case NetIncomingMessageType.DiscoveryRequest:
                            NetOutgoingMessage response = server.CreateMessage();
                            response.Write("Server InduZtry");
                            server.SendDiscoveryResponse(response, msg.SenderEndPoint);
                            recipient = server.Connect(msg.SenderEndPoint);
                            Console.WriteLine("Discovered Peer " + msg.SenderEndPoint);
                            break;
                        default:
                            Console.WriteLine("Unhandled type: " + msg.MessageType);
                            break;
                    }
                    server.Recycle(msg);
                }
            }
        }
    }
}