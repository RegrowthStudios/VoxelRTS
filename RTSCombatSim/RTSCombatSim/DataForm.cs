using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using Microsoft.Xna.Framework;
using XColor = Microsoft.Xna.Framework.Color;
using RTSEngine.Interfaces;
using RTSEngine.Data.Parsers;

namespace RTSCS {
    public partial class DataForm : Form, IDataForm {
        // Should Only Be One Form Ever Made
        public static DataForm Instance { get; private set; }

        public delegate void CloseDelegate();
        public CloseDelegate Closer;

        public event Action<RTSUISpawnArgs> OnUnitSpawn;

        // This Is The Unit Data That Must Be Modified By The Form
        private RTSUnit[] units;
        private string[][] unitControllers;

        // This Is Team Data That Must Be Modified In A Different Tab
        private RTSTeam[] teams;
        private Vector3[] teamSpawnPositions;
        private Vector2[] teamWaypoints;
        private XColor[] teamColors;
        private Dictionary<string, ReflectedEntityController> controllers;

        // This Should Be Where You Figure Out Which Team You Are Operating On
        int selectedIndex;
        int spawn1SelectedIndex;
        int spawn2SelectedIndex;
        int spawn3SelectedIndex;

        // Default Unit Parameters
        const int UNIT_DEFAULT_ARMOR = 0;
        const int UNIT_DEFAULT_ATTACK_DAMAGE = 10;
        const float UNIT_DEFAULT_ATTACK_TIMER = 0.5f;
        const double UNIT_DEFAULT_CRITICAL_CHANCE = 0.05;
        const int UNIT_DEFAULT_CRITICAL_DAMAGE = 20;
        const int UNIT_DEFAULT_MAX_RANGE = 28;
        const int UNIT_DEFAULT_MIN_RNAGE = 0;
        const int UNIT_DEFAULT_HEALTH = 100;
        const int UNIT_DEFAULT_SPEED = 55;

        // Default TextBox Parameters
        const String DEFAULT_UNIT_COUNT_TEXT = "1";
        const String DEFAULT_TEAM1_COLOR_TEXT = "1,0,0";
        const String DEFAULT_TEAM2_COLOR_TEXT = "0,0,1";
        const String DEFAULT_TEAM3_COLOR_TEXT = "0,1,0";
        const String DEFAULT_TEAM1_SPAWN_TEXT = "-180,-180,0";
        const String DEFAULT_TEAM2_SPAWN_TEXT = "180,-180,0";
        const String DEFAULT_TEAM3_SPAWN_TEXT = "180,180,0";
        const String DEFAULT_WAYPOINT_TEXT = "0,0";

        public DataForm(RTSUnit[] ud, RTSTeam[] t, Dictionary<string, ReflectedEntityController> c) {
            InitializeComponent();
            Closer = () => { Close(); };

            // Set Up Data
            units = ud;
            foreach(RTSUnit unit in units) {
                SetDefaultsForRTSUnit(unit);
            }
            unitControllers = new string[units.Length][];
            for(int i = 0; i < unitControllers.Length; i++) {
                unitControllers[i] = new string[4];
                unitControllers[i][0] = cbAC.Text.Trim();
                unitControllers[i][1] = cbCC.Text.Trim();
                unitControllers[i][2] = cbMC.Text.Trim();
                unitControllers[i][3] = cbTC.Text.Trim();
            }

            // Beware: Hardcoded To Expect 3 Teams Elsewhere
            teams = t;
            teamSpawnPositions = new Vector3[teams.Length];
            teamWaypoints = new Vector2[teams.Length];
            teamColors = new XColor[teams.Length];
            controllers = c;

            // Populate Combo Boxes
            unitTypeComboBox.Items.Add("Unit Type 1");
            unitTypeComboBox.Items.Add("Unit Type 2");
            unitTypeComboBox.Items.Add("Unit Type 3");
            unitTypeComboBox.SelectedIndex = 0;

            spawn1ComboBox.Items.Add("Unit Type 1");
            spawn1ComboBox.Items.Add("Unit Type 2");
            spawn1ComboBox.Items.Add("Unit Type 3");
            spawn1ComboBox.SelectedIndex = 0;

            spawn2ComboBox.Items.Add("Unit Type 1");
            spawn2ComboBox.Items.Add("Unit Type 2");
            spawn2ComboBox.Items.Add("Unit Type 3");
            spawn2ComboBox.SelectedIndex = 0;

            spawn3ComboBox.Items.Add("Unit Type 1");
            spawn3ComboBox.Items.Add("Unit Type 2");
            spawn3ComboBox.Items.Add("Unit Type 3");
            spawn3ComboBox.SelectedIndex = 0;

            // Populate Spawn Page
            SetDefaultsForSpawnPage();
        }

