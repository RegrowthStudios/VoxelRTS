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

            Spawn1ComboBox.Items.Add("Unit Type 1");
            Spawn1ComboBox.Items.Add("Unit Type 2");
            Spawn1ComboBox.Items.Add("Unit Type 3");

            Spawn2ComboBox.Items.Add("Unit Type 1");
            Spawn2ComboBox.Items.Add("Unit Type 2");
            Spawn2ComboBox.Items.Add("Unit Type 3");

            Spawn3ComboBox.Items.Add("Unit Type 1");
            Spawn3ComboBox.Items.Add("Unit Type 2");
            Spawn3ComboBox.Items.Add("Unit Type 3");
        }

        private void DataForm_Load(object sender, EventArgs e) {
            Instance = this;
        }
        private void DataForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(!e.Cancel) Instance = null;
        }

        private void SpawnUnit(RTSUnit ud, int teamIndex) {
            RTSUnitInstance u = teams[teamIndex].AddUnit(ud, teamSpawnPositions[teamIndex]);
            u.ActionController = new ActionController(u);
            u.MovementController = new MovementController(u, new Vector2[] { teamWaypoints[teamIndex] });
            u.CombatController = new CombatController(u);
            u.TargettingController = new TargettingController(u);
            if(OnUnitSpawn != null)
                OnUnitSpawn(u, teamColors[teamIndex]);
        }

        private void UnitTypeComboBoxChanged(object sender, EventArgs e) {
            selectedIndex = unitTypeComboBox.SelectedIndex;
            minRangeTextBox.Text = units[selectedIndex].BaseCombatData.MinRange.ToString();
            maxRangeTextBox.Text = units[selectedIndex].BaseCombatData.MaxRange.ToString();
            attackDamageTextBox.Text = units[selectedIndex].BaseCombatData.AttackDamage.ToString();
            attackTimerTextBox.Text = units[selectedIndex].BaseCombatData.AttackTimer.ToString();
            ArmorTextBox.Text = units[selectedIndex].BaseCombatData.Armor.ToString();
            criticalDamageTextBox.Text = units[selectedIndex].BaseCombatData.CriticalDamage.ToString();
            criticalChanceTextBox.Text = units[selectedIndex].BaseCombatData.CriticalChance.ToString();
            healthTextBox.Text = units[selectedIndex].Health.ToString();
            movementSpeedTextBox.Text = units[selectedIndex].MovementSpeed.ToString();
        }

        private void SaveButtonClick(object sender, EventArgs e) {
            units[selectedIndex].BaseCombatData.MinRange = int.Parse(minRangeTextBox.Text);
            units[selectedIndex].BaseCombatData.MaxRange = int.Parse(maxRangeTextBox.Text);
            units[selectedIndex].BaseCombatData.AttackTimer = int.Parse(attackTimerTextBox.Text);
            units[selectedIndex].BaseCombatData.AttackDamage = int.Parse(attackDamageTextBox.Text);
            units[selectedIndex].BaseCombatData.Armor = int.Parse(ArmorTextBox.Text);
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
      
        private void SpawnButtonClick(object sender, EventArgs e) {          
            teamSpawnPositions[1] = stringtoVector3(textBox19.Text);
            teamSpawnPositions[2] = stringtoVector3(textBox18.Text);
            teamSpawnPositions[3] = stringtoVector3(textBox17.Text);
            teamWaypoints[1] = stringtoVector2(textBox22.Text);
            teamWaypoints[2] = stringtoVector2(textBox21.Text);
            teamWaypoints[3] = stringtoVector2(textBox20.Text);

            System.Drawing.Color systemColor = System.Drawing.Color.FromName(textBox14.Text);
            XColor color1 = new XColor(systemColor.R, systemColor.G, systemColor.B, systemColor.A); //Here Color is Microsoft.Xna.Framework.Graphics.Color
            teamColors[1] = color1;
            System.Drawing.Color systemColor2 = System.Drawing.Color.FromName(textBox15.Text);
            XColor color2 = new XColor(systemColor2.R, systemColor2.G, systemColor2.B, systemColor2.A); 
            teamColors[2] = color2;
            System.Drawing.Color systemColor3 = System.Drawing.Color.FromName(textBox16.Text);
            XColor color3 = new XColor(systemColor3.R, systemColor3.G, systemColor3.B, systemColor3.A); 
            teamColors[3] = color3;
        }

        private void Spawn1Clicked(object sender, EventArgs e) {

        }

        private void Spawn2Clicked(object sender, EventArgs e) {

        }

        private void Spawn3Clicked(object sender, EventArgs e) {

        }
    }
}