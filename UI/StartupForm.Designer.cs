
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
            this.labelMsfsConnection = new System.Windows.Forms.Label();
            this.panelStatus = new System.Windows.Forms.Panel();
            this.darkLabel3 = new DarkUI.Controls.DarkLabel();
            this.txtBoxStatus = new DarkUI.Controls.DarkTextBox();
            this.notifyIcon1 = new System.Windows.Forms.NotifyIcon(this.components);
            this.lblVersion = new DarkUI.Controls.DarkLabel();
            this.darkMenuStrip1 = new DarkUI.Controls.DarkMenuStrip();
            this.fileToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_restart = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator1 = new System.Windows.Forms.ToolStripSeparator();
            this.preferencesToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_alwaysOnTop = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_autoPanning = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_autoStart = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_minimizeToSystemTray = new System.Windows.Forms.ToolStripMenuItem();
            this.toolStripSeparator2 = new System.Windows.Forms.ToolStripSeparator();
            this.menuItem_exit = new System.Windows.Forms.ToolStripMenuItem();
            this.viewToolStripMenuItem = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_minimizeAllPanels = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_help = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_userGuide = new System.Windows.Forms.ToolStripMenuItem();
            this.menuItem_downloadLatestRelease = new System.Windows.Forms.ToolStripMenuItem();
            this.panelStatus.SuspendLayout();
            this.darkMenuStrip1.SuspendLayout();
            this.SuspendLayout();
            // 
            // panelSteps
            // 
            this.panelSteps.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(61)))), ((int)(((byte)(101)))), ((int)(((byte)(171)))));
            this.panelSteps.Location = new System.Drawing.Point(0, 30);
            this.panelSteps.Name = "panelSteps";
            this.panelSteps.Size = new System.Drawing.Size(915, 405);
            this.panelSteps.TabIndex = 0;
            // 
            // labelMsfsConnection
            // 
            this.labelMsfsConnection.Anchor = ((System.Windows.Forms.AnchorStyles)((((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Bottom) 
            | System.Windows.Forms.AnchorStyles.Left) 
            | System.Windows.Forms.AnchorStyles.Right)));
            this.labelMsfsConnection.AutoSize = true;
            this.labelMsfsConnection.Font = new System.Drawing.Font("Segoe UI", 11F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.labelMsfsConnection.ForeColor = System.Drawing.Color.Red;
            this.labelMsfsConnection.Location = new System.Drawing.Point(763, 511);
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
            this.panelStatus.Location = new System.Drawing.Point(0, 435);
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
            // lblVersion
            // 
            this.lblVersion.AutoSize = true;
            this.lblVersion.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.lblVersion.Location = new System.Drawing.Point(423, 520);
            this.lblVersion.Name = "lblVersion";
            this.lblVersion.Size = new System.Drawing.Size(48, 15);
            this.lblVersion.TabIndex = 24;
            this.lblVersion.Text = "Version ";
            // 
            // darkMenuStrip1
            // 
            this.darkMenuStrip1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.darkMenuStrip1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.darkMenuStrip1.Items.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.fileToolStripMenuItem,
            this.viewToolStripMenuItem,
            this.menuItem_help});
            this.darkMenuStrip1.Location = new System.Drawing.Point(0, 0);
            this.darkMenuStrip1.Name = "darkMenuStrip1";
            this.darkMenuStrip1.Padding = new System.Windows.Forms.Padding(3, 2, 0, 2);
            this.darkMenuStrip1.Size = new System.Drawing.Size(914, 28);
            this.darkMenuStrip1.TabIndex = 29;
            this.darkMenuStrip1.Text = "darkMenuStrip1";
            // 
            // fileToolStripMenuItem
            // 
            this.fileToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.fileToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_restart,
            this.toolStripSeparator1,
            this.preferencesToolStripMenuItem,
            this.toolStripSeparator2,
            this.menuItem_exit});
            this.fileToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.fileToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.fileToolStripMenuItem.Name = "fileToolStripMenuItem";
            this.fileToolStripMenuItem.Size = new System.Drawing.Size(44, 24);
            this.fileToolStripMenuItem.Text = "File";
            // 
            // menuItem_restart
            // 
            this.menuItem_restart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.menuItem_restart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.menuItem_restart.Name = "menuItem_restart";
            this.menuItem_restart.Size = new System.Drawing.Size(154, 24);
            this.menuItem_restart.Text = "Restart";
            // 
            // toolStripSeparator1
            // 
            this.toolStripSeparator1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator1.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator1.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator1.Name = "toolStripSeparator1";
            this.toolStripSeparator1.Size = new System.Drawing.Size(151, 6);
            // 
            // preferencesToolStripMenuItem
            // 
            this.preferencesToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.preferencesToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_alwaysOnTop,
            this.menuItem_autoPanning,
            this.menuItem_autoStart,
            this.menuItem_minimizeToSystemTray});
            this.preferencesToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.preferencesToolStripMenuItem.Name = "preferencesToolStripMenuItem";
            this.preferencesToolStripMenuItem.Size = new System.Drawing.Size(154, 24);
            this.preferencesToolStripMenuItem.Text = "Preferences";
            // 
            // menuItem_alwaysOnTop
            // 
            this.menuItem_alwaysOnTop.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.menuItem_alwaysOnTop.CheckOnClick = true;
            this.menuItem_alwaysOnTop.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.menuItem_alwaysOnTop.ImageScaling = System.Windows.Forms.ToolStripItemImageScaling.None;
            this.menuItem_alwaysOnTop.Name = "menuItem_alwaysOnTop";
            this.menuItem_alwaysOnTop.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.T)));
            this.menuItem_alwaysOnTop.Size = new System.Drawing.Size(256, 24);
            this.menuItem_alwaysOnTop.Text = "Always on Top        ";
            this.menuItem_alwaysOnTop.TextImageRelation = System.Windows.Forms.TextImageRelation.TextBeforeImage;
            // 
            // menuItem_autoPanning
            // 
            this.menuItem_autoPanning.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.menuItem_autoPanning.CheckOnClick = true;
            this.menuItem_autoPanning.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.menuItem_autoPanning.Name = "menuItem_autoPanning";
            this.menuItem_autoPanning.Size = new System.Drawing.Size(256, 24);
            this.menuItem_autoPanning.Text = "Auto Panning";
            // 
            // menuItem_autoStart
            // 
            this.menuItem_autoStart.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.menuItem_autoStart.CheckOnClick = true;
            this.menuItem_autoStart.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.menuItem_autoStart.Name = "menuItem_autoStart";
            this.menuItem_autoStart.Size = new System.Drawing.Size(256, 24);
            this.menuItem_autoStart.Text = "Auto Start";
            // 
            // menuItem_minimizeToSystemTray
            // 
            this.menuItem_minimizeToSystemTray.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.menuItem_minimizeToSystemTray.CheckOnClick = true;
            this.menuItem_minimizeToSystemTray.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.menuItem_minimizeToSystemTray.Name = "menuItem_minimizeToSystemTray";
            this.menuItem_minimizeToSystemTray.Size = new System.Drawing.Size(256, 24);
            this.menuItem_minimizeToSystemTray.Text = "Minimize to Tray";
            // 
            // toolStripSeparator2
            // 
            this.toolStripSeparator2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.toolStripSeparator2.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.toolStripSeparator2.Margin = new System.Windows.Forms.Padding(0, 0, 0, 1);
            this.toolStripSeparator2.Name = "toolStripSeparator2";
            this.toolStripSeparator2.Size = new System.Drawing.Size(151, 6);
            // 
            // menuItem_exit
            // 
            this.menuItem_exit.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.menuItem_exit.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.menuItem_exit.Name = "menuItem_exit";
            this.menuItem_exit.Size = new System.Drawing.Size(154, 24);
            this.menuItem_exit.Text = "Exit";
            // 
            // viewToolStripMenuItem
            // 
            this.viewToolStripMenuItem.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.viewToolStripMenuItem.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_minimizeAllPanels});
            this.viewToolStripMenuItem.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.viewToolStripMenuItem.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.viewToolStripMenuItem.Name = "viewToolStripMenuItem";
            this.viewToolStripMenuItem.Size = new System.Drawing.Size(122, 24);
            this.viewToolStripMenuItem.Text = "View";
            // 
            // menuItem_minimizeAllPanels
            // 
            this.menuItem_minimizeAllPanels.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.menuItem_minimizeAllPanels.CheckOnClick = true;
            this.menuItem_minimizeAllPanels.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.menuItem_minimizeAllPanels.Name = "menuItem_minimizeAllPanels";
            this.menuItem_minimizeAllPanels.ShortcutKeys = ((System.Windows.Forms.Keys)((System.Windows.Forms.Keys.Control | System.Windows.Forms.Keys.M)));
            this.menuItem_minimizeAllPanels.Size = new System.Drawing.Size(281, 24);
            this.menuItem_minimizeAllPanels.Text = "Minimize All Panels     ";
            // 
            // menuItem_help
            // 
            this.menuItem_help.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.menuItem_help.DropDownItems.AddRange(new System.Windows.Forms.ToolStripItem[] {
            this.menuItem_userGuide,
            this.menuItem_downloadLatestRelease});
            this.menuItem_help.Font = new System.Drawing.Font("Segoe UI", 11.25F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point);
            this.menuItem_help.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.menuItem_help.Name = "menuItem_help";
            this.menuItem_help.Size = new System.Drawing.Size(53, 24);
            this.menuItem_help.Text = "Help";
            // 
            // menuItem_userGuide
            // 
            this.menuItem_userGuide.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.menuItem_userGuide.DisplayStyle = System.Windows.Forms.ToolStripItemDisplayStyle.Text;
            this.menuItem_userGuide.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.menuItem_userGuide.Name = "menuItem_userGuide";
            this.menuItem_userGuide.Size = new System.Drawing.Size(245, 24);
            this.menuItem_userGuide.Text = "User Guide";
            this.menuItem_userGuide.TextDirection = System.Windows.Forms.ToolStripTextDirection.Horizontal;
            // 
            // menuItem_downloadLatestRelease
            // 
            this.menuItem_downloadLatestRelease.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(60)))), ((int)(((byte)(63)))), ((int)(((byte)(65)))));
            this.menuItem_downloadLatestRelease.ForeColor = System.Drawing.Color.FromArgb(((int)(((byte)(220)))), ((int)(((byte)(220)))), ((int)(((byte)(220)))));
            this.menuItem_downloadLatestRelease.Name = "menuItem_downloadLatestRelease";
            this.menuItem_downloadLatestRelease.Size = new System.Drawing.Size(245, 24);
            this.menuItem_downloadLatestRelease.Text = "Download Latest Release";
            // 
            // StartupForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(914, 540);
            this.Controls.Add(this.lblVersion);
            this.Controls.Add(this.labelMsfsConnection);
            this.Controls.Add(this.panelStatus);
            this.Controls.Add(this.panelSteps);
            this.Controls.Add(this.darkMenuStrip1);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.FixedSingle;
            this.Icon = ((System.Drawing.Icon)(resources.GetObject("$this.Icon")));
            this.MainMenuStrip = this.darkMenuStrip1;
            this.MaximizeBox = false;
            this.Name = "StartupForm";
            this.SizeGripStyle = System.Windows.Forms.SizeGripStyle.Hide;
            this.Text = "MSFS Pop Out Panel Manager";
            this.Load += new System.EventHandler(this.StartupForm_Load);
            this.Resize += new System.EventHandler(this.StartupForm_Resize);
            this.panelStatus.ResumeLayout(false);
            this.panelStatus.PerformLayout();
            this.darkMenuStrip1.ResumeLayout(false);
            this.darkMenuStrip1.PerformLayout();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.Panel panelSteps;
        private System.Windows.Forms.Label labelMsfsConnection;
        private System.Windows.Forms.Panel panelStatus;
        private System.Windows.Forms.NotifyIcon notifyIcon1;
        private DarkUI.Controls.DarkTextBox txtBoxStatus;
        private DarkUI.Controls.DarkLabel lblVersion;
        private DarkUI.Controls.DarkLabel darkLabel3;
        private DarkUI.Controls.DarkMenuStrip darkMenuStrip1;
        private System.Windows.Forms.ToolStripMenuItem fileToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItem_exit;
        private System.Windows.Forms.ToolStripMenuItem menuItem_autoStart;
        private System.Windows.Forms.ToolStripMenuItem menuItem_alwaysOnTop;
        private System.Windows.Forms.ToolStripMenuItem menuItem_autoPanning;
        private System.Windows.Forms.ToolStripMenuItem menuItem_minimizeToSystemTray;
        private System.Windows.Forms.ToolStripMenuItem menuItem_help;
        private System.Windows.Forms.ToolStripMenuItem menuItem_userGuide;
        private System.Windows.Forms.ToolStripMenuItem menuItem_downloadLatestRelease;
        private System.Windows.Forms.ToolStripMenuItem menuItem_restart;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator1;
        private System.Windows.Forms.ToolStripMenuItem preferencesToolStripMenuItem;
        private System.Windows.Forms.ToolStripSeparator toolStripSeparator2;
        private System.Windows.Forms.ToolStripMenuItem viewToolStripMenuItem;
        private System.Windows.Forms.ToolStripMenuItem menuItem_minimizeAllPanels;
    }
}