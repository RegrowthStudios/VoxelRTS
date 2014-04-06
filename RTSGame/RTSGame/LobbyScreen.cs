using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Graphics;
using BlisterUI;
using BlisterUI.Input;
using BlisterUI.Widgets;
using RTSEngine.Data.Team;
using RTSEngine.Controllers;
using RTSEngine.Data;

namespace RTS {
    public class TeamInitWidget : IDisposable {
        public BaseWidget Parent {
            set {
                BackRect.Parent = value;
            }
        }
        public string Race {
            get { return TextRace.Text; }
            set { TextRace.Text = value; }
        }
        public string PlayerType {
            get { return TextPlayerType.Text; }
            set { TextPlayerType.Text = value; }
        }
        public string Scheme {
            get { return TextScheme.Text; }
            set { TextScheme.Text = value; }
        }

        public RectWidget BackRect {
            get;
            private set;
        }
        public TextWidget TextIndex {
            get;
            private set;
        }
        public TextWidget TextUser {
            get;
            private set;
        }
        public RectButton ButtonPlayerType {
            get;
            private set;
        }
        public TextWidget TextPlayerType {
            get;
            private set;
        }
        public RectButton ButtonRace {
            get;
            private set;
        }
        public TextWidget TextRace {
            get;
            private set;
        }
        public RectButton ButtonScheme {
            get;
            private set;
        }
        public TextWidget TextScheme {
            get;
            private set;
        }

        private string[] lRaces, lSchemes, lTypes;
        private int ri, si, ti;

        public TeamInitWidget(WidgetRenderer wr, int w, int h, int buf, Color cBack, ButtonHighlightOptions bh1, ButtonHighlightOptions bh2, Color cText) {
            BackRect = new RectWidget(wr);
            BackRect.Width = w;
            BackRect.Height = h;
            BackRect.Offset = new Point(0, 0);
            BackRect.OffsetAlignY = Alignment.BOTTOM;
            BackRect.Color = cBack;
            BackRect.LayerDepth = 1f;

            int wh = h - buf * 2;

            TextIndex = new TextWidget(wr);
            TextIndex.Offset = new Point(buf, 0);
            TextIndex.OffsetAlignX = Alignment.LEFT;
            TextIndex.OffsetAlignY = Alignment.MID;
            TextIndex.AlignX = Alignment.LEFT;
            TextIndex.AlignY = Alignment.MID;
            TextIndex.Height = wh;
            TextIndex.Color = cText;
            TextIndex.Parent = BackRect;
            TextIndex.LayerDepth = 0.3f;

            TextUser = new TextWidget(wr);
            TextUser.Offset = new Point(buf, 0);
            TextUser.OffsetAlignX = Alignment.RIGHT;
            TextUser.OffsetAlignY = Alignment.MID;
            TextUser.AlignX = Alignment.LEFT;
            TextUser.AlignY = Alignment.MID;
            TextUser.Height = wh;
            TextUser.Color = cText;
            TextUser.Parent = TextIndex;
            TextUser.LayerDepth = 0.3f;

            ButtonScheme = new RectButton(wr, bh1, bh2);
            ButtonScheme.Offset = new Point(-buf, 0);
            ButtonScheme.OffsetAlignX = Alignment.RIGHT;
            ButtonScheme.OffsetAlignY = Alignment.MID;
            ButtonScheme.AlignX = Alignment.RIGHT;
            ButtonScheme.AlignY = Alignment.MID;
            ButtonScheme.Parent = BackRect;
            ButtonScheme.LayerDepth = 0.3f;
            TextScheme = new TextWidget(wr);
            TextScheme.Height = bh1.Height;
            TextScheme.Text = "Default";
            TextScheme.Offset = new Point(0, 0);
            TextScheme.OffsetAlignX = Alignment.MID;
            TextScheme.OffsetAlignY = Alignment.MID;
            TextScheme.AlignX = Alignment.MID;
            TextScheme.AlignY = Alignment.MID;
            TextScheme.Parent = ButtonScheme;
            TextScheme.Color = cText;
            TextScheme.LayerDepth = 0f;

            ButtonRace = new RectButton(wr, bh1, bh2);
            ButtonRace.Offset = new Point(-buf, 0);
            ButtonRace.OffsetAlignX = Alignment.LEFT;
            ButtonRace.OffsetAlignY = Alignment.MID;
            ButtonRace.AlignX = Alignment.RIGHT;
            ButtonRace.AlignY = Alignment.MID;
            ButtonRace.Parent = ButtonScheme;
            ButtonRace.LayerDepth = 0.3f;
            TextRace = new TextWidget(wr);
            TextRace.Height = bh1.Height;
            TextRace.Text = "Race";
            TextRace.Offset = new Point(0, 0);
            TextRace.OffsetAlignX = Alignment.MID;
            TextRace.OffsetAlignY = Alignment.MID;
            TextRace.AlignX = Alignment.MID;
            TextRace.AlignY = Alignment.MID;
            TextRace.Parent = ButtonRace;
            TextRace.Color = cText;
            TextRace.LayerDepth = 0f;

            ButtonPlayerType = new RectButton(wr, bh1, bh2);
            ButtonPlayerType.Offset = new Point(-buf, 0);
            ButtonPlayerType.OffsetAlignX = Alignment.LEFT;
            ButtonPlayerType.OffsetAlignY = Alignment.MID;
            ButtonPlayerType.AlignX = Alignment.RIGHT;
            ButtonPlayerType.AlignY = Alignment.MID;
            ButtonPlayerType.Parent = ButtonRace;
            ButtonPlayerType.LayerDepth = 0.3f;
            TextPlayerType = new TextWidget(wr);
            TextPlayerType.Height = bh1.Height;
            TextPlayerType.Text = "None";
            TextPlayerType.Offset = new Point(0, 0);
            TextPlayerType.OffsetAlignX = Alignment.MID;
            TextPlayerType.OffsetAlignY = Alignment.MID;
            TextPlayerType.AlignX = Alignment.MID;
            TextPlayerType.AlignY = Alignment.MID;
            TextPlayerType.Parent = ButtonPlayerType;
            TextPlayerType.Color = cText;
            TextPlayerType.LayerDepth = 0f;

            ButtonScheme.Hook();
            ButtonRace.Hook();
            ButtonPlayerType.Hook();

            ButtonScheme.OnButtonPress += ButtonScheme_OnButtonPress;
            ButtonRace.OnButtonPress += ButtonRace_OnButtonPress;
            ButtonPlayerType.OnButtonPress += ButtonPlayerType_OnButtonPress;
        }

