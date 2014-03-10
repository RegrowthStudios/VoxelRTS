using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Net;
using System.Net.Sockets;

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
                    RunServer();
                }
            }
            else {
                message = null;
                Thread tc = new Thread(RunClient);
                tc.Start();
                while(message == null) Thread.Sleep(100);
                while(true) {
                    Console.WriteLine("New Message:");
                    message = Console.ReadLine();
                }
            }
        }

        public static void RunClient() {
            try {
                IPAddress addrLocal = ParseAddr("Local IP:");
                int portLocal = ParseInt("Local Port:");
                IPEndPoint lep = new IPEndPoint(addrLocal, portLocal);

                IPAddress addrRemote = ParseAddr("Remote IP:");
                int portRemote = ParseInt("Remote Port:");
                IPEndPoint rep = new IPEndPoint(addrRemote, portRemote);

                message = "";

                UdpClient sock = new UdpClient(lep);
                try {
                    sock.Connect(rep);
                    Console.WriteLine("Socket connected");
                    ulong num = 0u;
                    byte[] data = new byte[sizeof(ulong) * 1024 * 4];
                    while(true) {
                        BitConverter.GetBytes(num).CopyTo(data, 0);
                        num++;

                        byte[] mb = ASCIIEncoding.ASCII.GetBytes(message);
                        BitConverter.GetBytes(mb.LongLength).CopyTo(data, 8);

                        mb.CopyTo(data, 16);

                        sock.Send(data, data.Length);
                        Thread.Sleep(1);
                    }
                }
                catch(ArgumentNullException ane) {
                    Console.WriteLine("ArgumentNullException : {0}", ane.ToString());
                    Console.Read();
                }
                catch(SocketException se) {
                    Console.WriteLine("SocketException : {0}", se.ToString());
                    Console.Read();
                }
                catch(Exception e) {
                    Console.WriteLine("Unexpected exception : {0}", e.ToString());
                    Console.Read();
                }
            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
            }
        }
        public static void RunServer() {
            IPAddress addrListen = ParseAddr("Server IP:");
            int portListen = ParseInt("Server Port:");
            IPEndPoint sep = new IPEndPoint(addrListen, portListen);

            UdpClient listener = new UdpClient(sep);
            try {
                IPEndPoint rep = null;
                listener.Client.ReceiveTimeout = 10000;
                Console.WriteLine("Listening...");
                long dataRecv = 0u;
                while(true) {
                    byte[] data = listener.Receive(ref rep);
                    ulong num = data == null || data.Length < 8 ? 0u : BitConverter.ToUInt64(data, 0);
                    long sl = BitConverter.ToInt64(data, 8);
                    string s = ASCIIEncoding.ASCII.GetString(data, 16, (int)sl);
                    dataRecv += data == null ? 0 : data.LongLength;
                    Console.WriteLine("{0,12} - {1,8} - {2}", dataRecv, num, s);
                }
            }
            catch(Exception e) {
                Console.WriteLine(e.Message);
            }
            Console.WriteLine("Ended");
        }
    }
}

