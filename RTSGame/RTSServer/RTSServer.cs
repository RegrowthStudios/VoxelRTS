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
        private GameState state;
        private bool running;
        private ConcurrentQueue<string> commands;

        public RTSServer() {
            EngineLoadData eld = new EngineLoadData();
            eld.MapFile = new FileInfo(@"Packs\Default\maps\0\test.map");
            var teamRes = RTSRaceParser.ParseAll(new DirectoryInfo("Packs"));
            eld.Teams = new RTSTeamResult[2];
            eld.Teams[0].TeamType = (from res in teamRes where res.Name.StartsWith("P") select res).First((o) => { return true; }); ;
            eld.Teams[0].InputType = InputType.Player;
            eld.Teams[0].Colors = RTSColorScheme.Default;
            eld.Teams[0].Colors.Primary *= Vector3.UnitX;
            eld.Teams[0].Colors.Secondary *= Vector3.UnitX;
            eld.Teams[0].Colors.Tertiary *= Vector3.UnitX;
            eld.Teams[1].TeamType = (from res in teamRes where res.Name.StartsWith("M") select res).First((o) => { return true; }); ;
            eld.Teams[1].InputType = InputType.AI;
            eld.Teams[1].Colors = RTSColorScheme.Default;
            eld.Teams[1].Colors.Primary *= Vector3.UnitZ;
            eld.Teams[1].Colors.Secondary *= Vector3.UnitZ;
            eld.Teams[1].Colors.Tertiary *= Vector3.UnitZ;

            state = new GameState();
            GameEngine.BuildLocal(state, eld);

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