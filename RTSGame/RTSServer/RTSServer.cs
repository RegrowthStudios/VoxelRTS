using System;
using System.Collections.Generic;
using System.Collections.Concurrent;
using System.Linq;
using System.Text;
using System.IO;
using System.Threading;
using RTSEngine.Controllers;
using RTSEngine.Data;
using RTSEngine.Data.Parsers;
using RTSEngine.Data.Team;
using RTSEngine.Net;
using Microsoft.Xna.Framework;

namespace RTSServer {
    public class RTSServer : IDisposable {
        private bool running;
        private ConcurrentQueue<string> commands;

        public RTSServer() {
            commands = new ConcurrentQueue<string>();
        }
        public void Dispose() {
        }

        public void Run() {
            Thread t = new Thread(() => {
                NetStreamMultiSender s = new NetStreamMultiSender(RTSConstants.MC_ADDR, RTSConstants.MC_GAME_PORT_MIN);
                while(running) {
                    if(commands.Count > 0) {
                        string m;
                        if(commands.TryDequeue(out m)) {
                            Console.WriteLine("Sent Command [{0}]", m);
                            s.Send(m);
                        }
                    }
                }
                s.Dispose();
            });
            running = true;
            t.Start();

            bool input = true;
            while(input) {
                string c = Console.ReadLine();
                switch(c) {
                    case "exit":
                    case "quit":
                    case "q":
                        input = false;
                        break;
                    default:
                        commands.Enqueue(c);
                        break;
                }
            }
            running = false;
            t.Join();
        }

        #region Entry Point
        static void Main(string[] args) {
            using(RTSServer server = new RTSServer()) {
                Console.WriteLine("Engine Is Loaded");
                server.Run();
            }
            Console.ReadLine();
        }
        #endregion
    }
}