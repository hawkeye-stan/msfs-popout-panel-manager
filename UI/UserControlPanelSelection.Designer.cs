
namespace MSFSPopoutPanelManager
{
    partial class UserControlPanelSelection
    {
        /// <summary> 
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary> 
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.panel1 = new System.Windows.Forms.Panel();
            this.buttonSetDefault = new System.Windows.Forms.Button();
            this.label2 = new System.Windows.Forms.Label();
            this.comboBoxProfile = new System.Windows.Forms.ComboBox();
            this.panel2 = new System.Windows.Forms.Panel();
            this.label1 = new System.Windows.Forms.Label();
            this.label5 = new System.Windows.Forms.Label();
            this.label4 = new System.Windows.Forms.Label();
            this.buttonPanelSelection = new System.Windows.Forms.Button();
            this.label3 = new System.Windows.Forms.Label();
            this.label6 = new System.Windows.Forms.Label();
            this.textBoxPanelLocations = new System.Windows.Forms.TextBox();
            this.panel4 = new System.Windows.Forms.Panel();
            this.checkBoxShowPanelLocation = new System.Windows.Forms.CheckBox();
            this.buttonAnalyze = new System.Windows.Forms.Button();
            this.panel3 = new System.Windows.Forms.Panel();
            this.label7 = new System.Windows.Forms.Label();
            this.panel1.SuspendLayout();
            this.panel2.SuspendLayout();
            this.panel4.SuspendLayout();
            this.panel3.SuspendLayout();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.buttonSetDefault);
            this.panel1.Controls.Add(this.label2);
            this.panel1.Controls.Add(this.comboBoxProfile);
            this.panel1.ForeColor = System.Drawing.Color.White;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(516, 79);
            this.panel1.TabIndex = 0;
            // 
            // buttonSetDefault
            // 
            this.buttonSetDefault.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonSetDefault.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonSetDefault.ForeColor = System.Drawing.Color.White;
            this.buttonSetDefault.Location = new System.Drawing.Point(406, 37);
            this.buttonSetDefault.Name = "buttonSetDefault";
            this.buttonSetDefault.Size = new System.Drawing.Size(107, 35);
            this.buttonSetDefault.TabIndex = 19;
            this.buttonSetDefault.Text = "Set Default";
            this.buttonSetDefault.UseVisualStyleBackColor = false;
            this.buttonSetDefault.Click += new System.EventHandler(this.buttonSetDefault_Click);
            // 
            // label2
            // 
            this.label2.AutoSize = true;
            this.label2.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label2.ForeColor = System.Drawing.Color.White;
            this.label2.Location = new System.Drawing.Point(20, 10);
            this.label2.Name = "label2";
            this.label2.Size = new System.Drawing.Size(315, 20);
            this.label2.TabIndex = 7;
            this.label2.Text = "1. Please select a profile you would like to use.";
            // 
            // comboBoxProfile
            // 
            this.comboBoxProfile.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.comboBoxProfile.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.comboBoxProfile.ForeColor = System.Drawing.Color.Black;
            this.comboBoxProfile.FormattingEnabled = true;
            this.comboBoxProfile.Location = new System.Drawing.Point(35, 41);
            this.comboBoxProfile.Name = "comboBoxProfile";
            this.comboBoxProfile.Size = new System.Drawing.Size(365, 28);
            this.comboBoxProfile.TabIndex = 5;
            this.comboBoxProfile.SelectedIndexChanged += new System.EventHandler(this.comboBoxProfile_SelectedIndexChanged);
            // 
            // panel2
            // 
            this.panel2.Controls.Add(this.label1);
            this.panel2.Controls.Add(this.label5);
            this.panel2.Controls.Add(this.label4);
            this.panel2.Controls.Add(this.buttonPanelSelection);
            this.panel2.Controls.Add(this.label3);
            this.panel2.Location = new System.Drawing.Point(0, 80);
            this.panel2.Name = "panel2";
            this.panel2.Size = new System.Drawing.Size(516, 227);
            this.panel2.TabIndex = 8;
            // 
            // label1
            // 
            this.label1.AutoSize = true;
            this.label1.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label1.ForeColor = System.Drawing.Color.White;
            this.label1.Location = new System.Drawing.Point(35, 86);
            this.label1.Name = "label1";
            this.label1.Size = new System.Drawing.Size(213, 20);
            this.label1.TabIndex = 12;
            this.label1.Text = "LEFT CLICK to add a new panel";
            // 
            // label5
            // 
            this.label5.AutoSize = true;
            this.label5.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label5.ForeColor = System.Drawing.Color.White;
            this.label5.Location = new System.Drawing.Point(35, 146);
            this.label5.Name = "label5";
            this.label5.Size = new System.Drawing.Size(372, 20);
            this.label5.TabIndex = 11;
            this.label5.Text = "CTRL + LEFT CLICK when all panels have been selected.";
            // 
            // label4
            // 
            this.label4.AutoSize = true;
            this.label4.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label4.ForeColor = System.Drawing.Color.White;
            this.label4.Location = new System.Drawing.Point(35, 115);
            this.label4.Name = "label4";
            this.label4.Size = new System.Drawing.Size(418, 20);
            this.label4.TabIndex = 10;
            this.label4.Text = "SHIFT + LEFT CLICK to remove the most recently added panel.";
            // 
            // buttonPanelSelection
            // 
            this.buttonPanelSelection.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonPanelSelection.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonPanelSelection.ForeColor = System.Drawing.Color.White;
            this.buttonPanelSelection.Location = new System.Drawing.Point(35, 177);
            this.buttonPanelSelection.Name = "buttonPanelSelection";
            this.buttonPanelSelection.Size = new System.Drawing.Size(170, 35);
            this.buttonPanelSelection.TabIndex = 9;
            this.buttonPanelSelection.Text = "Start Panel Selection";
            this.buttonPanelSelection.UseVisualStyleBackColor = false;
            this.buttonPanelSelection.Click += new System.EventHandler(this.buttonPanelSelection_Click);
            // 
            // label3
            // 
            this.label3.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label3.ForeColor = System.Drawing.Color.White;
            this.label3.Location = new System.Drawing.Point(20, 10);
            this.label3.Name = "label3";
            this.label3.Size = new System.Drawing.Size(488, 63);
            this.label3.TabIndex = 7;
            this.label3.Text = "2. Identify the pop out panels in the game by clicking on them. Their locations w" +
    "ill be saved and for use on future flights. (You only need to do this once per p" +
    "rofile)";
            // 
            // label6
            // 
            this.label6.AutoSize = true;
            this.label6.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label6.ForeColor = System.Drawing.Color.White;
            this.label6.Location = new System.Drawing.Point(103, 11);
            this.label6.Name = "label6";
            this.label6.Size = new System.Drawing.Size(111, 20);
            this.label6.TabIndex = 11;
            this.label6.Text = "Panel Locations";
            // 
            // textBoxPanelLocations
            // 
            this.textBoxPanelLocations.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.textBoxPanelLocations.ForeColor = System.Drawing.SystemColors.WindowText;
            this.textBoxPanelLocations.Location = new System.Drawing.Point(21, 41);
            this.textBoxPanelLocations.Multiline = true;
            this.textBoxPanelLocations.Name = "textBoxPanelLocations";
            this.textBoxPanelLocations.ReadOnly = true;
            this.textBoxPanelLocations.Size = new System.Drawing.Size(301, 277);
            this.textBoxPanelLocations.TabIndex = 12;
            // 
            // panel4
            // 
            this.panel4.Controls.Add(this.checkBoxShowPanelLocation);
            this.panel4.Controls.Add(this.textBoxPanelLocations);
            this.panel4.Controls.Add(this.label6);
            this.panel4.Location = new System.Drawing.Point(522, 0);
            this.panel4.Name = "panel4";
            this.panel4.Size = new System.Drawing.Size(335, 403);
            this.panel4.TabIndex = 13;
            // 
            // checkBoxShowPanelLocation
            // 
            this.checkBoxShowPanelLocation.AutoSize = true;
            this.checkBoxShowPanelLocation.Checked = true;
            this.checkBoxShowPanelLocation.CheckState = System.Windows.Forms.CheckState.Checked;
            this.checkBoxShowPanelLocation.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.checkBoxShowPanelLocation.ForeColor = System.Drawing.Color.White;
            this.checkBoxShowPanelLocation.Location = new System.Drawing.Point(61, 337);
            this.checkBoxShowPanelLocation.Name = "checkBoxShowPanelLocation";
            this.checkBoxShowPanelLocation.Size = new System.Drawing.Size(213, 24);
            this.checkBoxShowPanelLocation.TabIndex = 17;
            this.checkBoxShowPanelLocation.Text = "Show Panel Location Ovelay";
            this.checkBoxShowPanelLocation.UseVisualStyleBackColor = true;
            this.checkBoxShowPanelLocation.CheckedChanged += new System.EventHandler(this.checkBoxShowPanelLocation_CheckedChanged);
            // 
            // buttonAnalyze
            // 
            this.buttonAnalyze.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(17)))), ((int)(((byte)(158)))), ((int)(((byte)(218)))));
            this.buttonAnalyze.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.buttonAnalyze.ForeColor = System.Drawing.Color.White;
            this.buttonAnalyze.Location = new System.Drawing.Point(32, 41);
            this.buttonAnalyze.Name = "buttonAnalyze";
            this.buttonAnalyze.Size = new System.Drawing.Size(107, 35);
            this.buttonAnalyze.TabIndex = 18;
            this.buttonAnalyze.Text = "Analyze";
            this.buttonAnalyze.UseVisualStyleBackColor = false;
            this.buttonAnalyze.Click += new System.EventHandler(this.buttonAnalyze_Click);
            // 
            // panel3
            // 
            this.panel3.Controls.Add(this.buttonAnalyze);
            this.panel3.Controls.Add(this.label7);
            this.panel3.Enabled = false;
            this.panel3.ForeColor = System.Drawing.Color.White;
            this.panel3.Location = new System.Drawing.Point(3, 309);
            this.panel3.Name = "panel3";
            this.panel3.Size = new System.Drawing.Size(513, 94);
            this.panel3.TabIndex = 14;
            // 
            // label7
            // 
            this.label7.AutoSize = true;
            this.label7.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.label7.ForeColor = System.Drawing.Color.White;
            this.label7.Location = new System.Drawing.Point(20, 10);
            this.label7.Name = "label7";
            this.label7.Size = new System.Drawing.Size(292, 20);
            this.label7.TabIndex = 7;
            this.label7.Text = "3. Pop out and analyze the selected panels.";
            // 
            // UserControlPanelSelection
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.Transparent;
            this.Controls.Add(this.panel3);
            this.Controls.Add(this.panel4);
            this.Controls.Add(this.panel2);
            this.Controls.Add(this.panel1);
            this.Name = "UserControlPanelSelection";
            this.Size = new System.Drawing.Size(860, 405);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.panel2.ResumeLayout(false);
            this.panel2.PerformLayout();
            this.panel4.ResumeLayout(false);
            this.panel4.PerformLayout();
            this.panel3.ResumeLayout(false);
            this.panel3.PerformLayout();
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.Label label2;
        private System.Windows.Forms.ComboBox comboBoxProfile;
        private System.Windows.Forms.Panel panel2;
        private System.Windows.Forms.Label label3;
        private System.Windows.Forms.Button buttonPanelSelection;
        private System.Windows.Forms.Label label5;
        private System.Windows.Forms.Label label4;
        private System.Windows.Forms.Label label1;
        private System.Windows.Forms.Label label6;
        private System.Windows.Forms.TextBox textBoxPanelLocations;
        private System.Windows.Forms.Panel panel3;
        private System.Windows.Forms.Label label10;
        private System.Windows.Forms.Panel panel4;
        private System.Windows.Forms.CheckBox checkBoxShowPanelLocation;
        private System.Windows.Forms.Button buttonAnalyze;
        private System.Windows.Forms.Label label7;
        private System.Windows.Forms.Button buttonSetDefault;
    }
}
