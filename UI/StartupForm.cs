using DarkUI.Forms;
using MSFSPopoutPanelManager.Shared;
using MSFSPopoutPanelManager.UIController;
using System;
using System.Diagnostics;
using System.Drawing;
using System.Threading;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager.UI
{
    public partial class StartupForm : DarkForm
    {
        private Color ERROR_MESSAGE_COLOR = Color.FromArgb(1, 255, 71, 71);
        private Color SUCCESS_MESSAGE_COLOR = Color.LightGreen;
        private Color INFO_MESSAGE_COLOR = Color.White;

        private SynchronizationContext _syncRoot;
        private UserControlPanelSelection _ucPanelSelection;
        private UserControlPanelConfiguration _ucPanelConfiguration;

        private StartUpController _controller;

        public StartupForm()
        {
            InitializeComponent();
            _syncRoot = SynchronizationContext.Current;
            _ucPanelSelection = new UserControlPanelSelection();
            _ucPanelConfiguration = new UserControlPanelConfiguration();
            panelSteps.Controls.Add(_ucPanelSelection);
            panelSteps.Controls.Add(_ucPanelConfiguration);

            // Set version number
            lblVersion.Text += System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.ToString();

            _controller = new StartUpController(this);
            _controller.OnSimConnectionChanged += HandleSimConnectionChanged;
            _controller.OnPanelSelectionActivated += (source, e) => { _ucPanelSelection.Visible = true; _ucPanelConfiguration.Visible = false; };
            _controller.OnPanelConfigurationActivated += (source, e) => { _ucPanelSelection.Visible = false; _ucPanelConfiguration.Visible = true; };
            _controller.Initialize();

            checkBoxMinimizeToTray.DataBindings.Add("Checked", _controller, "IsMinimizeToTray");
            checkBoxMinimizeToTray.CheckedChanged += (source, e) => _controller.SetMinimizeToTray(checkBoxMinimizeToTray.Checked);

            checkBoxAlwaysOnTop.DataBindings.Add("Checked", _controller, "IsAlwaysOnTop");
            checkBoxAlwaysOnTop.CheckedChanged += (source, e) => _controller.SetAlwaysOnTop(checkBoxAlwaysOnTop.Checked);

            checkBoxAutoStart.DataBindings.Add("Checked", _controller, "IsAutoStart");
            checkBoxAutoStart.CheckedChanged += (source, e) => _controller.SetAutoStart(checkBoxAutoStart.Checked);

            checkBoxAutoPanning.DataBindings.Add("Checked", _controller, "UseAutoPanning");
            checkBoxAutoPanning.CheckedChanged += (source, e) => _controller.SetAutoPanning(checkBoxAutoPanning.Checked);

            Logger.OnStatusLogged += Logger_OnStatusLogged;
            Logger.OnBackgroundStatusLogged += Logger_OnBackgroundStatusLogged;
        }

        private void HandleSimConnectionChanged(object sender, EventArgs<bool> e)
        {
            _syncRoot.Post((arg) =>
            {
                var connected = Convert.ToBoolean(arg);
                if (connected)
                {
                    labelMsfsConnection.ForeColor = SUCCESS_MESSAGE_COLOR;
                    labelMsfsConnection.Text = "MSFS Connected";
                }
                else
                {
                    labelMsfsConnection.ForeColor = ERROR_MESSAGE_COLOR;
                    labelMsfsConnection.Text = "MSFS Disconnected";
                }
            }, e.Value);
        }

        private void Logger_OnStatusLogged(object sender, EventArgs<StatusMessage> e)
        {
            if (e != null)
            {
                txtBoxStatus.ForeColor = e.Value.MessageType == StatusMessageType.Info ? INFO_MESSAGE_COLOR : ERROR_MESSAGE_COLOR;
                txtBoxStatus.Text = e.Value.Message;
            }

            if (e.Value.MessageType == StatusMessageType.Error)
                PInvoke.SetForegroundWindow(Handle);
        }

        private void Logger_OnBackgroundStatusLogged(object sender, EventArgs<StatusMessage> e)
        {
            _syncRoot.Post((arg) =>
            {
                var statusMessage = arg as StatusMessage;
                if (statusMessage != null)
                {
                    txtBoxStatus.ForeColor = statusMessage.MessageType == StatusMessageType.Info ? INFO_MESSAGE_COLOR : ERROR_MESSAGE_COLOR;
                    txtBoxStatus.Text = statusMessage.Message;
                }

                if (e.Value.MessageType == StatusMessageType.Error)
                    PInvoke.SetForegroundWindow(Handle);
            }, e.Value);
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
    }
}