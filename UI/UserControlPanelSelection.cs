using System;
using System.Collections.Generic;
using System.Data;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows.Forms;

namespace MSFSPopoutPanelManager
{
    public partial class UserControlPanelSelection : UserControlCommon
    {
        private SynchronizationContext _syncRoot;

        public UserControlPanelSelection(PanelManager panelManager) : base(panelManager)
        {
            InitializeComponent();
            _syncRoot = SynchronizationContext.Current;

            panelManager.OnSimulatorStarted += (source, e) => { _syncRoot.Post((arg) => { panel3.Enabled = true; }, null); };

            panelManager.PanelLocationSelection.OnSelectionStarted += PanelLocationSelection_OnSelectionStarted;
            panelManager.PanelLocationSelection.OnSelectionCompleted += PanelLocationSelection_OnSelectionCompleted;
            panelManager.PanelLocationSelection.OnLocationListChanged += PanelLocationSelection_OnLocationListChanged;

            SetProfileDropDown();
        }

        public event EventHandler<EventArgs<bool>> OnAnalyzeAvailabilityChanged;

        private void PanelLocationSelection_OnSelectionStarted(object sender, EventArgs e)
        {
            buttonPanelSelection.Enabled = false;
            buttonAnalyze.Enabled = false;

            OnAnalyzeAvailabilityChanged?.Invoke(this, new EventArgs<bool>(false));
        }

        private void PanelLocationSelection_OnSelectionCompleted(object sender, EventArgs e)
        {
            buttonPanelSelection.Enabled = true;
            buttonAnalyze.Enabled = PanelManager.CurrentPanelProfile.PanelSourceCoordinates.Count > 0;

            OnAnalyzeAvailabilityChanged?.Invoke(this, new EventArgs<bool>(true));
            buttonAnalyze.Focus();
        }

        private void PanelLocationSelection_OnLocationListChanged(object sender, EventArgs e)
        {
            var sb = new StringBuilder();

            if (PanelManager.CurrentPanelProfile.PanelSourceCoordinates.Count == 0)
            {
                textBoxPanelLocations.Text = null;
            }
            else
            {
                foreach (var coor in PanelManager.CurrentPanelProfile.PanelSourceCoordinates)
                {
                    sb.Append($"Panel: {coor.PanelIndex,-7} X-Pos: {coor.X,-10} Y-Pos: {coor.Y,-10}");
                    sb.Append(Environment.NewLine);
                }

                textBoxPanelLocations.Text = sb.ToString();
            }
        }

        public void SetProfileDropDown()
        {
            try
            {
                var defaultProfileId = FileManager.ReadUserData().DefaultProfileId;

                var profileData = FileManager.ReadPlaneProfileData();
                comboBoxProfile.DisplayMember = "ProfileName";
                comboBoxProfile.ValueMember = "ProfileId";
                comboBoxProfile.DataSource = profileData.OrderBy(x => x.ProfileName).ToList();
                comboBoxProfile.SelectedValue = defaultProfileId;
            }
            catch (Exception ex)
            {
                Logger.LogStatus(ex.Message);
            }
        }

        private void buttonPanelSelection_Click(object sender, EventArgs e)
        {
            bool continued = true;

            if (PanelManager.CurrentPanelProfile != null && PanelManager.CurrentPanelProfile.PanelSettings.PanelDestinationList.Count > 0)
            {
                var dialogResult = MessageBox.Show("Are you sure you want to overwrite existing saved panel locations and settings for this profile?", "Confirm Overwrite", MessageBoxButtons.YesNo);
                continued = dialogResult == DialogResult.Yes;
            }

            if (continued)
            {
                PanelManager.PanelLocationSelection.Start();
                checkBoxShowPanelLocation.Checked = true;
            }
        }

        private void comboBoxProfile_SelectedIndexChanged(object sender, EventArgs e)
        {
            PanelManager.PlaneProfileChanged(Convert.ToInt32(comboBoxProfile.SelectedValue), checkBoxShowPanelLocation.Checked);
            buttonPanelSelection.Enabled = true;
        }

        private void checkBoxShowPanelLocation_CheckedChanged(object sender, EventArgs e)
        {
            PanelManager.PanelLocationSelection.ShowPanelLocationOverlay(checkBoxShowPanelLocation.Checked);
        }

        private void buttonAnalyze_Click(object sender, EventArgs e)
        {
            Logger.LogStatus("Panel analysis in progress. Please wait...");

            panel1.Enabled = false;
            panel2.Enabled = false;
            panel3.Enabled = false;

            PanelManager.Analyze();

            panel1.Enabled = true;
            panel2.Enabled = true;
            panel3.Enabled = true;
        }

        private void buttonSetDefault_Click(object sender, EventArgs e)
        {
            PanelManager.SetDefaultProfile();
        }
    }
}