namespace Woodchop.Net {
    public static class UPnP {
        public static bool OpenFirewallPort(int port) {
            System.Net.NetworkInformation.NetworkInterface[] nics = System.Net.NetworkInformation.NetworkInterface.GetAllNetworkInterfaces();

            //for each nic in computer...
            foreach(System.Net.NetworkInformation.NetworkInterface nic in nics) {
                try {
                    string machineIP = nic.GetIPProperties().UnicastAddresses[0].Address.ToString();

                    //send msg to each gateway configured on this nic
                    foreach(System.Net.NetworkInformation.GatewayIPAddressInformation gwInfo in nic.GetIPProperties().GatewayAddresses) {
                        try {
                            OpenFirewallPort(machineIP, gwInfo.Address.ToString(), port);
                        }
                        catch { }
                    }
                }
                catch { }
            }
            return true;
        }
        public static void OpenFirewallPort(string machineIP, string firewallIP, int openPort) {
            string svc = getServicesFromDevice(firewallIP);
            openPortFromService(svc, "urn:schemas-upnp-org:service:WANIPConnection:1", machineIP, firewallIP, 5152, openPort);
            openPortFromService(svc, "urn:schemas-upnp-org:service:WANPPPConnection:1", machineIP, firewallIP, 5152, openPort);
        }
        private static string getServicesFromDevice(string firewallIP) {
            //To send a broadcast and get responses from all, send to 239.255.255.250
            string queryResponse = "";
            try {
                string query = "M-SEARCH * HTTP/1.1\r\n" +
                "Host:" + firewallIP + ":1900\r\n" +
                "ST:upnp:rootdevice\r\n" +
                "Man:\"ssdp:discover\"\r\n" +
                "MX:3\r\n" +
                "\r\n" +
                "\r\n";

                //use sockets instead of UdpClient so we can set a timeout easier
                Socket client = new Socket(AddressFamily.InterNetwork,
                SocketType.Dgram, ProtocolType.Udp);
                IPEndPoint endPoint = new
                IPEndPoint(IPAddress.Parse(firewallIP), 1900);

                //1.5 second timeout because firewall should be on same segment (fast)
                client.SetSocketOption(SocketOptionLevel.Socket,
                SocketOptionName.ReceiveTimeout, 1500);

                byte[] q = Encoding.ASCII.GetBytes(query);
                client.SendTo(q, q.Length, SocketFlags.None, endPoint);
                IPEndPoint sender = new IPEndPoint(IPAddress.Any, 0);
                EndPoint senderEP = (EndPoint)sender;

                byte[] data = new byte[1024];
                int recv = client.ReceiveFrom(data, ref senderEP);
                queryResponse = Encoding.ASCII.GetString(data);
            }
            catch { }

            if(queryResponse.Length == 0)
                return "";
            /* QueryResult is somthing like this:
            *
            HTTP/1.1 200 OK
            Cache-Control:max-age=60
            Location:http://10.10.10.1:80/upnp/service/des_ppp.xml
            Server:NT/5.0 UPnP/1.0
            ST:upnp:rootdevice
            EXT:

            USN:uuid:upnp-InternetGatewayDevice-1_0-00095bd945a2::upnp:rootdevice
            */

            string location = "";
            string[] parts = queryResponse.Split(new string[] {
System.Environment.NewLine }, StringSplitOptions.RemoveEmptyEntries);
            foreach(string part in parts) {
                if(part.ToLower().StartsWith("location")) {
                    location = part.Substring(part.IndexOf(':') + 1);
                    break;
                }
            }
            if(location.Length == 0)
                return "";

            //then using the location url, we get more information:

            System.Net.WebClient webClient = new WebClient();
            try {
                string ret = webClient.DownloadString(location);
                return ret;//return services
            }
            catch(System.Exception) {
            }
            finally {
                webClient.Dispose();
            }
            return "";
        }
        private static void openPortFromService(string services, string serviceType, string machineIP, string firewallIP, int gatewayPort, int portToForward) {
            if(services.Length == 0)
                return;
            int svcIndex = services.IndexOf(serviceType);
            if(svcIndex == -1)
                return;
            string controlUrl = services.Substring(svcIndex);
            string tag1 = "<controlURL>";
            string tag2 = "</controlURL>";
            controlUrl = controlUrl.Substring(controlUrl.IndexOf(tag1)
            + tag1.Length);
            controlUrl =
            controlUrl.Substring(0, controlUrl.IndexOf(tag2));
            string soapBody = "<s:Envelope " +
            "xmlns:s=\"http://schemas.xmlsoap.org/soap/envelope/ \" " +

            "s:encodingStyle=\"http://schemas.xmlsoap.org/soap/encoding/ \">" +
            "<s:Body>" +
            "<u:AddPortMapping xmlns:u=\"" + serviceType + "\">" +
            "<NewRemoteHost></NewRemoteHost>" +
            "<NewExternalPort>" + portToForward.ToString() +
            "</NewExternalPort>" +
            "<NewProtocol>TCP</NewProtocol>" +
            "<NewInternalPort>" + portToForward.ToString() +
            "</NewInternalPort>" +
            "<NewInternalClient>" + machineIP +
            "</NewInternalClient>" +
            "<NewEnabled>1</NewEnabled>" +
            "<NewPortMappingDescription>Woodchop Client</NewPortMappingDescription>" +
            "<NewLeaseDuration>0</NewLeaseDuration>" +
            "</u:AddPortMapping>" +
            "</s:Body>" +
            "</s:Envelope>";

            byte[] body =
            System.Text.UTF8Encoding.ASCII.GetBytes(soapBody);

            string url = "http://" + firewallIP + ":" +
            gatewayPort.ToString() + controlUrl;
            System.Net.WebRequest wr =
            System.Net.WebRequest.Create(url);//+ controlUrl);
            wr.Method = "POST";
            wr.Headers.Add("SOAPAction", "\"" + serviceType +
            "#AddPortMapping\"");
            wr.ContentType = "text/xml;charset=\"utf-8\"";
            wr.ContentLength = body.Length;

            System.IO.Stream stream = wr.GetRequestStream();
            stream.Write(body, 0, body.Length);
            stream.Flush();
            stream.Close();

            WebResponse wres = wr.GetResponse();
            System.IO.StreamReader sr = new
            System.IO.StreamReader(wres.GetResponseStream());
            string ret = sr.ReadToEnd();
            sr.Close();

        }
    }
}