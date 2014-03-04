namespace RTSCS {
    partial class DataForm {
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
            this.unitTypeLabel = new System.Windows.Forms.Label();
            this.unitTypeComboBox = new System.Windows.Forms.ComboBox();
            this.minRangeTextBox = new System.Windows.Forms.TextBox();
            this.minAttackRangeLabel = new System.Windows.Forms.Label();
            this.spawn1Label = new System.Windows.Forms.Label();
            this.spawn2Label = new System.Windows.Forms.Label();
            this.spawn3Label = new System.Windows.Forms.Label();
            this.unit1Label = new System.Windows.Forms.Label();
            this.unit2Label = new System.Windows.Forms.Label();
            this.unit3Label = new System.Windows.Forms.Label();
            this.team1Unit1TextBox = new System.Windows.Forms.TextBox();
            this.team1Unit3TextBox = new System.Windows.Forms.TextBox();
            this.team2Unit3TextBox = new System.Windows.Forms.TextBox();
            this.team2Unit2TextBox = new System.Windows.Forms.TextBox();
            this.team2Unit1TextBox = new System.Windows.Forms.TextBox();
            this.team3Unit3TextBox = new System.Windows.Forms.TextBox();
            this.team3Unit2TextBox = new System.Windows.Forms.TextBox();
            this.team3Unit1TextBox = new System.Windows.Forms.TextBox();
            this.spawnButton = new System.Windows.Forms.Button();
            this.addUnitLabel = new System.Windows.Forms.Label();
            this.spawn1Button = new System.Windows.Forms.Button();
            this.spawn1ComboBox = new System.Windows.Forms.ComboBox();
            this.spawn2ComboBox = new System.Windows.Forms.ComboBox();
            this.spawn2Button = new System.Windows.Forms.Button();
            this.spawn3ComboBox = new System.Windows.Forms.ComboBox();
            this.spawn3Button = new System.Windows.Forms.Button();
            this.team1Unit2TextBox = new System.Windows.Forms.TextBox();
            this.toSpawnLabel = new System.Windows.Forms.Label();
            this.helpProvider1 = new System.Windows.Forms.HelpProvider();
            this.tabControl1 = new System.Windows.Forms.TabControl();
            this.unitMenuTabPage = new System.Windows.Forms.TabPage();
            this.saveUnitTypeButton = new System.Windows.Forms.Button();
            this.movementSpeedTextBox = new System.Windows.Forms.TextBox();
            this.movementSpeedLabel = new System.Windows.Forms.Label();
            this.healthTextBox = new System.Windows.Forms.TextBox();
            this.healthLabel = new System.Windows.Forms.Label();
            this.criticalChanceTextBox = new System.Windows.Forms.TextBox();
            this.criticalChanceLabel = new System.Windows.Forms.Label();
            this.criticalDamageTextBox = new System.Windows.Forms.TextBox();
            this.criticalDamageLabel = new System.Windows.Forms.Label();
            this.attackTimerTextBox = new System.Windows.Forms.TextBox();
            this.attackTimerLabel = new System.Windows.Forms.Label();
            this.maxRangeTextBox = new System.Windows.Forms.TextBox();
            this.maxAttackRangeLabel = new System.Windows.Forms.Label();
            this.armorTextBox = new System.Windows.Forms.TextBox();
            this.armorLabel = new System.Windows.Forms.Label();
            this.attackDamageTextBox = new System.Windows.Forms.TextBox();
            this.attackDamageLabel = new System.Windows.Forms.Label();
            this.spawnMenuTabPage = new System.Windows.Forms.TabPage();
            this.team3WaypointTextBox = new System.Windows.Forms.TextBox();
            this.team2WaypointTextBox = new System.Windows.Forms.TextBox();
            this.waypointLabel = new System.Windows.Forms.Label();
            this.team1WaypointTextBox = new System.Windows.Forms.TextBox();
            this.team3SpawnPositionTextBox = new System.Windows.Forms.TextBox();
            this.team2SpawnPositionTextBox = new System.Windows.Forms.TextBox();
            this.spawnPositionLabel = new System.Windows.Forms.Label();
            this.team1SpawnPositionTextBox = new System.Windows.Forms.TextBox();
            this.team3ColorTextBox = new System.Windows.Forms.TextBox();
            this.team2ColorTextBox = new System.Windows.Forms.TextBox();
            this.teamColorLabel = new System.Windows.Forms.Label();
            this.team1ColorTextBox = new System.Windows.Forms.TextBox();
            this.btnScriptDialog = new System.Windows.Forms.Button();
            this.tabControl1.SuspendLayout();
            this.unitMenuTabPage.SuspendLayout();
            this.spawnMenuTabPage.SuspendLayout();
            this.SuspendLayout();
            // 
            // unitTypeLabel
            // 
            this.unitTypeLabel.AutoSize = true;
            this.unitTypeLabel.Location = new System.Drawing.Point(170, 34);
            this.unitTypeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.unitTypeLabel.Name = "unitTypeLabel";
            this.unitTypeLabel.Size = new System.Drawing.Size(53, 13);
            this.unitTypeLabel.TabIndex = 0;
            this.unitTypeLabel.Text = "Unit Type";
            // 
            // unitTypeComboBox
            // 
            this.unitTypeComboBox.FormattingEnabled = true;
            this.unitTypeComboBox.Location = new System.Drawing.Point(226, 34);
            this.unitTypeComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.unitTypeComboBox.Name = "unitTypeComboBox";
            this.unitTypeComboBox.Size = new System.Drawing.Size(92, 21);
            this.unitTypeComboBox.TabIndex = 1;
            this.unitTypeComboBox.SelectedIndexChanged += new System.EventHandler(this.unitTypeComboBox_Change);
            // 
            // minRangeTextBox
            // 
            this.minRangeTextBox.Location = new System.Drawing.Point(147, 87);
            this.minRangeTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.minRangeTextBox.Name = "minRangeTextBox";
            this.minRangeTextBox.Size = new System.Drawing.Size(64, 20);
            this.minRangeTextBox.TabIndex = 3;
            // 
            // minAttackRangeLabel
            // 
            this.minAttackRangeLabel.AutoSize = true;
            this.minAttackRangeLabel.Location = new System.Drawing.Point(45, 89);
            this.minAttackRangeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.minAttackRangeLabel.Name = "minAttackRangeLabel";
            this.minAttackRangeLabel.Size = new System.Drawing.Size(86, 13);
            this.minAttackRangeLabel.TabIndex = 4;
            this.minAttackRangeLabel.Text = "minAttackRange";
            // 
            // spawn1Label
            // 
            this.spawn1Label.AutoSize = true;
            this.spawn1Label.Location = new System.Drawing.Point(158, 46);
            this.spawn1Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.spawn1Label.Name = "spawn1Label";
            this.spawn1Label.Size = new System.Drawing.Size(81, 13);
            this.spawn1Label.TabIndex = 11;
            this.spawn1Label.Text = "Team/Spawn 1";
            // 
            // spawn2Label
            // 
            this.spawn2Label.AutoSize = true;
            this.spawn2Label.Location = new System.Drawing.Point(272, 46);
            this.spawn2Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.spawn2Label.Name = "spawn2Label";
            this.spawn2Label.Size = new System.Drawing.Size(81, 13);
            this.spawn2Label.TabIndex = 12;
            this.spawn2Label.Text = "Team/Spawn 2";
            // 
            // spawn3Label
            // 
            this.spawn3Label.AutoSize = true;
            this.spawn3Label.Location = new System.Drawing.Point(382, 46);
            this.spawn3Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.spawn3Label.Name = "spawn3Label";
            this.spawn3Label.Size = new System.Drawing.Size(81, 13);
            this.spawn3Label.TabIndex = 13;
            this.spawn3Label.Text = "Team/Spawn 3";
            // 
            // unit1Label
            // 
            this.unit1Label.AutoSize = true;
            this.unit1Label.Location = new System.Drawing.Point(67, 74);
            this.unit1Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.unit1Label.Name = "unit1Label";
            this.unit1Label.Size = new System.Drawing.Size(35, 13);
            this.unit1Label.TabIndex = 14;
            this.unit1Label.Text = "Unit 1";
            // 
            // unit2Label
            // 
            this.unit2Label.AutoSize = true;
            this.unit2Label.Location = new System.Drawing.Point(67, 110);
            this.unit2Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.unit2Label.Name = "unit2Label";
            this.unit2Label.Size = new System.Drawing.Size(35, 13);
            this.unit2Label.TabIndex = 15;
            this.unit2Label.Text = "Unit 2";
            // 
            // unit3Label
            // 
            this.unit3Label.AutoSize = true;
            this.unit3Label.Location = new System.Drawing.Point(67, 143);
            this.unit3Label.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.unit3Label.Name = "unit3Label";
            this.unit3Label.Size = new System.Drawing.Size(35, 13);
            this.unit3Label.TabIndex = 16;
            this.unit3Label.Text = "Unit 3";
            // 
            // team1Unit1TextBox
            // 
            this.team1Unit1TextBox.Location = new System.Drawing.Point(160, 72);
            this.team1Unit1TextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team1Unit1TextBox.Name = "team1Unit1TextBox";
            this.team1Unit1TextBox.Size = new System.Drawing.Size(79, 20);
            this.team1Unit1TextBox.TabIndex = 17;
            // 
            // team1Unit3TextBox
            // 
            this.team1Unit3TextBox.Location = new System.Drawing.Point(160, 141);
            this.team1Unit3TextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team1Unit3TextBox.Name = "team1Unit3TextBox";
            this.team1Unit3TextBox.Size = new System.Drawing.Size(79, 20);
            this.team1Unit3TextBox.TabIndex = 21;
            // 
            // team2Unit3TextBox
            // 
            this.team2Unit3TextBox.Location = new System.Drawing.Point(274, 141);
            this.team2Unit3TextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team2Unit3TextBox.Name = "team2Unit3TextBox";
            this.team2Unit3TextBox.Size = new System.Drawing.Size(79, 20);
            this.team2Unit3TextBox.TabIndex = 27;
            // 
            // team2Unit2TextBox
            // 
            this.team2Unit2TextBox.Location = new System.Drawing.Point(274, 106);
            this.team2Unit2TextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team2Unit2TextBox.Name = "team2Unit2TextBox";
            this.team2Unit2TextBox.Size = new System.Drawing.Size(79, 20);
            this.team2Unit2TextBox.TabIndex = 26;
            // 
            // team2Unit1TextBox
            // 
            this.team2Unit1TextBox.Location = new System.Drawing.Point(274, 72);
            this.team2Unit1TextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team2Unit1TextBox.Name = "team2Unit1TextBox";
            this.team2Unit1TextBox.Size = new System.Drawing.Size(79, 20);
            this.team2Unit1TextBox.TabIndex = 25;
            // 
            // team3Unit3TextBox
            // 
            this.team3Unit3TextBox.Location = new System.Drawing.Point(384, 141);
            this.team3Unit3TextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team3Unit3TextBox.Name = "team3Unit3TextBox";
            this.team3Unit3TextBox.Size = new System.Drawing.Size(79, 20);
            this.team3Unit3TextBox.TabIndex = 30;
            // 
            // team3Unit2TextBox
            // 
            this.team3Unit2TextBox.Location = new System.Drawing.Point(384, 106);
            this.team3Unit2TextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team3Unit2TextBox.Name = "team3Unit2TextBox";
            this.team3Unit2TextBox.Size = new System.Drawing.Size(79, 20);
            this.team3Unit2TextBox.TabIndex = 29;
            // 
            // team3Unit1TextBox
            // 
            this.team3Unit1TextBox.Location = new System.Drawing.Point(384, 72);
            this.team3Unit1TextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team3Unit1TextBox.Name = "team3Unit1TextBox";
            this.team3Unit1TextBox.Size = new System.Drawing.Size(79, 20);
            this.team3Unit1TextBox.TabIndex = 28;
            // 
            // spawnButton
            // 
            this.spawnButton.Location = new System.Drawing.Point(234, 340);
            this.spawnButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.spawnButton.Name = "spawnButton";
            this.spawnButton.Size = new System.Drawing.Size(90, 32);
            this.spawnButton.TabIndex = 31;
            this.spawnButton.Text = "Spawn";
            this.spawnButton.UseVisualStyleBackColor = true;
            this.spawnButton.Click += new System.EventHandler(this.spawnButton_Click);
            // 
            // addUnitLabel
            // 
            this.addUnitLabel.AutoSize = true;
            this.addUnitLabel.Location = new System.Drawing.Point(67, 280);
            this.addUnitLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.addUnitLabel.Name = "addUnitLabel";
            this.addUnitLabel.Size = new System.Drawing.Size(48, 13);
            this.addUnitLabel.TabIndex = 32;
            this.addUnitLabel.Text = "Add Unit";
            // 
            // spawn1Button
            // 
            this.spawn1Button.Location = new System.Drawing.Point(242, 288);
            this.spawn1Button.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.spawn1Button.Name = "spawn1Button";
            this.spawn1Button.Size = new System.Drawing.Size(20, 19);
            this.spawn1Button.TabIndex = 33;
            this.spawn1Button.Text = "+";
            this.spawn1Button.UseVisualStyleBackColor = true;
            this.spawn1Button.Click += new System.EventHandler(this.spawn3Button_Click);
            // 
            // spawn1ComboBox
            // 
            this.spawn1ComboBox.FormattingEnabled = true;
            this.spawn1ComboBox.Location = new System.Drawing.Point(160, 288);
            this.spawn1ComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.spawn1ComboBox.Name = "spawn1ComboBox";
            this.spawn1ComboBox.Size = new System.Drawing.Size(79, 21);
            this.spawn1ComboBox.TabIndex = 34;
            this.spawn1ComboBox.SelectedIndexChanged += new System.EventHandler(this.Spawn1ComboBox_SelectedIndexChanged);
            // 
            // spawn2ComboBox
            // 
            this.spawn2ComboBox.FormattingEnabled = true;
            this.spawn2ComboBox.Location = new System.Drawing.Point(274, 288);
            this.spawn2ComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.spawn2ComboBox.Name = "spawn2ComboBox";
            this.spawn2ComboBox.Size = new System.Drawing.Size(79, 21);
            this.spawn2ComboBox.TabIndex = 36;
            this.spawn2ComboBox.SelectedIndexChanged += new System.EventHandler(this.Spawn2ComboBox_SelectedIndexChanged);
            // 
            // spawn2Button
            // 
            this.spawn2Button.Location = new System.Drawing.Point(359, 288);
            this.spawn2Button.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.spawn2Button.Name = "spawn2Button";
            this.spawn2Button.Size = new System.Drawing.Size(20, 19);
            this.spawn2Button.TabIndex = 35;
            this.spawn2Button.Text = "+";
            this.spawn2Button.UseVisualStyleBackColor = true;
            this.spawn2Button.Click += new System.EventHandler(this.spawn2Button_Click);
            // 
            // spawn3ComboBox
            // 
            this.spawn3ComboBox.FormattingEnabled = true;
            this.spawn3ComboBox.Location = new System.Drawing.Point(384, 287);
            this.spawn3ComboBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.spawn3ComboBox.Name = "spawn3ComboBox";
            this.spawn3ComboBox.Size = new System.Drawing.Size(79, 21);
            this.spawn3ComboBox.TabIndex = 38;
            this.spawn3ComboBox.SelectedIndexChanged += new System.EventHandler(this.Spawn3ComboBox_SelectedIndexChanged);
            // 
            // spawn3Button
            // 
            this.spawn3Button.Location = new System.Drawing.Point(466, 287);
            this.spawn3Button.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.spawn3Button.Name = "spawn3Button";
            this.spawn3Button.Size = new System.Drawing.Size(20, 19);
            this.spawn3Button.TabIndex = 37;
            this.spawn3Button.Text = "+";
            this.spawn3Button.UseVisualStyleBackColor = true;
            this.spawn3Button.Click += new System.EventHandler(this.spawn3Button_Click);
            // 
            // team1Unit2TextBox
            // 
            this.team1Unit2TextBox.Location = new System.Drawing.Point(160, 106);
            this.team1Unit2TextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team1Unit2TextBox.Name = "team1Unit2TextBox";
            this.team1Unit2TextBox.Size = new System.Drawing.Size(79, 20);
            this.team1Unit2TextBox.TabIndex = 20;
            // 
            // toSpawnLabel
            // 
            this.toSpawnLabel.AutoSize = true;
            this.toSpawnLabel.Location = new System.Drawing.Point(67, 293);
            this.toSpawnLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.toSpawnLabel.Name = "toSpawnLabel";
            this.toSpawnLabel.Size = new System.Drawing.Size(56, 13);
            this.toSpawnLabel.TabIndex = 60;
            this.toSpawnLabel.Text = "To Spawn";
            // 
            // tabControl1
            // 
            this.tabControl1.Controls.Add(this.unitMenuTabPage);
            this.tabControl1.Controls.Add(this.spawnMenuTabPage);
            this.tabControl1.Location = new System.Drawing.Point(9, 10);
            this.tabControl1.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.tabControl1.Name = "tabControl1";
            this.tabControl1.SelectedIndex = 0;
            this.tabControl1.Size = new System.Drawing.Size(541, 432);
            this.tabControl1.TabIndex = 63;
            // 
            // unitMenuTabPage
            // 
            this.unitMenuTabPage.Controls.Add(this.btnScriptDialog);
            this.unitMenuTabPage.Controls.Add(this.saveUnitTypeButton);
            this.unitMenuTabPage.Controls.Add(this.movementSpeedTextBox);
            this.unitMenuTabPage.Controls.Add(this.movementSpeedLabel);
            this.unitMenuTabPage.Controls.Add(this.healthTextBox);
            this.unitMenuTabPage.Controls.Add(this.healthLabel);
            this.unitMenuTabPage.Controls.Add(this.criticalChanceTextBox);
            this.unitMenuTabPage.Controls.Add(this.criticalChanceLabel);
            this.unitMenuTabPage.Controls.Add(this.criticalDamageTextBox);
            this.unitMenuTabPage.Controls.Add(this.criticalDamageLabel);
            this.unitMenuTabPage.Controls.Add(this.attackTimerTextBox);
            this.unitMenuTabPage.Controls.Add(this.attackTimerLabel);
            this.unitMenuTabPage.Controls.Add(this.maxRangeTextBox);
            this.unitMenuTabPage.Controls.Add(this.maxAttackRangeLabel);
            this.unitMenuTabPage.Controls.Add(this.armorTextBox);
            this.unitMenuTabPage.Controls.Add(this.armorLabel);
            this.unitMenuTabPage.Controls.Add(this.attackDamageTextBox);
            this.unitMenuTabPage.Controls.Add(this.attackDamageLabel);
            this.unitMenuTabPage.Controls.Add(this.unitTypeLabel);
            this.unitMenuTabPage.Controls.Add(this.unitTypeComboBox);
            this.unitMenuTabPage.Controls.Add(this.minRangeTextBox);
            this.unitMenuTabPage.Controls.Add(this.minAttackRangeLabel);
            this.unitMenuTabPage.Location = new System.Drawing.Point(4, 22);
            this.unitMenuTabPage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.unitMenuTabPage.Name = "unitMenuTabPage";
            this.unitMenuTabPage.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.unitMenuTabPage.Size = new System.Drawing.Size(533, 406);
            this.unitMenuTabPage.TabIndex = 0;
            this.unitMenuTabPage.Text = "Unit Menu";
            this.unitMenuTabPage.UseVisualStyleBackColor = true;
            // 
            // saveUnitTypeButton
            // 
            this.saveUnitTypeButton.Location = new System.Drawing.Point(172, 351);
            this.saveUnitTypeButton.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.saveUnitTypeButton.Name = "saveUnitTypeButton";
            this.saveUnitTypeButton.Size = new System.Drawing.Size(145, 19);
            this.saveUnitTypeButton.TabIndex = 21;
            this.saveUnitTypeButton.Text = "Save Unit Type";
            this.saveUnitTypeButton.UseVisualStyleBackColor = true;
            this.saveUnitTypeButton.Click += new System.EventHandler(this.saveButton_Click);
            // 
            // movementSpeedTextBox
            // 
            this.movementSpeedTextBox.Location = new System.Drawing.Point(147, 293);
            this.movementSpeedTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.movementSpeedTextBox.Name = "movementSpeedTextBox";
            this.movementSpeedTextBox.Size = new System.Drawing.Size(64, 20);
            this.movementSpeedTextBox.TabIndex = 19;
            // 
            // movementSpeedLabel
            // 
            this.movementSpeedLabel.AutoSize = true;
            this.movementSpeedLabel.Location = new System.Drawing.Point(45, 296);
            this.movementSpeedLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.movementSpeedLabel.Name = "movementSpeedLabel";
            this.movementSpeedLabel.Size = new System.Drawing.Size(91, 13);
            this.movementSpeedLabel.TabIndex = 20;
            this.movementSpeedLabel.Text = "Movement Speed";
            // 
            // healthTextBox
            // 
            this.healthTextBox.Location = new System.Drawing.Point(371, 237);
            this.healthTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.healthTextBox.Name = "healthTextBox";
            this.healthTextBox.Size = new System.Drawing.Size(64, 20);
            this.healthTextBox.TabIndex = 17;
            // 
            // healthLabel
            // 
            this.healthLabel.AutoSize = true;
            this.healthLabel.Location = new System.Drawing.Point(281, 240);
            this.healthLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.healthLabel.Name = "healthLabel";
            this.healthLabel.Size = new System.Drawing.Size(38, 13);
            this.healthLabel.TabIndex = 18;
            this.healthLabel.Text = "Health";
            // 
            // criticalChanceTextBox
            // 
            this.criticalChanceTextBox.Location = new System.Drawing.Point(147, 237);
            this.criticalChanceTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.criticalChanceTextBox.Name = "criticalChanceTextBox";
            this.criticalChanceTextBox.Size = new System.Drawing.Size(64, 20);
            this.criticalChanceTextBox.TabIndex = 15;
            // 
            // criticalChanceLabel
            // 
            this.criticalChanceLabel.AutoSize = true;
            this.criticalChanceLabel.Location = new System.Drawing.Point(45, 240);
            this.criticalChanceLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.criticalChanceLabel.Name = "criticalChanceLabel";
            this.criticalChanceLabel.Size = new System.Drawing.Size(78, 13);
            this.criticalChanceLabel.TabIndex = 16;
            this.criticalChanceLabel.Text = "Critical Chance";
            // 
            // criticalDamageTextBox
            // 
            this.criticalDamageTextBox.Location = new System.Drawing.Point(371, 184);
            this.criticalDamageTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.criticalDamageTextBox.Name = "criticalDamageTextBox";
            this.criticalDamageTextBox.Size = new System.Drawing.Size(64, 20);
            this.criticalDamageTextBox.TabIndex = 13;
            // 
            // criticalDamageLabel
            // 
            this.criticalDamageLabel.AutoSize = true;
            this.criticalDamageLabel.Location = new System.Drawing.Point(281, 186);
            this.criticalDamageLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.criticalDamageLabel.Name = "criticalDamageLabel";
            this.criticalDamageLabel.Size = new System.Drawing.Size(81, 13);
            this.criticalDamageLabel.TabIndex = 14;
            this.criticalDamageLabel.Text = "Critical Damage";
            // 
            // attackTimerTextBox
            // 
            this.attackTimerTextBox.Location = new System.Drawing.Point(371, 133);
            this.attackTimerTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.attackTimerTextBox.Name = "attackTimerTextBox";
            this.attackTimerTextBox.Size = new System.Drawing.Size(64, 20);
            this.attackTimerTextBox.TabIndex = 11;
            // 
            // attackTimerLabel
            // 
            this.attackTimerLabel.AutoSize = true;
            this.attackTimerLabel.Location = new System.Drawing.Point(281, 136);
            this.attackTimerLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.attackTimerLabel.Name = "attackTimerLabel";
            this.attackTimerLabel.Size = new System.Drawing.Size(64, 13);
            this.attackTimerLabel.TabIndex = 12;
            this.attackTimerLabel.Text = "AttackTimer";
            // 
            // maxRangeTextBox
            // 
            this.maxRangeTextBox.Location = new System.Drawing.Point(371, 87);
            this.maxRangeTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.maxRangeTextBox.Name = "maxRangeTextBox";
            this.maxRangeTextBox.Size = new System.Drawing.Size(64, 20);
            this.maxRangeTextBox.TabIndex = 9;
            // 
            // maxAttackRangeLabel
            // 
            this.maxAttackRangeLabel.AutoSize = true;
            this.maxAttackRangeLabel.Location = new System.Drawing.Point(281, 89);
            this.maxAttackRangeLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.maxAttackRangeLabel.Name = "maxAttackRangeLabel";
            this.maxAttackRangeLabel.Size = new System.Drawing.Size(89, 13);
            this.maxAttackRangeLabel.TabIndex = 10;
            this.maxAttackRangeLabel.Text = "maxAttackRange";
            // 
            // armorTextBox
            // 
            this.armorTextBox.Location = new System.Drawing.Point(147, 184);
            this.armorTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.armorTextBox.Name = "armorTextBox";
            this.armorTextBox.Size = new System.Drawing.Size(64, 20);
            this.armorTextBox.TabIndex = 7;
            // 
            // armorLabel
            // 
            this.armorLabel.AutoSize = true;
            this.armorLabel.Location = new System.Drawing.Point(45, 186);
            this.armorLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.armorLabel.Name = "armorLabel";
            this.armorLabel.Size = new System.Drawing.Size(34, 13);
            this.armorLabel.TabIndex = 8;
            this.armorLabel.Text = "Armor";
            // 
            // attackDamageTextBox
            // 
            this.attackDamageTextBox.Location = new System.Drawing.Point(147, 133);
            this.attackDamageTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.attackDamageTextBox.Name = "attackDamageTextBox";
            this.attackDamageTextBox.Size = new System.Drawing.Size(64, 20);
            this.attackDamageTextBox.TabIndex = 5;
            // 
            // attackDamageLabel
            // 
            this.attackDamageLabel.AutoSize = true;
            this.attackDamageLabel.Location = new System.Drawing.Point(45, 136);
            this.attackDamageLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.attackDamageLabel.Name = "attackDamageLabel";
            this.attackDamageLabel.Size = new System.Drawing.Size(78, 13);
            this.attackDamageLabel.TabIndex = 6;
            this.attackDamageLabel.Text = "AttackDamage";
            // 
            // spawnMenuTabPage
            // 
            this.spawnMenuTabPage.Controls.Add(this.team3WaypointTextBox);
            this.spawnMenuTabPage.Controls.Add(this.team2WaypointTextBox);
            this.spawnMenuTabPage.Controls.Add(this.waypointLabel);
            this.spawnMenuTabPage.Controls.Add(this.team1WaypointTextBox);
            this.spawnMenuTabPage.Controls.Add(this.team3SpawnPositionTextBox);
            this.spawnMenuTabPage.Controls.Add(this.team2SpawnPositionTextBox);
            this.spawnMenuTabPage.Controls.Add(this.spawnPositionLabel);
            this.spawnMenuTabPage.Controls.Add(this.team1SpawnPositionTextBox);
            this.spawnMenuTabPage.Controls.Add(this.team3ColorTextBox);
            this.spawnMenuTabPage.Controls.Add(this.team2ColorTextBox);
            this.spawnMenuTabPage.Controls.Add(this.teamColorLabel);
            this.spawnMenuTabPage.Controls.Add(this.team1ColorTextBox);
            this.spawnMenuTabPage.Controls.Add(this.spawnButton);
            this.spawnMenuTabPage.Controls.Add(this.team1Unit1TextBox);
            this.spawnMenuTabPage.Controls.Add(this.spawn1Label);
            this.spawnMenuTabPage.Controls.Add(this.spawn2Label);
            this.spawnMenuTabPage.Controls.Add(this.spawn3Label);
            this.spawnMenuTabPage.Controls.Add(this.toSpawnLabel);
            this.spawnMenuTabPage.Controls.Add(this.unit1Label);
            this.spawnMenuTabPage.Controls.Add(this.unit2Label);
            this.spawnMenuTabPage.Controls.Add(this.unit3Label);
            this.spawnMenuTabPage.Controls.Add(this.team1Unit2TextBox);
            this.spawnMenuTabPage.Controls.Add(this.team1Unit3TextBox);
            this.spawnMenuTabPage.Controls.Add(this.team2Unit1TextBox);
            this.spawnMenuTabPage.Controls.Add(this.team2Unit2TextBox);
            this.spawnMenuTabPage.Controls.Add(this.team2Unit3TextBox);
            this.spawnMenuTabPage.Controls.Add(this.team3Unit1TextBox);
            this.spawnMenuTabPage.Controls.Add(this.spawn3ComboBox);
            this.spawnMenuTabPage.Controls.Add(this.team3Unit2TextBox);
            this.spawnMenuTabPage.Controls.Add(this.spawn3Button);
            this.spawnMenuTabPage.Controls.Add(this.team3Unit3TextBox);
            this.spawnMenuTabPage.Controls.Add(this.spawn2ComboBox);
            this.spawnMenuTabPage.Controls.Add(this.addUnitLabel);
            this.spawnMenuTabPage.Controls.Add(this.spawn2Button);
            this.spawnMenuTabPage.Controls.Add(this.spawn1Button);
            this.spawnMenuTabPage.Controls.Add(this.spawn1ComboBox);
            this.spawnMenuTabPage.Location = new System.Drawing.Point(4, 22);
            this.spawnMenuTabPage.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.spawnMenuTabPage.Name = "spawnMenuTabPage";
            this.spawnMenuTabPage.Padding = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.spawnMenuTabPage.Size = new System.Drawing.Size(533, 406);
            this.spawnMenuTabPage.TabIndex = 1;
            this.spawnMenuTabPage.Text = "Spawn Menu";
            this.spawnMenuTabPage.UseVisualStyleBackColor = true;
            // 
            // team3WaypointTextBox
            // 
            this.team3WaypointTextBox.Location = new System.Drawing.Point(384, 247);
            this.team3WaypointTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team3WaypointTextBox.Name = "team3WaypointTextBox";
            this.team3WaypointTextBox.Size = new System.Drawing.Size(79, 20);
            this.team3WaypointTextBox.TabIndex = 72;
            // 
            // team2WaypointTextBox
            // 
            this.team2WaypointTextBox.Location = new System.Drawing.Point(274, 247);
            this.team2WaypointTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team2WaypointTextBox.Name = "team2WaypointTextBox";
            this.team2WaypointTextBox.Size = new System.Drawing.Size(79, 20);
            this.team2WaypointTextBox.TabIndex = 71;
            // 
            // waypointLabel
            // 
            this.waypointLabel.AutoSize = true;
            this.waypointLabel.Location = new System.Drawing.Point(67, 246);
            this.waypointLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.waypointLabel.Name = "waypointLabel";
            this.waypointLabel.Size = new System.Drawing.Size(52, 13);
            this.waypointLabel.TabIndex = 69;
            this.waypointLabel.Text = "Waypoint";
            // 
            // team1WaypointTextBox
            // 
            this.team1WaypointTextBox.Location = new System.Drawing.Point(160, 247);
            this.team1WaypointTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team1WaypointTextBox.Name = "team1WaypointTextBox";
            this.team1WaypointTextBox.Size = new System.Drawing.Size(79, 20);
            this.team1WaypointTextBox.TabIndex = 70;
            // 
            // team3SpawnPositionTextBox
            // 
            this.team3SpawnPositionTextBox.Location = new System.Drawing.Point(384, 213);
            this.team3SpawnPositionTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team3SpawnPositionTextBox.Name = "team3SpawnPositionTextBox";
            this.team3SpawnPositionTextBox.Size = new System.Drawing.Size(79, 20);
            this.team3SpawnPositionTextBox.TabIndex = 68;
            // 
            // team2SpawnPositionTextBox
            // 
            this.team2SpawnPositionTextBox.Location = new System.Drawing.Point(274, 213);
            this.team2SpawnPositionTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team2SpawnPositionTextBox.Name = "team2SpawnPositionTextBox";
            this.team2SpawnPositionTextBox.Size = new System.Drawing.Size(79, 20);
            this.team2SpawnPositionTextBox.TabIndex = 67;
            // 
            // spawnPositionLabel
            // 
            this.spawnPositionLabel.AutoSize = true;
            this.spawnPositionLabel.Location = new System.Drawing.Point(67, 212);
            this.spawnPositionLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.spawnPositionLabel.Name = "spawnPositionLabel";
            this.spawnPositionLabel.Size = new System.Drawing.Size(80, 13);
            this.spawnPositionLabel.TabIndex = 65;
            this.spawnPositionLabel.Text = "Spawn Position";
            // 
            // team1SpawnPositionTextBox
            // 
            this.team1SpawnPositionTextBox.Location = new System.Drawing.Point(160, 213);
            this.team1SpawnPositionTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team1SpawnPositionTextBox.Name = "team1SpawnPositionTextBox";
            this.team1SpawnPositionTextBox.Size = new System.Drawing.Size(79, 20);
            this.team1SpawnPositionTextBox.TabIndex = 66;
            // 
            // team3ColorTextBox
            // 
            this.team3ColorTextBox.Location = new System.Drawing.Point(384, 178);
            this.team3ColorTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team3ColorTextBox.Name = "team3ColorTextBox";
            this.team3ColorTextBox.Size = new System.Drawing.Size(79, 20);
            this.team3ColorTextBox.TabIndex = 64;
            // 
            // team2ColorTextBox
            // 
            this.team2ColorTextBox.Location = new System.Drawing.Point(274, 178);
            this.team2ColorTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team2ColorTextBox.Name = "team2ColorTextBox";
            this.team2ColorTextBox.Size = new System.Drawing.Size(79, 20);
            this.team2ColorTextBox.TabIndex = 63;
            // 
            // teamColorLabel
            // 
            this.teamColorLabel.AutoSize = true;
            this.teamColorLabel.Location = new System.Drawing.Point(67, 177);
            this.teamColorLabel.Margin = new System.Windows.Forms.Padding(2, 0, 2, 0);
            this.teamColorLabel.Name = "teamColorLabel";
            this.teamColorLabel.Size = new System.Drawing.Size(61, 13);
            this.teamColorLabel.TabIndex = 61;
            this.teamColorLabel.Text = "Team Color";
            // 
            // team1ColorTextBox
            // 
            this.team1ColorTextBox.Location = new System.Drawing.Point(160, 178);
            this.team1ColorTextBox.Margin = new System.Windows.Forms.Padding(2, 2, 2, 2);
            this.team1ColorTextBox.Name = "team1ColorTextBox";
            this.team1ColorTextBox.Size = new System.Drawing.Size(79, 20);
            this.team1ColorTextBox.TabIndex = 62;
            // 
            // btnScriptDialog
            // 
            this.btnScriptDialog.Location = new System.Drawing.Point(424, 369);
            this.btnScriptDialog.Name = "btnScriptDialog";
            this.btnScriptDialog.Size = new System.Drawing.Size(104, 32);
            this.btnScriptDialog.TabIndex = 74;
            this.btnScriptDialog.Text = "Create New Script";
            this.btnScriptDialog.UseVisualStyleBackColor = true;
            this.btnScriptDialog.Click += new System.EventHandler(this.btnScriptDialog_Click);
            // 
            // DataForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(559, 452);
            this.Controls.Add(this.tabControl1);
            this.Name = "DataForm";
            this.Text = " ";
            this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.DataForm_FormClosing);
            this.Load += new System.EventHandler(this.DataForm_Load);
            this.tabControl1.ResumeLayout(false);
            this.unitMenuTabPage.ResumeLayout(false);
            this.unitMenuTabPage.PerformLayout();
            this.spawnMenuTabPage.ResumeLayout(false);
            this.spawnMenuTabPage.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Label unitTypeLabel;
        private System.Windows.Forms.ComboBox unitTypeComboBox;
        private System.Windows.Forms.TextBox minRangeTextBox;
        private System.Windows.Forms.Label minAttackRangeLabel;
        private System.Windows.Forms.Label spawn1Label;
        private System.Windows.Forms.Label spawn2Label;
        private System.Windows.Forms.Label spawn3Label;
        private System.Windows.Forms.Label unit1Label;
        private System.Windows.Forms.Label unit2Label;
        private System.Windows.Forms.Label unit3Label;
        private System.Windows.Forms.TextBox team1Unit1TextBox;
        private System.Windows.Forms.TextBox team1Unit3TextBox;
        private System.Windows.Forms.TextBox team2Unit3TextBox;
        private System.Windows.Forms.TextBox team2Unit2TextBox;
        private System.Windows.Forms.TextBox team2Unit1TextBox;
        private System.Windows.Forms.TextBox team3Unit3TextBox;
        private System.Windows.Forms.TextBox team3Unit2TextBox;
        private System.Windows.Forms.TextBox team3Unit1TextBox;
        private System.Windows.Forms.Button spawnButton;
        private System.Windows.Forms.Label addUnitLabel;
        private System.Windows.Forms.Button spawn1Button;
        private System.Windows.Forms.ComboBox spawn1ComboBox;
        private System.Windows.Forms.ComboBox spawn2ComboBox;
        private System.Windows.Forms.Button spawn2Button;
        private System.Windows.Forms.ComboBox spawn3ComboBox;
        private System.Windows.Forms.Button spawn3Button;
        private System.Windows.Forms.TextBox team1Unit2TextBox;
        private System.Windows.Forms.Label toSpawnLabel;
        private System.Windows.Forms.HelpProvider helpProvider1;
        private System.Windows.Forms.TabControl tabControl1;
        private System.Windows.Forms.TabPage unitMenuTabPage;
        private System.Windows.Forms.TabPage spawnMenuTabPage;
        private System.Windows.Forms.TextBox team3WaypointTextBox;
        private System.Windows.Forms.TextBox team2WaypointTextBox;
        private System.Windows.Forms.Label waypointLabel;
        private System.Windows.Forms.TextBox team1WaypointTextBox;
        private System.Windows.Forms.TextBox team3SpawnPositionTextBox;
        private System.Windows.Forms.TextBox team2SpawnPositionTextBox;
        private System.Windows.Forms.Label spawnPositionLabel;
        private System.Windows.Forms.TextBox team1SpawnPositionTextBox;
        private System.Windows.Forms.TextBox team3ColorTextBox;
        private System.Windows.Forms.TextBox team2ColorTextBox;
        private System.Windows.Forms.Label teamColorLabel;
        private System.Windows.Forms.TextBox team1ColorTextBox;
        private System.Windows.Forms.TextBox armorTextBox;
        private System.Windows.Forms.Label armorLabel;
        private System.Windows.Forms.TextBox attackDamageTextBox;
        private System.Windows.Forms.Label attackDamageLabel;
        private System.Windows.Forms.TextBox criticalChanceTextBox;
        private System.Windows.Forms.Label criticalChanceLabel;
        private System.Windows.Forms.TextBox criticalDamageTextBox;
        private System.Windows.Forms.Label criticalDamageLabel;
        private System.Windows.Forms.TextBox attackTimerTextBox;
        private System.Windows.Forms.Label attackTimerLabel;
        private System.Windows.Forms.TextBox maxRangeTextBox;
        private System.Windows.Forms.Label maxAttackRangeLabel;
        private System.Windows.Forms.TextBox movementSpeedTextBox;
        private System.Windows.Forms.Label movementSpeedLabel;
        private System.Windows.Forms.TextBox healthTextBox;
        private System.Windows.Forms.Label healthLabel;
        private System.Windows.Forms.Button saveUnitTypeButton;
        private System.Windows.Forms.Button btnScriptDialog;
    }
}