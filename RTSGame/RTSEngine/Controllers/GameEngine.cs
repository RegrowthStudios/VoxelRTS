using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using RTSEngine.Data;
using RTSEngine.Data.Parsers;
using RTSEngine.Data.Team;
using RTSEngine.Graphics;
using RTSEngine.Interfaces;

namespace RTSEngine.Controllers {
    public static class RTSConstants {
        public const float GAME_DELTA_TIME = 1f / 60f;
        public const float CGRID_SIZE = 2f;
    }

    // A Playable Team
    public struct RTSTeamResult {
        public RTSRaceData TeamType;
        public RTSColorScheme Colors;
        public InputType InputType;
    }

    // The Data The Engine Needs To Know About To Properly Create A Game
    public struct EngineLoadData {
        // Teams In The Battle
        public RTSTeamResult[] Teams;

        // Where To Load The Map
        public FileInfo MapFile;
    }

    public class GameEngine : IDisposable {
        // The Graphics Device
        private GraphicsDeviceManager gdm;
        public GraphicsDevice G {
            get { return gdm.GraphicsDevice; }
        }
        private GameWindow window;

        // Data To Be Managed By The Engine
        public GameState State {
            get;
            private set;
        }
        public GameplayController PlayController {
            get;
            private set;
        }
        private readonly ConcurrentBag<IDisposable> toDispose;

        public GameEngine(GraphicsDeviceManager _gdm, GameWindow w) {
            // Get That Reference
            gdm = _gdm;
            toDispose = new ConcurrentBag<IDisposable>();
            window = w;
        }
        public void Dispose() {
            if(State != null) {
                DevConsole.OnNewCommand -= PlayController.OnDevCommand;

                for(int ti = 0; ti < State.Teams.Length; ti++) {
                    State.Teams[ti].Input.Dispose();
                }
            }

            // Dispose All In The List
            IDisposable[] td = new IDisposable[toDispose.Count];
            toDispose.CopyTo(td, 0);
            for(int i = 0; i < td.Length; i++) {
                td[i].Dispose();
                td[i] = null;
            }
        }

        #region Graphics Data Creation That Will Be Ready For Disposal At The End Of The Game
        public VertexBuffer CreateVertexBuffer(VertexDeclaration vd, int s, BufferUsage usage = BufferUsage.WriteOnly) {
            VertexBuffer vb = new VertexBuffer(G, vd, s, usage);
            toDispose.Add(vb);
            return vb;
        }
        public DynamicVertexBuffer CreateDynamicVertexBuffer(VertexDeclaration vd, int s, BufferUsage usage = BufferUsage.WriteOnly) {
            DynamicVertexBuffer vb = new DynamicVertexBuffer(G, vd, s, usage);
            toDispose.Add(vb);
            return vb;
        }
        public IndexBuffer CreateIndexBuffer(IndexElementSize id, int s, BufferUsage usage = BufferUsage.WriteOnly) {
            IndexBuffer ib = new IndexBuffer(G, id, s, usage);
            toDispose.Add(ib);
            return ib;
        }
        public Texture2D CreateTexture2D(int w, int h, SurfaceFormat format = SurfaceFormat.Color, bool mipmap = false) {
            Texture2D t = new Texture2D(G, w, h, mipmap, format);
            toDispose.Add(t);
            return t;
        }
        public Texture2D LoadTexture2D(Stream s) {
            Texture2D t = Texture2D.FromStream(G, s);
            toDispose.Add(t);
            return t;
        }
        public Texture2D LoadTexture2D(string file) {
            Texture2D t;
            using(FileStream fs = File.OpenRead(file)) {
                t = LoadTexture2D(fs);
            }
            return t;
        }
        public RenderTarget2D CreateRenderTarget2D(int w, int h, SurfaceFormat sf = SurfaceFormat.Color, DepthFormat df = DepthFormat.Depth24Stencil8, RenderTargetUsage usage = RenderTargetUsage.DiscardContents, int msc = 1, bool mipmap = false) {
            RenderTarget2D t = new RenderTarget2D(G, w, h, mipmap, sf, df, msc, usage);
            toDispose.Add(t);
            return t;
        }
        public Effect LoadEffect(string file) {
            Effect fx = XNAEffect.Compile(G, file);
            toDispose.Add(fx);
            return fx;
        }
        public BasicEffect CreateEffect() {
            BasicEffect fx = new BasicEffect(G);
            toDispose.Add(fx);
            return fx;
        }
        public SpriteFont LoadFont(string file) {
            IDisposable disp;
            SpriteFont f = XNASpriteFont.Compile(G, file, out disp);
            toDispose.Add(disp);
            return f;
        }
        public SpriteFont LoadFont(string file, out IDisposable disp) {
            SpriteFont f = XNASpriteFont.Compile(G, file, out disp);
            toDispose.Add(disp);
            return f;
        }
        public SpriteFont CreateFont(string fontName, int size, int spacing = 0, bool useKerning = true, string style = "Regular", char defaultChar = '*', int cStart = 32, int cEnd = 126) {
            IDisposable disp;
            SpriteFont f = XNASpriteFont.Compile(G, fontName, size, out disp, spacing, useKerning, style, defaultChar, cStart, cEnd);
            toDispose.Add(disp);
            return f;
        }
        public SpriteFont CreateFont(string fontName, int size, out IDisposable disp, int spacing = 0, bool useKerning = true, string style = "Regular", char defaultChar = '*', int cStart = 32, int cEnd = 126) {
            SpriteFont f = XNASpriteFont.Compile(G, fontName, size, out disp, spacing, useKerning, style, defaultChar, cStart, cEnd);
            toDispose.Add(disp);
            return f;
        }
        #endregion

