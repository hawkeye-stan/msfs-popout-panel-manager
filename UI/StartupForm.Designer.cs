
namespace MSFSPopoutPanelManager.UI
{
    partial class StartupForm
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

        #region Windows Form Designer generated code

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.ComponentModel.ComponentResourceManager resources = new System.ComponentModel.ComponentResourceManager(typeof(StartupForm));
            this.panelSteps = new System.Windows.Forms.Panel();
            this.linkLabel1 = new System.Windows.Forms.LinkLabel();
            this.labelMsfsConnection = new System.Windows.Forms.Label();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.txtBoxStatus = new DarkUI.Controls.DarkTextBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.darkLabel1 = new DarkUI.Controls.DarkLabel();
            this.darkLabel2 = new DarkUI.Controls.DarkLabel();
            this.checkBoxMinimizeToTray = new DarkUI.Controls.DarkCheckBox();
            this.lblVersion = new DarkUI.Controls.DarkLabel();
            this.checkBoxAutoStart = new DarkUI.Controls.DarkCheckBox();
            this.checkBoxAutoPanning = new DarkUI.Controls.DarkCheckBox();
            this.checkBoxAlwaysOnTop = new DarkUI.Controls.DarkCheckBox();
            this.panelStatus.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelSteps
            // 
            this.panelSteps.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(61)))), ((int)(((byte)(101)))), ((int)(((byte)(171)))));
            this.panelSteps.Location = new System.Drawing.Point(0, 64);
            this.panelSteps.Name = "panelSteps";
            this.panelSteps.Size = new System.Drawing.Size(915, 405);
            this.panelSteps.TabIndex = 0;
            // 
            // linkLabel1
            // 
            this.linkLabel1.AutoSize = true;
            this.linkLabel1.Font = new System.Drawing.Font("Segoe UI", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.linkLabel1.LinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(192)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.linkLabel1.Location = new System.Drawing.Point(344, 35);
            this.linkLabel1.Name = "linkLabel1";
            this.linkLabel1.Size = new System.Drawing.Size(41, 21);
            this.linkLabel1.TabIndex = 1;
            this.linkLabel1.TabStop = true;
            this.linkLabel1.Text = "here";
            this.linkLabel1.VisitedLinkColor = System.Drawing.Color.FromArgb(((int)(((byte)(128)))), ((int)(((byte)(255)))), ((int)(((byte)(255)))));
            this.linkLabel1.LinkClicked += new System.Windows.Forms.LinkLabelLinkClickedEventHandler(this.linkLabel1_LinkClicked);
            // 
            // labelMsfsConnection
            // 
            this.labelMsfsConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMsfsConnection.AutoSize = true;
            this.labelMsfsConnection.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelMsfsConnection.ForeColor = System.Drawing.Color.Red;
            this.labelMsfsConnection.Location = new System.Drawing.Point(766, 546);
            this.labelMsfsConnection.Name = "labelMsfsConnection";
            this.labelMsfsConnection.RightToLeft = System.Windows.Forms.RightToLeft.Yes;
            this.labelMsfsConnection.Size = new System.Drawing.Size(33, 20);
            this.labelMsfsConnection.TabIndex = 10;
            this.labelMsfsConnection.Text = "      ";
            this.labelMsfsConnection.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            // 
            // panelStatus
            // 
            this.panelStatus.Controls.Add(this.darkLabel3);
            this.panelStatus.Controls.Add(this.txtBoxStatus);
            this.panelStatus.Location = new System.Drawing.Point(0, 469);
            this.panelStatus.Name = "panelStatus";
            this.panelStatus.Size = new System.Drawing.Size(915, 74);
            this.panelStatus.TabIndex = 20;
            // 
            // darkLabel3
            // 
            this.darkLabel3.AutoSize = true;
            this.darkLabel3.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.darkLabel3.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel3.Location = new System.Drawing.Point(12, 16);
            this.darkLabel3.Name = "darkLabel3";
            this.darkLabel3.Size = new System.Drawing.Size(49, 20);
            this.darkLabel3.TabIndex = 24;
            this.darkLabel3.Text = "Status";
            // 
            // txtBoxStatus
            // 
            this.txtBoxStatus.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(73)))), ((int)(((byte)(74)))));
            this.txtBoxStatus.BorderStyle = System.Windows.Forms.BorderStyle.FixedSingle;
            this.txtBoxStatus.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.txtBoxStatus.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.txtBoxStatus.Location = new System.Drawing.Point(67, 16);
            this.txtBoxStatus.Multiline = true;
            this.txtBoxStatus.Name = "txtBoxStatus";
            this.txtBoxStatus.ReadOnly = true;
            this.txtBoxStatus.Size = new System.Drawing.Size(835, 46);
            this.txtBoxStatus.TabIndex = 23;
            // 
            // notifyIcon1
            // 
            this.notifyIcon1.Icon = ((System.Drawing.Icon)(resources.GetObject("notifyIcon1.Icon")));
            this.notifyIcon1.Text = "MSFS 2020 Pop Out Panel Manager";
            this.notifyIcon1.Visible = true;
            this.notifyIcon1.DoubleClick += new System.EventHandler(this.notifyIcon1_DoubleClick);
            // 
            // darkLabel1
            // 
            this.darkLabel1.AutoSize = true;
            this.darkLabel1.Font = new System.Drawing.Font("Segoe UI", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point);
            this.darkLabel1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel1.Location = new System.Drawing.Point(13, 9);
            this.darkLabel1.Name = "darkLabel1";
            this.darkLabel1.Size = new System.Drawing.Size(591, 25);
            this.darkLabel1.TabIndex = 21;
            this.darkLabel1.Text = "Welcome and thank you for using MSFS Pop Out Panel Manager!";
            // 
            // darkLabel2
            // 
            this.darkLabel2.AutoSize = true;
            this.darkLabel2.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.darkLabel2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkLabel2.Location = new System.Drawing.Point(13, 36);
            this.darkLabel2.Name = "darkLabel2";
            this.darkLabel2.Size = new System.Drawing.Size(334, 20);
            this.darkLabel2.TabIndex = 22;
            this.darkLabel2.Text = "Instruction on how to use this utility can be found";
            // 
            // checkBoxMinimizeToTray
            // 
            this.checkBoxMinimizeToTray.AutoSize = true;
            this.checkBoxMinimizeToTray.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.checkBoxMinimizeToTray.Location = new System.Drawing.Point(13, 545);
            this.checkBoxMinimizeToTray.Name = "checkBoxMinimizeToTray";
            this.checkBoxMinimizeToTray.Size = new System.Drawing.Size(189, 24);
            this.checkBoxMinimizeToTray.TabIndex = 23;
            this.checkBoxMinimizeToTray.Text = "Minimize to System Tray";
            // 
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblVersion.Location = new System.Drawing.Point(423, 591);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(48, 15);
            this.lblVersion.TabIndex = 24;
            this.lblVersion.Text = "Version ";
            // 
            // checkBoxAutoStart
            // 
            this.checkBoxAutoStart.AutoSize = true;
            this.checkBoxAutoStart.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.checkBoxAutoStart.Location = new System.Drawing.Point(210, 545);
            this.checkBoxAutoStart.Name = "checkBoxAutoStart";
            this.checkBoxAutoStart.Size = new System.Drawing.Size(95, 24);
            this.checkBoxAutoStart.TabIndex = 25;
            this.checkBoxAutoStart.Text = "Auto Start";
            // 
            // checkBoxAutoPanning
            // 
            this.checkBoxAutoPanning.AutoSize = true;
            this.checkBoxAutoPanning.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.checkBoxAutoPanning.Location = new System.Drawing.Point(441, 545);
            this.checkBoxAutoPanning.Name = "checkBoxAutoPanning";
            this.checkBoxAutoPanning.Size = new System.Drawing.Size(116, 24);
            this.checkBoxAutoPanning.TabIndex = 27;
            this.checkBoxAutoPanning.Text = "Auto Panning";
            // 
            // checkBoxAlwaysOnTop
            // 
            this.checkBoxAlwaysOnTop.AutoSize = true;
            this.checkBoxAlwaysOnTop.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.checkBoxAlwaysOnTop.Location = new System.Drawing.Point(311, 545);
            this.checkBoxAlwaysOnTop.Name = "checkBoxAlwaysOnTop";
            this.checkBoxAlwaysOnTop.Size = new System.Drawing.Size(124, 24);
            this.checkBoxAlwaysOnTop.TabIndex = 28;
            this.checkBoxAlwaysOnTop.Text = "Always on Top";
            // 
            // StartupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 611);
            this.Controls.Add(this.checkBoxAlwaysOnTop);
            this.Controls.Add(this.checkBoxAutoPanning);
            this.Controls.Add(this.checkBoxAutoStart);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.checkBoxMinimizeToTray);
            this.Controls.Add(this.darkLabel2);
            this.Controls.Add(this.darkLabel1);
            this.Controls.Add(this.linkLabel1);
            this.Controls.Add(this.labelMsfsConnection);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.panelSteps);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MaximizeBox = false;
            this.Name = "StartupForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "MSFS Pop Out Panel Manager";
            this.Load += new System.EventHandler(this.StartupForm_Load);
            this.Resize += new System.EventHandler(this.StartupForm_Resize);
            this.panelStatus.ResumeLayout(false);
            this.panelStatus.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelSteps;
        private System.Windows.Forms.Label labelMsfsConnection;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private System.Windows.Forms.LinkLabel linkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel1;
        private DarkUI.Controls.DarkLabel darkLabel2;
        private DarkUI.Controls.DarkTextBox txtBoxStatus;
        private DarkUI.Controls.DarkCheckBox checkBoxMinimizeToTray;
        private DarkUI.Controls.DarkLabel lblVersion;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkCheckBox checkBoxAutoStart;
        private DarkUI.Controls.DarkCheckBox checkBoxAutoPanning;
        private DarkUI.Controls.DarkCheckBox checkBoxAlwaysOnTop;
    }
}