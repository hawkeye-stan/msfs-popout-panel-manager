using DarkUI.Forms;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
{
    public partial class StartupForm : DarkForm
    {
        private SynchronizationContext _syncRoot;
        private PanelManager _panelManager;
        private UserControlPanelSelection _ucPanelSelection;
        private UserControlApplySettings _ucApplySettings;


        public StartupForm()
        {
            InitializeComponent();
            _syncRoot = SynchronizationContext.Current;

            // Set version number
            lblVersion.Text += System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            Logger.OnStatusLogged += Logger_OnStatusLogged;
            
            _panelManager = new PanelManager(this);
            _panelManager.OnSimulatorStarted += PanelManager_OnSimulatorStarted;

            _ucPanelSelection = new UserControlPanelSelection(_panelManager);
            _ucPanelSelection.Visible = true;
            
            _ucApplySettings = new UserControlApplySettings(_panelManager);
            _ucApplySettings.OnRestart += (source, e) => { _ucPanelSelection.Visible = true; _ucApplySettings.Visible = false; };
            _ucApplySettings.Visible = false;

            panelSteps.Controls.Add(_ucPanelSelection);
            panelSteps.Controls.Add(_ucApplySettings);

            _panelManager.OnAnalysisCompleted += (source, e) => { _ucPanelSelection.Visible = false; _ucApplySettings.Visible = true; };
            _panelManager.CheckSimulatorStarted();

            checkBoxAutoStart.Checked = Autostart.CheckIsAutoStart();
        }

        private void Logger_OnStatusLogged(object sender, EventArgs<StatusMessage> e)
        {
            _syncRoot.Post((arg) =>
            {
                var msg = arg as string;
                if (msg != null)
                    txtBoxStatus.Text = msg;
            }, e.Value.Message);
        }

        private void PanelManager_OnSimulatorStarted(object sender, EventArgs e)
        {
            _syncRoot.Post((arg) =>
            {
                panelStatus.Enabled = true;
                labelMsfsRunning.Text = "MSFS is running";
                labelMsfsRunning.ForeColor = Color.LightGreen;
            }, null);
        }

        private void StartupForm_Load(object sender, EventArgs e)
        {
            notifyIcon1.BalloonTipText = "Application Minimized";
            notifyIcon1.BalloonTipTitle = "MSFS 2020 Pop Out Panel Manager";
        }

        private void StartupForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                if (checkBoxMinimizeToTray.Checked)
                {
                    ShowInTaskbar = false;
                    notifyIcon1.Visible = true;
                    notifyIcon1.ShowBalloonTip(1000);
                }
            }
        }

        private void StartupForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Put all panels popout back to original state
            //_windowManager.RestorePanelTitleBar();
        }

        private void notifyIcon1_DoubleClick(object sender, EventArgs e)
        {
            ShowInTaskbar = true;
            notifyIcon1.Visible = false;
            WindowState = FormWindowState.Normal;
        }

        private void linkLabel1_LinkClicked(object sender, LinkLabelLinkClickedEventArgs e)
        {
            linkLabel1.LinkVisited = true;

            Process.Start(new ProcessStartInfo("https://github.com/hawkeye-stan/msfs-popout-panel-manager") { UseShellExecute = true });
        }

        private void checkBoxAutoStart_CheckedChanged(object sender, EventArgs e)
        {
            if (checkBoxAutoStart.Checked)
                Autostart.Activate();
            else
                Autostart.Deactivate();
        }
    }
}