        void ButtonScheme_OnButtonPress(RectButton obj, Vector2 m) {
            si = (si + 1) % lSchemes.Length;
            Scheme = lSchemes[si];
        }
        void ButtonRace_OnButtonPress(RectButton obj, Vector2 m) {
            ri = (ri + 1) % lRaces.Length;
            Race = lRaces[ri];
        }
        void ButtonPlayerType_OnButtonPress(RectButton obj, Vector2 m) {
            ti = (ti + 1) % lTypes.Length;
            PlayerType = lTypes[ti];
        }

        public void Dispose() {
            ButtonScheme.OnButtonPress -= ButtonScheme_OnButtonPress;
            ButtonRace.OnButtonPress -= ButtonRace_OnButtonPress;
            ButtonPlayerType.OnButtonPress -= ButtonPlayerType_OnButtonPress;

            BackRect.Dispose();
            TextUser.Dispose();
            ButtonPlayerType.Dispose();
            TextPlayerType.Dispose();
            ButtonRace.Dispose();
            TextRace.Dispose();
            ButtonScheme.Dispose();
            TextScheme.Dispose();
        }

        public void Set(string[] pTypes, Dictionary<string, RTSRaceData> races, Dictionary<string, RTSColorScheme> schemes) {
            lRaces = races.Keys.ToArray();
            lSchemes = schemes.Keys.ToArray();
            lTypes = pTypes;

            ri = 0;
            Race = lRaces[ri];
            si = 0;
            Scheme = lSchemes[si];
            ti = 0;
            PlayerType = lTypes[ti];
        }
    }

    public class LobbyScreen : GameScreen<App> {
        public override int Next {
            get { return game.LoadScreen.Index; }
            protected set { }
        }
        public override int Previous {
            get { return game.MenuScreen.Index; }
            protected set { }
        }

        // Init Info Helper
        public Dictionary<string, RTSRaceData> Races {
            get;
            private set;
        }
        Dictionary<string, RTSColorScheme> schemes;

