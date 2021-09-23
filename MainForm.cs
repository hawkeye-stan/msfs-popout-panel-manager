using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
{
    public partial class MainForm : Form
    {
        private SynchronizationContext _syncRoot;
        private FileManager _fileManager;
        private WindowManager _windowManager;

        private static readonly IntPtr HWND_TOPMOST = new IntPtr(-1);
        private const UInt32 SWP_NOSIZE = 0x0001;
        private const UInt32 SWP_NOMOVE = 0x0002;
        private const UInt32 TOPMOST_FLAGS = SWP_NOMOVE | SWP_NOSIZE;

        [DllImport("user32.dll", EntryPoint = "SetWindowPos")]
        public static extern IntPtr SetWindowPos(IntPtr hWnd, IntPtr hWndInsertAfter, int x, int Y, int cx, int cy, uint wFlags);

        public MainForm()
        {
            InitializeComponent();
            _syncRoot = SynchronizationContext.Current;

            _fileManager = new FileManager(Application.StartupPath);

            _windowManager = new WindowManager(_fileManager);
            _windowManager.OnStatusUpdated += HandleOnStatusUpdated;
            _windowManager.OnSimulatorStarted += HandleOnSimulatorStarted;
            _windowManager.OnOcrDebugged += HandleOnOcrDebugged;
            _windowManager.CheckSimulatorStarted();

            SetProfileDropDown();

#if DEBUG
            // Set application windows always on top for easy debugging
            SetWindowPos(this.Handle, HWND_TOPMOST, this.Left, this.Top, this.Width, this.Height, TOPMOST_FLAGS);
#endif
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            lblVersion.Text += version.ToString();
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();

            var profile = GetProfileDropDown();
            var success = _windowManager.Analyze(profile);

            btnApplySettings.Enabled = success;
            btnSaveSettings.Enabled = success;
        }

        private void btnApplySettings_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();

            var profile = GetProfileDropDown();
            _windowManager.ApplySettings(profile, chkHidePanelTitleBar.Checked, chkAlwaysOnTop.Checked);
        }

        private void btnSaveSettings_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();

            var profile = GetProfileDropDown();
            _windowManager.SaveSettings(profile, chkHidePanelTitleBar.Checked, chkAlwaysOnTop.Checked);
        }

        private void SetProfileDropDown()
        {
            try
            {
                var profileData = _fileManager.ReadProfileData();
                var profiles = profileData.Select(x => x.Profile).Distinct().OrderBy(x => x);
                var defaultProfile = profileData.Find(x => x.DefaultProfile);

                comboBoxProfile.DataSource = profiles.ToList();
                comboBoxProfile.SelectedItem = defaultProfile.Profile;
            }
            catch (Exception ex)
            {
                SetStatusMessage(ex.Message);
            }
        }

        private string GetProfileDropDown()
        {
            return comboBoxProfile.SelectedItem.ToString();
        }

        private void comboBoxProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            _windowManager.Reset();

            var userData = _fileManager.ReadUserData();

            if (userData != null)
            {
                var userProfile = userData.Profiles.Find(x => x.Name == Convert.ToString(comboBoxProfile.SelectedValue));
                if (userProfile != null)
                {
                    chkAlwaysOnTop.Checked = userProfile.AlwaysOnTop;
                    chkHidePanelTitleBar.Checked = userProfile.HidePanelTitleBar;
                }
                else
                {
                    // default values
                    chkAlwaysOnTop.Checked = false;
                    chkHidePanelTitleBar.Checked = false;
                }
            }

            btnApplySettings.Enabled = false;
            btnSaveSettings.Enabled = false;
        }

        private void HandleOnStatusUpdated(object source, EventArgs<string> arg)
        {
            _syncRoot.Post(SetStatusMessage, arg.Value);
        }

        private void SetStatusMessage(object arg)
        {
            var msg = arg as string;
            if (msg != null)
                txtStatus.Text = msg;
        }

        private void HandleOnSimulatorStarted(object source, EventArgs arg)
        {
            _syncRoot.Post(SetMsfsRunningMessage, "MSFS is running");
            
        }

        private void SetMsfsRunningMessage(object arg)
        {
            var msg = arg as string;
            if (msg != null)
            {
                lblMsfsRunning.Text = "MSFS is running";
                lblMsfsRunning.ForeColor = Color.Green;
            }

            btnAnalyze.Enabled = true;
        }

        private void HandleOnOcrDebugged(object source, EventArgs<Dictionary<string, string>> arg)
        {
            _syncRoot.Post(SetOcrDebugInfo, arg.Value);
        }

        private void SetOcrDebugInfo(object arg)
        {
            tabControlOcrDebug.TabPages.Clear();

            var debugInfo = arg as Dictionary<string, string>;
            if (debugInfo != null && debugInfo.Count > 0)
            {
                foreach(var info in debugInfo)
                {
                    var tabPage = new TabPage();
                    tabPage.Name = info.Key;
                    tabPage.Text = info.Key;

                    var txtBox = new TextBox();
                    txtBox.Width = tabControlOcrDebug.Width;
                    txtBox.Height = tabControlOcrDebug.Height;
                    txtBox.ReadOnly = true;
                    txtBox.Multiline = true;
                    txtBox.BorderStyle = BorderStyle.None;
                    txtBox.Text = info.Value;

                    tabPage.Controls.Add(txtBox);

                    tabControlOcrDebug.TabPages.Add(tabPage);
                }
            }
        }

        private void MainForm_Load(object sender, EventArgs e)
        {
            notifyIcon1.BalloonTipText = "Application Minimized";
            notifyIcon1.BalloonTipTitle = "MSFS 2020 Pop Out Panel Manager";
        }

        private void MainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // Put all panels popout back to original state
            _windowManager.RestorePanelTitleBar();
        }

        private void MainForm_Resize(object sender, EventArgs e)
        {
            if (this.WindowState == FormWindowState.Minimized)
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

        
    }
}