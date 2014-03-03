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

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            selectedIndex = comboBox1.SelectedIndex;
            textBox1.Text = units[selectedIndex].BaseCombatData.MinRange.ToString();
            textBox4.Text = units[selectedIndex].BaseCombatData.MaxRange.ToString();
            textBox23.Text = units[selectedIndex].BaseCombatData.AttackTimer.ToString();
            textBox3.Text = units[selectedIndex].BaseCombatData.Armor.ToString();
            textBox24.Text = units[selectedIndex].BaseCombatData.CriticalDamage.ToString();
            textBox25.Text = units[selectedIndex].BaseCombatData.CriticalChance.ToString();
            textBox26.Text = units[selectedIndex].Health.ToString();
            textBox28.Text = units[selectedIndex].MovementSpeed.ToString();

        }

        private void button5_Click(object sender, EventArgs e)
        {
            units[selectedIndex].BaseCombatData.MinRange = int.Parse(textBox1.Text);
            units[selectedIndex].BaseCombatData.MaxRange = int.Parse(textBox4.Text);
            units[selectedIndex].BaseCombatData.AttackTimer = int.Parse(textBox23.Text);
            units[selectedIndex].BaseCombatData.Armor = int.Parse(textBox3.Text);
            units[selectedIndex].BaseCombatData.CriticalDamage = int.Parse(textBox24.Text);
            units[selectedIndex].BaseCombatData.CriticalChance = int.Parse(textBox25.Text);
            units[selectedIndex].Health = int.Parse(textBox26.Text);
            units[selectedIndex].MovementSpeed = int.Parse(textBox28.Text);
        }

        



       
    }
}