namespace AnimConverter {
    partial class ConvForm {
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
            this.gbIO = new System.Windows.Forms.GroupBox();
            this.tbPrefix = new System.Windows.Forms.TextBox();
            this.label2 = new System.Windows.Forms.Label();
            this.tbDir = new System.Windows.Forms.TextBox();
            this.label1 = new System.Windows.Forms.Label();
            this.rtbInfo = new System.Windows.Forms.RichTextBox();
            this.btnBuild = new System.Windows.Forms.Button();
            this.gbIO.SuspendLayout();
            this.SuspendLayout();
            // 
            // gbIO
            // 
            this.gbIO.Controls.Add(this.tbPrefix);
            this.gbIO.Controls.Add(this.label2);
            this.gbIO.Controls.Add(this.tbDir);
            this.gbIO.Controls.Add(this.label1);
            this.gbIO.Dock = System.Windows.Forms.DockStyle.Top;
            this.gbIO.Location = new System.Drawing.Point(0, 0);
            this.gbIO.Name = "gbIO";
            this.gbIO.Size = new System.Drawing.Size(334, 76);
            this.gbIO.TabIndex = 0;
            this.gbIO.TabStop = false;
            this.gbIO.Text = "IO";
            // 
            // tbPrefix
            // 
            this.tbPrefix.Location = new System.Drawing.Point(81, 39);
            this.tbPrefix.Name = "tbPrefix";
            this.tbPrefix.Size = new System.Drawing.Size(247, 20);
            this.tbPrefix.TabIndex = 1;
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Location = new System.Drawing.Point(6, 42);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(33, 13);
            this.label2.TabIndex = 0;
            this.label2.Text = "Prefix";
            // 
            // tbDir
            // 
            this.tbDir.Location = new System.Drawing.Point(81, 13);
            this.tbDir.Name = "tbDir";
            this.tbDir.Size = new System.Drawing.Size(247, 20);
            this.tbDir.TabIndex = 0;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Location = new System.Drawing.Point(6, 16);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(49, 13);
            this.label1.TabIndex = 1;
            this.label1.Text = "Directory";
            // 
            // rtbInfo
            // 
            this.rtbInfo.Location = new System.Drawing.Point(0, 82);
            this.rtbInfo.Name = "rtbInfo";
            this.rtbInfo.ReadOnly = true;
            this.rtbInfo.Size = new System.Drawing.Size(334, 237);
            this.rtbInfo.TabIndex = 1;
            this.rtbInfo.Text = "";
            // 
            // btnBuild
            // 
            this.btnBuild.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.btnBuild.Location = new System.Drawing.Point(0, 325);
            this.btnBuild.Name = "btnBuild";
            this.btnBuild.Size = new System.Drawing.Size(334, 23);
            this.btnBuild.TabIndex = 2;
            this.btnBuild.Text = "Build";
            this.btnBuild.UseVisualStyleBackColor = true;
            this.btnBuild.Click += new System.EventHandler(this.btnBuild_Click);
            // 
            // ConvForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(334, 348);
            this.Controls.Add(this.btnBuild);
            this.Controls.Add(this.rtbInfo);
            this.Controls.Add(this.gbIO);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedDialog;
            this.MaximizeBox = false;
            this.Name = "ConvForm";
            this.Text = "OBJ Animation Converter";
            this.Load += new System.EventHandler(this.ConvForm_Load);
            this.gbIO.ResumeLayout(false);
            this.gbIO.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.GroupBox gbIO;
        private System.Windows.Forms.RichTextBox rtbInfo;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.TextBox tbDir;
        private System.Windows.Forms.TextBox tbPrefix;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.Button btnBuild;
    }
}

