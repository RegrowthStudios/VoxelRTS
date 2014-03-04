using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using RTSEngine.Data;
using RTSEngine.Data.Team;
using Microsoft.Xna.Framework;
using XColor = Microsoft.Xna.Framework.Color;
using RTSCS.Controllers;

namespace RTSCS {
    public partial class DataForm : Form, IDataForm {
        // Should Only Be One Form Ever Made
        public static DataForm Instance { get; private set; }

        public delegate void CloseDelegate();
        public CloseDelegate Closer;

        public event Action<RTSUnitInstance, XColor> OnUnitSpawn;

        // This Is The Unit Data That Must Be Modified By The Form
        private RTSUnit[] units;

        // This Is Team Data That Must Be Modified In A Different Tab
        private RTSTeam[] teams;
        private Vector3[] teamSpawnPositions;
        private Vector2[] teamWaypoints;
        private XColor[] teamColors;

        // This Should Be Where You Figure Out Which Team You Are Operating On
        int selectedIndex;
        int spawn1SelectedIndex;
        int spawn2SelectedIndex;
        int spawn3SelectedIndex;

        public DataForm(RTSUnit[] ud, RTSTeam[] t) {
            InitializeComponent();
            Closer = () => { Close(); };

            // Set Up Data
            units = ud;
            teams = t;
            teamSpawnPositions = new Vector3[teams.Length];
            teamWaypoints = new Vector2[teams.Length];
            teamColors = new XColor[teams.Length];

            // Populate Combo Boxes
            unitTypeComboBox.Items.Add("Unit Type 1");
            unitTypeComboBox.Items.Add("Unit Type 2");
            unitTypeComboBox.Items.Add("Unit Type 3");

            spawn1ComboBox.Items.Add("Unit Type 1");
            spawn1ComboBox.Items.Add("Unit Type 2");
            spawn1ComboBox.Items.Add("Unit Type 3");

            spawn2ComboBox.Items.Add("Unit Type 1");
            spawn2ComboBox.Items.Add("Unit Type 2");
            spawn2ComboBox.Items.Add("Unit Type 3");

            spawn3ComboBox.Items.Add("Unit Type 1");
            spawn3ComboBox.Items.Add("Unit Type 2");
            spawn3ComboBox.Items.Add("Unit Type 3");
        }

        private void DataForm_Load(object sender, EventArgs e) {
            Instance = this;
        }
        private void DataForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(!e.Cancel) Instance = null;
        }

        private void SpawnUnit(RTSUnit ud, int teamIndex) {
            RTSUnitInstance u = teams[teamIndex].AddUnit(ud, teamSpawnPositions[teamIndex]);
            u.ActionController = new ActionController();
            u.MovementController = new MovementController();
            u.MovementController.SetWaypoints(new Vector2[] { teamWaypoints[teamIndex] });
            u.CombatController = new CombatController();
            u.TargettingController = new TargettingController();
            if(OnUnitSpawn != null)
                OnUnitSpawn(u, teamColors[teamIndex]);
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
            units[selectedIndex].BaseCombatData.AttackTimer = int.Parse(attackTimerTextBox.Text);
            units[selectedIndex].BaseCombatData.AttackDamage = int.Parse(attackDamageTextBox.Text);
            units[selectedIndex].BaseCombatData.Armor = int.Parse(armorTextBox.Text);
            units[selectedIndex].BaseCombatData.CriticalDamage = int.Parse(criticalDamageTextBox.Text);
            units[selectedIndex].BaseCombatData.CriticalChance = int.Parse(criticalChanceTextBox.Text);
            units[selectedIndex].Health = int.Parse(healthTextBox.Text);
            units[selectedIndex].MovementSpeed = int.Parse(movementSpeedTextBox.Text);
        }

        private Vector3 stringtoVector3(String s) {
            //Assumes data is inputted in the form x,y,z
            float x = float.Parse(s.Substring(0,1));
            float y = float.Parse(s.Substring(2,3));
            float z = float.Parse(s.Substring(4,5));
            Vector3 posvec = new Vector3(x,y,z);
            return posvec;
        }
        private Vector2 stringtoVector2(String s) {
            //Assumes data is inputted in the form x,y
            float x = float.Parse(s.Substring(0,1));
            float y = float.Parse(s.Substring(2,3));
            Vector2 posvec = new Vector2(x,y);
            return posvec;
        }
      
