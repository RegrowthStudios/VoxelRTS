using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RTSCS {
    public partial class DataForm : Form, IDataForm {
        // Should Only Be One Form Ever Made
        public static DataForm Instance { get; private set; }

        public delegate void CloseDelegate();
        public CloseDelegate Closer;

        public event Action<GameRestartArgs> OnGameRestart;

        int selectedIndex;
        int counter = 1;
        RTSEngine.Data.Team.RTSUnit unit = new RTSEngine.Data.Team.RTSUnit();
        RTSEngine.Data.BaseCombatData data = new RTSEngine.Data.BaseCombatData();
        RTSEngine.Data.Team.RTSTeam team = new RTSEngine.Data.Team.RTSTeam();

        public DataForm() {
            InitializeComponent();
            Closer = () => { Close(); };
        }

        private void DataForm_Load(object sender, EventArgs e) {
            Instance = this;
        }
        private void DataForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(!e.Cancel) Instance = null;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e) {
            selectedIndex = comboBox1.SelectedIndex;
        }

        private void button1_Click(object sender, EventArgs e) {
            comboBox1.Items.Add("Unit " + counter);
            counter++;
        }

        private void textBox1_TextChanged(object sender, EventArgs e) {
            data.AttackDamage = int.Parse(textBox1.Text);
        }

        private void textBox2_TextChanged(object sender, EventArgs e) {
            data.Armor = int.Parse(textBox1.Text);
        }

        private void textBox3_TextChanged(object sender, EventArgs e) {
            unit.MovementSpeed = int.Parse(textBox3.Text);
        }

        private void textBox4_TextChanged(object sender, EventArgs e) {
            unit.Health = int.Parse(textBox4.Text);
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

        private void button2_Click(object sender, EventArgs e)
        {

        }

    }
}