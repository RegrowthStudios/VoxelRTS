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
using System.Globalization;

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

        // Default Unit Parameters
        const int UNIT_SOLDIER_COST = 5;
        const int UNIT_SOLDIER_ARMOR = 0;
        const int UNIT_SOLDIER_ATTACK_DAMAGE = 10;
        const float UNIT_SOLDIER_ATTACK_TIMER = 0.33f;
        const double UNIT_SOLDIER_CRITICAL_CHANCE = 0.05;
        const int UNIT_SOLDIER_CRITICAL_DAMAGE = 15;
        const int UNIT_SOLDIER_MAX_RANGE = 90;
        const int UNIT_SOLDIER_MIN_RANGE = 0;
        const int UNIT_SOLDIER_HEALTH = 60;
        const int UNIT_SOLDIER_SPEED = 100;

        const int UNIT_HEAVY_SOLDIER_COST = 6;
        const int UNIT_HEAVY_SOLDIER_ARMOR = 0;
        const int UNIT_HEAVY_SOLDIER_ATTACK_DAMAGE = 25;
        const float UNIT_HEAVY_SOLDIER_ATTACK_TIMER = 1f;
        const double UNIT_HEAVY_SOLDIER_CRITICAL_CHANCE = 0.05;
        const int UNIT_HEAVY_SOLDIER_CRITICAL_DAMAGE = 30;
        const int UNIT_HEAVY_SOLDIER_MAX_RANGE = 90;
        const int UNIT_HEAVY_SOLDIER_MIN_RANGE = 0;
        const int UNIT_HEAVY_SOLDIER_HEALTH = 80;
        const int UNIT_HEAVY_SOLDIER_SPEED = 80;

        const int UNIT_ARMORED_COST = 20;
        const int UNIT_ARMORED_ARMOR = 8;
        const int UNIT_ARMORED_ATTACK_DAMAGE = 30;
        const float UNIT_ARMORED_ATTACK_TIMER = 0.5f;
        const double UNIT_ARMORED_CRITICAL_CHANCE = 0.05;
        const int UNIT_ARMORED_CRITICAL_DAMAGE = 30;
        const int UNIT_ARMORED_MAX_RANGE = 60;
        const int UNIT_ARMORED_MIN_RANGE = 0;
        const int UNIT_ARMORED_HEALTH = 120;
        const int UNIT_ARMORED_SPEED = 110;

        // Default TextBox Parameters
        const String DEFAULT_UNIT_COUNT_TEXT = "0";
        const String DEFAULT_TEAM1_COLOR_TEXT = "1,0,0";
        const String DEFAULT_TEAM2_COLOR_TEXT = "0,1,0";
        const String DEFAULT_TEAM3_COLOR_TEXT = "0,0,1";
        const String DEFAULT_TEAM1_SPAWN_TEXT = "-180,-180,0";
        const String DEFAULT_TEAM2_SPAWN_TEXT = "180,-180,0";
        const String DEFAULT_TEAM3_SPAWN_TEXT = "180,180,0";
        const String DEFAULT_WAYPOINT_TEXT = "0,0";
        const String DEFAULT_CAPITAL = "600";

        public DataForm(RTSUnit[] ud, RTSTeam[] t, Dictionary<string, ReflectedEntityController> c) {
            InitializeComponent();
            Closer = () => { Close(); };

            // Set Up Data
            units = ud;
            for(int type = 0; type < units.Length; type++) {
                SetDefaultsForRTSUnit(type);
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

        private void SetDefaultsForRTSUnit(int type) {
            RTSUnit unit = units[type];
            switch(type) {
                case 0:
                    unit.CapitalCost = UNIT_SOLDIER_COST;
                    unit.Health = UNIT_SOLDIER_HEALTH;
                    unit.MovementSpeed = UNIT_SOLDIER_SPEED;
                    unit.BaseCombatData.Armor = UNIT_SOLDIER_ARMOR;
                    unit.BaseCombatData.AttackDamage = UNIT_SOLDIER_ATTACK_DAMAGE;
                    unit.BaseCombatData.AttackTimer = UNIT_SOLDIER_ATTACK_TIMER;
                    unit.BaseCombatData.CriticalChance = UNIT_SOLDIER_CRITICAL_CHANCE;
                    unit.BaseCombatData.CriticalDamage = UNIT_SOLDIER_CRITICAL_DAMAGE;
                    unit.BaseCombatData.MaxRange = UNIT_SOLDIER_MAX_RANGE;
                    unit.BaseCombatData.MinRange = UNIT_SOLDIER_MIN_RANGE;
                    break;
                case 1:
                    unit.CapitalCost = UNIT_HEAVY_SOLDIER_COST;
                    unit.Health = UNIT_HEAVY_SOLDIER_HEALTH;
                    unit.MovementSpeed = UNIT_HEAVY_SOLDIER_SPEED;
                    unit.BaseCombatData.Armor = UNIT_HEAVY_SOLDIER_ARMOR;
                    unit.BaseCombatData.AttackDamage = UNIT_HEAVY_SOLDIER_ATTACK_DAMAGE;
                    unit.BaseCombatData.AttackTimer = UNIT_HEAVY_SOLDIER_ATTACK_TIMER;
                    unit.BaseCombatData.CriticalChance = UNIT_HEAVY_SOLDIER_CRITICAL_CHANCE;
                    unit.BaseCombatData.CriticalDamage = UNIT_HEAVY_SOLDIER_CRITICAL_DAMAGE;
                    unit.BaseCombatData.MaxRange = UNIT_HEAVY_SOLDIER_MAX_RANGE;
                    unit.BaseCombatData.MinRange = UNIT_HEAVY_SOLDIER_MIN_RANGE;
                    break;
                case 2:
                    unit.CapitalCost = UNIT_ARMORED_COST;
                    unit.Health = UNIT_ARMORED_HEALTH;
                    unit.MovementSpeed = UNIT_ARMORED_SPEED;
                    unit.BaseCombatData.Armor = UNIT_ARMORED_ARMOR;
                    unit.BaseCombatData.AttackDamage = UNIT_ARMORED_ATTACK_DAMAGE;
                    unit.BaseCombatData.AttackTimer = UNIT_ARMORED_ATTACK_TIMER;
                    unit.BaseCombatData.CriticalChance = UNIT_ARMORED_CRITICAL_CHANCE;
                    unit.BaseCombatData.CriticalDamage = UNIT_ARMORED_CRITICAL_DAMAGE;
                    unit.BaseCombatData.MaxRange = UNIT_ARMORED_MAX_RANGE;
                    unit.BaseCombatData.MinRange = UNIT_ARMORED_MIN_RANGE;
                    break;
            }
            unit.ICollidableShape = new CollisionCircle(10, Vector2.Zero);
        }

        private void SetDefaultsForSpawnPage() {
            for(int t = 0; t < teams.Length; t++) {
                // Initialize Counts for Unit Types
                for(int u = 0; u < units.Length; u++) {
                    TextBox tb = PickUnitCountTextBox(t, u);
                    tb.Text = DEFAULT_UNIT_COUNT_TEXT;
                }
            }

            team1ColorTextBox.Text = DEFAULT_TEAM1_COLOR_TEXT;
            team2ColorTextBox.Text = DEFAULT_TEAM2_COLOR_TEXT;
            team3ColorTextBox.Text = DEFAULT_TEAM3_COLOR_TEXT;
            teamColors[0] = XColor.Red;
            teamColors[1] = XColor.Blue;
            teamColors[2] = XColor.Green;

            team1SpawnPositionTextBox.Text = DEFAULT_TEAM1_SPAWN_TEXT;
            team2SpawnPositionTextBox.Text = DEFAULT_TEAM2_SPAWN_TEXT;
            team3SpawnPositionTextBox.Text = DEFAULT_TEAM3_SPAWN_TEXT;
            teamSpawnPositions[0] = new Vector3(-20, -20, 0);
            teamSpawnPositions[1] = new Vector3(20, -20, 0);
            teamSpawnPositions[2] = new Vector3(20, 20, 0);

            team1WaypointTextBox.Text = DEFAULT_WAYPOINT_TEXT;
            team2WaypointTextBox.Text = DEFAULT_WAYPOINT_TEXT;
            team3WaypointTextBox.Text = DEFAULT_WAYPOINT_TEXT;

            capital1TextBox.Text = DEFAULT_CAPITAL;
            capital2TextBox.Text = DEFAULT_CAPITAL;
            capital3TextBox.Text = DEFAULT_CAPITAL;

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
        private List<ReflectedEntityController> GetControllers(string[] names) {
            List<ReflectedEntityController> c = new List<ReflectedEntityController>(3);
            foreach(string name in names.Distinct()) {
                if(!controllers.ContainsKey(name)) continue;
                ReflectedEntityController rec = controllers[name];
                c.Add(rec);
            }
            return c;
        }
        private void SpawnUnit(int unitIndex, int teamIndex) {
            RTSUISpawnArgs a = new RTSUISpawnArgs();
            a.UnitData = units[unitIndex];
            a.Team = teams[teamIndex];
            a.SpawnPos = new List<Vector3>();
            a.SpawnPos.Add(teamSpawnPositions[teamIndex]);
            a.Waypoints = new Vector2[] { teamWaypoints[teamIndex] };
            a.Controllers = GetControllers(unitControllers[unitIndex]);
            if(OnUnitSpawn != null)
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
            capitalCostTextBox.Text = units[selectedIndex].CapitalCost.ToString();
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
            units[selectedIndex].CapitalCost = int.Parse(capitalCostTextBox.Text);
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
            String[] splitString = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            if(splitString.Length != 3) return XColor.Green;
            return new XColor(float.Parse(splitString[0]), float.Parse(splitString[1]), float.Parse(splitString[2]));
        }

        private void UpdateSpawnInfo() {
            teamSpawnPositions[0] = StringToVector3(team1SpawnPositionTextBox.Text);
            teamSpawnPositions[1] = StringToVector3(team2SpawnPositionTextBox.Text);
            teamSpawnPositions[2] = StringToVector3(team3SpawnPositionTextBox.Text);

            teamWaypoints[0] = StringToVector2(team1WaypointTextBox.Text);
            teamWaypoints[1] = StringToVector2(team2WaypointTextBox.Text);
            teamWaypoints[2] = StringToVector2(team3WaypointTextBox.Text);

            teamColors[0] = StringToXColor(team1ColorTextBox.Text);
            teamColors[1] = StringToXColor(team2ColorTextBox.Text);
            teamColors[2] = StringToXColor(team3ColorTextBox.Text);
            for(int i = 0; i < 3; i++) teams[i].Color = teamColors[i];
        }
        private void spawnButton_Click(object sender, EventArgs e) {
            UpdateSpawnInfo();
            for(int t = 0; t < teams.Length; t++) {
                for(int u = 0; u < units.Length; u++) {
                    int spawnCount = int.Parse(PickUnitCountTextBox(t, u).Text);
                    for(int count = 0; count < spawnCount; count++) {
                        if(int.Parse(GetCapitalTextBox(t).Text) >= 0) SpawnUnit(u, t);
                        else Console.WriteLine("Team {0} has overdrawn its balance and can't spawn any units!", t); 
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

        private void spawn1Button_Click(object sender, EventArgs e) {
            UpdateSpawnInfo();
            SpawnUnit(spawn1ComboBox.SelectedIndex, 0);
        }
        private void spawn2Button_Click(object sender, EventArgs e) {
            UpdateSpawnInfo();
            SpawnUnit(spawn2ComboBox.SelectedIndex, 1);
        }
        private void spawn3Button_Click(object sender, EventArgs e) {
            UpdateSpawnInfo();
            SpawnUnit(spawn3ComboBox.SelectedIndex, 2);
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

        private int GetUnitCost(int type) {
            switch(type) {
                case 0:
                    return UNIT_SOLDIER_COST;
                case 1:
                    return UNIT_HEAVY_SOLDIER_COST;
                case 2:
                    return UNIT_ARMORED_COST;
            }
            return 0;
        }

        private TextBox GetCapitalTextBox(int team) {
            switch(team) {
                case 0:
                    return capital1TextBox;
                case 1:
                    return capital2TextBox;
                case 2:
                    return capital3TextBox;
            }
            return capital1TextBox;
        }

        // Called When Any Spawn Counts Change
        private void ArmyComposition_Change(object sender, EventArgs e) {
            for(int t = 0; t < teams.Length; t++) {
                int totalCost = 0;
                for(int u = 0; u < units.Length; u++) {
                    String textCount = PickUnitCountTextBox(t, u).Text;
                    int spawnCount = String.IsNullOrEmpty(textCount) ? 0 : int.Parse(textCount);
                    totalCost += GetUnitCost(u) * spawnCount;
                }
                TextBox tb = GetCapitalTextBox(t);
                tb.Text = (int.Parse(DEFAULT_CAPITAL) - totalCost).ToString();
            }
        }
    }
}