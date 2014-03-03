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

namespace RTSCS {
    public partial class DataForm : Form, IDataForm {
        // Should Only Be One Form Ever Made
        public static DataForm Instance { get; private set; }

        public delegate void CloseDelegate();
        public CloseDelegate Closer;

        public event Action<RTSUnitInstance, XColor> OnUnitSpawn;

        // This Is The Data That Must Be Modified By The Form
        private RTSUnit[] units;
        private RTSTeam[] teams;
        private Vector3[] teamSpawnPositions;
        private XColor[] teamColors;

        int selectedIndex;
        RTSUnit unit = new RTSUnit();
        BaseCombatData data = new BaseCombatData();
        RTSTeam team = new RTSTeam();
        int[] unittype1 = new int[4] { 4, 2, 3, 4 };
        int[] unittype2 = new int[4] { 3, 3, 3, 3 };
        int[] unittype3 = new int[4] { 2, 4, 4, 3 };

        public DataForm(RTSUnit[] ud, RTSTeam[] t) {
            InitializeComponent();
            Closer = () => { Close(); };

            // Set Up Data
            units = ud;
            teams = t;
            teamSpawnPositions = new Vector3[teams.Length];
            teamColors = new XColor[teams.Length];
        }

        private void DataForm_Load(object sender, EventArgs e) {
            Instance = this;
        }
        private void DataForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(!e.Cancel) Instance = null;
        }

        private void SendUnitEvent(RTSUnit ud, int teamIndex) {
            RTSUnitInstance u = new RTSUnitInstance(teams[teamIndex], ud, teamSpawnPositions[teamIndex]);
            if(OnUnitSpawn != null)
                OnUnitSpawn(u, teamColors[teamIndex]);
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            selectedIndex = comboBox1.SelectedIndex;
            textBox1.Text = unittype1[1].ToString();
            textBox2.Text = unittype1[2].ToString();
            textBox3.Text = unittype1[3].ToString();
            textBox4.Text = unittype1[1].ToString();

        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            data.AttackDamage = int.Parse(textBox1.Text);
            if(selectedIndex == 1)
                unittype1[1] = int.Parse(textBox1.Text);
            if(selectedIndex == 2)
                unittype2[1] = int.Parse(textBox1.Text);
            if(selectedIndex == 3)
                unittype3[1] = int.Parse(textBox1.Text);

        }

        private void textBox2_TextChanged(object sender, EventArgs e) {
            data.Armor = int.Parse(textBox1.Text);
            if(selectedIndex == 1)
                unittype1[2] = int.Parse(textBox2.Text);
            if(selectedIndex == 2)
                unittype2[2] = int.Parse(textBox2.Text);
            if(selectedIndex == 3)
                unittype3[2] = int.Parse(textBox2.Text);
        }

        private void textBox3_TextChanged(object sender, EventArgs e) {
            unit.MovementSpeed = int.Parse(textBox3.Text);
            if(selectedIndex == 1)
                unittype1[3] = int.Parse(textBox3.Text);
            if(selectedIndex == 2)
                unittype2[3] = int.Parse(textBox3.Text);
            if(selectedIndex == 3)
                unittype3[3] = int.Parse(textBox3.Text);
        }

        private void textBox4_TextChanged(object sender, EventArgs e) {
            unit.Health = int.Parse(textBox4.Text);
            if(selectedIndex == 1)
                unittype1[4] = int.Parse(textBox4.Text);
            if(selectedIndex == 2)
                unittype2[4] = int.Parse(textBox4.Text);
            if(selectedIndex == 3)
                unittype3[4] = int.Parse(textBox4.Text);
        }

        private void textBox5_TextChanged(object sender, EventArgs e) {

        }

        private void textBox8_TextChanged(object sender, EventArgs e) {

        }

        private void textBox6_TextChanged(object sender, EventArgs e) {

        }

        private void textBox10_TextChanged(object sender, EventArgs e) {

        }

        private void textBox9_TextChanged(object sender, EventArgs e) {

        }

        private void textBox7_TextChanged(object sender, EventArgs e) {

        }

        private void textBox13_TextChanged(object sender, EventArgs e) {

        }

        private void textBox12_TextChanged(object sender, EventArgs e) {

        }

        private void textBox11_TextChanged(object sender, EventArgs e) {

        }

        private void button2_Click(object sender, EventArgs e) {

        }

        private void comboBox2_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void button1_Click(object sender, EventArgs e) {

        }

        private void comboBox3_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void button3_Click(object sender, EventArgs e) {

        }

        private void comboBox4_SelectedIndexChanged(object sender, EventArgs e) {

        }

        private void button4_Click(object sender, EventArgs e) {

        }


    }
}