        private void SetDefaultsForRTSUnit(RTSUnit unit) {
            unit.Health = UNIT_DEFAULT_HEALTH;
            unit.MovementSpeed = UNIT_DEFAULT_SPEED;
            unit.BaseCombatData.Armor = UNIT_DEFAULT_ARMOR;
            unit.BaseCombatData.AttackDamage = UNIT_DEFAULT_ATTACK_DAMAGE;
            unit.BaseCombatData.AttackTimer = UNIT_DEFAULT_ATTACK_TIMER;
            unit.BaseCombatData.CriticalChance = UNIT_DEFAULT_CRITICAL_CHANCE;
            unit.BaseCombatData.CriticalDamage = UNIT_DEFAULT_CRITICAL_DAMAGE;
            unit.BaseCombatData.MaxRange = UNIT_DEFAULT_MAX_RANGE;
            unit.BaseCombatData.MinRange = UNIT_DEFAULT_MIN_RNAGE;
            unit.ICollidableShape = new CollisionCircle(10, Vector2.Zero);
        }

        private void SetDefaultsForSpawnPage() {
            for(int t = 0; t < teams.Length; t++) {
                // Initialize Counts for Unit Types
                for(int u = 0; u < teams.Length; u++) {
                    TextBox tb = PickUnitCountTextBox(t, u);
                    tb.Text = DEFAULT_UNIT_COUNT_TEXT;
                }
            }

            team1ColorTextBox.Text = DEFAULT_TEAM1_COLOR_TEXT;
            team2ColorTextBox.Text = DEFAULT_TEAM2_COLOR_TEXT;
            team3ColorTextBox.Text = DEFAULT_TEAM3_COLOR_TEXT;

            team1SpawnPositionTextBox.Text = DEFAULT_TEAM1_SPAWN_TEXT;
            team2SpawnPositionTextBox.Text = DEFAULT_TEAM2_SPAWN_TEXT;
            team3SpawnPositionTextBox.Text = DEFAULT_TEAM3_SPAWN_TEXT;

            team1WaypointTextBox.Text = DEFAULT_WAYPOINT_TEXT;
            team2WaypointTextBox.Text = DEFAULT_WAYPOINT_TEXT;
            team3WaypointTextBox.Text = DEFAULT_WAYPOINT_TEXT;

        }

        private void DataForm_Load(object sender, EventArgs e) {
            Instance = this;
        }
        private void DataForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(!e.Cancel) Instance = null;
        }

        private IActionController GetActionController(string name) {
            if(!controllers.ContainsKey(name)) return null;
            return controllers[name].CreateInstance() as IActionController;
        }
        private IMovementController GetMovementController(string name) {
            if(!controllers.ContainsKey(name)) return null;
            return controllers[name].CreateInstance() as IMovementController;
        }
        private ITargettingController GetTargettingController(string name) {
            if(!controllers.ContainsKey(name)) return null;
            return controllers[name].CreateInstance() as ITargettingController;
        }
        private ICombatController GetCombatController(string name) {
            if(!controllers.ContainsKey(name)) return null;
            return controllers[name].CreateInstance() as ICombatController;
        }
        private void GetControllers(
            string[] names,
            out IActionController ac,
            out ICombatController cc,
            out IMovementController mc,
            out ITargettingController tc) {
            ac = null; cc = null; mc = null; tc = null;
            IEnumerable<string> su = names.Distinct();
            foreach(string name in su) {
                if(!controllers.ContainsKey(name)) continue;
                ReflectedEntityController rec = controllers[name];
                if(rec.ControllerType != EntityControllerType.None) {
                    IEntityController ec = rec.CreateInstance();
                    if(rec.ControllerType.HasFlag(EntityControllerType.Action))
                        ac = ec as IActionController;
                    if(rec.ControllerType.HasFlag(EntityControllerType.Combat))
                        cc = ec as ICombatController;
                    if(rec.ControllerType.HasFlag(EntityControllerType.Movement))
                        mc = ec as IMovementController;
                    if(rec.ControllerType.HasFlag(EntityControllerType.Targetting))
                        tc = ec as ITargettingController;
                }
            }
        }
        private void SpawnUnit(int unitIndex, int teamIndex) {
            RTSUISpawnArgs a = new RTSUISpawnArgs {
                UnitData = units[unitIndex],
                Team = teams[teamIndex],
                SpawnPos = teamSpawnPositions[teamIndex],
                Waypoints = new Vector2[] { teamWaypoints[teamIndex] },
                Color = teamColors[teamIndex]
            };
            GetControllers(unitControllers[unitIndex], out a.AC, out a.CC, out a.MC, out a.TC);
            OnUnitSpawn(a);
        }