        private EngineLoadData eld;
        public EngineLoadData InitInfo {
            get { return eld; }
        }

        WidgetRenderer wr;
        TeamInitWidget[] widgets;
        IDisposable tFont;

        public override void Build() {
        }
        public override void Destroy(GameTime gameTime) {
        }

        public override void OnEntry(GameTime gameTime) {
            // Load All The Races And Schemes
            Races = new Dictionary<string, RTSRaceData>();
            schemes = new Dictionary<string, RTSColorScheme>();
            GameEngine.SearchAllInitInfo(new DirectoryInfo("Packs"), Races, schemes);
            if(schemes.Count < 1)
                schemes.Add("Default", RTSColorScheme.Default);
            string defScheme = "Default";
            foreach(var kvp in schemes) {
                defScheme = kvp.Key;
                break;
            }

            // Set Init Data To Be Nothing
            eld = new EngineLoadData();
            eld.Teams = new TeamInitOption[GameState.MAX_PLAYERS];
            for(int i = 0; i < eld.Teams.Length; i++) {
                eld.Teams[i].InputType = InputType.None;
                eld.Teams[i].Race = null;
                eld.Teams[i].PlayerName = null;
                eld.Teams[i].Colors = schemes[defScheme];
            }

            wr = new WidgetRenderer(G, XNASpriteFont.Compile(G, "Times New Roman", 36, out tFont));
            widgets = new TeamInitWidget[eld.Teams.Length];
            ButtonHighlightOptions bh1 = new ButtonHighlightOptions(120, 36, Color.Black);
            ButtonHighlightOptions bh2 = new ButtonHighlightOptions(120, 36, Color.DarkGray);
            string[] pt = { "None", "Player", "Computer", "Environment" };
            for(int i = 0; i < widgets.Length; i++) {
                widgets[i] = new TeamInitWidget(wr, 600, 44, 8, new Color(8, 8, 8), bh1, bh2, Color.Lime);
                if(i > 0) {
                    widgets[i].Parent = widgets[i - 1].BackRect;
                }
                widgets[i].TextIndex.Text = (i + 1).ToString();
                widgets[i].TextUser.Text = "Unknown";
                widgets[i].Set(pt, Races, schemes);
            }
            widgets[0].PlayerType = "Player";
            widgets[0].Race = "Mechanica";
            widgets[1].PlayerType = "Computer";
            widgets[1].Race = "Mechanica";

            KeyboardEventDispatcher.OnKeyPressed += KeyboardEventDispatcher_OnKeyPressed;
        }
        public override void OnExit(GameTime gameTime) {
            KeyboardEventDispatcher.OnKeyPressed -= KeyboardEventDispatcher_OnKeyPressed;
            BuildELDFromWidgets();

            foreach(var w in widgets) w.Dispose();
            wr.Dispose();
            tFont.Dispose();

            // Clear Init Info
            schemes.Clear();
            schemes = null;
        }

        public void SetUserPlayer(int team, string name) {

        }

        public override void Update(GameTime gameTime) {

        }
        public override void Draw(GameTime gameTime) {
            G.Clear(Color.Black);

            wr.Draw(SB);

            // Draw The Mouse
            game.mRenderer.BeginPass(G);
            game.mRenderer.Draw(G);
        }

        private void BuildELDFromWidgets() {
            for(int i = 0; i < widgets.Length; i++) {
                eld.Teams[i].Colors = schemes[widgets[i].Scheme];
                eld.Teams[i].Race = widgets[i].Race;
                eld.Teams[i].PlayerName = widgets[i].TextUser.Text;
                switch(widgets[i].TextPlayerType.Text.ToLower()) {
                    case "player":
                        eld.Teams[i].InputType = InputType.Player;
                        break;
                    case "computer":
                        eld.Teams[i].InputType = InputType.AI;
                        break;
                    case "environment":
                        eld.Teams[i].InputType = InputType.Environment;
                        break;
                    default:
                        eld.Teams[i].InputType = InputType.None;
                        break;
                }
            }
            eld.MapFile = new FileInfo(@"Packs\Default\maps\0\test.map");
        }

        void KeyboardEventDispatcher_OnKeyPressed(object sender, KeyEventArgs args) {
            switch(args.KeyCode) {
                case Keys.Enter:
                    State = ScreenState.ChangeNext;
                    break;
            }
        }
    }
}