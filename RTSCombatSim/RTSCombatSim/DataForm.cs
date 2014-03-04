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
            comboBox1.Items.Add("Unit Type 1");
            comboBox1.Items.Add("Unit Type 2");
            comboBox1.Items.Add("Unit Type 3");
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

        private Vector3 stringtoVector3(String s)
        {
            //Assumes data is inputted in the form x,y,z
            float x = float.Parse(s.Substring(0, 1));
            float y = float.Parse(s.Substring(2,3));
            float z = float.Parse(s.Substring(4,5));
            Vector3 posvec = new Vector3(x, y, z);
            return posvec;
        }
        private Vector2 stringtoVector2(String s)
        {
            //Assumes data is inputted in the form x,y
            float x = float.Parse(s.Substring(0, 1));
            float y = float.Parse(s.Substring(2,3));
            Vector2 posvec = new Vector2(x, y);
            return posvec;
        }
      
        private void button2_Click(object sender, EventArgs e)
        {          
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
            int max1 = Math.Max(int.Parse(textBox5.Text),int.Parse(textBox10.Text));
            int max2 = Math.Max(int.Parse(textBox13.Text),int.Parse(textBox8.Text));
            int max3 = Math.Max(int.Parse(textBox9.Text),int.Parse(textBox12.Text));
            int max4 = Math.Max(int.Parse(textBox6.Text),int.Parse(textBox7.Text));
            int max5 = Math.Max(max1,int.Parse(textBox11.Text));
            int max6 = Math.Max(max5, max2);
            int max7 = Math.Max(max6, max3);
            int max8 = Math.Max(max7, max4);

           for (int j = 1; j <= max8; j++)
           {
                    if (j <= int.Parse(textBox5.Text))
                        teams[1].AddUnit(units[1], teamSpawnPositions[1]);
                    if (j <= int.Parse(textBox10.Text))
                        teams[2].AddUnit(units[1], teamSpawnPositions[2]);
                    if (j <= int.Parse(textBox13.Text))
                        teams[3].AddUnit(units[1], teamSpawnPositions[3]);

                    if (j <= int.Parse(textBox8.Text))
                        teams[1].AddUnit(units[2], teamSpawnPositions[1]);
                    if (j <= int.Parse(textBox9.Text))
                        teams[2].AddUnit(units[2], teamSpawnPositions[2]);
                    if (j <= int.Parse(textBox12.Text))
                        teams[3].AddUnit(units[2], teamSpawnPositions[3]);

                    if (j <= int.Parse(textBox6.Text))
                        teams[1].AddUnit(units[3], teamSpawnPositions[1]);
                    if (j <= int.Parse(textBox7.Text))
                        teams[2].AddUnit(units[3], teamSpawnPositions[2]);
                    if (j <= int.Parse(textBox11.Text))
                        teams[3].AddUnit(units[3], teamSpawnPositions[3]);
           }
            
        }

        



       
    }
}