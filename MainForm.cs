using System;
using System.Collections.Generic;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Threading;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
{
    public partial class MainForm : Form
    {
        private SynchronizationContext _syncRoot;
        private WindowManager _popoutWindowsManager;

        public MainForm()
        {
            InitializeComponent();

            _syncRoot = SynchronizationContext.Current;

            SetProfileDropDown();

            _popoutWindowsManager = new WindowManager();
            _popoutWindowsManager.OnStatusUpdated += HandleOnStatusUpdated;
            _popoutWindowsManager.OnSimulatorStarted += HandleOnSimulatorStarted;
            _popoutWindowsManager.OnOcrDebugged += HandleOnOcrDebugged;
            _popoutWindowsManager.CheckSimulatorStarted();
        }

        private void btnAnalyze_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();

            var profile = GetProfileDropDown();
            _popoutWindowsManager.Analyze(profile);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            txtStatus.Clear();

            var profile = GetProfileDropDown();
            _popoutWindowsManager.SaveProfile(profile);
        }

        private void SetProfileDropDown()
        {
            try
            {
                var profileData = FileManager.ReadProfileData();
                var profiles = profileData.Select(x => x.Profile).Distinct();
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
            btnSave.Enabled = true;
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
    }
}