        public void Load(EngineLoadData d) {
            PlayController = new GameplayController();
            DevConsole.OnNewCommand += PlayController.OnDevCommand;

            // Create Game State
            State = new GameState();
            LoadControllers();
            State.SetTeams(LoadTeams(d.Teams));

            for(int ti = 0; ti < State.Teams.Length; ti++) {
                switch(d.Teams[ti].InputType) {
                    case InputType.Player:
                        var pic = new PlayerInputController(State, State.Teams[ti]);
                        State.Teams[ti].Input = pic;
                        break;
                    case InputType.AI:
                        // TODO: Make This Class
                        State.Teams[ti].Input = new AIInputController(State, State.Teams[ti], ti);
                        break;
                    case InputType.Environment:
                        // TODO: Make This Class
                        break;
                    default:
                        throw new Exception("Type does not exist");
                }
            }

            // Load The Map
            LoadMap(d.MapFile);

            for(int ti = 0; ti < State.Teams.Length; ti++) {
                switch(d.Teams[ti].InputType) {
                    case InputType.AI:
                        (State.Teams[ti].Input as AIInputController).Start();
                        break;
                }
            }

            PlayController.Init(State);
        }

        // Data Parsing And Loading
        private void LoadControllers() {
            // Add Controllers
            string error;
            string[] references = {
               "System.dll",
               "System.Core.dll",
               "System.Data.dll",
               "System.Xml.dll",
               "System.Xml.Linq.dll",
               @"lib\Microsoft.Xna.Framework.dll",
               "RTSEngine.dll"
           };
            DirectoryInfo dir = new DirectoryInfo(@"Packs\Default\scripts");
            var files = dir.GetFiles();
            string[] toCompile = (from fi in files where fi.Extension.EndsWith("cs") select fi.FullName).ToArray();
            DynCompiledResults res = DynControllerParser.Compile(toCompile, references, out error);
            foreach(KeyValuePair<string, ReflectedUnitController> kv in res.UnitControllers)
                State.UnitControllers.Add(kv.Key, kv.Value);
            foreach(KeyValuePair<string, ReflectedSquadController> kv in res.SquadControllers)
                State.SquadControllers.Add(kv.Key, kv.Value);
        }
        private void LoadMap(FileInfo infoFile) {
            // Parse Map Data
            State.Map = HeightmapParser.ParseData(infoFile);
            if(State.Map == null)
                throw new ArgumentNullException("Could Not Load Heightmap");

            // Create Grid
            State.CGrid = new CollisionGrid(State.Map.Width, State.Map.Depth, RTSConstants.CGRID_SIZE);
        }
        private RTSTeam[] LoadTeams(RTSTeamResult[] teamResults) {
            RTSTeam[] t = new RTSTeam[teamResults.Length];
            RTSTeam team;
            int i = 0;
            foreach(var res in teamResults) {
                team = new RTSTeam();
                team.ColorScheme = res.Colors;
                team.scDefaultAction = State.SquadControllers[res.TeamType.DefaultSquadActionController];
                team.scDefaultMovement = State.SquadControllers[res.TeamType.DefaultSquadMovementController];
                team.scDefaultTargetting = State.SquadControllers[res.TeamType.DefaultSquadTargettingController];
                foreach(FileInfo unitDataFile in res.TeamType.UnitTypes) {
                    RTSUnitData data = RTSUnitDataParser.ParseData(State.UnitControllers, unitDataFile);
                    team.AddUnitData(data);
                }
                t[i++] = team;
            }
            return t;
        }

        // Update Logic
        public void Update() {
            PlayController.Update(State, RTSConstants.GAME_DELTA_TIME);
        }
    }
}