        private void spawnButton_Click(object sender, EventArgs e) {          
            teamSpawnPositions[0] = stringtoVector3(team1SpawnPositionTextBox.Text);
            teamSpawnPositions[1] = stringtoVector3(team2SpawnPositionTextBox.Text);
            teamSpawnPositions[2] = stringtoVector3(team3SpawnPositionTextBox.Text);
            teamWaypoints[0] = stringtoVector2(team1WaypointTextBox.Text);
            teamWaypoints[1] = stringtoVector2(team2WaypointTextBox.Text);
            teamWaypoints[2] = stringtoVector2(team3WaypointTextBox.Text);

            System.Drawing.Color systemColor = System.Drawing.Color.FromName(team1ColorTextBox.Text);
            XColor color1 = new XColor(systemColor.R, systemColor.G, systemColor.B, systemColor.A); //Here Color is Microsoft.Xna.Framework.Graphics.Color
            teamColors[0] = color1;
            System.Drawing.Color systemColor2 = System.Drawing.Color.FromName(team2ColorTextBox.Text);
            XColor color2 = new XColor(systemColor2.R, systemColor2.G, systemColor2.B, systemColor2.A); 
            teamColors[1] = color2;
            System.Drawing.Color systemColor3 = System.Drawing.Color.FromName(team3ColorTextBox.Text);
            XColor color3 = new XColor(systemColor3.R, systemColor3.G, systemColor3.B, systemColor3.A); 
            teamColors[2] = color3;
       
            int max1 = Math.Max(int.Parse(team1Unit1TextBox.Text),int.Parse(team2Unit1TextBox.Text));
            int max2 = Math.Max(int.Parse(team3Unit1TextBox.Text),int.Parse(team1Unit2TextBox.Text));
            int max3 = Math.Max(int.Parse(team2Unit2TextBox.Text),int.Parse(team3Unit2TextBox.Text));
            int max4 = Math.Max(int.Parse(team1Unit3TextBox.Text),int.Parse(team2Unit3TextBox.Text));
            int max5 = Math.Max(max1,int.Parse(team3Unit3TextBox.Text));
            int max6 = Math.Max(max5, max2);
            int max7 = Math.Max(max6, max3);
            int max8 = Math.Max(max7, max4);

           for (int j = 0; j < max8; j++)
           {
                    if (j < int.Parse(team1Unit1TextBox.Text))
                        teams[0].AddUnit(units[0], teamSpawnPositions[0]);
                    if (j < int.Parse(team2Unit1TextBox.Text))
                        teams[1].AddUnit(units[0], teamSpawnPositions[1]);
                    if (j < int.Parse(team3Unit1TextBox.Text))
                        teams[2].AddUnit(units[0], teamSpawnPositions[2]);

                    if (j < int.Parse(team1Unit2TextBox.Text))
                        teams[0].AddUnit(units[1], teamSpawnPositions[0]);
                    if (j < int.Parse(team2Unit2TextBox.Text))
                        teams[1].AddUnit(units[1], teamSpawnPositions[1]);
                    if (j < int.Parse(team3Unit2TextBox.Text))
                        teams[2].AddUnit(units[1], teamSpawnPositions[2]);

                    if (j < int.Parse(team1Unit3TextBox.Text))
                        teams[0].AddUnit(units[2], teamSpawnPositions[0]);
                    if (j < int.Parse(team2Unit3TextBox.Text))
                        teams[1].AddUnit(units[2], teamSpawnPositions[1]);
                    if (j < int.Parse(team3Unit3TextBox.Text))
                        teams[2].AddUnit(units[2], teamSpawnPositions[2]);
           }
     
        }

        private void Spawn1ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            spawn1SelectedIndex = spawn1ComboBox.SelectedIndex;
        }

        private void Spawn2ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            spawn2SelectedIndex = spawn2ComboBox.SelectedIndex;
        }

        private void Spawn3ComboBox_SelectedIndexChanged(object sender, EventArgs e)
        {
            spawn3SelectedIndex = spawn3ComboBox.SelectedIndex;
        }

        private void spawn1Button_Click(object sender, EventArgs e)
        {
            teams[0].AddUnit(units[spawn1SelectedIndex], teamSpawnPositions[0]);
        }

        private void spawn2Button_Click(object sender, EventArgs e)
        {
            teams[1].AddUnit(units[spawn2SelectedIndex], teamSpawnPositions[1]);
        }

        private void spawn3Button_Click(object sender, EventArgs e)
        {
            teams[2].AddUnit(units[spawn3SelectedIndex], teamSpawnPositions[2]);
        }
    }
}