        private void CreateScriptPage() {
            Thread t = new Thread(() => {
                using(var f = new ScriptControlForm(controllers)) {
                    FormClosingEventHandler eh = (s, e) => {
                        f.Invoke(f.Closer);
                    };
                    FormClosing += eh;
                    f.FormClosing += (s, e) => {
                        FormClosing -= eh;
                    };
                    f.ShowDialog();
                }
            });
            t.SetApartmentState(ApartmentState.STA);
            t.IsBackground = true;
            t.Priority = ThreadPriority.BelowNormal;
            t.Start();
        }

        private void unitTypeComboBox_Change(object sender, EventArgs e) {
            selectedIndex = unitTypeComboBox.SelectedIndex;
            minRangeTextBox.Text = units[selectedIndex].BaseCombatData.MinRange.ToString();
            maxRangeTextBox.Text = units[selectedIndex].BaseCombatData.MaxRange.ToString();
            attackDamageTextBox.Text = units[selectedIndex].BaseCombatData.AttackDamage.ToString();
            attackTimerTextBox.Text = units[selectedIndex].BaseCombatData.AttackTimer.ToString();
            armorTextBox.Text = units[selectedIndex].BaseCombatData.Armor.ToString();
            criticalDamageTextBox.Text = units[selectedIndex].BaseCombatData.CriticalDamage.ToString();
            criticalChanceTextBox.Text = units[selectedIndex].BaseCombatData.CriticalChance.ToString();
            healthTextBox.Text = units[selectedIndex].Health.ToString();
            movementSpeedTextBox.Text = units[selectedIndex].MovementSpeed.ToString();
        }

        private void saveButton_Click(object sender, EventArgs e) {
            units[selectedIndex].BaseCombatData.MinRange = int.Parse(minRangeTextBox.Text);
            units[selectedIndex].BaseCombatData.MaxRange = int.Parse(maxRangeTextBox.Text);
            units[selectedIndex].BaseCombatData.AttackTimer = float.Parse(attackTimerTextBox.Text);
            units[selectedIndex].BaseCombatData.AttackDamage = int.Parse(attackDamageTextBox.Text);
            units[selectedIndex].BaseCombatData.Armor = int.Parse(armorTextBox.Text);
            units[selectedIndex].BaseCombatData.CriticalDamage = int.Parse(criticalDamageTextBox.Text);
            units[selectedIndex].BaseCombatData.CriticalChance = double.Parse(criticalChanceTextBox.Text);
            units[selectedIndex].Health = int.Parse(healthTextBox.Text);
            units[selectedIndex].MovementSpeed = int.Parse(movementSpeedTextBox.Text);
            unitControllers[selectedIndex][0] = cbAC.Text.Trim();
            unitControllers[selectedIndex][1] = cbCC.Text.Trim();
            unitControllers[selectedIndex][2] = cbMC.Text.Trim();
            unitControllers[selectedIndex][3] = cbTC.Text.Trim();
        }

        // Assumes Data Is Input As (x,y)
        private Vector2 StringToVector2(String s) {
            String[] splitString = s.Split(',');
            if(splitString.Length != 2) return Vector2.Zero;
            return new Vector2(float.Parse(splitString[0]), float.Parse(splitString[1]));
        }

        // Assumes Data Is Input As (x,y,z)
        private Vector3 StringToVector3(String s) {
            String[] splitString = s.Split(',');
            if(splitString.Length != 3) return Vector3.Zero;
            return new Vector3(float.Parse(splitString[0]), float.Parse(splitString[1]), float.Parse(splitString[2]));
        }

