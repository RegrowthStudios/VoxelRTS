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

        public RTSEngine.Data.Team.Unit[] Units {
            get { throw new NotImplementedException(); }
        }

        public event Action OnGameRestart;

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

        int selectedIndex;
        int counter = 1;
        String[] stats;
        String[] spawn1;
        String[] spawn2;
        String[] spawn3;

        private void DataForm_Load(object sender, EventArgs e)
        {
            Instance = this;
        }
        private void DataForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            if (!e.Cancel) Instance = null;
        }

        private void comboBox1_SelectedIndexChanged(object sender, EventArgs e)
        {
            selectedIndex = comboBox1.SelectedIndex;
        }

        private void button1_Click(object sender, EventArgs e)
        {
            comboBox1.Items.Add("Unit " + counter);
            counter++;
        }

        private void textBox1_TextChanged(object sender, EventArgs e)
        {
            stats[0] = textBox1.Text;
        }

        private void textBox2_TextChanged(object sender, EventArgs e)
        {
            stats[1] = textBox2.Text;
        }

        private void textBox3_TextChanged(object sender, EventArgs e)
        {
            stats[2] = textBox3.Text;
        }

        private void textBox4_TextChanged(object sender, EventArgs e)
        {
            stats[3] = textBox4.Text;
        }

        private void textBox5_TextChanged(object sender, EventArgs e)
        {
            spawn1[0] = textBox5.Text;
        }

        private void textBox8_TextChanged(object sender, EventArgs e)
        {
            spawn1[1] = textBox8.Text;
        }

        private void textBox6_TextChanged(object sender, EventArgs e)
        {
            spawn1[2] = textBox6.Text;
        }

        private void textBox10_TextChanged(object sender, EventArgs e)
        {
            spawn2[0] = textBox10.Text;
        }

        private void textBox9_TextChanged(object sender, EventArgs e)
        {
            spawn2[1] = textBox9.Text;
        }

        private void textBox7_TextChanged(object sender, EventArgs e)
        {
            spawn2[2] = textBox7.Text;
        }

        private void textBox13_TextChanged(object sender, EventArgs e)
        {
            spawn3[0] = textBox13.Text;
        }

        private void textBox12_TextChanged(object sender, EventArgs e)
        {
            spawn3[1] = textBox12.Text;
        }

        private void textBox11_TextChanged(object sender, EventArgs e)
        {
            spawn3[2] = textBox11.Text;
        }

    }
}
