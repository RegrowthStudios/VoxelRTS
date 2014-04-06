using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.IO;
using System.Drawing.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Microsoft.Win32;

namespace RTSInstaller {
    public partial class AppForm : Form {
        public AppForm() {
            InitializeComponent();
        }

        private void btnInstall_Click(object sender, EventArgs e) {
            pbInstall.Value = 5;
            InstallFont("Chintzy CPU BRK", new FileInfo(@"Data\chintzy.ttf"));
            pbInstall.Value = 50;
            InstallFont("Chintzy CPU Shadow BRK", new FileInfo(@"Data\chintzys.ttf"));
            pbInstall.Value = 100;
        }

        private static void InstallFont(string name, FileInfo fi) {
            string p = Path.Combine(@"C:\Windows\Fonts", fi.Name);
            if(!File.Exists(p))
                File.Copy(fi.FullName, p);

            Registry.SetValue(@"HKEY_LOCAL_MACHINE\SOFTWARE\Microsoft\Windows NT\CurrentVersion\Fonts", name + " (TrueType)", fi.Name);
        }
    }
}