        // Assumes Data Is Input As (x,y,z)
        private XColor StringToXColor(String s) {
            String[] splitString = s.Split(',');
            if(splitString.Length != 3) return XColor.Green;
            return new XColor(float.Parse(splitString[0]), float.Parse(splitString[1]), float.Parse(splitString[2]));
        }

        private void spawnButton_Click(object sender, EventArgs e) {
            teamSpawnPositions[0] = StringToVector3(team1SpawnPositionTextBox.Text);
            teamSpawnPositions[1] = StringToVector3(team2SpawnPositionTextBox.Text);
            teamSpawnPositions[2] = StringToVector3(team3SpawnPositionTextBox.Text);

            teamWaypoints[0] = StringToVector2(team1WaypointTextBox.Text);
            teamWaypoints[1] = StringToVector2(team2WaypointTextBox.Text);
            teamWaypoints[2] = StringToVector2(team3WaypointTextBox.Text);

            teamColors[0] = StringToXColor(team1ColorTextBox.Text);
            teamColors[1] = StringToXColor(team2ColorTextBox.Text);
            teamColors[2] = StringToXColor(team3ColorTextBox.Text);

            for(int t = 0; t < teams.Length; t++) {
                for(int u = 0; u < units.Length; u++) {
                    int spawnCount = int.Parse(PickUnitCountTextBox(t, u).Text);
                    for(int count = 0; count < spawnCount; count++) {
                        SpawnUnit(u, t);
                    }
                }
            }
        }

        // There Has To Be A Cleaner Way To Do This... I Wish I Had OCaml's Match...
        private TextBox PickUnitCountTextBox(int team, int unit) {
            if(team == 0 && unit == 0) return team1Unit1TextBox;
            else if(team == 0 && unit == 1) return team1Unit2TextBox;
            else if(team == 0 && unit == 2) return team1Unit3TextBox;
            else if(team == 1 && unit == 0) return team2Unit1TextBox;
            else if(team == 1 && unit == 1) return team2Unit2TextBox;
            else if(team == 1 && unit == 2) return team2Unit3TextBox;
            else if(team == 2 && unit == 0) return team3Unit1TextBox;
            else if(team == 2 && unit == 1) return team3Unit2TextBox;
            else return team3Unit3TextBox;
        }

        private void spawn1ComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            spawn1SelectedIndex = spawn1ComboBox.SelectedIndex;
        }

        private void spawn2ComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            spawn2SelectedIndex = spawn2ComboBox.SelectedIndex;
        }

        private void spawn3ComboBox_SelectedIndexChanged(object sender, EventArgs e) {
            spawn3SelectedIndex = spawn3ComboBox.SelectedIndex;
        }

        private void spawn1Button_Click(object sender, EventArgs e) {
            SpawnUnit(spawn1SelectedIndex, 0);
        }

        private void spawn2Button_Click(object sender, EventArgs e) {
            SpawnUnit(spawn2SelectedIndex, 1);
        }

        private void spawn3Button_Click(object sender, EventArgs e) {
            SpawnUnit(spawn3SelectedIndex, 2);
        }

        private void btnScriptDialog_Click(object sender, EventArgs e) {
            CreateScriptPage();
        }

        private void btnRefreshScripts_Click(object sender, EventArgs e) {
            cbAC.Items.Clear();
            cbCC.Items.Clear();
            cbMC.Items.Clear();
            cbTC.Items.Clear();
            foreach(KeyValuePair<string, ReflectedEntityController> kv in controllers) {
                if(kv.Value.ControllerType.HasFlag(EntityControllerType.Action))
                    cbAC.Items.Add(kv.Key);
                if(kv.Value.ControllerType.HasFlag(EntityControllerType.Combat))
                    cbCC.Items.Add(kv.Key);
                if(kv.Value.ControllerType.HasFlag(EntityControllerType.Movement))
                    cbMC.Items.Add(kv.Key);
                if(kv.Value.ControllerType.HasFlag(EntityControllerType.Targetting))
                    cbTC.Items.Add(kv.Key);
            }
            if(cbAC.Items.Count > 0) cbAC.SelectedIndex = 0;
            if(cbCC.Items.Count > 0) cbCC.SelectedIndex = 0;
            if(cbMC.Items.Count > 0) cbMC.SelectedIndex = 0;
            if(cbTC.Items.Count > 0) cbTC.SelectedIndex = 0;
        }
    }
}