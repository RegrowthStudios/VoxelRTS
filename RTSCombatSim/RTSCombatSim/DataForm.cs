using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace RTSCS {
    public partial class DataForm : Form {
        // Should Only Be One Form Ever Made
        public static DataForm Instance { get; private set; }

        public DataForm() {
            InitializeComponent();
        }

        private void DataForm_Load(object sender, EventArgs e) {
            Instance = this;
        }
        private void DataForm_FormClosing(object sender, FormClosingEventArgs e) {
            if(!e.Cancel) Instance = null;
        }

    }
}
