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
    public partial class StartupForm : DarkForm, IApplicationView
    {
        private Color ERROR_MESSAGE_COLOR = Color.FromArgb(1, 255, 71, 71);
        private Color SUCCESS_MESSAGE_COLOR = Color.LightGreen;
        private Color INFO_MESSAGE_COLOR = Color.White;

        private SynchronizationContext _syncRoot;
        private UserControlPanelSelection _ucPanelSelection;
        private UserControlPanelConfiguration _ucPanelConfiguration;

        private ApplicationController _controller;

        public StartupForm()
        {
            InitializeComponent();

            _ucPanelSelection = new UserControlPanelSelection();
            _ucPanelConfiguration = new UserControlPanelConfiguration();
            panelSteps.Controls.Add(_ucPanelSelection);
            panelSteps.Controls.Add(_ucPanelConfiguration);

            _syncRoot = SynchronizationContext.Current;

            // Set version number
            lblVersion.Text += $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Major}.{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Minor}.{System.Reflection.Assembly.GetExecutingAssembly().GetName().Version.Build}";

            Logger.OnStatusLogged += Logger_OnStatusLogged;
            Logger.OnBackgroundStatusLogged += Logger_OnBackgroundStatusLogged;
         
            _controller = new ApplicationController(this);
            _controller.OnSimConnectionChanged += HandleSimConnectionChanged;
            _controller.OnPanelSelectionActivated += HandleShowPanelSelection;
            _controller.OnPanelConfigurationActivated += HandleShowPanelConfiguration;
            _controller.Initialize();
            _ucPanelSelection.Initialize(_controller.PanelSelectionController);
            _ucPanelConfiguration.Initialize(_controller.PanelConfigurationController);

            menuItem_restart.Click += HandleMenuClicked;
            menuItem_exit.Click += HandleMenuClicked;
            menuItem_alwaysOnTop.Click += HandleMenuClicked;
            menuItem_autoPanning.Click += HandleMenuClicked;
            menuItem_autoStart.Click += HandleMenuClicked;
            menuItem_minimizeToSystemTray.Click += HandleMenuClicked;
            menuItem_minimizeAllPanels.Click += HandleMenuClicked;
            menuItem_userGuide.Click += HandleMenuClicked;
            menuItem_downloadLatestRelease.Click += HandleMenuClicked;
        }

        #region Implement view interface

        public Form Form { get => this; }

        public IPanelSelectionView PanelSelection { get => _ucPanelSelection; }

        public IPanelConfigurationView PanelConfiguration { get => _ucPanelConfiguration; }

        public bool MinimizeToTray { get => menuItem_minimizeToSystemTray.Checked; set => menuItem_minimizeToSystemTray.Checked = value; }

        public bool AlwaysOnTop { get => menuItem_alwaysOnTop.Checked; set => menuItem_alwaysOnTop.Checked = value; }

        public bool AutoStart { get => menuItem_autoStart.Checked; set => menuItem_autoStart.Checked = value; }

        public bool AutoPanning { get => menuItem_autoPanning.Checked; set => menuItem_autoPanning.Checked = value; }

        #endregion

        private void StartupForm_Load(object sender, EventArgs e)
        {
            notifyIcon1.BalloonTipText = "Application Minimized";
            notifyIcon1.BalloonTipTitle = "MSFS 2020 Pop Out Panel Manager";
        }

        private void StartupForm_Resize(object sender, EventArgs e)
        {
            if (WindowState == FormWindowState.Minimized)
            {
                if (menuItem_minimizeToSystemTray.Checked)
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
                this.ActiveControl = this.panelStatus;
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
                    this.ActiveControl = this.panelStatus;
                }

                if (e.Value.MessageType == StatusMessageType.Error)
                    PInvoke.SetForegroundWindow(Handle);
            }, e.Value);
        }

        private void HandleMenuClicked(object sender, EventArgs e)
        {
            var itemName = ((ToolStripMenuItem)sender).Name;

            switch (itemName)
            {
                case nameof(menuItem_restart):
                    _controller.Restart();
                    break;
                case nameof(menuItem_exit):
                    Application.Exit();
                    break;
                case nameof(menuItem_alwaysOnTop):
                    _controller.SetAlwaysOnTop(menuItem_alwaysOnTop.Checked);
                    break;
                case nameof(menuItem_autoPanning):
                    _controller.SetAutoPanning(menuItem_autoPanning.Checked);
                    break;
                case nameof(menuItem_autoStart):
                    _controller.SetAutoStart(menuItem_autoStart.Checked);
                    break;
                case nameof(menuItem_minimizeToSystemTray):
                    _controller.SetMinimizeToTray(menuItem_minimizeToSystemTray.Checked);
                    break;
                case nameof(menuItem_minimizeAllPanels):
                    _controller.MinimizeAllPanels(menuItem_minimizeAllPanels.Checked);
                    break;
                case nameof(menuItem_userGuide):
                    Process.Start(new ProcessStartInfo("https://github.com/hawkeye-stan/msfs-popout-panel-manager#msfs-pop-out-panel-manager") { UseShellExecute = true });
                    return;
                case nameof(menuItem_downloadLatestRelease):
                    Process.Start(new ProcessStartInfo("https://github.com/hawkeye-stan/msfs-popout-panel-manager/releases") { UseShellExecute = true });
                    return;
            }
        }

        private void HandleShowPanelSelection(object sender, EventArgs e)
        {
            if (_ucPanelSelection != null && _ucPanelConfiguration != null)
            {
                _ucPanelSelection.Visible = true;
                _ucPanelConfiguration.Visible = false;

                menuItem_restart.Enabled = false;
                menuItem_minimizeAllPanels.Checked = false;
                menuItem_minimizeAllPanels.Enabled = false;
            }
        }

        private void HandleShowPanelConfiguration(object sender, EventArgs e)
        {
            if (_ucPanelSelection != null && _ucPanelConfiguration != null)
            {
                _ucPanelSelection.Visible = false;
                _ucPanelConfiguration.Visible = true;

                menuItem_restart.Enabled = true;
                menuItem_minimizeAllPanels.Enabled = true;
            }
        }
    }
}