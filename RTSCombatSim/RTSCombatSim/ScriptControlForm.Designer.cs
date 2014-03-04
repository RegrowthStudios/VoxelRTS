namespace RTSCS {
    partial class ScriptControlForm {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing) {
            if(disposing && (components != null)) {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent() {
            this.gbCompiler = new System.Windows.Forms.GroupBox();
            this.btnCompile = new System.Windows.Forms.Button();
            this.rtbCompile = new System.Windows.Forms.RichTextBox();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.tabPage1 = new System.Windows.Forms.TabPage();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.rtbScript = new System.Windows.Forms.RichTextBox();
            this.btnGenerate = new System.Windows.Forms.Button();
            this.cbTemplates = new System.Windows.Forms.ComboBox();
            this.tabPage2 = new System.Windows.Forms.TabPage();
            this.splitContainer2 = new System.Windows.Forms.SplitContainer();
            this.groupBox1 = new System.Windows.Forms.GroupBox();
            this.rtbReferences = new System.Windows.Forms.RichTextBox();
            this.gbCompiler.SuspendLayout();
            this.tabControl1.SuspendLayout();
            this.tabPage1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.tabPage2.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).BeginInit();
            this.splitContainer2.Panel1.SuspendLayout();
            this.splitContainer2.Panel2.SuspendLayout();
            this.splitContainer2.SuspendLayout();
            this.groupBox1.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbCompiler
            // 
            this.gbCompiler.Controls.Add(this.btnCompile);
            this.gbCompiler.Controls.Add(this.rtbCompile);
            this.gbCompiler.Dock = System.Windows.Forms.DockStyle.Fill;
            this.gbCompiler.Location = new System.Drawing.Point(0, 0);
            this.gbCompiler.Name = "gbCompiler";
            this.gbCompiler.Size = new System.Drawing.Size(498, 206);
            this.gbCompiler.TabIndex = 0;
            this.gbCompiler.TabStop = false;
            this.gbCompiler.Text = "Compiler";
            // 
            // btnCompile
            // 
            this.btnCompile.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnCompile.Location = new System.Drawing.Point(3, 180);
            this.btnCompile.Name = "btnCompile";
            this.btnCompile.Size = new System.Drawing.Size(492, 23);
            this.btnCompile.TabIndex = 1;
            this.btnCompile.Text = "Compile";
            this.btnCompile.UseVisualStyleBackColor = true;
            this.btnCompile.Click += new System.EventHandler(this.btnCompile_Click);
            // 
            // rtbCompile
            // 
            this.rtbCompile.Dock = System.Windows.Forms.DockStyle.Top;
            this.rtbCompile.Location = new System.Drawing.Point(3, 16);
            this.rtbCompile.Name = "rtbCompile";
            this.rtbCompile.Size = new System.Drawing.Size(492, 158);
            this.rtbCompile.TabIndex = 0;
            this.rtbCompile.Text = "";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.tabPage1);
            this.tabControl1.Controls.Add(this.tabPage2);
            this.tabControl1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabControl1.Location = new System.Drawing.Point(0, 0);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(512, 450);
            this.tabControl1.TabIndex = 1;
            // 
            // tabPage1
            // 
            this.tabPage1.Controls.Add(this.splitContainer1);
            this.tabPage1.Location = new System.Drawing.Point(4, 22);
            this.tabPage1.Name = "tabPage1";
            this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage1.Size = new System.Drawing.Size(504, 424);
            this.tabPage1.TabIndex = 0;
            this.tabPage1.Text = "Scripting";
            this.tabPage1.UseVisualStyleBackColor = true;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(3, 3);
            this.splitContainer1.Name = "splitContainer1";
            this.splitContainer1.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.rtbScript);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.btnGenerate);
            this.splitContainer1.Panel2.Controls.Add(this.cbTemplates);
            this.splitContainer1.Size = new System.Drawing.Size(498, 418);
            this.splitContainer1.SplitterDistance = 369;
            this.splitContainer1.TabIndex = 3;
            // 
            // rtbScript
            // 
            this.rtbScript.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbScript.Location = new System.Drawing.Point(0, 0);
            this.rtbScript.Name = "rtbScript";
            this.rtbScript.Size = new System.Drawing.Size(498, 369);
            this.rtbScript.TabIndex = 0;
            this.rtbScript.Text = "";
            // 
            // btnGenerate
            // 
            this.btnGenerate.Location = new System.Drawing.Point(5, 3);
            this.btnGenerate.Name = "btnGenerate";
            this.btnGenerate.Size = new System.Drawing.Size(75, 23);
            this.btnGenerate.TabIndex = 1;
            this.btnGenerate.Text = "Generate";
            this.btnGenerate.UseVisualStyleBackColor = true;
            this.btnGenerate.Click += new System.EventHandler(this.btnGenerate_Click);
            // 
            // cbTemplates
            // 
            this.cbTemplates.FormattingEnabled = true;
            this.cbTemplates.Items.AddRange(new object[] {
            "Action",
            "Combat",
            "Movement",
            "Targetting"});
            this.cbTemplates.Location = new System.Drawing.Point(86, 5);
            this.cbTemplates.Name = "cbTemplates";
            this.cbTemplates.Size = new System.Drawing.Size(264, 21);
            this.cbTemplates.TabIndex = 2;
            this.cbTemplates.Text = "Action";
            // 
            // tabPage2
            // 
            this.tabPage2.Controls.Add(this.splitContainer2);
            this.tabPage2.Location = new System.Drawing.Point(4, 22);
            this.tabPage2.Name = "tabPage2";
            this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
            this.tabPage2.Size = new System.Drawing.Size(504, 424);
            this.tabPage2.TabIndex = 1;
            this.tabPage2.Text = "Options";
            this.tabPage2.UseVisualStyleBackColor = true;
            // 
            // splitContainer2
            // 
            this.splitContainer2.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer2.Location = new System.Drawing.Point(3, 3);
            this.splitContainer2.Name = "splitContainer2";
            this.splitContainer2.Orientation = System.Windows.Forms.Orientation.Horizontal;
            // 
            // splitContainer2.Panel1
            // 
            this.splitContainer2.Panel1.Controls.Add(this.groupBox1);
            // 
            // splitContainer2.Panel2
            // 
            this.splitContainer2.Panel2.Controls.Add(this.gbCompiler);
            this.splitContainer2.Size = new System.Drawing.Size(498, 418);
            this.splitContainer2.SplitterDistance = 208;
            this.splitContainer2.TabIndex = 2;
            // 
            // groupBox1
            // 
            this.groupBox1.Controls.Add(this.rtbReferences);
            this.groupBox1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.groupBox1.Location = new System.Drawing.Point(0, 0);
            this.groupBox1.Name = "groupBox1";
            this.groupBox1.Size = new System.Drawing.Size(498, 208);
            this.groupBox1.TabIndex = 1;
            this.groupBox1.TabStop = false;
            this.groupBox1.Text = "Referenced Assemblies";
            // 
            // rtbReferences
            // 
            this.rtbReferences.Dock = System.Windows.Forms.DockStyle.Fill;
            this.rtbReferences.Location = new System.Drawing.Point(3, 16);
            this.rtbReferences.Name = "rtbReferences";
            this.rtbReferences.Size = new System.Drawing.Size(492, 189);
            this.rtbReferences.TabIndex = 0;
            this.rtbReferences.Text = "System.dll\nSystem.Data.dll\nSystem.Core.dll\nSystem.Xml.dll\nSystem.Xml.Linq.dll\nlib" +
    "\\Microsoft.Xna.Framework.dll\nRTSEngine.dll";
            // 
            // ScriptControlForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(512, 450);
            this.Controls.Add(this.tabControl1);
            this.Name = "ScriptControlForm";
            this.Text = "Script Maker V1";
            this.gbCompiler.ResumeLayout(false);
            this.tabControl1.ResumeLayout(false);
            this.tabPage1.ResumeLayout(false);
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer1)).EndInit();
            this.splitContainer1.ResumeLayout(false);
            this.tabPage2.ResumeLayout(false);
            this.splitContainer2.Panel1.ResumeLayout(false);
            this.splitContainer2.Panel2.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.splitContainer2)).EndInit();
            this.splitContainer2.ResumeLayout(false);
            this.groupBox1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbCompiler;
        private System.Windows.Forms.Button btnCompile;
        private System.Windows.Forms.RichTextBox rtbCompile;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage tabPage1;
        private System.Windows.Forms.RichTextBox rtbScript;
        private System.Windows.Forms.TabPage tabPage2;
        private System.Windows.Forms.RichTextBox rtbReferences;
        private System.Windows.Forms.Button btnGenerate;
        private System.Windows.Forms.ComboBox cbTemplates;
        private System.Windows.Forms.SplitContainer splitContainer1;
        private System.Windows.Forms.SplitContainer splitContainer2;
        private System.Windows.Forms.GroupBox groupBox1;

    }
}